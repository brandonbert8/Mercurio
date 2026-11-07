using Mercurio.Domain.Enums;

namespace Mercurio.Domain.Entities
{
    public class Venta
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string SucursalId { get; set; } = string.Empty;
        public string AlmacenId { get; set; } = string.Empty;
        public string UsuarioId { get; set; } = string.Empty;
        public string? ClienteId { get; set; }
        public string? CajaSesionId { get; set; }
        public string NumeroDocumento { get; set; } = string.Empty;
        public TipoDocumento TipoDocumento { get; set; } = TipoDocumento.Ticket;
        public DateTime Fecha { get; set; } = DateTime.Now;
        public decimal SubTotal { get; set; }
        public decimal Descuento { get; set; }
        public decimal Impuesto { get; set; }
        public decimal Total { get; set; }
        public EstadoVenta Estado { get; set; } = EstadoVenta.Registrada;
        public string Observacion { get; set; } = string.Empty;
    }

    public class VentaDetalle
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string VentaId { get; set; } = string.Empty;
        public string ProductoId { get; set; } = string.Empty;
        public string ProductoNombre { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Descuento { get; set; }
        public decimal ImpuestoPorcentaje { get; set; }
        public decimal SubTotal { get; set; }
    }

    public class Pago
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string? VentaId { get; set; }
        public string? CompraId { get; set; }
        public string? CuentaPorCobrarId { get; set; }
        public string? ClienteId { get; set; }
        public string? CajaSesionId { get; set; }
        public TipoPago Tipo { get; set; } = TipoPago.Venta;
        public MetodoPago MetodoPago { get; set; } = MetodoPago.Efectivo;
        public decimal Monto { get; set; }
        public string Referencia { get; set; } = string.Empty;
        public string? UsuarioId { get; set; }
        public DateTime Fecha { get; set; } = DateTime.Now;
        public string Observacion { get; set; } = string.Empty;
    }
}
