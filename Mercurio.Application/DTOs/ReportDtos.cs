using Mercurio.Domain.Enums;

namespace Mercurio.Application.DTOs.Reports
{
    public class ResumenVentasDto
    {
        public DateTime Fecha { get; set; }
        public int CantidadVentas { get; set; }
        public decimal SubTotal { get; set; }
        public decimal Descuento { get; set; }
        public decimal Impuesto { get; set; }
        public decimal Total { get; set; }
    }

    public class VentasPorProductoDto
    {
        public string ProductoId { get; set; } = string.Empty;
        public string ProductoNombre { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal Total { get; set; }
    }

    public class VentasPorUsuarioDto
    {
        public string UsuarioId { get; set; } = string.Empty;
        public string UsuarioNombre { get; set; } = string.Empty;
        public int CantidadVentas { get; set; }
        public decimal Total { get; set; }
    }

    public class VentasPorMetodoPagoDto
    {
        public MetodoPago MetodoPago { get; set; }
        public int Cantidad { get; set; }
        public decimal Total { get; set; }
    }

    public class ValorizacionInventarioDto
    {
        public string AlmacenId { get; set; } = string.Empty;
        public string AlmacenNombre { get; set; } = string.Empty;
        public string ProductoId { get; set; } = string.Empty;
        public string ProductoNombre { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal CostoUnitario { get; set; }
        public decimal Valor { get; set; }
    }

    public class StockBajoDto
    {
        public string AlmacenId { get; set; } = string.Empty;
        public string AlmacenNombre { get; set; } = string.Empty;
        public string ProductoId { get; set; } = string.Empty;
        public string ProductoNombre { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public int StockMinimo { get; set; }
    }

    public class ResumenComprasDto
    {
        public DateTime Fecha { get; set; }
        public int CantidadCompras { get; set; }
        public decimal SubTotal { get; set; }
        public decimal Impuesto { get; set; }
        public decimal Total { get; set; }
    }

    public class CuentaPorCobrarReporteDto
    {
        public string CuentaId { get; set; } = string.Empty;
        public string ClienteId { get; set; } = string.Empty;
        public string Cliente { get; set; } = string.Empty;
        public string VentaId { get; set; } = string.Empty;
        public DateTime FechaEmision { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public decimal MontoOriginal { get; set; }
        public decimal Saldo { get; set; }
        public int DiasVencido { get; set; }
        public EstadoCuentaCobrar Estado { get; set; }
    }

    public class ResumenCajaDto
    {
        public string CajaSesionId { get; set; } = string.Empty;
        public string CajaNombre { get; set; } = string.Empty;
        public string UsuarioNombre { get; set; } = string.Empty;
        public DateTime FechaApertura { get; set; }
        public DateTime? FechaCierre { get; set; }
        public decimal MontoInicial { get; set; }
        public decimal Ingresos { get; set; }
        public decimal Egresos { get; set; }
        public decimal MontoDeclarado { get; set; }
        public decimal Diferencia { get; set; }
        public EstadoCajaSesion Estado { get; set; }
    }

    public class TopClienteDto
    {
        public string ClienteId { get; set; } = string.Empty;
        public string Cliente { get; set; } = string.Empty;
        public int CantidadVentas { get; set; }
        public decimal Total { get; set; }
    }

    public class VentasDiariasDto
    {
        public DateTime Fecha { get; set; }
        public int CantidadVentas { get; set; }
        public decimal Total { get; set; }
    }
}
