using Mercurio.Application.DTOs.Reports;
using Mercurio.Application.Interfaces;

namespace Mercurio.Application.Services
{
    public class ReporteService : IReporteService
    {
        private readonly IReporteRepository _reporteRepository;

        public ReporteService(IReporteRepository reporteRepository)
        {
            _reporteRepository = reporteRepository;
        }

        public Task<IEnumerable<ResumenVentasDto>> GetResumenVentasAsync(string negocioId, string? sucursalId, DateTime desde, DateTime hasta)
        {
            return _reporteRepository.GetResumenVentasAsync(negocioId, sucursalId, desde, hasta);
        }

        public Task<IEnumerable<VentasPorProductoDto>> GetVentasPorProductoAsync(string negocioId, string? sucursalId, DateTime desde, DateTime hasta)
        {
            return _reporteRepository.GetVentasPorProductoAsync(negocioId, sucursalId, desde, hasta);
        }

        public Task<IEnumerable<VentasPorUsuarioDto>> GetVentasPorUsuarioAsync(string negocioId, string? sucursalId, DateTime desde, DateTime hasta)
        {
            return _reporteRepository.GetVentasPorUsuarioAsync(negocioId, sucursalId, desde, hasta);
        }

        public Task<IEnumerable<VentasPorMetodoPagoDto>> GetVentasPorMetodoPagoAsync(string negocioId, string? sucursalId, DateTime desde, DateTime hasta)
        {
            return _reporteRepository.GetVentasPorMetodoPagoAsync(negocioId, sucursalId, desde, hasta);
        }

        public Task<IEnumerable<VentasDiariasDto>> GetVentasDiariasAsync(string negocioId, string? sucursalId, DateTime desde, DateTime hasta)
        {
            return _reporteRepository.GetVentasDiariasAsync(negocioId, sucursalId, desde, hasta);
        }

        public Task<IEnumerable<ValorizacionInventarioDto>> GetValorizacionInventarioAsync(string negocioId, string? sucursalId)
        {
            return _reporteRepository.GetValorizacionInventarioAsync(negocioId, sucursalId);
        }

        public Task<IEnumerable<StockBajoDto>> GetStockBajoAsync(string negocioId, string? sucursalId)
        {
            return _reporteRepository.GetStockBajoAsync(negocioId, sucursalId);
        }

        public Task<IEnumerable<ResumenComprasDto>> GetResumenComprasAsync(string negocioId, string? sucursalId, DateTime desde, DateTime hasta)
        {
            return _reporteRepository.GetResumenComprasAsync(negocioId, sucursalId, desde, hasta);
        }

        public Task<IEnumerable<CuentaPorCobrarReporteDto>> GetCuentasPorCobrarAsync(string negocioId, DateTime fechaCorte, bool soloVencidas = true)
        {
            return _reporteRepository.GetCuentasPorCobrarAsync(negocioId, fechaCorte, soloVencidas);
        }

        public Task<IEnumerable<ResumenCajaDto>> GetResumenCajaAsync(string negocioId, string? sucursalId, DateTime desde, DateTime hasta)
        {
            return _reporteRepository.GetResumenCajaAsync(negocioId, sucursalId, desde, hasta);
        }

        public Task<IEnumerable<TopClienteDto>> GetTopClientesAsync(string negocioId, DateTime desde, DateTime hasta, int limite = 10)
        {
            if (limite <= 0)
                limite = 10;

            return _reporteRepository.GetTopClientesAsync(negocioId, desde, hasta, limite);
        }
    }
}
