using Plc_Modbus.Model;
using System.Diagnostics;
using System.Threading.Channels;

namespace Plc_Modbus.data
{
    public sealed class Plc_Reader : IDisposable
    {
        private const int ProductionCountIndex = 0;
        private const int EquipmentStatusIndex = 1;
        private const int CycleTimeIndex = 2;
        private const int AlarmCodeIndex = 3;
        private const int VisionResultIndex = 4;
        private const int VisionSequenceIndex = 5;
        private const int TemperatureIndex = 9;
        private const int PressureIndex = 10;
        private const int CurrentIndex = 11;
        private const int PowerIndex = 12;
        private const int EnergyHighIndex = 13;
        private const int EnergyLowIndex = 14;

        private readonly Mod_Conn _connection;
        private readonly Action<PlcDto> _onDataReceived;
        private readonly Action<bool>? _onConnectionChanged;
        private readonly Plc_DBMapper _mapper = new();
        private readonly PLC_Repository _repository = new();
        private readonly Channel<IReadOnlyCollection<PlcDBDto>> _storageQueue;

        private ushort? _previousProductionCount;
        private ushort? _previousVisionSequence;
        private ushort? _previousAlarmCode;
        private bool? _lastConnectionState;
        private long _lastMetricQueuedAt;
        private bool _disposed;

        public Plc_Reader(
            Mod_Conn connection,
            Action<PlcDto> onDataReceived,
            Action<bool>? onConnectionChanged = null)
        {
            _connection = connection;
            _onDataReceived = onDataReceived;
            _onConnectionChanged = onConnectionChanged;
            _storageQueue = Channel.CreateBounded<IReadOnlyCollection<PlcDBDto>>(
                new BoundedChannelOptions(100)
                {
                    FullMode = BoundedChannelFullMode.Wait,
                    SingleReader = true,
                    SingleWriter = true
                });
        }

        public PlcDto LatestData { get; private set; } = new();

        public async Task ReadPlcAsync(CancellationToken cancellationToken)
        {
            Task storageTask = StoreDataAsync(cancellationToken);
            try
            {
                await PollAsync(cancellationToken);
            }
            finally
            {
                _storageQueue.Writer.TryComplete();
                try
                {
                    await storageTask;
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    // Normal application shutdown.
                }
            }
        }

        private async Task PollAsync(CancellationToken cancellationToken)
        {
            var settings = _connection.Settings;
            _lastMetricQueuedAt = Stopwatch.GetTimestamp();

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    ushort[] registers = await _connection.ExecuteAsync(
                        master => master.ReadHoldingRegistersAsync(
                            settings.UnitId,
                            settings.StartAddress,
                            settings.RegisterCount),
                        cancellationToken);

                    SetConnectionState(true);
                    PlcDto currentData = MapCurrentData(registers);
                    DetectEvents(currentData);

                    LatestData = currentData;
                    _onDataReceived(currentData);

                    if (ElapsedMilliseconds(_lastMetricQueuedAt)
                        >= settings.MetricIntervalMs)
                    {
                        IReadOnlyCollection<PlcDBDto> sensorData =
                            _mapper.HoldingMapping(registers);
                        if (!_storageQueue.Writer.TryWrite(sensorData))
                        {
                            Debug.WriteLine(
                                "[DB] 저장 큐가 가득 차 데이터를 추가하지 못했습니다.");
                        }
                        _lastMetricQueuedAt = Stopwatch.GetTimestamp();
                    }
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    SetConnectionState(false);
                    Debug.WriteLine($"[Collector] Poll 실패: {ex.Message}");
                }

