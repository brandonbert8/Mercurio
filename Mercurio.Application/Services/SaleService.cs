using Mercurio.Application.Interfaces;
using Mercurio.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using Mercurio.Domain.Entities;

namespace Mercurio.Application.Services
{
    public class SaleService : ISaleService
    {
        private readonly IProductRepository _productRepository;

        public SaleService(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<IEnumerable<Product>> GetCatalogAsync()
        {
            return await _productRepository.GetAllAsync();
        }

        public async Task CreateProductAsync(Product product)
        {
            await _productRepository.InsertarAsync(product);
        }
    }
}
