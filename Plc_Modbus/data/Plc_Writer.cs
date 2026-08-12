using System.Diagnostics;

namespace Plc_Modbus.data
{
    public class Plc_Writer
    {
        private readonly Mod_Conn _connection;

        public Plc_Writer(Mod_Conn connection)
        {
            _connection = connection;
        }

        public async Task<bool> WriteCommandsAsync(
            bool runCommand,
            bool forceError,
            CancellationToken cancellationToken = default)
        {
            try
            {
                bool[] values = [runCommand, forceError];
                await _connection.ExecuteAsync(
                    master => master.WriteMultipleCoilsAsync(
                        _connection.Settings.UnitId, 0, values),
                    cancellationToken);
                return true;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[PLC] 명령 쓰기 실패: {ex.Message}");
                return false;
            }
        }
    }
}
