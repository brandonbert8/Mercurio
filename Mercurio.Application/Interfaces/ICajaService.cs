using Mercurio.Application.DTOs;
using Mercurio.Domain.Entities;

namespace Mercurio.Application.Interfaces
{
    public interface ICajaService
    {
        Task<IEnumerable<Caja>> GetCajasAsync(string sucursalId);
        Task<Caja?> GetCajaAsync(string id);
        Task<Caja> CrearCajaAsync(Caja caja);
        Task ActualizarCajaAsync(Caja caja);

        Task<CajaSesion> AbrirCajaAsync(AbrirCajaRequest request);
        Task<CierreCajaResult> CerrarCajaAsync(CerrarCajaRequest request);
        Task<CajaSesion?> GetSesionAbiertaAsync(string cajaId);
        Task<CajaSesion?> GetSesionAsync(string id);
        Task<CajaMovimiento> RegistrarMovimientoAsync(MovimientoCajaRequest request);
        Task<IEnumerable<CajaMovimiento>> GetMovimientosAsync(string cajaSesionId);
        Task<IEnumerable<CajaSesion>> GetSesionesAsync(string sucursalId, DateTime desde, DateTime hasta);
    }
}
