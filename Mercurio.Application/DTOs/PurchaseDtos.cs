using Mercurio.Domain.Entities;
using Mercurio.Domain.Enums;

namespace Mercurio.Application.DTOs
{
    public class CompraItemRequest
    {
        public string ProductoId { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal CostoUnitario { get; set; }
        public decimal Descuento { get; set; }
        public decimal? ImpuestoPorcentaje { get; set; }
    }

    public class CompraCreateRequest
    {
        public string SucursalId { get; set; } = string.Empty;
        public string AlmacenId { get; set; } = string.Empty;
        public string ProveedorId { get; set; } = string.Empty;
        public string UsuarioId { get; set; } = string.Empty;
        public TipoDocumento TipoDocumento { get; set; } = TipoDocumento.ComprobanteCompra;
        public string NumeroDocumento { get; set; } = string.Empty;
        public List<CompraItemRequest> Items { get; set; } = new();
        public MetodoPago MetodoPago { get; set; } = MetodoPago.Efectivo;
        public decimal MontoPagado { get; set; }
        public string Observacion { get; set; } = string.Empty;
    }

    public class CompraResult
    {
        public Compra Compra { get; set; } = new();
        public List<CompraDetalle> Detalles { get; set; } = new();
    }
}
