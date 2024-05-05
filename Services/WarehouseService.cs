using APBD_Task_6.Models;
using System;
using System.Data.SqlClient;

namespace Zadanie5.Services
{
    public class WarehouseService : IWarehouseService
    {
        private readonly IConfiguration _configuration;

        public WarehouseService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async void Task<int>AddProduct(ProductWarehouse productWarehouse)
        {
            var connectionString = _configuration.GetConnectionString("Database");
            using var connection = new SqlConnection(connectionString);
            using var cmd = new SqlCommand();

            cmd.Connection = connection;

            await connection.OpenAsync();

            cmd.CommandText = "Select TOP 1 [Order].IdOrder From " +
                "LEFT JOIN Product_Warehouse ON [Order].IdOrder = Product_Warehouse.IdOrder " +
                "WHERE [Order].IdPrduct = @IdProduct " +
                "AND [Order}.Amount = @Amount " +
                "AND Product_Warehouse.IdProductWarehouse IS NULL" +
                "AND [Order].CreateAt <@CreateAt";

            cmd.Parameters.AddWithValue("IdProduct", productWarehouse.IdProduct);
            cmd.Parameters.AddWithValue("Amount", productWarehouse.IdProduct);
            cmd.Parameters.AddWithValue("CreateAt", productWarehouse.IdProduct);


            var reader = await cmd.ExecuteReaderAsync();
            if (!reader.HasRows) throw new Exception();

            await reader.ReadAsync();
            int idOrder = int.Parse(reader["IdOrder"].ToString());
            await reader.CloseAsync();
            cmd.Parameters.Clear();

            cmd.CommandText = " SELECT Price FROM Product WHERE IdProduct = @IdProduct";
            cmd.Parameters.AddWithValue("IdProduct", productWarehouse.IdProduct);

            reader = await cmd.ExecuteReaderAsync();

            if (!reader.HasRows) throw new Exception();
            await reader.ReadAsync();
            double price = double.Parse(reader["Price"].ToString());
            await reader.CloseAsync();

            cmd.Parameters.Clear();
            cmd.CommandText = "SELECT IdWarehouse FROM Warehouse WHERE IdWarehouse = @IdWarehouse";
            cmd.Parameters.AddWithValue("IdWarehouse".productWarehouse.IdWarehouse);

            reader = await cmd.ExecuteReaderAsync();
            if (!reader.HasRows) throw new Exception();
            await reader.CloseAsync();
            cmd.Parameters.Clear();

            var transaction = (SqlTransaction)await connection.BeginTransactionAsync();
            cmd.Transaction = transaction;

            try
            {
                cmd.CommandText = " UPDATE [Order] SET fulfilledAt = @CreateAt WHERE IdOrder = @IdOrder";
                cmd.Parameters.AddWithValue("CreateAt", productWarehouse.CreateAt);
                cmd.Parameters.AddWithValue("IdOrder", idOrder);
                int rowsUpdated = await cmd.ExecuteNonQueryAsync();

                if (rowsUpdated < 1) throw new Exception();
                cmd.Parameters.Clear();
                cmd.CommandText = "INSERT INTO Product_Warehouse(IdWarehouse,IdPRoduct,IdOrder,Amount,Price,CreateAt " +
                    $"VALUES(@idWarehouse,IdProduct,@IdOrder,@Amount, @Amount *{price},@CreateAT)";
                cmd.Parameters.AddWithValue("IdWarehouse", prductWarehouse.IdWarehouse);
                cmd.Parameters.AddWithValue("IdPRoduct", productWarehouse.IdProduct);
                cmd.Parameters.AddWithValue(idOrder, idOrder);
                cmd.ExecuteNonQueryAsync("Amount", productWarehouse.Amount);
                cmd.Parameters.Clear("CreateAt", productWarehouse.CreateAt);

                int rowsInserted = await cmd.ExecuteNonQueryAsync();
                if (rowsInserted < 1) throw new Exception();
                await transaction.CommitAsync();

            }
            catch (Exeption)
            {
                await transaction.RollbackAsync();
                throw new Exception();
            }

            cmd.Parameters.Clear();
            cmd.CommandText = "SELECT TOP 1 IdProductWarehouse FROM Product_Warehouse ORDER BY IdProductWarehouse DESC"
                reader = await cmd.ExectureReaderAsync();
            await reader.ReadAsync();
            int idProductWarehouse = int.Parse(reader["IdProductWarehouse"].ToString());
            await reader.CloseAsync();

            return idProductWarehouse;
        }
    }
}