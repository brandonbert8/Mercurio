using Mercurio.Domain.Enums;

namespace Mercurio.Domain.Entities
{
    public class Caja
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string SucursalId { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public bool Activa { get; set; } = true;
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
    }

    public class CajaSesion
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string CajaId { get; set; } = string.Empty;
        public string UsuarioId { get; set; } = string.Empty;
        public DateTime FechaApertura { get; set; } = DateTime.Now;
        public DateTime? FechaCierre { get; set; }
        public decimal MontoInicial { get; set; }
        public decimal MontoEsperado { get; set; }
        public decimal MontoDeclarado { get; set; }
        public decimal Diferencia { get; set; }
        public EstadoCajaSesion Estado { get; set; } = EstadoCajaSesion.Abierta;
        public string Observacion { get; set; } = string.Empty;
    }

    public class CajaMovimiento
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string CajaSesionId { get; set; } = string.Empty;
        public TipoMovimientoCaja Tipo { get; set; } = TipoMovimientoCaja.Ingreso;
        public string Concepto { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public DateTime Fecha { get; set; } = DateTime.Now;
        public string? UsuarioId { get; set; }
        public string Referencia { get; set; } = string.Empty;
    }
}
