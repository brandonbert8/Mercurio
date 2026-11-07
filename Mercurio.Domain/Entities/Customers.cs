using Mercurio.Domain.Enums;

namespace Mercurio.Domain.Entities
{
    public class Cliente
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string NegocioId { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string TipoDocumento { get; set; } = "DNI";
        public string NumeroDocumento { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public decimal LimiteCredito { get; set; }
        public bool Activo { get; set; } = true;
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
    }

    public class CuentaPorCobrar
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string ClienteId { get; set; } = string.Empty;
        public string VentaId { get; set; } = string.Empty;
        public DateTime FechaEmision { get; set; } = DateTime.Now;
        public DateTime FechaVencimiento { get; set; }
        public decimal MontoOriginal { get; set; }
        public decimal Saldo { get; set; }
        public EstadoCuentaCobrar Estado { get; set; } = EstadoCuentaCobrar.Pendiente;
        public string Observacion { get; set; } = string.Empty;
    }

    public class AbonoCobro
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string CuentaPorCobrarId { get; set; } = string.Empty;
        public string ClienteId { get; set; } = string.Empty;
        public DateTime Fecha { get; set; } = DateTime.Now;
        public decimal Monto { get; set; }
        public MetodoPago MetodoPago { get; set; } = MetodoPago.Efectivo;
        public string Referencia { get; set; } = string.Empty;
        public string? UsuarioId { get; set; }
        public string? CajaSesionId { get; set; }
        public string Observacion { get; set; } = string.Empty;
    }
}
