using System;
using System.Collections.Generic;
using System.Text;

namespace Mercurio.Domain.Entities
{
    public class Product
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string CodigoBarras { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public decimal PrecioCompra { get; set; }
        public decimal PrecioVenta { get; set; }
        public int StockActual { get; set; }
        public bool Activo { get; set; } = true;

    }
}
