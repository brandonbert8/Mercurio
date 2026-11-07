using Mercurio.Domain.Entities;

namespace Mercurio.Domain.Interfaces
{
    public interface ICashRepository
    {
        Task<IEnumerable<Caja>> GetCajasAsync(string sucursalId);
        Task<Caja?> GetCajaByIdAsync(string id);
        Task InsertarCajaAsync(Caja caja);
        Task ActualizarCajaAsync(Caja caja);

        Task<CajaSesion?> GetSesionByIdAsync(string id);
        Task<CajaSesion?> GetSesionAbiertaAsync(string cajaId);
        Task<IEnumerable<CajaSesion>> GetSesionesAsync(string sucursalId, DateTime desde, DateTime hasta);
        Task InsertarSesionAsync(CajaSesion sesion);
        Task ActualizarSesionAsync(CajaSesion sesion);

        Task InsertarMovimientoAsync(CajaMovimiento movimiento);
        Task<IEnumerable<CajaMovimiento>> GetMovimientosAsync(string cajaSesionId);
        Task<decimal> CalcularMontoSistemaAsync(string cajaSesionId);
    }
}
