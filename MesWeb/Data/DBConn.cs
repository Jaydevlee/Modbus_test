using System.Data.Common;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Internal;
using Npgsql;

namespace MesWeb.Data{
    public class DBConn
    {
        private readonly string _connectionString;

        public DBConn(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("PlcDB") 
                                ?? throw new InvalidOperationException("db연결 정보를 찾을 수 없습니다.");
        }

        public NpgsqlConnection CreateConnection()
        {
            return new NpgsqlConnection(_connectionString);
        }
    }
}