                try
                {
                    await Task.Delay(settings.PollIntervalMs, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }

        private async Task StoreDataAsync(CancellationToken cancellationToken)
        {
            await foreach (IReadOnlyCollection<PlcDBDto> batch in
                _storageQueue.Reader.ReadAllAsync(cancellationToken))
            {
                bool saved = false;
                for (int attempt = 1; attempt <= 3 && !saved; attempt++)
                {
                    saved = await _repository.InsertSensorDataAsync(
                        batch, cancellationToken);
                    if (!saved && attempt < 3)
                    {
                        int retryDelayMs = 500 * (1 << (attempt - 1));
                        Debug.WriteLine(
                            $"[DB] Batch 저장 재시도 {attempt}/3: {retryDelayMs}ms 후");
                        await Task.Delay(retryDelayMs, cancellationToken);
                    }
                }

                if (!saved)
                {
                    Debug.WriteLine(
                        $"[DB] Batch 저장 3회 실패: {batch.Count}건을 보관하지 못했습니다.");
                }
            }
        }

        private static PlcDto MapCurrentData(ushort[] registers)
        {
            if (registers.Length <= EnergyLowIndex)
            {
                throw new InvalidOperationException(
                    $"필요한 레지스터는 15개지만 {registers.Length}개만 수신했습니다.");
            }

            uint accumulatedEnergyRaw =
                ((uint)registers[EnergyHighIndex] << 16)
                | registers[EnergyLowIndex];

            return new PlcDto
            {
                IsConnected = true,
                ProductionCount = registers[ProductionCountIndex],
                EquipmentStatus = registers[EquipmentStatusIndex],
                CycleTimeSeconds = registers[CycleTimeIndex] * 0.01,
                AlarmCode = registers[AlarmCodeIndex],
                VisionResult = registers[VisionResultIndex],
                VisionEventSequence = registers[VisionSequenceIndex],
                Temperature = registers[TemperatureIndex] * 0.1,
                Pressure = registers[PressureIndex] * 0.01,
                Current = registers[CurrentIndex] * 0.1,
                InstantPower = registers[PowerIndex] * 0.01,
                AccumulatedEnergy = accumulatedEnergyRaw * 0.01,
                CollectedAt = DateTimeOffset.UtcNow
            };
        }

        private void DetectEvents(PlcDto current)
        {
            if (_previousProductionCount.HasValue
                && current.ProductionCount != _previousProductionCount.Value)
            {
                if (current.ProductionCount > _previousProductionCount.Value)
                {
                    int increase = current.ProductionCount
                        - _previousProductionCount.Value;
                    Debug.WriteLine(
                        $"[생산] 수량 증가: +{increase}, 현재={current.ProductionCount}");
                }
                else
                {
                    Debug.WriteLine(
                        $"[생산] Counter 감소 감지: {_previousProductionCount}"
                        + $" -> {current.ProductionCount} (재시작/rollover 확인 필요)");
                }
            }

            if (_previousVisionSequence.HasValue
                && current.VisionEventSequence != _previousVisionSequence.Value)
            {
                Debug.WriteLine(
                    $"[Vision] 검사 이벤트: Seq={current.VisionEventSequence}, "
                    + $"Result={current.VisionResultText}");
            }

            if (_previousAlarmCode.HasValue
                && current.AlarmCode != _previousAlarmCode.Value)
            {
                string state = current.AlarmCode == 0 ? "해제" : "발생";
                Debug.WriteLine($"[Alarm] {state}: Code={current.AlarmCode}");
            }

            _previousProductionCount = current.ProductionCount;
            _previousVisionSequence = current.VisionEventSequence;
            _previousAlarmCode = current.AlarmCode;
        }

        private void SetConnectionState(bool connected)
        {
            if (_lastConnectionState == connected)
                return;

            _lastConnectionState = connected;
            _onConnectionChanged?.Invoke(connected);
            Debug.WriteLine(connected
                ? "[Collector] PLC ONLINE"
                : "[Collector] PLC DISCONNECTED");
        }

        private static double ElapsedMilliseconds(long startTimestamp)
        {
            return Stopwatch.GetElapsedTime(startTimestamp).TotalMilliseconds;
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            _repository.Dispose();
        }
    }
}
