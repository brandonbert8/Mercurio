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
            return await connection.QueryAsync<Product>("SELECT * FROM Productos ORDER BY Nombre");
        }

        public async Task<IEnumerable<Product>> GetActivosAsync()
        {
            using var connection = new SqliteConnection(_connectionString);
            return await connection.QueryAsync<Product>("SELECT * FROM Productos WHERE Activo = 1 ORDER BY Nombre");
        }

        public async Task<Product?> GetByIdAsync(string id)
        {
            using var connection = new SqliteConnection(_connectionString);
            return await connection.QueryFirstOrDefaultAsync<Product>("SELECT * FROM Productos WHERE Id = @Id", new { Id = id });
        }

        public async Task<Product?> GetByCodigoBarrasAsync(string codigoBarras)
        {
            using var connection = new SqliteConnection(_connectionString);
            return await connection.QueryFirstOrDefaultAsync<Product>(
                "SELECT * FROM Productos WHERE CodigoBarras = @CodigoBarras",
                new { CodigoBarras = codigoBarras });
        }

        public async Task<IEnumerable<Product>> BuscarAsync(string termino)
        {
            using var connection = new SqliteConnection(_connectionString);
            const string sql = @"
                SELECT * FROM Productos
                WHERE Activo = 1 AND (Nombre LIKE @Termino OR CodigoBarras LIKE @Termino OR Descripcion LIKE @Termino)
                ORDER BY Nombre";
            return await connection.QueryAsync<Product>(sql, new { Termino = $"%{termino}%" });
        }

        public async Task InsertarAsync(Product product)
        {
            using var connection = new SqliteConnection(_connectionString);
            const string sql = @"
                INSERT INTO Productos (Id, CodigoBarras, Nombre, Descripcion, CategoriaId, UnidadMedida,
                    PrecioCompra, PrecioVenta, ImpuestoPorcentaje, StockActual, StockMinimo, EsServicio, Activo, FechaCreacion)
                VALUES (@Id, @CodigoBarras, @Nombre, @Descripcion, @CategoriaId, @UnidadMedida,
                    @PrecioCompra, @PrecioVenta, @ImpuestoPorcentaje, @StockActual, @StockMinimo, @EsServicio, @Activo, @FechaCreacion)";
            await connection.ExecuteAsync(sql, product);
        }

        public async Task ActualizarAsync(Product product)
        {
            using var connection = new SqliteConnection(_connectionString);
            const string sql = @"
                UPDATE Productos SET
                    CodigoBarras = @CodigoBarras, Nombre = @Nombre, Descripcion = @Descripcion,
                    CategoriaId = @CategoriaId, UnidadMedida = @UnidadMedida, PrecioCompra = @PrecioCompra,
                    PrecioVenta = @PrecioVenta, ImpuestoPorcentaje = @ImpuestoPorcentaje,
                    StockMinimo = @StockMinimo, EsServicio = @EsServicio, Activo = @Activo
                WHERE Id = @Id";
            await connection.ExecuteAsync(sql, product);
        }

        public async Task DesactivarAsync(string id)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.ExecuteAsync("UPDATE Productos SET Activo = 0 WHERE Id = @Id", new { Id = id });
        }

        public async Task<bool> ExisteCodigoBarrasAsync(string codigoBarras, string? excluirId = null)
        {
            using var connection = new SqliteConnection(_connectionString);
            const string sql = @"
                SELECT COUNT(1) FROM Productos
                WHERE CodigoBarras = @CodigoBarras AND (@ExcluirId IS NULL OR Id <> @ExcluirId)";
            var count = await connection.ExecuteScalarAsync<int>(sql, new { CodigoBarras = codigoBarras, ExcluirId = excluirId });
            return count > 0;
        }
    }

    public class CategoriaRepository : ICategoriaRepository
    {
        private readonly string _connectionString;

        public CategoriaRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<IEnumerable<Categoria>> GetAllAsync(string negocioId)
        {
            using var connection = new SqliteConnection(_connectionString);
            return await connection.QueryAsync<Categoria>(
                "SELECT * FROM Categorias WHERE NegocioId = @NegocioId ORDER BY Nombre",
                new { NegocioId = negocioId });
        }

        public async Task<Categoria?> GetByIdAsync(string id)
        {
            using var connection = new SqliteConnection(_connectionString);
            return await connection.QueryFirstOrDefaultAsync<Categoria>("SELECT * FROM Categorias WHERE Id = @Id", new { Id = id });
        }

        public async Task InsertarAsync(Categoria categoria)
        {
            using var connection = new SqliteConnection(_connectionString);
            const string sql = @"
                INSERT INTO Categorias (Id, NegocioId, Nombre, Descripcion, Activo, FechaCreacion)
                VALUES (@Id, @NegocioId, @Nombre, @Descripcion, @Activo, @FechaCreacion)";
            await connection.ExecuteAsync(sql, categoria);
        }

        public async Task ActualizarAsync(Categoria categoria)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.ExecuteAsync(
                "UPDATE Categorias SET Nombre = @Nombre, Descripcion = @Descripcion, Activo = @Activo WHERE Id = @Id",
                categoria);
        }

        public async Task DesactivarAsync(string id)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.ExecuteAsync("UPDATE Categorias SET Activo = 0 WHERE Id = @Id", new { Id = id });
        }
    }
}
