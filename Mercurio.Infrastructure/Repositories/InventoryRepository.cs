using Dapper;
using Mercurio.Domain.Entities;
using Mercurio.Domain.Enums;
using Mercurio.Domain.Interfaces;
using Microsoft.Data.Sqlite;

namespace Mercurio.Infrastructure.Repositories
{
    public class InventoryRepository : IInventoryRepository
    {
        private readonly string _connectionString;

        public InventoryRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<IEnumerable<Almacen>> GetAlmacenesAsync(string sucursalId)
        {
            using var connection = new SqliteConnection(_connectionString);
            return await connection.QueryAsync<Almacen>(
                "SELECT * FROM Almacenes WHERE SucursalId = @SucursalId ORDER BY Nombre",
                new { SucursalId = sucursalId });
        }

        public async Task<Almacen?> GetAlmacenByIdAsync(string id)
        {
            using var connection = new SqliteConnection(_connectionString);
            return await connection.QueryFirstOrDefaultAsync<Almacen>("SELECT * FROM Almacenes WHERE Id = @Id", new { Id = id });
        }

        public async Task InsertarAlmacenAsync(Almacen almacen)
        {
            using var connection = new SqliteConnection(_connectionString);
            const string sql = @"
                INSERT INTO Almacenes (Id, SucursalId, Nombre, Codigo, Direccion, Activo, FechaCreacion)
                VALUES (@Id, @SucursalId, @Nombre, @Codigo, @Direccion, @Activo, @FechaCreacion)";
            await connection.ExecuteAsync(sql, almacen);
        }

        public async Task ActualizarAlmacenAsync(Almacen almacen)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.ExecuteAsync(
                "UPDATE Almacenes SET Nombre = @Nombre, Codigo = @Codigo, Direccion = @Direccion, Activo = @Activo WHERE Id = @Id",
                almacen);
        }

        public async Task<IEnumerable<StockAlmacen>> GetStockAsync(string almacenId)
        {
            using var connection = new SqliteConnection(_connectionString);
            return await connection.QueryAsync<StockAlmacen>(
                "SELECT * FROM StockAlmacen WHERE AlmacenId = @AlmacenId ORDER BY ProductoId",
                new { AlmacenId = almacenId });
        }

        public async Task<StockAlmacen?> GetStockProductoAsync(string almacenId, string productoId)
        {
            using var connection = new SqliteConnection(_connectionString);
            return await connection.QueryFirstOrDefaultAsync<StockAlmacen>(
                "SELECT * FROM StockAlmacen WHERE AlmacenId = @AlmacenId AND ProductoId = @ProductoId",
                new { AlmacenId = almacenId, ProductoId = productoId });
        }

        public async Task<int> GetStockTotalAsync(string productoId)
        {
            using var connection = new SqliteConnection(_connectionString);
            return await connection.ExecuteScalarAsync<int>(
                "SELECT COALESCE(SUM(Cantidad), 0) FROM StockAlmacen WHERE ProductoId = @ProductoId",
                new { ProductoId = productoId });
        }

        public async Task<IEnumerable<StockAlmacen>> GetStockBajoMinimoAsync(string? almacenId = null)
        {
            using var connection = new SqliteConnection(_connectionString);
            const string sql = @"
                SELECT s.* FROM StockAlmacen s
                INNER JOIN Productos p ON p.Id = s.ProductoId
                WHERE p.Activo = 1 AND s.Cantidad <= s.StockMinimo
                  AND (@AlmacenId IS NULL OR s.AlmacenId = @AlmacenId)
                ORDER BY s.AlmacenId, s.ProductoId";
            return await connection.QueryAsync<StockAlmacen>(sql, new { AlmacenId = almacenId });
        }

        public async Task RegistrarMovimientoAsync(MovimientoInventario movimiento)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();
            using var transaction = connection.BeginTransaction();

            switch (movimiento.Tipo)
            {
                case TipoMovimientoInventario.Entrada:
                    await StockWriter.AplicarEntradaAsync(
                        connection, transaction, movimiento.AlmacenId, movimiento.ProductoId,
                        movimiento.Cantidad, movimiento.CostoUnitario, actualizarCosto: true);
                    break;

                case TipoMovimientoInventario.Salida:
                    await StockWriter.AplicarSalidaAsync(
                        connection, transaction, movimiento.AlmacenId, movimiento.ProductoId, movimiento.Cantidad);
                    break;

                case TipoMovimientoInventario.Ajuste:
                    await StockWriter.AplicarAjusteAsync(
                        connection, transaction, movimiento.AlmacenId, movimiento.ProductoId,
                        movimiento.Cantidad, movimiento.CostoUnitario);
                    break;

                default:
                    throw new InvalidOperationException("Tipo de movimiento no soportado.");
            }

            await StockWriter.InsertarMovimientoAsync(connection, transaction, movimiento);
            transaction.Commit();
        }

        public async Task TransferirAsync(MovimientoInventario salida, MovimientoInventario entrada)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();
            using var transaction = connection.BeginTransaction();

            var costo = await StockWriter.ObtenerCostoPromedioAsync(
                connection, transaction, salida.AlmacenId, salida.ProductoId);

            await StockWriter.AplicarSalidaAsync(
                connection, transaction, salida.AlmacenId, salida.ProductoId, salida.Cantidad);

            await StockWriter.AplicarEntradaAsync(
                connection, transaction, entrada.AlmacenId, entrada.ProductoId,
                entrada.Cantidad, costo, actualizarCosto: costo > 0);

            await StockWriter.InsertarMovimientoAsync(connection, transaction, salida);
            await StockWriter.InsertarMovimientoAsync(connection, transaction, entrada);

            transaction.Commit();
        }

        public async Task<IEnumerable<MovimientoInventario>> GetMovimientosAsync(string? almacenId, DateTime desde, DateTime hasta)
        {
            using var connection = new SqliteConnection(_connectionString);
            const string sql = @"
                SELECT * FROM MovimientosInventario
                WHERE Fecha >= @Desde AND Fecha <= @Hasta
                  AND (@AlmacenId IS NULL OR AlmacenId = @AlmacenId)
                ORDER BY Fecha DESC";
            return await connection.QueryAsync<MovimientoInventario>(sql, new { AlmacenId = almacenId, Desde = desde, Hasta = hasta });
        }
    }
}
