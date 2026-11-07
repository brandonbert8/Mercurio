using Mercurio.Domain.Entities;

namespace Mercurio.Application.Interfaces
{
    public interface ICatalogoService
    {
        Task<IEnumerable<Categoria>> GetCategoriasAsync(string negocioId);
        Task<Categoria?> GetCategoriaAsync(string id);
        Task<Categoria> CrearCategoriaAsync(Categoria categoria);
        Task ActualizarCategoriaAsync(Categoria categoria);
        Task DesactivarCategoriaAsync(string id);

        Task<IEnumerable<Product>> GetProductosAsync(bool soloActivos = true);
        Task<Product?> GetProductoAsync(string id);
        Task<Product?> GetProductoPorCodigoAsync(string codigoBarras);
        Task<IEnumerable<Product>> BuscarProductosAsync(string termino);
        Task<Product> CrearProductoAsync(Product producto);
        Task ActualizarProductoAsync(Product producto);
        Task DesactivarProductoAsync(string id);
    }
}
