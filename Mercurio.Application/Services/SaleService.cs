using Mercurio.Application.Common;
using Mercurio.Application.DTOs;
using Mercurio.Application.Interfaces;
using Mercurio.Domain.Entities;
using Mercurio.Domain.Enums;
using Mercurio.Domain.Interfaces;

namespace Mercurio.Application.Services
{
    public class SaleService : ISaleService
    {
        private readonly IProductRepository _productRepository;
        private readonly ISaleRepository _saleRepository;
        private readonly IInventoryRepository _inventoryRepository;
        private readonly IClienteRepository _clienteRepository;
        private readonly ICreditoRepository _creditoRepository;

        public SaleService(
            IProductRepository productRepository,
            ISaleRepository saleRepository,
            IInventoryRepository inventoryRepository,
            IClienteRepository clienteRepository,
            ICreditoRepository creditoRepository)
        {
            _productRepository = productRepository;
            _saleRepository = saleRepository;
            _inventoryRepository = inventoryRepository;
            _clienteRepository = clienteRepository;
            _creditoRepository = creditoRepository;
        }

        public async Task<IEnumerable<Product>> GetCatalogAsync()
        {
            return await _productRepository.GetActivosAsync();
        }

        public async Task<IEnumerable<Product>> ObtenerInventarioAsync()
        {
            return await _productRepository.GetActivosAsync();
        }

        public async Task CreateProductAsync(Product product)
        {
            if (string.IsNullOrWhiteSpace(product.CodigoBarras))
                product.CodigoBarras = $"P{Guid.NewGuid():N}"[..9].ToUpperInvariant();

            await _productRepository.InsertarAsync(product);
        }

        public async Task<VentaResult> RegistrarVentaAsync(VentaCreateRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.SucursalId))
                throw new BusinessException("La sucursal es obligatoria.");
            if (string.IsNullOrWhiteSpace(request.AlmacenId))
                throw new BusinessException("El almacén es obligatorio.");
            if (string.IsNullOrWhiteSpace(request.UsuarioId))
                throw new BusinessException("El usuario es obligatorio.");
            if (request.Items.Count == 0)
                throw new BusinessException("La venta debe tener al menos un producto.");

            var almacen = await _inventoryRepository.GetAlmacenByIdAsync(request.AlmacenId);
            if (almacen is null || !almacen.Activo)
                throw new BusinessException("El almacén no existe o está inactivo.");

            Cliente? cliente = null;
            if (!string.IsNullOrWhiteSpace(request.ClienteId))
            {
                cliente = await _clienteRepository.GetByIdAsync(request.ClienteId);
                if (cliente is null || !cliente.Activo)
                    throw new BusinessException("El cliente no existe o está inactivo.");
            }

            var ventaId = Guid.NewGuid().ToString();
            var detalles = new List<VentaDetalle>();
            var cantidadesPorProducto = new Dictionary<string, int>();
            var productos = new Dictionary<string, Product>();
            decimal subTotal = 0;
            decimal impuesto = 0;

            foreach (var item in request.Items)
            {
                if (item.Cantidad <= 0)
                    throw new BusinessException("Todas las cantidades deben ser mayores a cero.");
                if (item.Descuento < 0)
                    throw new BusinessException("El descuento no puede ser negativo.");

                if (!productos.TryGetValue(item.ProductoId, out var producto))
                {
                    producto = await _productRepository.GetByIdAsync(item.ProductoId);
                    if (producto is null || !producto.Activo)
                        throw new BusinessException($"El producto {item.ProductoId} no existe o está inactivo.");
                    productos[item.ProductoId] = producto;
                }

                var precio = item.PrecioUnitario ?? producto.PrecioVenta;
                if (precio < 0)
                    throw new BusinessException($"El precio del producto {producto.Nombre} no puede ser negativo.");

                var importe = Money.Round(item.Cantidad * precio - item.Descuento);
                if (importe < 0)
                    throw new BusinessException($"El descuento del producto {producto.Nombre} supera el importe.");

                var tasa = item.ImpuestoPorcentaje ?? producto.ImpuestoPorcentaje;
                var impuestoLinea = Money.Round(importe * tasa / 100m);

                subTotal += importe;
                impuesto += impuestoLinea;

                cantidadesPorProducto[producto.Id] = cantidadesPorProducto.GetValueOrDefault(producto.Id) + item.Cantidad;

                detalles.Add(new VentaDetalle
                {
                    VentaId = ventaId,
                    ProductoId = producto.Id,
                    ProductoNombre = producto.Nombre,
                    Cantidad = item.Cantidad,
                    PrecioUnitario = precio,
                    Descuento = item.Descuento,
                    ImpuestoPorcentaje = tasa,
                    SubTotal = Money.Round(importe + impuestoLinea)
                });
            }

