using Mercurio.Application.DTOs.Reports;

namespace Mercurio.Application.Interfaces
{
    public interface IReporteRepository
    {
        Task<IEnumerable<ResumenVentasDto>> GetResumenVentasAsync(string negocioId, string? sucursalId, DateTime desde, DateTime hasta);
        Task<IEnumerable<VentasPorProductoDto>> GetVentasPorProductoAsync(string negocioId, string? sucursalId, DateTime desde, DateTime hasta);
        Task<IEnumerable<VentasPorUsuarioDto>> GetVentasPorUsuarioAsync(string negocioId, string? sucursalId, DateTime desde, DateTime hasta);
        Task<IEnumerable<VentasPorMetodoPagoDto>> GetVentasPorMetodoPagoAsync(string negocioId, string? sucursalId, DateTime desde, DateTime hasta);
        Task<IEnumerable<VentasDiariasDto>> GetVentasDiariasAsync(string negocioId, string? sucursalId, DateTime desde, DateTime hasta);
        Task<IEnumerable<ValorizacionInventarioDto>> GetValorizacionInventarioAsync(string negocioId, string? sucursalId);
        Task<IEnumerable<StockBajoDto>> GetStockBajoAsync(string negocioId, string? sucursalId);
        Task<IEnumerable<ResumenComprasDto>> GetResumenComprasAsync(string negocioId, string? sucursalId, DateTime desde, DateTime hasta);
        Task<IEnumerable<CuentaPorCobrarReporteDto>> GetCuentasPorCobrarAsync(string negocioId, DateTime fechaCorte, bool soloVencidas);
        Task<IEnumerable<ResumenCajaDto>> GetResumenCajaAsync(string negocioId, string? sucursalId, DateTime desde, DateTime hasta);
        Task<IEnumerable<TopClienteDto>> GetTopClientesAsync(string negocioId, DateTime desde, DateTime hasta, int limite);
    }

    public interface IReporteService
    {
        Task<IEnumerable<ResumenVentasDto>> GetResumenVentasAsync(string negocioId, string? sucursalId, DateTime desde, DateTime hasta);
        Task<IEnumerable<VentasPorProductoDto>> GetVentasPorProductoAsync(string negocioId, string? sucursalId, DateTime desde, DateTime hasta);
        Task<IEnumerable<VentasPorUsuarioDto>> GetVentasPorUsuarioAsync(string negocioId, string? sucursalId, DateTime desde, DateTime hasta);
        Task<IEnumerable<VentasPorMetodoPagoDto>> GetVentasPorMetodoPagoAsync(string negocioId, string? sucursalId, DateTime desde, DateTime hasta);
        Task<IEnumerable<VentasDiariasDto>> GetVentasDiariasAsync(string negocioId, string? sucursalId, DateTime desde, DateTime hasta);
        Task<IEnumerable<ValorizacionInventarioDto>> GetValorizacionInventarioAsync(string negocioId, string? sucursalId);
        Task<IEnumerable<StockBajoDto>> GetStockBajoAsync(string negocioId, string? sucursalId);
        Task<IEnumerable<ResumenComprasDto>> GetResumenComprasAsync(string negocioId, string? sucursalId, DateTime desde, DateTime hasta);
        Task<IEnumerable<CuentaPorCobrarReporteDto>> GetCuentasPorCobrarAsync(string negocioId, DateTime fechaCorte, bool soloVencidas = true);
        Task<IEnumerable<ResumenCajaDto>> GetResumenCajaAsync(string negocioId, string? sucursalId, DateTime desde, DateTime hasta);
        Task<IEnumerable<TopClienteDto>> GetTopClientesAsync(string negocioId, DateTime desde, DateTime hasta, int limite = 10);
    }
}
