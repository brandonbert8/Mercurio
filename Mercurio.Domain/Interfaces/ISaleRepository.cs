using Mercurio.Domain.Entities;

namespace Mercurio.Domain.Interfaces
{
    public interface ISaleRepository
    {
        Task<Venta?> GetByIdAsync(string id);
        Task<IEnumerable<Venta>> GetByRangoAsync(string sucursalId, DateTime desde, DateTime hasta);
        Task<IEnumerable<VentaDetalle>> GetDetallesAsync(string ventaId);
        Task CrearAsync(Venta venta, IEnumerable<VentaDetalle> detalles, Pago? pago, CuentaPorCobrar? cuenta);
        Task AnularAsync(string ventaId, string usuarioId, string motivo);
        Task<string> GenerarNumeroAsync(string sucursalId, DateTime fecha);
    }

    public interface IPagoRepository
    {
        Task<IEnumerable<Pago>> GetByVentaAsync(string ventaId);
        Task InsertarAsync(Pago pago);
    }
}
