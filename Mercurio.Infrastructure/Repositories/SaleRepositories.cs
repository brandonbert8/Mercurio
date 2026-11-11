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

            if (string.IsNullOrWhiteSpace(venta.NumeroDocumento))
                venta.NumeroDocumento = await DocumentNumberGenerator.GenerarAsync(
                    connection, transaction, venta.SucursalId, venta.TipoDocumento, venta.Fecha);

            if (pago is not null && string.IsNullOrWhiteSpace(pago.Referencia))
                pago.Referencia = venta.NumeroDocumento;

            if (cuenta is not null)
            {
                var limite = await connection.ExecuteScalarAsync<decimal>(
                    "SELECT COALESCE(LimiteCredito, 0) FROM Clientes WHERE Id = @ClienteId",
                    new { cuenta.ClienteId },
                    transaction);

                var saldoActual = await connection.ExecuteScalarAsync<decimal>(
                    "SELECT COALESCE(SUM(Saldo), 0) FROM CuentasPorCobrar WHERE ClienteId = @ClienteId AND Estado IN (@Pendiente, @Parcial, @Vencida)",
                    new
                    {
                        cuenta.ClienteId,
                        Pendiente = (int)EstadoCuentaCobrar.Pendiente,
                        Parcial = (int)EstadoCuentaCobrar.Parcial,
                        Vencida = (int)EstadoCuentaCobrar.Vencida
                    },
                    transaction);

                if (limite <= 0 || saldoActual + cuenta.Saldo > limite)
                    throw new InvalidOperationException("El cliente no tiene crédito disponible suficiente.");
            }

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
            using var transaction = connection.BeginTransaction(deferred: false);

            var venta = await connection.QueryFirstOrDefaultAsync<Venta>(
                "SELECT * FROM Ventas WHERE Id = @Id", new { Id = ventaId }, transaction);

            if (venta is null)
                throw new InvalidOperationException("La venta no existe.");
            if (venta.Estado == EstadoVenta.Anulada)
                throw new InvalidOperationException("La venta ya está anulada.");

            var tieneAbonos = await connection.ExecuteScalarAsync<int>(@"
                SELECT COUNT(1)
                FROM AbonosCobro a
                INNER JOIN CuentasPorCobrar c ON c.Id = a.CuentaPorCobrarId
                WHERE c.VentaId = @VentaId",
                new { VentaId = ventaId },
                transaction);

            if (tieneAbonos > 0)
                throw new InvalidOperationException("No se puede anular una venta con abonos registrados.");

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

            await connection.ExecuteAsync(@"
                UPDATE CuentasPorCobrar
                SET Estado = @Estado, Saldo = 0
                WHERE VentaId = @VentaId AND Estado <> @Estado",
                new { Estado = (int)EstadoCuentaCobrar.Anulada, VentaId = ventaId },
                transaction);

            transaction.Commit();
        }

        public async Task<string> GenerarNumeroAsync(string sucursalId, TipoDocumento tipoDocumento, DateTime fecha)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();
            using var transaction = connection.BeginTransaction();
            var numero = await DocumentNumberGenerator.GenerarAsync(connection, transaction, sucursalId, tipoDocumento, fecha);
            transaction.Commit();
            return numero;
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

            try
            {
                await StockWriter.AplicarSalidaAsync(
                    connection, transaction, venta.AlmacenId, detalle.ProductoId, detalle.Cantidad);
            }
            catch (InvalidOperationException)
            {
                throw new InvalidOperationException($"Stock insuficiente para el producto {detalle.ProductoNombre}.");
            }

            await StockWriter.InsertarMovimientoAsync(connection, transaction, new MovimientoInventario
            {
                AlmacenId = venta.AlmacenId,
                ProductoId = detalle.ProductoId,
                Tipo = TipoMovimientoInventario.Salida,
                Cantidad = detalle.Cantidad,
                CostoUnitario = 0m,
                Referencia = venta.NumeroDocumento,
                Observacion = "Venta",
                UsuarioId = venta.UsuarioId,
                Fecha = venta.Fecha
            });
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

            await StockWriter.AplicarEntradaAsync(
                connection, transaction, venta.AlmacenId, detalle.ProductoId,
                detalle.Cantidad, 0m, actualizarCosto: false);

            await StockWriter.InsertarMovimientoAsync(connection, transaction, new MovimientoInventario
            {
                AlmacenId = venta.AlmacenId,
                ProductoId = detalle.ProductoId,
                Tipo = TipoMovimientoInventario.Entrada,
                Cantidad = detalle.Cantidad,
                CostoUnitario = 0m,
                Referencia = venta.NumeroDocumento,
                Observacion = $"Anulación de venta: {motivo}",
                UsuarioId = usuarioId,
                Fecha = DateTime.Now
            });
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
