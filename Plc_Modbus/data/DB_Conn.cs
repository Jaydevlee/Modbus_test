using Microsoft.Win32.SafeHandles;
using Npgsql;
using Plc_Modbus.Config;
using Plc_Modbus.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace Plc_Modbus.data
{
    public class DB_Conn
    {
        private readonly string? _connString = null;
        
        private readonly AppConfig _appConfig = new();

        private readonly CancellationTokenSource _cts = new CancellationTokenSource();

        public DB_Conn()
        {
            _appConfig = JsonSerializer.Deserialize<AppConfig>(File.ReadAllText("config.json"));
            var db = _appConfig?.DBSettings;
            _connString = $"Host={db?.Host};" +
                          $"Port={db?.Port};" +
                          $"Username={db?.Username};" +
                          $"Password={db?.Password};" +
                          $"Database={db?.Database};";
        }

        public async Task<bool> connectDB()
        {
            try
            {
                using (var conn = new NpgsqlConnection(_connString))
                {
                    conn.Open();
                    Debug.WriteLine("연결성공");
                    return true;
                }
            } 
           
            catch (Exception ex)
            {       
                Debug.WriteLine(ex.Message);
                return false;
            }
        }
        public NpgsqlConnection CreateConn()
        {
            return new NpgsqlConnection(_connString);
        }

        public async Task<bool> Retry()
        {
            while (!_cts.IsCancellationRequested)
            {
                if (await connectDB()) return true;
                try
                {
                    await Task.Delay(2000, _cts.Token);
                } catch(OperationCanceledException)
                {
                    return false;
                }
            }
            return false;
        }
        public void Dispose()
        {
            _cts.Cancel();
        }
    }
}
