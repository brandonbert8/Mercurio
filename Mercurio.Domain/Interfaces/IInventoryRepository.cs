using Mercurio.Domain.Entities;

namespace Mercurio.Domain.Interfaces
{
    public interface IInventoryRepository
    {
        Task<IEnumerable<Almacen>> GetAlmacenesAsync(string sucursalId);
        Task<Almacen?> GetAlmacenByIdAsync(string id);
        Task InsertarAlmacenAsync(Almacen almacen);
        Task ActualizarAlmacenAsync(Almacen almacen);

        Task<IEnumerable<StockAlmacen>> GetStockAsync(string almacenId);
        Task<StockAlmacen?> GetStockProductoAsync(string almacenId, string productoId);
        Task<int> GetStockTotalAsync(string productoId);
        Task<IEnumerable<StockAlmacen>> GetStockBajoMinimoAsync(string? almacenId = null);

        Task RegistrarMovimientoAsync(MovimientoInventario movimiento);
        Task TransferirAsync(MovimientoInventario salida, MovimientoInventario entrada);
        Task<IEnumerable<MovimientoInventario>> GetMovimientosAsync(string? almacenId, DateTime desde, DateTime hasta);
    }
}
