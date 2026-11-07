namespace Mercurio.Domain.Enums
{
    public enum RolUsuario
    {
        Administrador = 1,
        Supervisor = 2,
        Cajero = 3,
        Almacenero = 4,
        Contador = 5
    }

    public enum EstadoVenta
    {
        Borrador = 0,
        Registrada = 1,
        Pagada = 2,
        AlCredito = 3,
        Anulada = 4
    }

    public enum EstadoCompra
    {
        Borrador = 0,
        Registrada = 1,
        Recibida = 2,
        Anulada = 3
    }

    public enum TipoMovimientoInventario
    {
        Entrada = 1,
        Salida = 2,
        Ajuste = 3,
        TransferenciaSalida = 4,
        TransferenciaEntrada = 5
    }

    public enum MetodoPago
    {
        Efectivo = 1,
        Tarjeta = 2,
        Transferencia = 3,
        Credito = 4,
        Otro = 5
    }

    public enum TipoPago
    {
        Venta = 1,
        Compra = 2,
        Cobro = 3
    }

    public enum EstadoCuentaCobrar
    {
        Pendiente = 1,
        Parcial = 2,
        Pagada = 3,
        Vencida = 4,
        Anulada = 5
    }

    public enum EstadoCajaSesion
    {
        Abierta = 1,
        Cerrada = 2
    }

    public enum TipoMovimientoCaja
    {
        Apertura = 1,
        Ingreso = 2,
        Egreso = 3,
        Venta = 4,
        Cobro = 5,
        Cierre = 6
    }

    public enum TipoDocumento
    {
        Ticket = 1,
        Boleta = 2,
        Factura = 3,
        NotaCredito = 4,
        ComprobanteCompra = 5
    }
}
