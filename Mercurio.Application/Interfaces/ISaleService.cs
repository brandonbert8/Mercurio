using Mercurio.Application.DTOs;
using Mercurio.Domain.Entities;

namespace Mercurio.Application.Interfaces
{
    public interface ISaleService
    {
        Task<IEnumerable<Product>> GetCatalogAsync();
        Task<IEnumerable<Product>> ObtenerInventarioAsync();
        Task CreateProductAsync(Product product);

        Task<VentaResult> RegistrarVentaAsync(VentaCreateRequest request);
        Task AnularVentaAsync(string ventaId, string usuarioId, string motivo);
        Task<Venta?> GetVentaAsync(string id);
        Task<IEnumerable<Venta>> GetVentasAsync(string sucursalId, DateTime desde, DateTime hasta);
        Task<IEnumerable<VentaDetalle>> GetDetallesAsync(string ventaId);
    }
}
