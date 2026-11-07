using Mercurio.Domain.Entities;

namespace Mercurio.Domain.Interfaces
{
    public interface IClienteRepository
    {
        Task<IEnumerable<Cliente>> GetAllAsync(string negocioId);
        Task<Cliente?> GetByIdAsync(string id);
        Task<Cliente?> GetByDocumentoAsync(string negocioId, string numeroDocumento);
        Task<IEnumerable<Cliente>> BuscarAsync(string negocioId, string termino);
        Task InsertarAsync(Cliente cliente);
        Task ActualizarAsync(Cliente cliente);
        Task DesactivarAsync(string id);
    }

    public interface ICreditoRepository
    {
        Task<CuentaPorCobrar?> GetByIdAsync(string id);
        Task<IEnumerable<CuentaPorCobrar>> GetByClienteAsync(string clienteId, bool soloPendientes = false);
        Task<IEnumerable<CuentaPorCobrar>> GetPendientesAsync(string negocioId, DateTime? fechaCorte = null);
        Task<IEnumerable<CuentaPorCobrar>> GetVencidasAsync(string negocioId, DateTime fechaCorte);
        Task<IEnumerable<AbonoCobro>> GetAbonosAsync(string cuentaId);
        Task<decimal> GetSaldoClienteAsync(string clienteId);
        Task CrearAsync(CuentaPorCobrar cuenta);
        Task RegistrarAbonoAsync(AbonoCobro abono, decimal nuevoSaldo, int estado);
        Task ActualizarEstadoAsync(string cuentaId, int estado);
        Task AnularPorVentaAsync(string ventaId);
    }
}
