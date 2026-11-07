using Dapper;
using Mercurio.Application.DTOs.Reports;
using Mercurio.Application.Interfaces;
using Mercurio.Domain.Enums;
using Microsoft.Data.Sqlite;

namespace Mercurio.Infrastructure.Repositories
{
    public class ReporteRepository : IReporteRepository
    {
        private readonly string _connectionString;

        public ReporteRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<IEnumerable<ResumenVentasDto>> GetResumenVentasAsync(string negocioId, string? sucursalId, DateTime desde, DateTime hasta)
        {
            using var connection = new SqliteConnection(_connectionString);
            const string sql = @"
                SELECT date(v.Fecha) AS Fecha, COUNT(1) AS CantidadVentas,
                    COALESCE(SUM(v.SubTotal), 0) AS SubTotal, COALESCE(SUM(v.Descuento), 0) AS Descuento,
                    COALESCE(SUM(v.Impuesto), 0) AS Impuesto, COALESCE(SUM(v.Total), 0) AS Total
                FROM Ventas v
                INNER JOIN Sucursales s ON s.Id = v.SucursalId
                WHERE s.NegocioId = @NegocioId AND v.Estado <> @Anulada
                    AND date(v.Fecha) BETWEEN date(@Desde) AND date(@Hasta)
                    AND (@SucursalId IS NULL OR v.SucursalId = @SucursalId)
                GROUP BY date(v.Fecha)
                ORDER BY date(v.Fecha)";
            return await connection.QueryAsync<ResumenVentasDto>(sql, new
            {
                NegocioId = negocioId,
                SucursalId = sucursalId,
                Desde = desde,
                Hasta = hasta,
                Anulada = (int)EstadoVenta.Anulada
            });
        }

        public async Task<IEnumerable<VentasPorProductoDto>> GetVentasPorProductoAsync(string negocioId, string? sucursalId, DateTime desde, DateTime hasta)
        {
            using var connection = new SqliteConnection(_connectionString);
            const string sql = @"
                SELECT d.ProductoId, d.ProductoNombre, SUM(d.Cantidad) AS Cantidad, COALESCE(SUM(d.SubTotal), 0) AS Total
                FROM VentaDetalles d
                INNER JOIN Ventas v ON v.Id = d.VentaId
                INNER JOIN Sucursales s ON s.Id = v.SucursalId
                WHERE s.NegocioId = @NegocioId AND v.Estado <> @Anulada
                    AND date(v.Fecha) BETWEEN date(@Desde) AND date(@Hasta)
                    AND (@SucursalId IS NULL OR v.SucursalId = @SucursalId)
                GROUP BY d.ProductoId, d.ProductoNombre
                ORDER BY Total DESC";
            return await connection.QueryAsync<VentasPorProductoDto>(sql, new
            {
                NegocioId = negocioId,
                SucursalId = sucursalId,
                Desde = desde,
                Hasta = hasta,
                Anulada = (int)EstadoVenta.Anulada
            });
        }

        public async Task<IEnumerable<VentasPorUsuarioDto>> GetVentasPorUsuarioAsync(string negocioId, string? sucursalId, DateTime desde, DateTime hasta)
        {
            using var connection = new SqliteConnection(_connectionString);
            const string sql = @"
                SELECT v.UsuarioId, COALESCE(u.NombreCompleto, '') AS UsuarioNombre,
                    COUNT(1) AS CantidadVentas, COALESCE(SUM(v.Total), 0) AS Total
                FROM Ventas v
                INNER JOIN Sucursales s ON s.Id = v.SucursalId
                LEFT JOIN Usuarios u ON u.Id = v.UsuarioId
                WHERE s.NegocioId = @NegocioId AND v.Estado <> @Anulada
                    AND date(v.Fecha) BETWEEN date(@Desde) AND date(@Hasta)
                    AND (@SucursalId IS NULL OR v.SucursalId = @SucursalId)
                GROUP BY v.UsuarioId, u.NombreCompleto
                ORDER BY Total DESC";
            return await connection.QueryAsync<VentasPorUsuarioDto>(sql, new
            {
                NegocioId = negocioId,
                SucursalId = sucursalId,
                Desde = desde,
                Hasta = hasta,
                Anulada = (int)EstadoVenta.Anulada
            });
        }

