using Dapper;
using Mercurio.Domain.Entities;
using Mercurio.Domain.Enums;
using Mercurio.Domain.Interfaces;
using Microsoft.Data.Sqlite;

namespace Mercurio.Infrastructure.Repositories
{
    public class SaleRepository : ISaleRepository
    {
        private readonly string _connectionString;

        public SaleRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<Venta?> GetByIdAsync(string id)
        {
            using var connection = new SqliteConnection(_connectionString);
            return await connection.QueryFirstOrDefaultAsync<Venta>("SELECT * FROM Ventas WHERE Id = @Id", new { Id = id });
        }

        public async Task<IEnumerable<Venta>> GetByRangoAsync(string sucursalId, DateTime desde, DateTime hasta)
        {
            using var connection = new SqliteConnection(_connectionString);
            const string sql = @"
                SELECT * FROM Ventas
                WHERE SucursalId = @SucursalId AND Fecha >= @Desde AND Fecha <= @Hasta
                ORDER BY Fecha DESC";
            return await connection.QueryAsync<Venta>(sql, new { SucursalId = sucursalId, Desde = desde, Hasta = hasta });
        }

        public async Task<IEnumerable<VentaDetalle>> GetDetallesAsync(string ventaId)
        {
            using var connection = new SqliteConnection(_connectionString);
            return await connection.QueryAsync<VentaDetalle>(
                "SELECT * FROM VentaDetalles WHERE VentaId = @VentaId ORDER BY ProductoNombre",
                new { VentaId = ventaId });
        }

        public async Task CrearAsync(Venta venta, IEnumerable<VentaDetalle> detalles, Pago? pago, CuentaPorCobrar? cuenta)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();
            using var transaction = connection.BeginTransaction();

            const string sqlVenta = @"
                INSERT INTO Ventas (Id, SucursalId, AlmacenId, UsuarioId, ClienteId, CajaSesionId, NumeroDocumento,
                    TipoDocumento, Fecha, SubTotal, Descuento, Impuesto, Total, Estado, Observacion)
                VALUES (@Id, @SucursalId, @AlmacenId, @UsuarioId, @ClienteId, @CajaSesionId, @NumeroDocumento,
                    @TipoDocumento, @Fecha, @SubTotal, @Descuento, @Impuesto, @Total, @Estado, @Observacion)";
            await connection.ExecuteAsync(sqlVenta, venta, transaction);

            const string sqlDetalle = @"
                INSERT INTO VentaDetalles (Id, VentaId, ProductoId, ProductoNombre, Cantidad, PrecioUnitario,
                    Descuento, ImpuestoPorcentaje, SubTotal)
                VALUES (@Id, @VentaId, @ProductoId, @ProductoNombre, @Cantidad, @PrecioUnitario,
                    @Descuento, @ImpuestoPorcentaje, @SubTotal)";

            foreach (var detalle in detalles)
            {
                await connection.ExecuteAsync(sqlDetalle, detalle, transaction);
                await AplicarSalidaStockAsync(connection, transaction, venta, detalle);
            }

            if (pago is not null)
            {
                await connection.ExecuteAsync(@"
                    INSERT INTO Pagos (Id, VentaId, CompraId, CuentaPorCobrarId, ClienteId, CajaSesionId, Tipo,
                        MetodoPago, Monto, Referencia, UsuarioId, Fecha, Observacion)
                    VALUES (@Id, @VentaId, @CompraId, @CuentaPorCobrarId, @ClienteId, @CajaSesionId, @Tipo,
                        @MetodoPago, @Monto, @Referencia, @UsuarioId, @Fecha, @Observacion)",
                    pago,
                    transaction);

                if (pago.MetodoPago == MetodoPago.Efectivo && !string.IsNullOrWhiteSpace(pago.CajaSesionId))
                {
                    await connection.ExecuteAsync(@"
                        INSERT INTO CajaMovimientos (Id, CajaSesionId, Tipo, Concepto, Monto, Fecha, UsuarioId, Referencia)
                        VALUES (@Id, @CajaSesionId, @Tipo, @Concepto, @Monto, @Fecha, @UsuarioId, @Referencia)",
                        new
                        {
                            Id = Guid.NewGuid().ToString(),
                            CajaSesionId = pago.CajaSesionId,
                            Tipo = (int)TipoMovimientoCaja.Venta,
                            Concepto = $"Venta {venta.NumeroDocumento}",
                            Monto = pago.Monto,
                            Fecha = pago.Fecha,
                            UsuarioId = pago.UsuarioId,
                            Referencia = venta.NumeroDocumento
                        },
                        transaction);
                }
            }

            if (cuenta is not null)
            {
                await connection.ExecuteAsync(@"
                    INSERT INTO CuentasPorCobrar (Id, ClienteId, VentaId, FechaEmision, FechaVencimiento,
                        MontoOriginal, Saldo, Estado, Observacion)
                    VALUES (@Id, @ClienteId, @VentaId, @FechaEmision, @FechaVencimiento,
                        @MontoOriginal, @Saldo, @Estado, @Observacion)",
                    cuenta,
                    transaction);
            }

