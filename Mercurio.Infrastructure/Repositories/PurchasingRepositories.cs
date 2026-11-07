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

        public async Task CrearAsync(Compra compra, IEnumerable<CompraDetalle> detalles)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();
            using var transaction = connection.BeginTransaction();

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
            }

            transaction.Commit();
        }

        public async Task AnularAsync(string compraId)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();
            using var transaction = connection.BeginTransaction();

            var compra = await connection.QueryFirstOrDefaultAsync<Compra>(
                "SELECT * FROM Compras WHERE Id = @Id", new { Id = compraId }, transaction);

            if (compra is null)
                throw new InvalidOperationException("La compra no existe.");

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

            transaction.Commit();
        }

        public async Task<string> GenerarNumeroAsync(string sucursalId, DateTime fecha)
        {
            using var connection = new SqliteConnection(_connectionString);
            var prefijo = $"C-{fecha:yyyy}-";
            var count = await connection.ExecuteScalarAsync<int>(
                "SELECT COUNT(1) FROM Compras WHERE SucursalId = @SucursalId AND NumeroDocumento LIKE @Prefijo",
                new { SucursalId = sucursalId, Prefijo = $"{prefijo}%" });

            return $"{prefijo}{count + 1:000000}";
        }

        private static async Task AplicarEntradaStockAsync(
            SqliteConnection connection,
            SqliteTransaction transaction,
            Compra compra,
            CompraDetalle detalle)
        {
            var stock = await connection.QueryFirstOrDefaultAsync<StockAlmacen>(
                "SELECT * FROM StockAlmacen WHERE AlmacenId = @AlmacenId AND ProductoId = @ProductoId",
                new { AlmacenId = compra.AlmacenId, ProductoId = detalle.ProductoId },
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
                        AlmacenId = compra.AlmacenId,
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

            await connection.ExecuteAsync(@"
                UPDATE Productos SET StockActual = StockActual + @Cantidad, PrecioCompra = @Costo
                WHERE Id = @ProductoId",
                new { Cantidad = detalle.Cantidad, Costo = detalle.CostoUnitario, ProductoId = detalle.ProductoId },
                transaction);

            await connection.ExecuteAsync(@"
                INSERT INTO MovimientosInventario (Id, AlmacenId, AlmacenDestinoId, ProductoId, Tipo, Cantidad,
                    CostoUnitario, Referencia, Observacion, UsuarioId, Fecha)
                VALUES (@Id, @AlmacenId, NULL, @ProductoId, @Tipo, @Cantidad,
                    @CostoUnitario, @Referencia, @Observacion, @UsuarioId, @Fecha)",
                new
                {
                    Id = Guid.NewGuid().ToString(),
                    AlmacenId = compra.AlmacenId,
                    ProductoId = detalle.ProductoId,
                    Tipo = (int)TipoMovimientoInventario.Entrada,
                    Cantidad = detalle.Cantidad,
                    CostoUnitario = detalle.CostoUnitario,
                    Referencia = compra.NumeroDocumento,
                    Observacion = "Compra de proveedor",
                    UsuarioId = compra.UsuarioId,
                    Fecha = compra.Fecha
                },
                transaction);
        }

        private static async Task AplicarSalidaStockAsync(
            SqliteConnection connection,
            SqliteTransaction transaction,
            Compra compra,
            CompraDetalle detalle)
        {
            var stock = await connection.QueryFirstOrDefaultAsync<StockAlmacen>(
                "SELECT * FROM StockAlmacen WHERE AlmacenId = @AlmacenId AND ProductoId = @ProductoId",
                new { AlmacenId = compra.AlmacenId, ProductoId = detalle.ProductoId },
                transaction);

            if (stock is null || stock.Cantidad < detalle.Cantidad)
                throw new InvalidOperationException("No hay stock suficiente para anular la compra.");

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
                    AlmacenId = compra.AlmacenId,
                    ProductoId = detalle.ProductoId,
                    Tipo = (int)TipoMovimientoInventario.Salida,
                    Cantidad = detalle.Cantidad,
                    CostoUnitario = detalle.CostoUnitario,
                    Referencia = compra.NumeroDocumento,
                    Observacion = "Anulación de compra",
                    UsuarioId = compra.UsuarioId,
                    Fecha = DateTime.Now
                },
                transaction);
        }
    }
}
