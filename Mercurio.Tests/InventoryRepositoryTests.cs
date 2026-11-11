using Dapper;
using Mercurio.Domain.Entities;
using Mercurio.Domain.Enums;
using Mercurio.Infrastructure;
using Mercurio.Infrastructure.Repositories;
using Microsoft.Data.Sqlite;

namespace Mercurio.Tests;

public sealed class InventoryRepositoryTests : IDisposable
{
    private readonly string _dbPath;
    private readonly string _connectionString;
    private readonly InventoryRepository _inventory;
    private readonly ProductRepository _products;
    private readonly string _almacenA = Guid.NewGuid().ToString();
    private readonly string _almacenB = Guid.NewGuid().ToString();
    private readonly string _productoId = Guid.NewGuid().ToString();

    public InventoryRepositoryTests()
    {
        _dbPath = Path.Combine(Path.GetTempPath(), $"mercurio-tests-{Guid.NewGuid():N}.db");
        _connectionString = $"Data Source={_dbPath};Cache=Shared;Foreign Keys=True;Default Timeout=30";

        DatabaseInitializer.Initialize(_connectionString);

        _inventory = new InventoryRepository(_connectionString);
        _products = new ProductRepository(_connectionString);

        _products.InsertarAsync(new Product
        {
            Id = _productoId,
            CodigoBarras = $"TEST-{Guid.NewGuid():N}",
            Nombre = "Producto de prueba",
            PrecioCompra = 0m,
            PrecioVenta = 11.30m,
            ImpuestoPorcentaje = 13m,
            StockActual = 0,
            StockMinimo = 2,
            Activo = true,
            FechaCreacion = DateTime.Now
        }).GetAwaiter().GetResult();

        _inventory.InsertarAlmacenAsync(new Almacen
        {
            Id = _almacenA,
            SucursalId = "SUC-1",
            Nombre = "Almacén A",
            Codigo = "A"
        }).GetAwaiter().GetResult();

        _inventory.InsertarAlmacenAsync(new Almacen
        {
            Id = _almacenB,
            SucursalId = "SUC-1",
            Nombre = "Almacén B",
            Codigo = "B"
        }).GetAwaiter().GetResult();
    }

    [Fact]
    public async Task Entrada_InicializaStockCostoPromedioYStockDelProducto()
    {
        await RegistrarEntradaAsync(_almacenA, 10, 10m);

        var stock = await _inventory.GetStockProductoAsync(_almacenA, _productoId);
        var producto = await _products.GetByIdAsync(_productoId);

        Assert.NotNull(stock);
        Assert.Equal(10, stock!.Cantidad);
        Assert.Equal(10m, stock.CostoPromedio);
        Assert.Equal(10, producto!.StockActual);
    }

    [Fact]
    public async Task EntradasSucesivas_CalculanCostoPromedioPonderado()
    {
        await RegistrarEntradaAsync(_almacenA, 10, 10m);
        await RegistrarEntradaAsync(_almacenA, 10, 20m);

        var stock = await _inventory.GetStockProductoAsync(_almacenA, _productoId);
        var producto = await _products.GetByIdAsync(_productoId);

        Assert.Equal(20, stock!.Cantidad);
        Assert.Equal(15m, stock.CostoPromedio);
        Assert.Equal(15m, producto!.PrecioCompra);
    }

    [Fact]
    public async Task Salida_ReduceStockSinAlterarCostoPromedio()
    {
        await RegistrarEntradaAsync(_almacenA, 10, 10m);

        await RegistrarSalidaAsync(_almacenA, 4);

        var stock = await _inventory.GetStockProductoAsync(_almacenA, _productoId);
        var producto = await _products.GetByIdAsync(_productoId);

        Assert.Equal(6, stock!.Cantidad);
        Assert.Equal(10m, stock.CostoPromedio);
        Assert.Equal(6, producto!.StockActual);
    }

    [Fact]
    public async Task Salida_SinStockSuficiente_LanzaError()
    {
        await RegistrarEntradaAsync(_almacenA, 2, 10m);

        await Assert.ThrowsAsync<InvalidOperationException>(() => RegistrarSalidaAsync(_almacenA, 3));
    }

    [Fact]
    public async Task Ajuste_Negativo_ReduceStock()
    {
        await RegistrarEntradaAsync(_almacenA, 10, 10m);

        await _inventory.RegistrarMovimientoAsync(new MovimientoInventario
        {
            AlmacenId = _almacenA,
            ProductoId = _productoId,
            Tipo = TipoMovimientoInventario.Ajuste,
            Cantidad = -3,
            Referencia = "AJUSTE"
        });

        var stock = await _inventory.GetStockProductoAsync(_almacenA, _productoId);
        var producto = await _products.GetByIdAsync(_productoId);

        Assert.Equal(7, stock!.Cantidad);
        Assert.Equal(7, producto!.StockActual);
    }

    [Fact]
    public async Task Transferencia_ConservaCostoPromedioYStockTotal()
    {
        await RegistrarEntradaAsync(_almacenA, 10, 10m);

        await _inventory.TransferirAsync(
            new MovimientoInventario
            {
                AlmacenId = _almacenA,
                AlmacenDestinoId = _almacenB,
                ProductoId = _productoId,
                Tipo = TipoMovimientoInventario.TransferenciaSalida,
                Cantidad = 4,
                Referencia = "TRANSFERENCIA"
            },
            new MovimientoInventario
            {
                AlmacenId = _almacenB,
                AlmacenDestinoId = _almacenA,
                ProductoId = _productoId,
                Tipo = TipoMovimientoInventario.TransferenciaEntrada,
                Cantidad = 4,
                Referencia = "TRANSFERENCIA"
            });

        var origen = await _inventory.GetStockProductoAsync(_almacenA, _productoId);
        var destino = await _inventory.GetStockProductoAsync(_almacenB, _productoId);
        var producto = await _products.GetByIdAsync(_productoId);

        Assert.Equal(6, origen!.Cantidad);
        Assert.Equal(4, destino!.Cantidad);
        Assert.Equal(10m, destino.CostoPromedio);
        Assert.Equal(10, producto!.StockActual);
    }

    [Fact]
    public async Task BaseDeDatos_ImpideStockNegativo()
    {
        await RegistrarEntradaAsync(_almacenA, 5, 10m);

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        await Assert.ThrowsAsync<SqliteException>(() =>
            connection.ExecuteAsync(
                "UPDATE StockAlmacen SET Cantidad = -1 WHERE AlmacenId = @AlmacenId AND ProductoId = @ProductoId",
                new { AlmacenId = _almacenA, ProductoId = _productoId }));
    }

    private Task RegistrarEntradaAsync(string almacenId, int cantidad, decimal costo)
    {
        return _inventory.RegistrarMovimientoAsync(new MovimientoInventario
        {
            AlmacenId = almacenId,
            ProductoId = _productoId,
            Tipo = TipoMovimientoInventario.Entrada,
            Cantidad = cantidad,
            CostoUnitario = costo,
            Referencia = "TEST"
        });
    }

    private Task RegistrarSalidaAsync(string almacenId, int cantidad)
    {
        return _inventory.RegistrarMovimientoAsync(new MovimientoInventario
        {
            AlmacenId = almacenId,
            ProductoId = _productoId,
            Tipo = TipoMovimientoInventario.Salida,
            Cantidad = cantidad,
            Referencia = "TEST"
        });
    }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();

        foreach (var suffix in new[] { string.Empty, "-wal", "-shm" })
        {
            var path = _dbPath + suffix;
            if (File.Exists(path))
                File.Delete(path);
        }
    }
}
