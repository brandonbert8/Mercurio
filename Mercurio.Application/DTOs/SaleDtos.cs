using Mercurio.Domain.Entities;
using Mercurio.Domain.Enums;

namespace Mercurio.Application.DTOs
{
    public class VentaItemRequest
    {
        public string ProductoId { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal? PrecioUnitario { get; set; }
        public decimal Descuento { get; set; }
        public decimal? ImpuestoPorcentaje { get; set; }
    }

    public class VentaCreateRequest
    {
        public string SucursalId { get; set; } = string.Empty;
        public string AlmacenId { get; set; } = string.Empty;
        public string UsuarioId { get; set; } = string.Empty;
        public string? ClienteId { get; set; }
        public string? CajaSesionId { get; set; }
        public TipoDocumento TipoDocumento { get; set; } = TipoDocumento.Ticket;
        public List<VentaItemRequest> Items { get; set; } = new();
        public decimal Descuento { get; set; }
        public MetodoPago MetodoPago { get; set; } = MetodoPago.Efectivo;
        public decimal MontoPagado { get; set; }
        public int DiasCredito { get; set; } = 30;
        public string Observacion { get; set; } = string.Empty;
    }

    public class VentaResult
    {
        public Venta Venta { get; set; } = new();
        public List<VentaDetalle> Detalles { get; set; } = new();
        public Pago? Pago { get; set; }
        public CuentaPorCobrar? CuentaPorCobrar { get; set; }
        public decimal Vuelto { get; set; }
    }
}