        public async Task<IEnumerable<VentasPorMetodoPagoDto>> GetVentasPorMetodoPagoAsync(string negocioId, string? sucursalId, DateTime desde, DateTime hasta)
        {
            using var connection = new SqliteConnection(_connectionString);
            const string sql = @"
                SELECT p.MetodoPago, COUNT(1) AS Cantidad, COALESCE(SUM(p.Monto), 0) AS Total
                FROM Pagos p
                INNER JOIN Ventas v ON v.Id = p.VentaId
                INNER JOIN Sucursales s ON s.Id = v.SucursalId
                WHERE s.NegocioId = @NegocioId AND p.Tipo = @TipoVenta AND v.Estado <> @Anulada
                    AND date(v.Fecha) BETWEEN date(@Desde) AND date(@Hasta)
                    AND (@SucursalId IS NULL OR v.SucursalId = @SucursalId)
                GROUP BY p.MetodoPago
                ORDER BY Total DESC";
            return await connection.QueryAsync<VentasPorMetodoPagoDto>(sql, new
            {
                NegocioId = negocioId,
                SucursalId = sucursalId,
                Desde = desde,
                Hasta = hasta,
                TipoVenta = (int)TipoPago.Venta,
                Anulada = (int)EstadoVenta.Anulada
            });
        }

        public async Task<IEnumerable<VentasDiariasDto>> GetVentasDiariasAsync(string negocioId, string? sucursalId, DateTime desde, DateTime hasta)
        {
            using var connection = new SqliteConnection(_connectionString);
            const string sql = @"
                SELECT date(v.Fecha) AS Fecha, COUNT(1) AS CantidadVentas, COALESCE(SUM(v.Total), 0) AS Total
                FROM Ventas v
                INNER JOIN Sucursales s ON s.Id = v.SucursalId
                WHERE s.NegocioId = @NegocioId AND v.Estado <> @Anulada
                    AND date(v.Fecha) BETWEEN date(@Desde) AND date(@Hasta)
                    AND (@SucursalId IS NULL OR v.SucursalId = @SucursalId)
                GROUP BY date(v.Fecha)
                ORDER BY date(v.Fecha)";
            return await connection.QueryAsync<VentasDiariasDto>(sql, new
            {
                NegocioId = negocioId,
                SucursalId = sucursalId,
                Desde = desde,
                Hasta = hasta,
                Anulada = (int)EstadoVenta.Anulada
            });
        }

        public async Task<IEnumerable<ValorizacionInventarioDto>> GetValorizacionInventarioAsync(string negocioId, string? sucursalId)
        {
            using var connection = new SqliteConnection(_connectionString);
            const string sql = @"
                SELECT a.Id AS AlmacenId, a.Nombre AS AlmacenNombre, p.Id AS ProductoId, p.Nombre AS ProductoNombre,
                    s.Cantidad, p.PrecioCompra AS CostoUnitario, ROUND(s.Cantidad * p.PrecioCompra, 2) AS Valor
                FROM StockAlmacen s
                INNER JOIN Productos p ON p.Id = s.ProductoId
                INNER JOIN Almacenes a ON a.Id = s.AlmacenId
                INNER JOIN Sucursales su ON su.Id = a.SucursalId
                WHERE su.NegocioId = @NegocioId AND p.Activo = 1
                    AND (@SucursalId IS NULL OR su.Id = @SucursalId)
                ORDER BY a.Nombre, p.Nombre";
            return await connection.QueryAsync<ValorizacionInventarioDto>(sql, new { NegocioId = negocioId, SucursalId = sucursalId });
        }

        public async Task<IEnumerable<StockBajoDto>> GetStockBajoAsync(string negocioId, string? sucursalId)
        {
            using var connection = new SqliteConnection(_connectionString);
            const string sql = @"
                SELECT a.Id AS AlmacenId, a.Nombre AS AlmacenNombre, p.Id AS ProductoId, p.Nombre AS ProductoNombre,
                    s.Cantidad, s.StockMinimo
                FROM StockAlmacen s
                INNER JOIN Productos p ON p.Id = s.ProductoId
                INNER JOIN Almacenes a ON a.Id = s.AlmacenId
                INNER JOIN Sucursales su ON su.Id = a.SucursalId
                WHERE su.NegocioId = @NegocioId AND p.Activo = 1 AND s.Cantidad <= s.StockMinimo
                    AND (@SucursalId IS NULL OR su.Id = @SucursalId)
                ORDER BY a.Nombre, p.Nombre";
            return await connection.QueryAsync<StockBajoDto>(sql, new { NegocioId = negocioId, SucursalId = sucursalId });
        }

        public async Task<IEnumerable<ResumenComprasDto>> GetResumenComprasAsync(string negocioId, string? sucursalId, DateTime desde, DateTime hasta)
        {
            using var connection = new SqliteConnection(_connectionString);
            const string sql = @"
                SELECT date(c.Fecha) AS Fecha, COUNT(1) AS CantidadCompras,
                    COALESCE(SUM(c.SubTotal), 0) AS SubTotal, COALESCE(SUM(c.Impuesto), 0) AS Impuesto,
                    COALESCE(SUM(c.Total), 0) AS Total
                FROM Compras c
                INNER JOIN Sucursales s ON s.Id = c.SucursalId
                WHERE s.NegocioId = @NegocioId AND c.Estado <> @Anulada
                    AND date(c.Fecha) BETWEEN date(@Desde) AND date(@Hasta)
                    AND (@SucursalId IS NULL OR c.SucursalId = @SucursalId)
                GROUP BY date(c.Fecha)
                ORDER BY date(c.Fecha)";
            return await connection.QueryAsync<ResumenComprasDto>(sql, new
            {
                NegocioId = negocioId,
                SucursalId = sucursalId,
                Desde = desde,
                Hasta = hasta,
                Anulada = (int)EstadoCompra.Anulada
            });
        }

