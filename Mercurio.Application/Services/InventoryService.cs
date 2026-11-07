using Mercurio.Application.Common;
using Mercurio.Application.Interfaces;
using Mercurio.Domain.Entities;
using Mercurio.Domain.Enums;
using Mercurio.Domain.Interfaces;

namespace Mercurio.Application.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly IInventoryRepository _inventoryRepository;
        private readonly IProductRepository _productRepository;

        public InventoryService(IInventoryRepository inventoryRepository, IProductRepository productRepository)
        {
            _inventoryRepository = inventoryRepository;
            _productRepository = productRepository;
        }

        public Task<IEnumerable<Almacen>> GetAlmacenesAsync(string sucursalId)
        {
            return _inventoryRepository.GetAlmacenesAsync(sucursalId);
        }

        public Task<Almacen?> GetAlmacenAsync(string id)
        {
            return _inventoryRepository.GetAlmacenByIdAsync(id);
        }

        public async Task<Almacen> CrearAlmacenAsync(Almacen almacen)
        {
            if (string.IsNullOrWhiteSpace(almacen.SucursalId))
                throw new BusinessException("La sucursal es obligatoria.");
            if (string.IsNullOrWhiteSpace(almacen.Nombre))
                throw new BusinessException("El nombre del almacén es obligatorio.");
            if (string.IsNullOrWhiteSpace(almacen.Codigo))
                throw new BusinessException("El código del almacén es obligatorio.");

            await _inventoryRepository.InsertarAlmacenAsync(almacen);
            return almacen;
        }

        public async Task ActualizarAlmacenAsync(Almacen almacen)
        {
            var existente = await _inventoryRepository.GetAlmacenByIdAsync(almacen.Id);
            if (existente is null)
                throw new BusinessException("El almacén no existe.");

            if (string.IsNullOrWhiteSpace(almacen.Nombre))
                throw new BusinessException("El nombre del almacén es obligatorio.");

            await _inventoryRepository.ActualizarAlmacenAsync(almacen);
        }

        public Task<IEnumerable<StockAlmacen>> GetStockAsync(string almacenId)
        {
            return _inventoryRepository.GetStockAsync(almacenId);
        }

        public Task<StockAlmacen?> GetStockProductoAsync(string almacenId, string productoId)
        {
            return _inventoryRepository.GetStockProductoAsync(almacenId, productoId);
        }

        public Task<int> GetStockTotalAsync(string productoId)
        {
            return _inventoryRepository.GetStockTotalAsync(productoId);
        }

        public Task<IEnumerable<StockAlmacen>> GetStockBajoMinimoAsync(string? almacenId = null)
        {
            return _inventoryRepository.GetStockBajoMinimoAsync(almacenId);
        }

        public async Task<MovimientoInventario> RegistrarEntradaAsync(string almacenId, string productoId, int cantidad, decimal costoUnitario, string referencia, string? usuarioId, string observacion = "")
        {
            if (cantidad <= 0)
                throw new BusinessException("La cantidad de entrada debe ser mayor a cero.");
            if (costoUnitario < 0)
                throw new BusinessException("El costo unitario no puede ser negativo.");

            await ValidarAlmacenYProductoAsync(almacenId, productoId);

            var movimiento = new MovimientoInventario
            {
                AlmacenId = almacenId,
                ProductoId = productoId,
                Tipo = TipoMovimientoInventario.Entrada,
                Cantidad = cantidad,
                CostoUnitario = costoUnitario,
                Referencia = referencia ?? string.Empty,
                Observacion = observacion ?? string.Empty,
                UsuarioId = usuarioId
            };

            await _inventoryRepository.RegistrarMovimientoAsync(movimiento);
            return movimiento;
        }

        public async Task<MovimientoInventario> RegistrarSalidaAsync(string almacenId, string productoId, int cantidad, string referencia, string? usuarioId, string observacion = "")
        {
            if (cantidad <= 0)
                throw new BusinessException("La cantidad de salida debe ser mayor a cero.");

            await ValidarAlmacenYProductoAsync(almacenId, productoId);

            var stock = await _inventoryRepository.GetStockProductoAsync(almacenId, productoId);
            if (stock is null || stock.Cantidad < cantidad)
                throw new BusinessException("No hay stock suficiente en el almacén para realizar la salida.");

            var movimiento = new MovimientoInventario
            {
                AlmacenId = almacenId,
                ProductoId = productoId,
                Tipo = TipoMovimientoInventario.Salida,
                Cantidad = cantidad,
                Referencia = referencia ?? string.Empty,
                Observacion = observacion ?? string.Empty,
                UsuarioId = usuarioId
            };

            await _inventoryRepository.RegistrarMovimientoAsync(movimiento);
            return movimiento;
        }

        public async Task<MovimientoInventario> RegistrarAjusteAsync(string almacenId, string productoId, int cantidadFinal, string? usuarioId, string observacion = "")
        {
            if (cantidadFinal < 0)
                throw new BusinessException("La cantidad final no puede ser negativa.");

            await ValidarAlmacenYProductoAsync(almacenId, productoId);

            var stock = await _inventoryRepository.GetStockProductoAsync(almacenId, productoId);
            var actual = stock?.Cantidad ?? 0;
            var diferencia = cantidadFinal - actual;

            if (diferencia == 0)
                throw new BusinessException("El ajuste no genera cambios en el stock.");

            var movimiento = new MovimientoInventario
            {
                AlmacenId = almacenId,
                ProductoId = productoId,
                Tipo = TipoMovimientoInventario.Ajuste,
                Cantidad = diferencia,
                Referencia = "AJUSTE",
                Observacion = observacion ?? string.Empty,
                UsuarioId = usuarioId
            };

            await _inventoryRepository.RegistrarMovimientoAsync(movimiento);
            return movimiento;
        }

        public async Task TransferirAsync(string almacenOrigenId, string almacenDestinoId, string productoId, int cantidad, string? usuarioId, string observacion = "")
        {
            if (cantidad <= 0)
                throw new BusinessException("La cantidad a transferir debe ser mayor a cero.");
            if (almacenOrigenId == almacenDestinoId)
                throw new BusinessException("El almacén de origen y destino deben ser diferentes.");

            await ValidarAlmacenYProductoAsync(almacenOrigenId, productoId);
            var destino = await _inventoryRepository.GetAlmacenByIdAsync(almacenDestinoId);
            if (destino is null)
                throw new BusinessException("El almacén de destino no existe.");

            var stock = await _inventoryRepository.GetStockProductoAsync(almacenOrigenId, productoId);
            if (stock is null || stock.Cantidad < cantidad)
                throw new BusinessException("No hay stock suficiente en el almacén de origen.");

            var salida = new MovimientoInventario
            {
                AlmacenId = almacenOrigenId,
                AlmacenDestinoId = almacenDestinoId,
                ProductoId = productoId,
                Tipo = TipoMovimientoInventario.TransferenciaSalida,
                Cantidad = cantidad,
                Referencia = "TRANSFERENCIA",
                Observacion = observacion ?? string.Empty,
                UsuarioId = usuarioId
            };

            var entrada = new MovimientoInventario
            {
                AlmacenId = almacenDestinoId,
                AlmacenDestinoId = almacenOrigenId,
                ProductoId = productoId,
                Tipo = TipoMovimientoInventario.TransferenciaEntrada,
                Cantidad = cantidad,
                Referencia = "TRANSFERENCIA",
                Observacion = observacion ?? string.Empty,
                UsuarioId = usuarioId
            };

            await _inventoryRepository.TransferirAsync(salida, entrada);
        }

        public Task<IEnumerable<MovimientoInventario>> GetMovimientosAsync(string? almacenId, DateTime desde, DateTime hasta)
        {
            return _inventoryRepository.GetMovimientosAsync(almacenId, desde, hasta);
        }

        private async Task ValidarAlmacenYProductoAsync(string almacenId, string productoId)
        {
            var almacen = await _inventoryRepository.GetAlmacenByIdAsync(almacenId);
            if (almacen is null || !almacen.Activo)
                throw new BusinessException("El almacén no existe o está inactivo.");

            var producto = await _productRepository.GetByIdAsync(productoId);
            if (producto is null || !producto.Activo)
                throw new BusinessException("El producto no existe o está inactivo.");
        }
    }
}
