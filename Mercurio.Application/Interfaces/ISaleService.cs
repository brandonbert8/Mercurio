using Mercurio.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mercurio.Application.Interfaces
{
    public interface ISaleService
    {
        Task<IEnumerable<Product>> GetCatalogAsync();
        Task CreateProductAsync(Product product);
    }
}
