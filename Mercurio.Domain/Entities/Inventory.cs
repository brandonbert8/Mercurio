using Mercurio.Domain.Enums;

namespace Mercurio.Domain.Entities
{
    public class Almacen
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string SucursalId { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Codigo { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public bool Activo { get; set; } = true;
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
    }

    public class StockAlmacen
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string AlmacenId { get; set; } = string.Empty;
        public string ProductoId { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public int StockMinimo { get; set; }
        public DateTime FechaActualizacion { get; set; } = DateTime.Now;
    }

    public class MovimientoInventario
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string AlmacenId { get; set; } = string.Empty;
        public string? AlmacenDestinoId { get; set; }
        public string ProductoId { get; set; } = string.Empty;
        public TipoMovimientoInventario Tipo { get; set; }
        public int Cantidad { get; set; }
        public decimal CostoUnitario { get; set; }
        public string Referencia { get; set; } = string.Empty;
        public string Observacion { get; set; } = string.Empty;
        public string? UsuarioId { get; set; }
        public DateTime Fecha { get; set; } = DateTime.Now;
    }
}
