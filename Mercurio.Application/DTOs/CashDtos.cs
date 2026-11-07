using Mercurio.Domain.Entities;
using Mercurio.Domain.Enums;

namespace Mercurio.Application.DTOs
{
    public class AbrirCajaRequest
    {
        public string CajaId { get; set; } = string.Empty;
        public string UsuarioId { get; set; } = string.Empty;
        public decimal MontoInicial { get; set; }
        public string Observacion { get; set; } = string.Empty;
    }

    public class CerrarCajaRequest
    {
        public string CajaSesionId { get; set; } = string.Empty;
        public decimal MontoDeclarado { get; set; }
        public string Observacion { get; set; } = string.Empty;
    }

    public class MovimientoCajaRequest
    {
        public string CajaSesionId { get; set; } = string.Empty;
        public TipoMovimientoCaja Tipo { get; set; } = TipoMovimientoCaja.Ingreso;
        public string Concepto { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public string? UsuarioId { get; set; }
        public string Referencia { get; set; } = string.Empty;
    }

    public class CierreCajaResult
    {
        public CajaSesion Sesion { get; set; } = new();
        public decimal VentasEfectivo { get; set; }
        public decimal CobrosEfectivo { get; set; }
        public decimal Ingresos { get; set; }
        public decimal Egresos { get; set; }
        public decimal MontoEsperado { get; set; }
        public decimal Diferencia { get; set; }
    }
}