            foreach (var par in cantidadesPorProducto)
            {
                var producto = productos[par.Key];
                if (producto.EsServicio)
                    continue;

                var stock = await _inventoryRepository.GetStockProductoAsync(request.AlmacenId, producto.Id);
                if (stock is null || stock.Cantidad < par.Value)
                    throw new BusinessException($"Stock insuficiente para el producto {producto.Nombre}.");
            }

            subTotal = Money.Round(subTotal);
            impuesto = Money.Round(impuesto);
            if (request.Descuento < 0)
                throw new BusinessException("El descuento global no puede ser negativo.");

            var total = Money.Round(subTotal + impuesto - request.Descuento);
            if (total < 0)
                throw new BusinessException("El descuento global supera el total de la venta.");

            var esCredito = request.MetodoPago == MetodoPago.Credito;
            if (esCredito && cliente is null)
                throw new BusinessException("Las ventas al crédito requieren un cliente.");

            if (!esCredito && request.MontoPagado < total)
                throw new BusinessException("El monto pagado es menor al total de la venta.");

            var montoAplicado = Math.Min(Math.Max(request.MontoPagado, 0), total);
            var saldo = esCredito ? Money.Round(total - montoAplicado) : 0m;

            CuentaPorCobrar? cuenta = null;

            if (saldo > 0 && cliente is not null)
            {
                var saldoActual = await _creditoRepository.GetSaldoClienteAsync(cliente.Id);
                if (cliente.LimiteCredito <= 0 || saldoActual + saldo > cliente.LimiteCredito)
                    throw new BusinessException("El cliente no tiene crédito disponible suficiente.");

                var dias = request.DiasCredito <= 0 ? 30 : request.DiasCredito;
                cuenta = new CuentaPorCobrar
                {
                    ClienteId = cliente.Id,
                    VentaId = ventaId,
                    FechaEmision = DateTime.Now,
                    FechaVencimiento = DateTime.Now.AddDays(dias),
                    MontoOriginal = saldo,
                    Saldo = saldo,
                    Estado = montoAplicado > 0 ? EstadoCuentaCobrar.Parcial : EstadoCuentaCobrar.Pendiente,
                    Observacion = request.Observacion ?? string.Empty
                };
            }

            var numero = await _saleRepository.GenerarNumeroAsync(request.SucursalId, DateTime.Now);

            var venta = new Venta
            {
                Id = ventaId,
                SucursalId = request.SucursalId,
                AlmacenId = request.AlmacenId,
                UsuarioId = request.UsuarioId,
                ClienteId = cliente?.Id,
                CajaSesionId = request.CajaSesionId,
                NumeroDocumento = numero,
                TipoDocumento = request.TipoDocumento,
                Fecha = DateTime.Now,
                SubTotal = subTotal,
                Descuento = request.Descuento,
                Impuesto = impuesto,
                Total = total,
                Estado = saldo > 0 ? EstadoVenta.AlCredito : EstadoVenta.Pagada,
                Observacion = request.Observacion ?? string.Empty
            };

            Pago? pago = null;
            if (montoAplicado > 0)
            {
                pago = new Pago
                {
                    VentaId = ventaId,
                    ClienteId = cliente?.Id,
                    CajaSesionId = request.CajaSesionId,
                    Tipo = TipoPago.Venta,
                    MetodoPago = request.MetodoPago == MetodoPago.Credito ? MetodoPago.Efectivo : request.MetodoPago,
                    Monto = montoAplicado,
                    Referencia = numero,
                    UsuarioId = request.UsuarioId,
                    Fecha = DateTime.Now,
                    Observacion = request.Observacion ?? string.Empty
                };
            }

            await _saleRepository.CrearAsync(venta, detalles, pago, cuenta);

            return new VentaResult
            {
                Venta = venta,
                Detalles = detalles,
                Pago = pago,
                CuentaPorCobrar = cuenta,
                Vuelto = esCredito ? 0 : Money.Round(Math.Max(request.MontoPagado - total, 0))
            };
        }

        public async Task AnularVentaAsync(string ventaId, string usuarioId, string motivo)
        {
            var venta = await _saleRepository.GetByIdAsync(ventaId);
            if (venta is null)
                throw new BusinessException("La venta no existe.");
            if (venta.Estado == EstadoVenta.Anulada)
                throw new BusinessException("La venta ya está anulada.");
            if (string.IsNullOrWhiteSpace(motivo))
                throw new BusinessException("Debe indicar el motivo de la anulación.");

            await _saleRepository.AnularAsync(ventaId, usuarioId, motivo);
            await _creditoRepository.AnularPorVentaAsync(ventaId);
        }

        public Task<Venta?> GetVentaAsync(string id)
        {
            return _saleRepository.GetByIdAsync(id);
        }

        public Task<IEnumerable<Venta>> GetVentasAsync(string sucursalId, DateTime desde, DateTime hasta)
        {
            return _saleRepository.GetByRangoAsync(sucursalId, desde, hasta);
        }

        public Task<IEnumerable<VentaDetalle>> GetDetallesAsync(string ventaId)
        {
            return _saleRepository.GetDetallesAsync(ventaId);
        }
    }
}
