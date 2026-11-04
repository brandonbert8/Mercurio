using Mercurio.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mercurio.Domain.Interfaces
{
    public interface IProductRepository
    {
        Task<IEnumerable<Product>> GetAllAsync();
        Task InsertarAsync(Product product);
    }
}