            transaction.Commit();
        }

        public async Task AnularAsync(string ventaId, string usuarioId, string motivo)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();
            using var transaction = connection.BeginTransaction();

            var venta = await connection.QueryFirstOrDefaultAsync<Venta>(
                "SELECT * FROM Ventas WHERE Id = @Id", new { Id = ventaId }, transaction);

            if (venta is null)
                throw new InvalidOperationException("La venta no existe.");
            if (venta.Estado == EstadoVenta.Anulada)
                throw new InvalidOperationException("La venta ya está anulada.");

            var detalles = await connection.QueryAsync<VentaDetalle>(
                "SELECT * FROM VentaDetalles WHERE VentaId = @VentaId", new { VentaId = ventaId }, transaction);

            foreach (var detalle in detalles)
            {
                await AplicarEntradaStockAsync(connection, transaction, venta, detalle, usuarioId, motivo);
            }

            var pagos = await connection.QueryAsync<Pago>(
                "SELECT * FROM Pagos WHERE VentaId = @VentaId AND CajaSesionId IS NOT NULL",
                new { VentaId = ventaId },
                transaction);

            foreach (var pago in pagos)
            {
                await connection.ExecuteAsync(@"
                    INSERT INTO CajaMovimientos (Id, CajaSesionId, Tipo, Concepto, Monto, Fecha, UsuarioId, Referencia)
                    VALUES (@Id, @CajaSesionId, @Tipo, @Concepto, @Monto, @Fecha, @UsuarioId, @Referencia)",
                    new
                    {
                        Id = Guid.NewGuid().ToString(),
                        pago.CajaSesionId,
                        Tipo = (int)TipoMovimientoCaja.Egreso,
                        Concepto = $"Anulación venta {venta.NumeroDocumento}",
                        Monto = pago.Monto,
                        Fecha = DateTime.Now,
                        UsuarioId = usuarioId,
                        Referencia = venta.NumeroDocumento
                    },
                    transaction);
            }

            await connection.ExecuteAsync(@"
                UPDATE Ventas SET Estado = @Estado,
                    Observacion = CASE WHEN Observacion = '' THEN @Motivo ELSE Observacion || ' | ANULADA: ' || @Motivo END
                WHERE Id = @Id",
                new { Estado = (int)EstadoVenta.Anulada, Motivo = motivo, Id = ventaId },
                transaction);

