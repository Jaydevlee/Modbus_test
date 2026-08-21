using MesWeb.Data;
using Dapper;
using MesWeb.Dto;
using System.Diagnostics;

namespace MesWeb.Repository
{
    public class EquipRepository
    {
        private readonly DBConn _DBConn;

        public EquipRepository(DBConn dBConn)
        {
            _DBConn = dBConn;
        }

        public async Task<List<CodeDto>> GetEquipCodeAsync(string groupCode)
        {
            string codeSql = $"""
                            SELECT
                                code AS Code,
                                code_name AS CodeNames
                            FROM common_code
                            WHERE group_code = {groupCode}
                                AND is_active = TRUE
                            ORDER BY sort_order
                            """;
            await using var connection = _DBConn.CreateConnection();
            try
            {
                var codes = await connection.QueryAsync<CodeDto>(codeSql);
                Debug.WriteLine(codes.ToString());
                return codes.AsList();
            }
            catch (InvalidOperationException ex)
            {
                throw new Exception($"오류가 발생했습니다: {ex.Message}");
            }
        }

        public async Task<List<EquipDto>> GetEquipAsync()
        {
            string sql = """
                        SELECT 
                            equip_id AS EquipId,
                            name AS Name,
                            location AS Location,
                            status AS status,
                            is_active AS IsActive
                        FROM equipment
                            WHERE is_active = true
                        """;
            await using var connection = _DBConn.CreateConnection();
            try
            {
                var result = await connection.QueryAsync<EquipDto>(sql);
                Debug.WriteLine(result.Count());
                return result.AsList();
            } 
            catch (InvalidOperationException ex)
            {
                throw new Exception($"오류가 발생했습니다: {ex.Message}");
            }
        }

        public async Task<int> InsertEquipAsync(EquipDto equipDto)
        {
            string updateSql = """
                                UPDATE primary_sequence
                                    SET current_val = current_val + 1
                                WHERE table_name = @TableName
                                AND years = TO_CHAR(CURRENT_DATE, 'YY') 
                                Returning
                                    prefix || years || LPAD(current_val::text, 6, '0') AS Generated_id
                                """;
            string insertSql = """
                                INSERT INTO EQUIPMENT
                                    (equip_id, name, location)
                                VALUES
                                    (@EquipId, @Name, @Location)
                                """;
            await using var connection =  _DBConn.CreateConnection();
            await connection.OpenAsync();
            await using var transaction = await connection.BeginTransactionAsync();

            try
            {
                var generatedId = await connection.QueryFirstAsync<string>(updateSql,
                                                                           new {TableName = "equipment"},
                                                                           transaction);
                int result = await connection.ExecuteAsync(insertSql,
                                                            new
                                                            {
                                                                EquipId = generatedId,
                                                                equipDto.Name,
                                                                equipDto.Location
                                                            },
                                                            transaction);
                await transaction.CommitAsync();
                return result;
            }
            catch (InvalidOperationException ex)
            {
                await transaction.RollbackAsync();
                throw new Exception($"오류가 발생했습니다: {ex.Message}");
            }
        }
    }
}