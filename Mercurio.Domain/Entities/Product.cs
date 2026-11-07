namespace Mercurio.Domain.Entities
{
    public class Product
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string CodigoBarras { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string? CategoriaId { get; set; }
        public string UnidadMedida { get; set; } = "UNIDAD";
        public decimal PrecioCompra { get; set; }
        public decimal PrecioVenta { get; set; }
        public decimal ImpuestoPorcentaje { get; set; }
        public int StockActual { get; set; }
        public int StockMinimo { get; set; }
        public bool EsServicio { get; set; }
        public bool Activo { get; set; } = true;
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
    }
}
