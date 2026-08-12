using Npgsql;
using Plc_Modbus.Config;
using System.Diagnostics;

namespace Plc_Modbus.data
{
    public sealed class DB_Conn : IDisposable
    {
        private readonly string _connectionString;
        private readonly CancellationTokenSource _cts = new();

        public DB_Conn()
        {
            DBSettings db = AppConfigLoader.Load().DBSettings
                ?? throw new InvalidOperationException("DBSettings가 필요합니다.");

            var builder = new NpgsqlConnectionStringBuilder
            {
                Host = db.Host,
                Port = int.TryParse(db.Port, out int port) ? port : 5432,
                Username = db.Username,
                Password = db.Password,
                Database = db.Database,
                Timeout = 3,
                CommandTimeout = 5
            };
            _connectionString = builder.ConnectionString;
        }

        public async Task<bool> ConnectDbAsync(CancellationToken cancellationToken)
        {
            try
            {
                await using var connection = CreateConn();
                await connection.OpenAsync(cancellationToken);
                Debug.WriteLine("[DB] 연결 성공");
                return true;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DB] 연결 실패: {ex.Message}");
                return false;
            }
        }

        public NpgsqlConnection CreateConn() => new(_connectionString);

        public async Task<bool> Retry(CancellationToken cancellationToken = default)
        {
            using CancellationTokenSource linkedCts =
                CancellationTokenSource.CreateLinkedTokenSource(
                    cancellationToken, _cts.Token);

            while (!linkedCts.Token.IsCancellationRequested)
            {
                if (await ConnectDbAsync(linkedCts.Token))
                    return true;

                await Task.Delay(2000, linkedCts.Token);
            }

            return false;
        }

        public void Dispose()
        {
            _cts.Cancel();
            _cts.Dispose();
        }
    }
}
