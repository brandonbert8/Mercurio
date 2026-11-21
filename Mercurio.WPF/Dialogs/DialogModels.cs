namespace Mercurio.WPF.Dialogs
{
    public sealed record ProductoFormData(
        string Nombre,
        string CodigoBarras,
        decimal PrecioCompra,
        decimal PrecioVenta,
        int StockMinimo,
        int StockInicial,
        bool EsServicio);

    public sealed record ClienteFormData(
        string Nombre,
        string NumeroDocumento,
        string Telefono,
        decimal LimiteCredito);

    public sealed record ProveedorFormData(
        string Nombre,
        string Contacto,
        string Telefono,
        string Ruc,
        string Email);

    /// <summary>Datos para mostrar el resumen de una venta antes de confirmar.</summary>
    public class ResumenVentaDialog
    {
        public List<ItemVentaResumen> Items { get; set; } = [];
        public decimal Subtotal { get; set; }
        public decimal Descuento { get; set; }
        public decimal Total { get; set; }
        public decimal MontoPagado { get; set; }
        public decimal Vuelto { get; set; }
        public string MetodoPago { get; set; } = string.Empty;
        public string Cliente { get; set; } = string.Empty;
    }

    public class ItemVentaResumen
    {
        public string Nombre { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Total { get; set; }
    }

    /// <summary>Datos para mostrar el cuadre al cerrar la caja.</summary>
    public class ResumenCierreCajaDialog
    {
        public decimal MontoInicial { get; set; }
        public decimal VentasEfectivo { get; set; }
        public decimal CobrosEfectivo { get; set; }
        public decimal Ingresos { get; set; }
        public decimal Egresos { get; set; }
        public decimal MontoEsperado { get; set; }
        public decimal MontoDeclarado { get; set; }
        public decimal Diferencia { get; set; }
    }
}
