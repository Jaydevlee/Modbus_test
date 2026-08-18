using MesWeb.Data;
using MesWeb.Dto;
using Dapper;
using System.Diagnostics;

namespace MesWeb.Repository{
    public class ProductRepository
    {
        private readonly DBConn _DBConn;

        public ProductRepository(DBConn DbConn)
        {
            _DBConn = DbConn;
        }
    
        // 품목 조회 
       public async Task<List<ProductDto>> GetProductDataAsync(){
            string sql = """
                        SELECT
                            product_id AS ProductId,
                            name AS Name,
                            recipe_version AS RecipeVersion,
                            is_active AS IsActive
                        FROM product    
                        """;
            await using var connection = _DBConn.CreateConnection();
            var result = await connection.QueryAsync<ProductDto>(sql);
            return result.AsList();
        }

        // 품목 추가
        public async Task<string> InsertProductAsync(ProductDto productDto)
        {
            await using var connection = _DBConn.CreateConnection();
            await connection.OpenAsync();
            await using var transaction = await connection.BeginTransactionAsync();

            string sql1 =   """
                            UPDATE primary_sequence
                                SET current_val = current_val + 1
                            WHERE table_name = @TableName
                            AND years = TO_CHAR(CURRENT_DATE, 'YY') 
                            Returning
                                prefix || years || LPAD(current_val::text, 6, '0') AS Generated_id
                            """;

            string sql2 =   """
                            INSERT INTO product 
                                (product_id, name, recipe_version, is_active)
                            VALUES
                                (@ProductId, @Name, @RecipeVersion, @IsActive)
                            """;
            try
            {
                var generatedId = await connection.QuerySingleAsync<string>(sql1, new {TableName = "product"}, transaction);
                int result = await connection.ExecuteAsync(sql2, 
                                                            new 
                                                            {
                                                                ProductId = generatedId,
                                                                productDto.Name,
                                                                productDto.RecipeVersion,
                                                                productDto.IsActive
                                                            },
                                                            transaction
                                                            );
                await transaction.CommitAsync();
                return generatedId;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception(ex.Message);
            }
        }
    }
}