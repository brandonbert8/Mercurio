using Mercurio.Application.Common;
using Mercurio.Application.DTOs;
using Mercurio.Application.Interfaces;
using Mercurio.Domain.Entities;
using Mercurio.Domain.Enums;
using Mercurio.Domain.Interfaces;

namespace Mercurio.Application.Services
{
    public class CompraService : ICompraService
    {
        private readonly ICompraRepository _compraRepository;
        private readonly IProveedorRepository _proveedorRepository;
        private readonly IProductRepository _productRepository;
        private readonly IInventoryRepository _inventoryRepository;

        public CompraService(
            ICompraRepository compraRepository,
            IProveedorRepository proveedorRepository,
            IProductRepository productRepository,
            IInventoryRepository inventoryRepository)
        {
            _compraRepository = compraRepository;
            _proveedorRepository = proveedorRepository;
            _productRepository = productRepository;
            _inventoryRepository = inventoryRepository;
        }

        public async Task<CompraResult> RegistrarCompraAsync(CompraCreateRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.SucursalId))
                throw new BusinessException("La sucursal es obligatoria.");
            if (string.IsNullOrWhiteSpace(request.UsuarioId))
                throw new BusinessException("El usuario es obligatorio.");
            if (string.IsNullOrWhiteSpace(request.ProveedorId))
                throw new BusinessException("El proveedor es obligatorio.");
            if (string.IsNullOrWhiteSpace(request.AlmacenId))
                throw new BusinessException("El almacén es obligatorio.");
            if (request.Items.Count == 0)
                throw new BusinessException("La compra debe tener al menos un producto.");

            var proveedor = await _proveedorRepository.GetByIdAsync(request.ProveedorId);
            if (proveedor is null || !proveedor.Activo)
                throw new BusinessException("El proveedor no existe o está inactivo.");

            var almacen = await _inventoryRepository.GetAlmacenByIdAsync(request.AlmacenId);
            if (almacen is null || !almacen.Activo)
                throw new BusinessException("El almacén no existe o está inactivo.");

            var compraId = Guid.NewGuid().ToString();
            var detalles = new List<CompraDetalle>();
            decimal subTotal = 0;
            decimal impuesto = 0;

            foreach (var item in request.Items)
            {
                if (item.Cantidad <= 0)
                    throw new BusinessException("Todas las cantidades deben ser mayores a cero.");
                if (item.CostoUnitario < 0)
                    throw new BusinessException("El costo unitario no puede ser negativo.");
                if (item.Descuento < 0)
                    throw new BusinessException("El descuento no puede ser negativo.");

                var producto = await _productRepository.GetByIdAsync(item.ProductoId);
                if (producto is null || !producto.Activo)
                    throw new BusinessException($"El producto {item.ProductoId} no existe o está inactivo.");

                var importe = Money.Round(item.Cantidad * item.CostoUnitario - item.Descuento);
                if (importe < 0)
                    throw new BusinessException($"El descuento del producto {producto.Nombre} supera el importe.");

                var tasa = item.ImpuestoPorcentaje ?? producto.ImpuestoPorcentaje;
                var impuestoLinea = Money.Round(importe * tasa / 100m);

                subTotal += importe;
                impuesto += impuestoLinea;

                detalles.Add(new CompraDetalle
                {
                    CompraId = compraId,
                    ProductoId = producto.Id,
                    ProductoNombre = producto.Nombre,
                    Cantidad = item.Cantidad,
                    CostoUnitario = item.CostoUnitario,
                    Descuento = item.Descuento,
                    ImpuestoPorcentaje = tasa,
                    SubTotal = Money.Round(importe + impuestoLinea)
                });
            }

            subTotal = Money.Round(subTotal);
            impuesto = Money.Round(impuesto);
            var total = Money.Round(subTotal + impuesto);

            if (request.MontoPagado < 0)
                throw new BusinessException("El monto pagado no puede ser negativo.");

            var numero = string.IsNullOrWhiteSpace(request.NumeroDocumento)
                ? await _compraRepository.GenerarNumeroAsync(request.SucursalId, DateTime.Now)
                : request.NumeroDocumento.Trim();

            var compra = new Compra
            {
                Id = compraId,
                SucursalId = request.SucursalId,
                ProveedorId = request.ProveedorId,
                AlmacenId = request.AlmacenId,
                UsuarioId = request.UsuarioId,
                NumeroDocumento = numero,
                TipoDocumento = request.TipoDocumento,
                Fecha = DateTime.Now,
                SubTotal = subTotal,
                Impuesto = impuesto,
                Total = total,
                MontoPagado = Math.Min(request.MontoPagado, total),
                MetodoPago = request.MetodoPago,
                Estado = EstadoCompra.Recibida,
                Observacion = request.Observacion ?? string.Empty
            };

            await _compraRepository.CrearAsync(compra, detalles);

            return new CompraResult { Compra = compra, Detalles = detalles };
        }

        public async Task AnularCompraAsync(string compraId)
        {
            var compra = await _compraRepository.GetByIdAsync(compraId);
            if (compra is null)
                throw new BusinessException("La compra no existe.");
            if (compra.Estado == EstadoCompra.Anulada)
                throw new BusinessException("La compra ya está anulada.");

            await _compraRepository.AnularAsync(compraId);
        }

        public Task<Compra?> GetCompraAsync(string id)
        {
            return _compraRepository.GetByIdAsync(id);
        }

        public Task<IEnumerable<Compra>> GetComprasAsync(string sucursalId, DateTime desde, DateTime hasta)
        {
            return _compraRepository.GetByRangoAsync(sucursalId, desde, hasta);
        }

        public Task<IEnumerable<CompraDetalle>> GetDetallesAsync(string compraId)
        {
            return _compraRepository.GetDetallesAsync(compraId);
        }
    }
}
