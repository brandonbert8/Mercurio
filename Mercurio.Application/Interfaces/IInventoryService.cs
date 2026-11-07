using Mercurio.Domain.Entities;

namespace Mercurio.Application.Interfaces
{
    public interface IInventoryService
    {
        Task<IEnumerable<Almacen>> GetAlmacenesAsync(string sucursalId);
        Task<Almacen?> GetAlmacenAsync(string id);
        Task<Almacen> CrearAlmacenAsync(Almacen almacen);
        Task ActualizarAlmacenAsync(Almacen almacen);

        Task<IEnumerable<StockAlmacen>> GetStockAsync(string almacenId);
        Task<StockAlmacen?> GetStockProductoAsync(string almacenId, string productoId);
        Task<int> GetStockTotalAsync(string productoId);
        Task<IEnumerable<StockAlmacen>> GetStockBajoMinimoAsync(string? almacenId = null);

        Task<MovimientoInventario> RegistrarEntradaAsync(string almacenId, string productoId, int cantidad, decimal costoUnitario, string referencia, string? usuarioId, string observacion = "");
        Task<MovimientoInventario> RegistrarSalidaAsync(string almacenId, string productoId, int cantidad, string referencia, string? usuarioId, string observacion = "");
        Task<MovimientoInventario> RegistrarAjusteAsync(string almacenId, string productoId, int cantidadFinal, string? usuarioId, string observacion = "");
        Task TransferirAsync(string almacenOrigenId, string almacenDestinoId, string productoId, int cantidad, string? usuarioId, string observacion = "");

        Task<IEnumerable<MovimientoInventario>> GetMovimientosAsync(string? almacenId, DateTime desde, DateTime hasta);
    }
}
