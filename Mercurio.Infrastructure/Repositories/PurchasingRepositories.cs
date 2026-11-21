using Dapper;
using Mercurio.Domain.Entities;
using Mercurio.Domain.Enums;
using Mercurio.Domain.Interfaces;
using Microsoft.Data.Sqlite;

namespace Mercurio.Infrastructure.Repositories
{
    public class ProveedorRepository : IProveedorRepository
    {
        private readonly string _connectionString;

        public ProveedorRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<IEnumerable<Proveedor>> GetAllAsync(string negocioId)
        {
            using var connection = new SqliteConnection(_connectionString);
            return await connection.QueryAsync<Proveedor>(
                "SELECT * FROM Proveedores WHERE NegocioId = @NegocioId ORDER BY Nombre",
                new { NegocioId = negocioId });
        }

        public async Task<Proveedor?> GetByIdAsync(string id)
        {
            using var connection = new SqliteConnection(_connectionString);
            return await connection.QueryFirstOrDefaultAsync<Proveedor>("SELECT * FROM Proveedores WHERE Id = @Id", new { Id = id });
        }

        public async Task<IEnumerable<Proveedor>> BuscarAsync(string negocioId, string termino)
        {
            using var connection = new SqliteConnection(_connectionString);
            const string sql = @"
                SELECT * FROM Proveedores
                WHERE NegocioId = @NegocioId AND (Nombre LIKE @Termino OR Ruc LIKE @Termino OR Contacto LIKE @Termino)
                ORDER BY Nombre";
            return await connection.QueryAsync<Proveedor>(sql, new { NegocioId = negocioId, Termino = $"%{termino}%" });
        }

        public async Task InsertarAsync(Proveedor proveedor)
        {
            using var connection = new SqliteConnection(_connectionString);
            const string sql = @"
                INSERT INTO Proveedores (Id, NegocioId, Nombre, Ruc, Contacto, Telefono, Email, Direccion, Activo, FechaCreacion)
                VALUES (@Id, @NegocioId, @Nombre, @Ruc, @Contacto, @Telefono, @Email, @Direccion, @Activo, @FechaCreacion)";
            await connection.ExecuteAsync(sql, proveedor);
        }

        public async Task ActualizarAsync(Proveedor proveedor)
        {
            using var connection = new SqliteConnection(_connectionString);
            const string sql = @"
                UPDATE Proveedores SET Nombre = @Nombre, Ruc = @Ruc, Contacto = @Contacto, Telefono = @Telefono,
                    Email = @Email, Direccion = @Direccion, Activo = @Activo
                WHERE Id = @Id";
            await connection.ExecuteAsync(sql, proveedor);
        }

