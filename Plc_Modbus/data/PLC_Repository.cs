using Dapper;
using Plc_Modbus.Model;
using System.Diagnostics;

namespace Plc_Modbus.data
{
    public sealed class PLC_Repository : IDisposable
    {
        private readonly DB_Conn _connectionFactory = new();

        public async Task<bool> InsertSensorDataAsync(
            IReadOnlyCollection<PlcDBDto> sensorData,
            CancellationToken cancellationToken)
        {
            if (sensorData.Count == 0)
                return true;

            const string sql = """
                INSERT INTO sensor_data
                    (time, equip_id, address, metric_name, metric_value, unit, quality, source_time, collected_at)
                VALUES
                    (NOW(), @Equip_id, @Address, @Metric_name, @Metric_value, @Unit, @Quality, @Source_time, @Collected_at)
                """;

            try
            {
                await using var connection = _connectionFactory.CreateConn();
                await connection.OpenAsync(cancellationToken);
                var command = new CommandDefinition(
                    sql,
                    sensorData,
                    cancellationToken: cancellationToken);
                await connection.ExecuteAsync(command);
                return true;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DB] 센서 데이터 저장 실패: {ex.Message}");
                return false;
            }
        }

        public void Dispose()
        {
            _connectionFactory.Dispose();
        }
    }
}
