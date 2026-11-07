using Mercurio.Domain.Entities;

namespace Mercurio.Domain.Interfaces
{
    public interface IProductRepository
    {
        Task<IEnumerable<Product>> GetAllAsync();
        Task<IEnumerable<Product>> GetActivosAsync();
        Task<Product?> GetByIdAsync(string id);
        Task<Product?> GetByCodigoBarrasAsync(string codigoBarras);
        Task<IEnumerable<Product>> BuscarAsync(string termino);
        Task InsertarAsync(Product product);
        Task ActualizarAsync(Product product);
        Task DesactivarAsync(string id);
        Task<bool> ExisteCodigoBarrasAsync(string codigoBarras, string? excluirId = null);
    }

    public interface ICategoriaRepository
    {
        Task<IEnumerable<Categoria>> GetAllAsync(string negocioId);
        Task<Categoria?> GetByIdAsync(string id);
        Task InsertarAsync(Categoria categoria);
        Task ActualizarAsync(Categoria categoria);
        Task DesactivarAsync(string id);
    }
}
