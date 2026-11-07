using Mercurio.Domain.Entities;
using Mercurio.Domain.Enums;

namespace Mercurio.Application.DTOs
{
    public class RegistrarAbonoRequest
    {
        public string CuentaPorCobrarId { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public MetodoPago MetodoPago { get; set; } = MetodoPago.Efectivo;
        public string Referencia { get; set; } = string.Empty;
        public string? UsuarioId { get; set; }
        public string? CajaSesionId { get; set; }
        public string Observacion { get; set; } = string.Empty;
    }

    public class AbonoResult
    {
        public AbonoCobro Abono { get; set; } = new();
        public CuentaPorCobrar Cuenta { get; set; } = new();
    }
}
