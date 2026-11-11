using Mercurio.Domain.Entities;

namespace Mercurio.Domain.Interfaces
{
    public interface ICuentaPorPagarRepository
    {
        Task<CuentaPorPagar?> GetByIdAsync(string id);
        Task<IEnumerable<CuentaPorPagar>> GetByProveedorAsync(string proveedorId, bool soloPendientes = false);
        Task<IEnumerable<CuentaPorPagar>> GetPendientesAsync(string negocioId, DateTime? fechaCorte = null);
        Task<IEnumerable<CuentaPorPagar>> GetVencidasAsync(string negocioId, DateTime fechaCorte);
        Task<IEnumerable<AbonoPago>> GetAbonosAsync(string cuentaId);
        Task<decimal> GetSaldoProveedorAsync(string proveedorId);
        Task RegistrarAbonoAsync(
            AbonoPago abono,
            decimal saldoAnterior,
            decimal nuevoSaldo,
            int estado,
            CajaMovimiento? movimientoCaja = null);
        Task ActualizarEstadoAsync(string cuentaId, int estado);
    }
}
