using Dapper;
using Mercurio.Domain.Entities;
using Mercurio.Domain.Interfaces;
using Microsoft.Data.Sqlite;

namespace Mercurio.Infrastructure.Repositories
{
    public class ProductRepository : IProductRepository
    {

        private readonly string _connectionString;

        public ProductRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            using var connection = new SqliteConnection(_connectionString);
            string sql = "SELECT * FROM Productos WHERE Activo = 1";
            return await connection.QueryAsync<Product>(sql);
        }

        public async Task InsertarAsync(Product product)
        {
            using var connection = new SqliteConnection(_connectionString);
            string sql = @"
                INSERT INTO Productos (Id, CodigoBarras, Nombre, PrecioCompra, PrecioVenta, StockActual, Activo) 
                VALUES (@Id, @CodigoBarras, @Nombre, @PrecioCompra, @PrecioVenta, @StockActual, @Activo)";

            await connection.ExecuteAsync(sql, product);
        }
    }
}
