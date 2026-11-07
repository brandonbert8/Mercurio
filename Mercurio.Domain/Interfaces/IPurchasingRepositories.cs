using Mercurio.Domain.Entities;

namespace Mercurio.Domain.Interfaces
{
    public interface IProveedorRepository
    {
        Task<IEnumerable<Proveedor>> GetAllAsync(string negocioId);
        Task<Proveedor?> GetByIdAsync(string id);
        Task<IEnumerable<Proveedor>> BuscarAsync(string negocioId, string termino);
        Task InsertarAsync(Proveedor proveedor);
        Task ActualizarAsync(Proveedor proveedor);
        Task DesactivarAsync(string id);
    }

    public interface ICompraRepository
    {
        Task<Compra?> GetByIdAsync(string id);
        Task<IEnumerable<Compra>> GetByRangoAsync(string sucursalId, DateTime desde, DateTime hasta);
        Task<IEnumerable<CompraDetalle>> GetDetallesAsync(string compraId);
        Task CrearAsync(Compra compra, IEnumerable<CompraDetalle> detalles);
        Task AnularAsync(string compraId);
        Task<string> GenerarNumeroAsync(string sucursalId, DateTime fecha);
    }
}
