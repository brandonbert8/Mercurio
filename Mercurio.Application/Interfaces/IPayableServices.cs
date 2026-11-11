using Mercurio.Application.DTOs;
using Mercurio.Domain.Entities;

namespace Mercurio.Application.Interfaces
{
    public interface ICuentaPorPagarService
    {
        Task<AbonoPagoResult> RegistrarAbonoAsync(RegistrarAbonoPagoRequest request);
        Task<CuentaPorPagar?> GetCuentaAsync(string id);
        Task<IEnumerable<CuentaPorPagar>> GetCuentasProveedorAsync(string proveedorId, bool soloPendientes = false);
        Task<IEnumerable<CuentaPorPagar>> GetCuentasPendientesAsync(string negocioId);
        Task<IEnumerable<CuentaPorPagar>> GetCuentasVencidasAsync(string negocioId, DateTime fechaCorte);
        Task<IEnumerable<AbonoPago>> GetAbonosAsync(string cuentaId);
        Task<decimal> GetSaldoProveedorAsync(string proveedorId);
    }
}