        public async Task DesactivarAsync(string id)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.ExecuteAsync("UPDATE Proveedores SET Activo = 0 WHERE Id = @Id", new { Id = id });
        }
    }

    public class CompraRepository : ICompraRepository
    {
        private readonly string _connectionString;

        public CompraRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<Compra?> GetByIdAsync(string id)
        {
            using var connection = new SqliteConnection(_connectionString);
            return await connection.QueryFirstOrDefaultAsync<Compra>("SELECT * FROM Compras WHERE Id = @Id", new { Id = id });
        }

        public async Task<IEnumerable<Compra>> GetByRangoAsync(string sucursalId, DateTime desde, DateTime hasta)
        {
            using var connection = new SqliteConnection(_connectionString);
            const string sql = @"
                SELECT * FROM Compras
                WHERE SucursalId = @SucursalId AND Fecha >= @Desde AND Fecha <= @Hasta
                ORDER BY Fecha DESC";
            return await connection.QueryAsync<Compra>(sql, new { SucursalId = sucursalId, Desde = desde, Hasta = hasta });
        }

        public async Task<IEnumerable<CompraDetalle>> GetDetallesAsync(string compraId)
        {
            using var connection = new SqliteConnection(_connectionString);
            return await connection.QueryAsync<CompraDetalle>(
                "SELECT * FROM CompraDetalles WHERE CompraId = @CompraId ORDER BY ProductoNombre",
                new { CompraId = compraId });
        }

        public async Task CrearAsync(Compra compra, IEnumerable<CompraDetalle> detalles, CuentaPorPagar? cuenta, string? cajaSesionId = null)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();
            using var transaction = connection.BeginTransaction();

            if (string.IsNullOrWhiteSpace(compra.NumeroDocumento))
                compra.NumeroDocumento = await DocumentNumberGenerator.GenerarAsync(
                    connection, transaction, compra.SucursalId, compra.TipoDocumento, compra.Fecha);

            const string sqlCompra = @"
                INSERT INTO Compras (Id, SucursalId, ProveedorId, AlmacenId, UsuarioId, NumeroDocumento, TipoDocumento,
                    Fecha, SubTotal, Impuesto, Total, MontoPagado, MetodoPago, Estado, Observacion)
                VALUES (@Id, @SucursalId, @ProveedorId, @AlmacenId, @UsuarioId, @NumeroDocumento, @TipoDocumento,
                    @Fecha, @SubTotal, @Impuesto, @Total, @MontoPagado, @MetodoPago, @Estado, @Observacion)";
            await connection.ExecuteAsync(sqlCompra, compra, transaction);

            const string sqlDetalle = @"
                INSERT INTO CompraDetalles (Id, CompraId, ProductoId, ProductoNombre, Cantidad, CostoUnitario,
                    Descuento, ImpuestoPorcentaje, SubTotal)
                VALUES (@Id, @CompraId, @ProductoId, @ProductoNombre, @Cantidad, @CostoUnitario,
                    @Descuento, @ImpuestoPorcentaje, @SubTotal)";

            foreach (var detalle in detalles)
            {
                await connection.ExecuteAsync(sqlDetalle, detalle, transaction);
                await AplicarEntradaStockAsync(connection, transaction, compra, detalle);
            }

            if (compra.MontoPagado > 0)
            {
                var pago = new Pago
                {
                    CompraId = compra.Id,
                    Tipo = TipoPago.Compra,
                    MetodoPago = compra.MetodoPago,
                    Monto = compra.MontoPagado,
                    CajaSesionId = cajaSesionId,
                    Referencia = compra.NumeroDocumento,
                    UsuarioId = compra.UsuarioId,
                    Fecha = compra.Fecha,
                    Observacion = compra.Observacion
                };

                await connection.ExecuteAsync(@"
                    INSERT INTO Pagos (Id, VentaId, CompraId, CuentaPorCobrarId, ClienteId, CajaSesionId, Tipo,
                        MetodoPago, Monto, Referencia, UsuarioId, Fecha, Observacion)
                    VALUES (@Id, @VentaId, @CompraId, @CuentaPorCobrarId, @ClienteId, @CajaSesionId, @Tipo,
                        @MetodoPago, @Monto, @Referencia, @UsuarioId, @Fecha, @Observacion)",
                    pago,
                    transaction);

                if (compra.MetodoPago == MetodoPago.Efectivo && !string.IsNullOrWhiteSpace(cajaSesionId))
                {
                    await connection.ExecuteAsync(@"
                        INSERT INTO CajaMovimientos (Id, CajaSesionId, Tipo, Concepto, Monto, Fecha, UsuarioId, Referencia)
                        VALUES (@Id, @CajaSesionId, @Tipo, @Concepto, @Monto, @Fecha, @UsuarioId, @Referencia)",
                        new
                        {
                            Id = Guid.NewGuid().ToString(),
                            CajaSesionId = cajaSesionId,
                            Tipo = (int)TipoMovimientoCaja.Egreso,
                            Concepto = $"Pago de compra {compra.NumeroDocumento}",
                            Monto = compra.MontoPagado,
                            Fecha = compra.Fecha,
                            UsuarioId = compra.UsuarioId,
                            Referencia = compra.NumeroDocumento
                        },
                        transaction);
                }
            }

            if (cuenta is not null)
            {
                await connection.ExecuteAsync(@"
                    INSERT INTO CuentasPorPagar (Id, CompraId, ProveedorId, FechaEmision, FechaVencimiento,
                        MontoOriginal, Saldo, Estado, Observacion)
                    VALUES (@Id, @CompraId, @ProveedorId, @FechaEmision, @FechaVencimiento,
                        @MontoOriginal, @Saldo, @Estado, @Observacion)",
                    cuenta,
                    transaction);
            }

            transaction.Commit();
        }

        public async Task AnularAsync(string compraId)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();
            using var transaction = connection.BeginTransaction(deferred: false);

            var compra = await connection.QueryFirstOrDefaultAsync<Compra>(
                "SELECT * FROM Compras WHERE Id = @Id", new { Id = compraId }, transaction);

            if (compra is null)
                throw new InvalidOperationException("La compra no existe.");
            if (compra.Estado == EstadoCompra.Anulada)
                throw new InvalidOperationException("La compra ya está anulada.");

            var tieneAbonos = await connection.ExecuteScalarAsync<int>(@"
                SELECT COUNT(1)
                FROM AbonosPago a
                INNER JOIN CuentasPorPagar c ON c.Id = a.CuentaPorPagarId
                WHERE c.CompraId = @CompraId",
                new { CompraId = compraId },
                transaction);

            if (tieneAbonos > 0)
                throw new InvalidOperationException("No se puede anular una compra con abonos registrados.");

            var detalles = await connection.QueryAsync<CompraDetalle>(
                "SELECT * FROM CompraDetalles WHERE CompraId = @CompraId", new { CompraId = compraId }, transaction);

            foreach (var detalle in detalles)
            {
                await AplicarSalidaStockAsync(connection, transaction, compra, detalle);
            }

            await connection.ExecuteAsync(
                "UPDATE Compras SET Estado = @Estado WHERE Id = @Id",
                new { Estado = (int)EstadoCompra.Anulada, Id = compraId },
                transaction);

            await connection.ExecuteAsync(@"
                UPDATE CuentasPorPagar
                SET Estado = @Estado, Saldo = 0
                WHERE CompraId = @CompraId AND Estado <> @Estado",
                new { Estado = (int)EstadoCuentaPagar.Anulada, CompraId = compraId },
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

        private static async Task AplicarEntradaStockAsync(
            SqliteConnection connection,
            SqliteTransaction transaction,
            Compra compra,
            CompraDetalle detalle)
        {
            await StockWriter.AplicarEntradaAsync(
                connection, transaction, compra.AlmacenId, detalle.ProductoId,
                detalle.Cantidad, detalle.CostoUnitario, actualizarCosto: true);

            await StockWriter.InsertarMovimientoAsync(connection, transaction, new MovimientoInventario
            {
                AlmacenId = compra.AlmacenId,
                ProductoId = detalle.ProductoId,
                Tipo = TipoMovimientoInventario.Entrada,
                Cantidad = detalle.Cantidad,
                CostoUnitario = detalle.CostoUnitario,
                Referencia = compra.NumeroDocumento,
                Observacion = "Compra de proveedor",
                UsuarioId = compra.UsuarioId,
                Fecha = compra.Fecha
            });
        }

        private static async Task AplicarSalidaStockAsync(
            SqliteConnection connection,
            SqliteTransaction transaction,
            Compra compra,
            CompraDetalle detalle)
        {
            await StockWriter.AplicarSalidaAsync(
                connection, transaction, compra.AlmacenId, detalle.ProductoId, detalle.Cantidad);

            await StockWriter.InsertarMovimientoAsync(connection, transaction, new MovimientoInventario
            {
                AlmacenId = compra.AlmacenId,
                ProductoId = detalle.ProductoId,
                Tipo = TipoMovimientoInventario.Salida,
                Cantidad = detalle.Cantidad,
                CostoUnitario = detalle.CostoUnitario,
                Referencia = compra.NumeroDocumento,
                Observacion = "Anulación de compra",
                UsuarioId = compra.UsuarioId,
                Fecha = DateTime.Now
            });
        }
    }
}
