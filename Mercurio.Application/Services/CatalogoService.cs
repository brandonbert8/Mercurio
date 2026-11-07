using Mercurio.Application.Common;
using Mercurio.Application.Interfaces;
using Mercurio.Domain.Entities;
using Mercurio.Domain.Interfaces;

namespace Mercurio.Application.Services
{
    public class CatalogoService : ICatalogoService
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoriaRepository _categoriaRepository;

        public CatalogoService(IProductRepository productRepository, ICategoriaRepository categoriaRepository)
        {
            _productRepository = productRepository;
            _categoriaRepository = categoriaRepository;
        }

        public Task<IEnumerable<Categoria>> GetCategoriasAsync(string negocioId)
        {
            return _categoriaRepository.GetAllAsync(negocioId);
        }

        public Task<Categoria?> GetCategoriaAsync(string id)
        {
            return _categoriaRepository.GetByIdAsync(id);
        }

        public async Task<Categoria> CrearCategoriaAsync(Categoria categoria)
        {
            if (string.IsNullOrWhiteSpace(categoria.NegocioId))
                throw new BusinessException("El negocio es obligatorio.");
            if (string.IsNullOrWhiteSpace(categoria.Nombre))
                throw new BusinessException("El nombre de la categoría es obligatorio.");

            await _categoriaRepository.InsertarAsync(categoria);
            return categoria;
        }

        public async Task ActualizarCategoriaAsync(Categoria categoria)
        {
            var existente = await _categoriaRepository.GetByIdAsync(categoria.Id);
            if (existente is null)
                throw new BusinessException("La categoría no existe.");

            if (string.IsNullOrWhiteSpace(categoria.Nombre))
                throw new BusinessException("El nombre de la categoría es obligatorio.");

            await _categoriaRepository.ActualizarAsync(categoria);
        }

        public async Task DesactivarCategoriaAsync(string id)
        {
            var existente = await _categoriaRepository.GetByIdAsync(id);
            if (existente is null)
                throw new BusinessException("La categoría no existe.");

            await _categoriaRepository.DesactivarAsync(id);
        }

        public async Task<IEnumerable<Product>> GetProductosAsync(bool soloActivos = true)
        {
            return soloActivos
                ? await _productRepository.GetActivosAsync()
                : await _productRepository.GetAllAsync();
        }

        public Task<Product?> GetProductoAsync(string id)
        {
            return _productRepository.GetByIdAsync(id);
        }

        public Task<Product?> GetProductoPorCodigoAsync(string codigoBarras)
        {
            if (string.IsNullOrWhiteSpace(codigoBarras))
                return Task.FromResult<Product?>(null);

            return _productRepository.GetByCodigoBarrasAsync(codigoBarras.Trim());
        }

        public Task<IEnumerable<Product>> BuscarProductosAsync(string termino)
        {
            if (string.IsNullOrWhiteSpace(termino))
                return _productRepository.GetActivosAsync();

            return _productRepository.BuscarAsync(termino.Trim());
        }

        public async Task<Product> CrearProductoAsync(Product producto)
        {
            ValidarProducto(producto);

            if (string.IsNullOrWhiteSpace(producto.CodigoBarras))
                producto.CodigoBarras = GenerarCodigoBarras();

            if (await _productRepository.ExisteCodigoBarrasAsync(producto.CodigoBarras))
                throw new BusinessException("Ya existe un producto con ese código de barras.");

            producto.StockMinimo = Math.Max(0, producto.StockMinimo);
            producto.ImpuestoPorcentaje = Math.Max(0, producto.ImpuestoPorcentaje);

            await _productRepository.InsertarAsync(producto);
            return producto;
        }

        public async Task ActualizarProductoAsync(Product producto)
        {
            var existente = await _productRepository.GetByIdAsync(producto.Id);
            if (existente is null)
                throw new BusinessException("El producto no existe.");

            ValidarProducto(producto);

            if (await _productRepository.ExisteCodigoBarrasAsync(producto.CodigoBarras, producto.Id))
                throw new BusinessException("Ya existe otro producto con ese código de barras.");

            producto.StockMinimo = Math.Max(0, producto.StockMinimo);
            producto.ImpuestoPorcentaje = Math.Max(0, producto.ImpuestoPorcentaje);

            await _productRepository.ActualizarAsync(producto);
        }

        public async Task DesactivarProductoAsync(string id)
        {
            var existente = await _productRepository.GetByIdAsync(id);
            if (existente is null)
                throw new BusinessException("El producto no existe.");

            await _productRepository.DesactivarAsync(id);
        }

        private static void ValidarProducto(Product producto)
        {
            if (string.IsNullOrWhiteSpace(producto.Nombre))
                throw new BusinessException("El nombre del producto es obligatorio.");
            if (producto.PrecioCompra < 0)
                throw new BusinessException("El precio de compra no puede ser negativo.");
            if (producto.PrecioVenta < 0)
                throw new BusinessException("El precio de venta no puede ser negativo.");
            if (string.IsNullOrWhiteSpace(producto.UnidadMedida))
                producto.UnidadMedida = "UNIDAD";
        }

        private static string GenerarCodigoBarras()
        {
            return $"P{Guid.NewGuid():N}"[..9].ToUpperInvariant();
        }
    }
}
