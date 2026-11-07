using Mercurio.Application.DTOs;
using Mercurio.Domain.Entities;

namespace Mercurio.Application.Interfaces
{
    public interface IClienteService
    {
        Task<IEnumerable<Cliente>> GetClientesAsync(string negocioId);
        Task<Cliente?> GetClienteAsync(string id);
        Task<Cliente?> GetClientePorDocumentoAsync(string negocioId, string numeroDocumento);
        Task<IEnumerable<Cliente>> BuscarClientesAsync(string negocioId, string termino);
        Task<Cliente> CrearClienteAsync(Cliente cliente);
        Task ActualizarClienteAsync(Cliente cliente);
        Task DesactivarClienteAsync(string id);
        Task<bool> PuedeComprarAlCreditoAsync(string clienteId, decimal monto);
    }

    public interface ICreditoService
    {
        Task<AbonoResult> RegistrarAbonoAsync(RegistrarAbonoRequest request);
        Task<CuentaPorCobrar?> GetCuentaAsync(string id);
        Task<IEnumerable<CuentaPorCobrar>> GetCuentasClienteAsync(string clienteId, bool soloPendientes = false);
        Task<IEnumerable<CuentaPorCobrar>> GetCuentasPendientesAsync(string negocioId);
        Task<IEnumerable<CuentaPorCobrar>> GetCuentasVencidasAsync(string negocioId, DateTime fechaCorte);
        Task<IEnumerable<AbonoCobro>> GetAbonosAsync(string cuentaId);
        Task<decimal> GetSaldoClienteAsync(string clienteId);
        Task ActualizarVencidasAsync(string negocioId, DateTime fechaCorte);
        Task AnularCuentaPorVentaAsync(string ventaId);
    }
}