        public async Task<IEnumerable<CuentaPorCobrarReporteDto>> GetCuentasPorCobrarAsync(string negocioId, DateTime fechaCorte, bool soloVencidas)
        {
            using var connection = new SqliteConnection(_connectionString);
            const string sql = @"
                SELECT c.Id AS CuentaId, c.ClienteId, cl.Nombre AS Cliente, c.VentaId,
                    c.FechaEmision, c.FechaVencimiento, c.MontoOriginal, c.Saldo,
                    CAST(MAX(0, julianday(date(@FechaCorte)) - julianday(date(c.FechaVencimiento))) AS INTEGER) AS DiasVencido,
                    c.Estado
                FROM CuentasPorCobrar c
                INNER JOIN Clientes cl ON cl.Id = c.ClienteId
                WHERE cl.NegocioId = @NegocioId AND c.Estado IN (1, 2, 4) AND c.Saldo > 0
                    AND (@SoloVencidas = 0 OR date(c.FechaVencimiento) < date(@FechaCorte))
                ORDER BY c.FechaVencimiento";
            return await connection.QueryAsync<CuentaPorCobrarReporteDto>(sql, new
            {
                NegocioId = negocioId,
                FechaCorte = fechaCorte,
                SoloVencidas = soloVencidas ? 1 : 0
            });
        }

        public async Task<IEnumerable<ResumenCajaDto>> GetResumenCajaAsync(string negocioId, string? sucursalId, DateTime desde, DateTime hasta)
        {
            using var connection = new SqliteConnection(_connectionString);
            const string sql = @"
                SELECT s.Id AS CajaSesionId, c.Nombre AS CajaNombre, COALESCE(u.NombreCompleto, '') AS UsuarioNombre,
                    s.FechaApertura, s.FechaCierre, s.MontoInicial,
                    COALESCE((SELECT SUM(m.Monto) FROM CajaMovimientos m
                        WHERE m.CajaSesionId = s.Id AND m.Tipo IN (@Ingreso, @Venta, @Cobro)), 0) AS Ingresos,
                    COALESCE((SELECT SUM(m.Monto) FROM CajaMovimientos m
                        WHERE m.CajaSesionId = s.Id AND m.Tipo = @Egreso), 0) AS Egresos,
                    s.MontoDeclarado, s.Diferencia, s.Estado
                FROM CajaSesiones s
                INNER JOIN Cajas c ON c.Id = s.CajaId
                INNER JOIN Sucursales su ON su.Id = c.SucursalId
                LEFT JOIN Usuarios u ON u.Id = s.UsuarioId
                WHERE su.NegocioId = @NegocioId
                    AND date(s.FechaApertura) BETWEEN date(@Desde) AND date(@Hasta)
                    AND (@SucursalId IS NULL OR su.Id = @SucursalId)
                ORDER BY s.FechaApertura DESC";
            return await connection.QueryAsync<ResumenCajaDto>(sql, new
            {
                NegocioId = negocioId,
                SucursalId = sucursalId,
                Desde = desde,
                Hasta = hasta,
                Ingreso = (int)TipoMovimientoCaja.Ingreso,
                Venta = (int)TipoMovimientoCaja.Venta,
                Cobro = (int)TipoMovimientoCaja.Cobro,
                Egreso = (int)TipoMovimientoCaja.Egreso
            });
        }

        public async Task<IEnumerable<TopClienteDto>> GetTopClientesAsync(string negocioId, DateTime desde, DateTime hasta, int limite)
        {
            using var connection = new SqliteConnection(_connectionString);
            const string sql = @"
                SELECT cl.Id AS ClienteId, cl.Nombre AS Cliente, COUNT(1) AS CantidadVentas,
                    COALESCE(SUM(v.Total), 0) AS Total
                FROM Ventas v
                INNER JOIN Clientes cl ON cl.Id = v.ClienteId
                INNER JOIN Sucursales s ON s.Id = v.SucursalId
                WHERE s.NegocioId = @NegocioId AND v.Estado <> @Anulada
                    AND date(v.Fecha) BETWEEN date(@Desde) AND date(@Hasta)
                GROUP BY cl.Id, cl.Nombre
                ORDER BY Total DESC
                LIMIT @Limite";
            return await connection.QueryAsync<TopClienteDto>(sql, new
            {
                NegocioId = negocioId,
                Desde = desde,
                Hasta = hasta,
                Limite = limite,
                Anulada = (int)EstadoVenta.Anulada
            });
        }
    }
}
