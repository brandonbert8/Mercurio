using Mercurio.Application.DTOs;
using Mercurio.Domain.Entities;

namespace Mercurio.Application.Interfaces
{
    public interface IProveedorService
    {
        Task<IEnumerable<Proveedor>> GetProveedoresAsync(string negocioId);
        Task<Proveedor?> GetProveedorAsync(string id);
        Task<IEnumerable<Proveedor>> BuscarProveedoresAsync(string negocioId, string termino);
        Task<Proveedor> CrearProveedorAsync(Proveedor proveedor);
        Task ActualizarProveedorAsync(Proveedor proveedor);
        Task DesactivarProveedorAsync(string id);
    }

    public interface ICompraService
    {
        Task<CompraResult> RegistrarCompraAsync(CompraCreateRequest request);
        Task AnularCompraAsync(string compraId);
        Task<Compra?> GetCompraAsync(string id);
        Task<IEnumerable<Compra>> GetComprasAsync(string sucursalId, DateTime desde, DateTime hasta);
        Task<IEnumerable<CompraDetalle>> GetDetallesAsync(string compraId);
    }
}
