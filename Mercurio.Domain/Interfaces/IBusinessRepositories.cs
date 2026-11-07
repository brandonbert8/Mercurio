using Mercurio.Domain.Entities;

namespace Mercurio.Domain.Interfaces
{
    public interface INegocioRepository
    {
        Task<IEnumerable<Negocio>> GetAllAsync();
        Task<Negocio?> GetByIdAsync(string id);
        Task InsertarAsync(Negocio negocio);
        Task ActualizarAsync(Negocio negocio);
    }

    public interface ISucursalRepository
    {
        Task<IEnumerable<Sucursal>> GetAllAsync(string negocioId);
        Task<Sucursal?> GetByIdAsync(string id);
        Task InsertarAsync(Sucursal sucursal);
        Task ActualizarAsync(Sucursal sucursal);
    }

    public interface IUsuarioRepository
    {
        Task<IEnumerable<Usuario>> GetAllAsync(string negocioId);
        Task<Usuario?> GetByIdAsync(string id);
        Task<Usuario?> GetByNombreUsuarioAsync(string negocioId, string nombreUsuario);
        Task InsertarAsync(Usuario usuario);
        Task ActualizarAsync(Usuario usuario);
        Task ActualizarUltimoAccesoAsync(string id, DateTime fecha);
    }
}
