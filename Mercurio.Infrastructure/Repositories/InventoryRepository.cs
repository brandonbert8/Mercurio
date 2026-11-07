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

            var delta = ObtenerDelta(movimiento.Tipo, movimiento.Cantidad);
            await AplicarDeltaAsync(connection, transaction, movimiento.AlmacenId, movimiento.ProductoId, delta, movimiento.CostoUnitario, movimiento.Tipo);
            await InsertarMovimientoAsync(connection, transaction, movimiento);

            transaction.Commit();
        }

        public async Task TransferirAsync(MovimientoInventario salida, MovimientoInventario entrada)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();
            using var transaction = connection.BeginTransaction();

            await AplicarDeltaAsync(connection, transaction, salida.AlmacenId, salida.ProductoId, -salida.Cantidad, 0, salida.Tipo);
            await AplicarDeltaAsync(connection, transaction, entrada.AlmacenId, entrada.ProductoId, entrada.Cantidad, 0, entrada.Tipo);
            await InsertarMovimientoAsync(connection, transaction, salida);
            await InsertarMovimientoAsync(connection, transaction, entrada);

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

        private static int ObtenerDelta(TipoMovimientoInventario tipo, int cantidad)
        {
            return tipo switch
            {
                TipoMovimientoInventario.Entrada => cantidad,
                TipoMovimientoInventario.TransferenciaEntrada => cantidad,
                TipoMovimientoInventario.Ajuste => cantidad,
                TipoMovimientoInventario.Salida => -cantidad,
                TipoMovimientoInventario.TransferenciaSalida => -cantidad,
                _ => cantidad
            };
        }

        private static async Task AplicarDeltaAsync(
            SqliteConnection connection,
            SqliteTransaction transaction,
            string almacenId,
            string productoId,
            int delta,
            decimal costoUnitario,
            TipoMovimientoInventario tipo)
        {
            var stock = await connection.QueryFirstOrDefaultAsync<StockAlmacen>(
                "SELECT * FROM StockAlmacen WHERE AlmacenId = @AlmacenId AND ProductoId = @ProductoId",
                new { AlmacenId = almacenId, ProductoId = productoId },
                transaction);

            if (stock is null)
            {
                if (delta < 0)
                    throw new InvalidOperationException("No hay stock registrado para el producto en el almacén.");

                var stockMinimo = await connection.ExecuteScalarAsync<int>(
                    "SELECT COALESCE(StockMinimo, 0) FROM Productos WHERE Id = @ProductoId",
                    new { ProductoId = productoId },
                    transaction);

                await connection.ExecuteAsync(@"
                    INSERT INTO StockAlmacen (Id, AlmacenId, ProductoId, Cantidad, StockMinimo, FechaActualizacion)
                    VALUES (@Id, @AlmacenId, @ProductoId, @Cantidad, @StockMinimo, @FechaActualizacion)",
                    new
                    {
                        Id = Guid.NewGuid().ToString(),
                        AlmacenId = almacenId,
                        ProductoId = productoId,
                        Cantidad = delta,
                        StockMinimo = stockMinimo,
                        FechaActualizacion = DateTime.Now
                    },
                    transaction);
            }
            else
            {
                var nuevaCantidad = stock.Cantidad + delta;
                if (nuevaCantidad < 0)
                    throw new InvalidOperationException("La operación dejaría el stock en negativo.");

                await connection.ExecuteAsync(@"
                    UPDATE StockAlmacen SET Cantidad = @Cantidad, FechaActualizacion = @FechaActualizacion
                    WHERE Id = @Id",
                    new { Id = stock.Id, Cantidad = nuevaCantidad, FechaActualizacion = DateTime.Now },
                    transaction);
            }

            await connection.ExecuteAsync(
                "UPDATE Productos SET StockActual = StockActual + @Delta WHERE Id = @ProductoId",
                new { Delta = delta, ProductoId = productoId },
                transaction);

            if (tipo == TipoMovimientoInventario.Entrada && costoUnitario > 0)
            {
                await connection.ExecuteAsync(
                    "UPDATE Productos SET PrecioCompra = @Costo WHERE Id = @ProductoId",
                    new { Costo = costoUnitario, ProductoId = productoId },
                    transaction);
            }
        }

        private static async Task InsertarMovimientoAsync(
            SqliteConnection connection,
            SqliteTransaction transaction,
            MovimientoInventario movimiento)
        {
            const string sql = @"
                INSERT INTO MovimientosInventario (Id, AlmacenId, AlmacenDestinoId, ProductoId, Tipo, Cantidad,
                    CostoUnitario, Referencia, Observacion, UsuarioId, Fecha)
                VALUES (@Id, @AlmacenId, @AlmacenDestinoId, @ProductoId, @Tipo, @Cantidad,
                    @CostoUnitario, @Referencia, @Observacion, @UsuarioId, @Fecha)";
            await connection.ExecuteAsync(sql, movimiento, transaction);
        }
    }
}
