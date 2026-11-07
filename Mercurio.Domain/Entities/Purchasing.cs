using Mercurio.Domain.Enums;

namespace Mercurio.Domain.Entities
{
    public class Proveedor
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string NegocioId { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Ruc { get; set; } = string.Empty;
        public string Contacto { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public bool Activo { get; set; } = true;
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
    }

    public class Compra
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string SucursalId { get; set; } = string.Empty;
        public string ProveedorId { get; set; } = string.Empty;
        public string AlmacenId { get; set; } = string.Empty;
        public string UsuarioId { get; set; } = string.Empty;
        public string NumeroDocumento { get; set; } = string.Empty;
        public TipoDocumento TipoDocumento { get; set; } = TipoDocumento.ComprobanteCompra;
        public DateTime Fecha { get; set; } = DateTime.Now;
        public decimal SubTotal { get; set; }
        public decimal Impuesto { get; set; }
        public decimal Total { get; set; }
        public decimal MontoPagado { get; set; }
        public MetodoPago MetodoPago { get; set; } = MetodoPago.Efectivo;
        public EstadoCompra Estado { get; set; } = EstadoCompra.Registrada;
        public string Observacion { get; set; } = string.Empty;
    }

    public class CompraDetalle
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string CompraId { get; set; } = string.Empty;
        public string ProductoId { get; set; } = string.Empty;
        public string ProductoNombre { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal CostoUnitario { get; set; }
        public decimal Descuento { get; set; }
        public decimal ImpuestoPorcentaje { get; set; }
        public decimal SubTotal { get; set; }
    }
}
