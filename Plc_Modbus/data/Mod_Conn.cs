using NModbus;
using Plc_Modbus.Config;
using System.Diagnostics;
using System.Net.Sockets;

namespace Plc_Modbus.data
{
    public sealed class Mod_Conn : IDisposable
    {
        private readonly PlcSettings _settings;
        private readonly SemaphoreSlim _connectionLock = new(1, 1);
        private readonly SemaphoreSlim _ioLock = new(1, 1);
        private readonly CancellationTokenSource _shutdownCts = new();

        private TcpClient? _client;
        private IModbusMaster? _master;
        private bool _disposed;

        public Mod_Conn()
            : this(AppConfigLoader.Load().PlcSettings)
        {
        }

        public Mod_Conn(PlcSettings settings)
        {
            _settings = settings;
        }

        public PlcSettings Settings => _settings;

        public bool IsConnected =>
            !_disposed && _client?.Connected == true && _master is not null;

        public async Task EnsureConnectedAsync(CancellationToken cancellationToken)
        {
            using CancellationTokenSource linkedCts =
                CancellationTokenSource.CreateLinkedTokenSource(
                    cancellationToken, _shutdownCts.Token);

            int attempt = 0;
            while (!linkedCts.Token.IsCancellationRequested)
            {
                if (IsConnected)
                    return;

                if (await TryConnectAsync(linkedCts.Token))
                    return;

                attempt++;
                int delayMs = Math.Min(500 * (1 << Math.Min(attempt, 4)), 5000);
                Debug.WriteLine(
                    $"[PLC] 재연결 대기: {delayMs}ms (시도 {attempt})");
                await Task.Delay(delayMs, linkedCts.Token);
            }

            linkedCts.Token.ThrowIfCancellationRequested();
        }

        public async Task<T> ExecuteAsync<T>(
            Func<IModbusMaster, Task<T>> operation,
            CancellationToken cancellationToken)
        {
            while (true)
            {
                await EnsureConnectedAsync(cancellationToken);
                await _ioLock.WaitAsync(cancellationToken);
                try
                {
                    if (!IsConnected || _master is null)
                        continue;

                    return await operation(_master);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[PLC] 통신 실패: {ex.Message}");
                    await DisconnectAsync();
                    throw;
                }
                finally
                {
                    _ioLock.Release();
                }
            }
        }

        public async Task ExecuteAsync(
            Func<IModbusMaster, Task> operation,
            CancellationToken cancellationToken)
        {
            await ExecuteAsync(
                async master =>
                {
                    await operation(master);
                    return true;
                },
                cancellationToken);
        }

        public async Task DisconnectAsync()
        {
            await _connectionLock.WaitAsync();
            try
            {
                CloseConnection();
            }
            finally
            {
                _connectionLock.Release();
            }
        }

        private async Task<bool> TryConnectAsync(CancellationToken cancellationToken)
        {
            await _connectionLock.WaitAsync(cancellationToken);
            try
            {
                if (IsConnected)
                    return true;

                CloseConnection();

                using CancellationTokenSource timeoutCts =
                    CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                timeoutCts.CancelAfter(_settings.ConnectTimeoutMs);

                TcpClient client = new();
                try
                {
                    await client.ConnectAsync(
                        _settings.Host, _settings.Port, timeoutCts.Token);

                    IModbusMaster master = new ModbusFactory().CreateMaster(client);
                    master.Transport.ReadTimeout = _settings.ReadTimeoutMs;
                    master.Transport.WriteTimeout = _settings.ReadTimeoutMs;
                    master.Transport.Retries = 0;

                    _client = client;
                    _master = master;
                    Debug.WriteLine(
                        $"[PLC] 연결 성공: {_settings.Host}:{_settings.Port}");
                    return true;
                }
                catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
                {
                    client.Dispose();
                    Debug.WriteLine("[PLC] 연결 시간 초과");
                    return false;
                }
                catch (Exception ex)
                {
                    client.Dispose();
                    Debug.WriteLine($"[PLC] 연결 실패: {ex.Message}");
                    return false;
                }
            }
            finally
            {
                _connectionLock.Release();
            }
        }

        private void CloseConnection()
        {
            _master?.Dispose();
            _client?.Dispose();
            _master = null;
            _client = null;
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            _shutdownCts.Cancel();
            CloseConnection();
            _shutdownCts.Dispose();
            _connectionLock.Dispose();
            _ioLock.Dispose();
        }
    }
}
