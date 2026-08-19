using Npgsql;
using MesWeb.Data;
using MesWeb.Dto;
using Dapper;

namespace MesWeb.Repository{
    public class DBRepository
    {
        private readonly DBConn _dbConn;         
        
        public DBRepository(DBConn dBConn)
        {
            _dbConn = dBConn;
        }

        public async Task<List<PlcDto>> GetMetricDataAsync()
        {
            string sql = """
                        SELECT
                            time AS Time,
                            equip_id AS EquipId,
                            address AS Address,
                            metric_name AS MetricName,
                            metric_value AS MetricValue,
                            unit AS Unit,
                            quality AS Quality,
                            collected_at AS CollectedAt
                        FROM equipment_metric
                        ORDER BY time DESC    
                        LIMIT 100
                        """;
            await using var connection = _dbConn.CreateConnection();
            try
            {
                var result = await connection.QueryAsync<PlcDto>(sql);
                return result.AsList();
            }
            catch (Exception ex)
            {
                throw new Exception($"오류가 발생했습니다.: {ex.Message}");
            }
            
        }
    }
}