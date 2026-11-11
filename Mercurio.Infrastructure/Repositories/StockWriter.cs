using Dapper;
using Mercurio.Domain.Entities;
using Microsoft.Data.Sqlite;

namespace Mercurio.Infrastructure.Repositories
{
    internal static class StockWriter
    {
        public static async Task AplicarEntradaAsync(
            SqliteConnection connection,
            SqliteTransaction transaction,
            string almacenId,
            string productoId,
            int cantidad,
            decimal costoUnitario,
            bool actualizarCosto)
        {
            if (cantidad <= 0)
                throw new InvalidOperationException("La cantidad de entrada debe ser mayor a cero.");

            var stock = await ObtenerStockAsync(connection, transaction, almacenId, productoId);

            if (stock is null)
            {
                var costoInicial = costoUnitario;
                if (!actualizarCosto || costoInicial <= 0)
                    costoInicial = await ObtenerPrecioCompraAsync(connection, transaction, productoId);

                var stockMinimo = await ObtenerStockMinimoAsync(connection, transaction, productoId);

                await connection.ExecuteAsync(@"
                    INSERT INTO StockAlmacen (Id, AlmacenId, ProductoId, Cantidad, StockMinimo, CostoPromedio, FechaActualizacion)
                    VALUES (@Id, @AlmacenId, @ProductoId, @Cantidad, @StockMinimo, @CostoPromedio, @FechaActualizacion)",
                    new
                    {
                        Id = Guid.NewGuid().ToString(),
                        AlmacenId = almacenId,
                        ProductoId = productoId,
                        Cantidad = cantidad,
                        StockMinimo = stockMinimo,
                        CostoPromedio = costoInicial,
                        FechaActualizacion = DateTime.Now
                    },
                    transaction);
            }
            else
            {
                var nuevoCosto = stock.CostoPromedio;
                if (actualizarCosto && costoUnitario > 0)
                {
                    var cantidadTotal = stock.Cantidad + cantidad;
                    nuevoCosto = cantidadTotal > 0
                        ? Pricing.Redondear(((stock.Cantidad * stock.CostoPromedio) + (cantidad * costoUnitario)) / cantidadTotal)
                        : costoUnitario;
                }

                await connection.ExecuteAsync(@"
                    UPDATE StockAlmacen
                    SET Cantidad = Cantidad + @Cantidad, CostoPromedio = @CostoPromedio, FechaActualizacion = @FechaActualizacion
                    WHERE Id = @Id",
                    new
                    {
                        Id = stock.Id,
                        Cantidad = cantidad,
                        CostoPromedio = nuevoCosto,
                        FechaActualizacion = DateTime.Now
                    },
                    transaction);
            }

            if (actualizarCosto)
                await RecalcularPrecioCompraAsync(connection, transaction, productoId);
        }

        public static async Task AplicarSalidaAsync(
            SqliteConnection connection,
            SqliteTransaction transaction,
            string almacenId,
            string productoId,
            int cantidad)
        {
            if (cantidad <= 0)
                throw new InvalidOperationException("La cantidad de salida debe ser mayor a cero.");

            var stock = await ObtenerStockAsync(connection, transaction, almacenId, productoId);
            if (stock is null || stock.Cantidad < cantidad)
                throw new InvalidOperationException("No hay stock suficiente en el almacén para realizar la salida.");

            await connection.ExecuteAsync(@"
                UPDATE StockAlmacen
                SET Cantidad = Cantidad - @Cantidad, FechaActualizacion = @FechaActualizacion
                WHERE Id = @Id",
                new
                {
                    Id = stock.Id,
                    Cantidad = cantidad,
                    FechaActualizacion = DateTime.Now
                },
                transaction);
        }

        public static async Task AplicarAjusteAsync(
            SqliteConnection connection,
            SqliteTransaction transaction,
            string almacenId,
            string productoId,
            int diferencia,
            decimal costoUnitario)
        {
            if (diferencia == 0)
                return;

            if (diferencia > 0)
            {
                await AplicarEntradaAsync(
                    connection,
                    transaction,
                    almacenId,
                    productoId,
                    diferencia,
                    costoUnitario,
                    actualizarCosto: costoUnitario > 0);
            }
            else
            {
                await AplicarSalidaAsync(connection, transaction, almacenId, productoId, -diferencia);
            }
        }

        public static async Task<decimal> ObtenerCostoPromedioAsync(
            SqliteConnection connection,
            SqliteTransaction transaction,
            string almacenId,
            string productoId)
        {
            var stock = await ObtenerStockAsync(connection, transaction, almacenId, productoId);
            return stock?.CostoPromedio ?? 0m;
        }

        public static async Task InsertarMovimientoAsync(
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

        private static async Task<StockAlmacen?> ObtenerStockAsync(
            SqliteConnection connection,
            SqliteTransaction transaction,
            string almacenId,
            string productoId)
        {
            return await connection.QueryFirstOrDefaultAsync<StockAlmacen>(
                "SELECT * FROM StockAlmacen WHERE AlmacenId = @AlmacenId AND ProductoId = @ProductoId",
                new { AlmacenId = almacenId, ProductoId = productoId },
                transaction);
        }

        private static async Task<int> ObtenerStockMinimoAsync(
            SqliteConnection connection,
            SqliteTransaction transaction,
            string productoId)
        {
            return await connection.ExecuteScalarAsync<int>(
                "SELECT COALESCE(StockMinimo, 0) FROM Productos WHERE Id = @ProductoId",
                new { ProductoId = productoId },
                transaction);
        }

        private static async Task<decimal> ObtenerPrecioCompraAsync(
            SqliteConnection connection,
            SqliteTransaction transaction,
            string productoId)
        {
            return await connection.ExecuteScalarAsync<decimal>(
                "SELECT COALESCE(PrecioCompra, 0) FROM Productos WHERE Id = @ProductoId",
                new { ProductoId = productoId },
                transaction);
        }

        private static async Task RecalcularPrecioCompraAsync(
            SqliteConnection connection,
            SqliteTransaction transaction,
            string productoId)
        {
            await connection.ExecuteAsync(@"
                UPDATE Productos
                SET PrecioCompra = COALESCE((
                    SELECT ROUND(SUM(s.Cantidad * s.CostoPromedio) / NULLIF(SUM(s.Cantidad), 0), 4)
                    FROM StockAlmacen s
                    WHERE s.ProductoId = @ProductoId
                ), PrecioCompra)
                WHERE Id = @ProductoId",
                new { ProductoId = productoId },
                transaction);
        }

        private static class Pricing
        {
            public static decimal Redondear(decimal valor)
            {
                return decimal.Round(valor, 4, MidpointRounding.AwayFromZero);
            }
        }
    }
}
