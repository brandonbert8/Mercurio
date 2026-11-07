using Mercurio.Application.DTOs;
using Mercurio.Domain.Entities;

namespace Mercurio.Application.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResult> LoginAsync(LoginRequest request);
        Task<Usuario> CrearUsuarioAsync(CrearUsuarioRequest request);
        Task<bool> CambiarPasswordAsync(CambiarPasswordRequest request);
        Task<IEnumerable<Usuario>> GetUsuariosAsync(string negocioId);
        Task<Usuario?> GetUsuarioAsync(string usuarioId);
        Task DesactivarUsuarioAsync(string usuarioId);
    }

    public interface IPasswordHasher
    {
        string CreateSalt();
        string Hash(string password, string salt);
        bool Verify(string password, string salt, string hash);
    }

    public interface INegocioService
    {
        Task<IEnumerable<Negocio>> GetNegociosAsync();
        Task<Negocio?> GetNegocioAsync(string id);
        Task<Negocio> CrearNegocioAsync(Negocio negocio);
        Task ActualizarNegocioAsync(Negocio negocio);

        Task<IEnumerable<Sucursal>> GetSucursalesAsync(string negocioId);
        Task<Sucursal?> GetSucursalAsync(string id);
        Task<Sucursal> CrearSucursalAsync(Sucursal sucursal);
        Task ActualizarSucursalAsync(Sucursal sucursal);
    }
}
