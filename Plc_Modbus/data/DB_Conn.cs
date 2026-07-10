using Npgsql;
using Plc_Modbus.Config;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace Plc_Modbus.data
{
    public class DB_Conn
    {
        private string host;
        private string port;
        private string username;
        private string password;
        private string database;
        private bool isConnected = false;

        private readonly AppConfig _appConfig = new();
  
        public DB_Conn()
        {
            _appConfig = JsonSerializer.Deserialize<AppConfig>(File.ReadAllText("config.json"));
            host = _appConfig.DBSettings.Host;
            port = _appConfig.DBSettings.Port;
            username = _appConfig.DBSettings.Username;
            password = _appConfig.DBSettings.Password;
            database = _appConfig.DBSettings.Database;
        }

        public bool connectDB()
        {
            string connString = $"Host={host};" +
                                $"Port={port};" +
                                $"Username={username};" +
                                $"Password={password};" +
                                $"Database={database}";
            if (!isConnected)
            {
                try
                {
                    using (var conn = new NpgsqlConnection(connString))
                    {
                        conn.Open();
                        Debug.WriteLine("연결성공");
                        return true;
                    }
                } 
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    isConnected = false;
                    return isConnected;
                }
            }
            return isConnected;
        }
    }
}