            transaction.Commit();
        }

        public async Task<string> GenerarNumeroAsync(string sucursalId, DateTime fecha)
        {
            using var connection = new SqliteConnection(_connectionString);
            var prefijo = $"V-{fecha:yyyy}-";
            var count = await connection.ExecuteScalarAsync<int>(
                "SELECT COUNT(1) FROM Ventas WHERE SucursalId = @SucursalId AND NumeroDocumento LIKE @Prefijo",
                new { SucursalId = sucursalId, Prefijo = $"{prefijo}%" });

            return $"{prefijo}{count + 1:000000}";
        }

        private static async Task AplicarSalidaStockAsync(
            SqliteConnection connection,
            SqliteTransaction transaction,
            Venta venta,
            VentaDetalle detalle)
        {
            var esServicio = await connection.ExecuteScalarAsync<int>(
                "SELECT COALESCE(EsServicio, 0) FROM Productos WHERE Id = @ProductoId",
                new { ProductoId = detalle.ProductoId },
                transaction);

            if (esServicio == 1)
                return;

            var stock = await connection.QueryFirstOrDefaultAsync<StockAlmacen>(
                "SELECT * FROM StockAlmacen WHERE AlmacenId = @AlmacenId AND ProductoId = @ProductoId",
                new { AlmacenId = venta.AlmacenId, ProductoId = detalle.ProductoId },
                transaction);

            if (stock is null || stock.Cantidad < detalle.Cantidad)
                throw new InvalidOperationException($"Stock insuficiente para el producto {detalle.ProductoNombre}.");

            await connection.ExecuteAsync(
                "UPDATE StockAlmacen SET Cantidad = Cantidad - @Cantidad, FechaActualizacion = @Fecha WHERE Id = @Id",
                new { Cantidad = detalle.Cantidad, Fecha = DateTime.Now, Id = stock.Id },
                transaction);

            await connection.ExecuteAsync(
                "UPDATE Productos SET StockActual = StockActual - @Cantidad WHERE Id = @ProductoId",
                new { Cantidad = detalle.Cantidad, ProductoId = detalle.ProductoId },
                transaction);

            await connection.ExecuteAsync(@"
                INSERT INTO MovimientosInventario (Id, AlmacenId, AlmacenDestinoId, ProductoId, Tipo, Cantidad,
                    CostoUnitario, Referencia, Observacion, UsuarioId, Fecha)
                VALUES (@Id, @AlmacenId, NULL, @ProductoId, @Tipo, @Cantidad,
                    @CostoUnitario, @Referencia, @Observacion, @UsuarioId, @Fecha)",
                new
                {
                    Id = Guid.NewGuid().ToString(),
                    AlmacenId = venta.AlmacenId,
                    ProductoId = detalle.ProductoId,
                    Tipo = (int)TipoMovimientoInventario.Salida,
                    Cantidad = detalle.Cantidad,
                    CostoUnitario = 0m,
                    Referencia = venta.NumeroDocumento,
                    Observacion = "Venta",
                    UsuarioId = venta.UsuarioId,
                    Fecha = venta.Fecha
                },
                transaction);
        }

        private static async Task AplicarEntradaStockAsync(
            SqliteConnection connection,
            SqliteTransaction transaction,
            Venta venta,
            VentaDetalle detalle,
            string usuarioId,
            string motivo)
        {
            var esServicio = await connection.ExecuteScalarAsync<int>(
                "SELECT COALESCE(EsServicio, 0) FROM Productos WHERE Id = @ProductoId",
                new { ProductoId = detalle.ProductoId },
                transaction);

            if (esServicio == 1)
                return;

            var stock = await connection.QueryFirstOrDefaultAsync<StockAlmacen>(
                "SELECT * FROM StockAlmacen WHERE AlmacenId = @AlmacenId AND ProductoId = @ProductoId",
                new { AlmacenId = venta.AlmacenId, ProductoId = detalle.ProductoId },
                transaction);

            if (stock is null)
            {
                var stockMinimo = await connection.ExecuteScalarAsync<int>(
                    "SELECT COALESCE(StockMinimo, 0) FROM Productos WHERE Id = @ProductoId",
                    new { ProductoId = detalle.ProductoId },
                    transaction);

                await connection.ExecuteAsync(@"
                    INSERT INTO StockAlmacen (Id, AlmacenId, ProductoId, Cantidad, StockMinimo, FechaActualizacion)
                    VALUES (@Id, @AlmacenId, @ProductoId, @Cantidad, @StockMinimo, @FechaActualizacion)",
                    new
                    {
                        Id = Guid.NewGuid().ToString(),
                        AlmacenId = venta.AlmacenId,
                        ProductoId = detalle.ProductoId,
                        Cantidad = detalle.Cantidad,
                        StockMinimo = stockMinimo,
                        FechaActualizacion = DateTime.Now
                    },
                    transaction);
            }
            else
            {
                await connection.ExecuteAsync(
                    "UPDATE StockAlmacen SET Cantidad = Cantidad + @Cantidad, FechaActualizacion = @Fecha WHERE Id = @Id",
                    new { Cantidad = detalle.Cantidad, Fecha = DateTime.Now, Id = stock.Id },
                    transaction);
            }

            await connection.ExecuteAsync(
                "UPDATE Productos SET StockActual = StockActual + @Cantidad WHERE Id = @ProductoId",
                new { Cantidad = detalle.Cantidad, ProductoId = detalle.ProductoId },
                transaction);

            await connection.ExecuteAsync(@"
                INSERT INTO MovimientosInventario (Id, AlmacenId, AlmacenDestinoId, ProductoId, Tipo, Cantidad,
                    CostoUnitario, Referencia, Observacion, UsuarioId, Fecha)
                VALUES (@Id, @AlmacenId, NULL, @ProductoId, @Tipo, @Cantidad,
                    @CostoUnitario, @Referencia, @Observacion, @UsuarioId, @Fecha)",
                new
                {
                    Id = Guid.NewGuid().ToString(),
                    AlmacenId = venta.AlmacenId,
                    ProductoId = detalle.ProductoId,
                    Tipo = (int)TipoMovimientoInventario.Entrada,
                    Cantidad = detalle.Cantidad,
                    CostoUnitario = 0m,
                    Referencia = venta.NumeroDocumento,
                    Observacion = $"Anulación de venta: {motivo}",
                    UsuarioId = usuarioId,
                    Fecha = DateTime.Now
                },
                transaction);
        }
    }

    public class PagoRepository : IPagoRepository
    {
        private readonly string _connectionString;

        public PagoRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<IEnumerable<Pago>> GetByVentaAsync(string ventaId)
        {
            using var connection = new SqliteConnection(_connectionString);
            return await connection.QueryAsync<Pago>(
                "SELECT * FROM Pagos WHERE VentaId = @VentaId ORDER BY Fecha",
                new { VentaId = ventaId });
        }

        public async Task InsertarAsync(Pago pago)
        {
            using var connection = new SqliteConnection(_connectionString);
            const string sql = @"
                INSERT INTO Pagos (Id, VentaId, CompraId, CuentaPorCobrarId, ClienteId, CajaSesionId, Tipo,
                    MetodoPago, Monto, Referencia, UsuarioId, Fecha, Observacion)
                VALUES (@Id, @VentaId, @CompraId, @CuentaPorCobrarId, @ClienteId, @CajaSesionId, @Tipo,
                    @MetodoPago, @Monto, @Referencia, @UsuarioId, @Fecha, @Observacion)";
            await connection.ExecuteAsync(sql, pago);
        }
    }
}
