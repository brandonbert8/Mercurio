using Dapper;
using Mercurio.Application.DTOs;
using Mercurio.Application.Services;
using Mercurio.Domain.Entities;
using Mercurio.Domain.Enums;
using Mercurio.Infrastructure;
using Mercurio.Infrastructure.Repositories;
using Microsoft.Data.Sqlite;

namespace Mercurio.Tests;

public sealed class SalesAndCreditTests : IDisposable
{
    private readonly string _dbPath;
    private readonly string _connectionString;
    private readonly ProductRepository _products;
    private readonly InventoryRepository _inventory;
    private readonly ClienteRepository _clientes;
    private readonly CreditoRepository _credito;
    private readonly SaleRepository _sales;
    private readonly CashRepository _cash;
    private readonly CompraRepository _compras;
    private readonly ProveedorRepository _proveedores;
    private readonly SaleService _saleService;
    private readonly CreditoService _creditoService;
    private readonly CajaService _cajaService;
    private readonly CompraService _compraService;

    private readonly string _sucursalId = "SUC-1";
    private readonly string _almacenId = Guid.NewGuid().ToString();
    private readonly string _productoId = Guid.NewGuid().ToString();
    private readonly string _clienteId = Guid.NewGuid().ToString();
    private readonly string _proveedorId = Guid.NewGuid().ToString();
    private readonly string _usuarioId = "USR-1";

    public SalesAndCreditTests()
    {
        _dbPath = Path.Combine(Path.GetTempPath(), $"mercurio-sales-tests-{Guid.NewGuid():N}.db");
        _connectionString = $"Data Source={_dbPath};Cache=Shared;Foreign Keys=True;Default Timeout=30";

        DatabaseInitializer.Initialize(_connectionString);

        _products = new ProductRepository(_connectionString);
        _inventory = new InventoryRepository(_connectionString);
        _clientes = new ClienteRepository(_connectionString);
        _credito = new CreditoRepository(_connectionString);
        _sales = new SaleRepository(_connectionString);
        _cash = new CashRepository(_connectionString);
        _compras = new CompraRepository(_connectionString);
        _proveedores = new ProveedorRepository(_connectionString);

        _saleService = new SaleService(_products, _sales, _inventory, _clientes, _credito);
        _creditoService = new CreditoService(_credito);
        _cajaService = new CajaService(_cash);
        _compraService = new CompraService(_compras, _proveedores, _products, _inventory);

        SeedAsync().GetAwaiter().GetResult();
    }

    [Fact]
    public async Task VentasSucesivas_GeneranNumeracionSecuencial()
    {
        var primera = await RegistrarVentaContadoAsync(1);
        var segunda = await RegistrarVentaContadoAsync(1);
        var anio = DateTime.Now.Year;

        Assert.Equal($"T-{anio}-000001", primera.Venta.NumeroDocumento);
        Assert.Equal($"T-{anio}-000002", segunda.Venta.NumeroDocumento);
    }

    [Fact]
    public async Task VentaContado_DesglosaIvaYReduceStock()
    {
        var resultado = await RegistrarVentaContadoAsync(1);
        var producto = await _products.GetByIdAsync(_productoId);

        Assert.Equal(100m, resultado.Venta.SubTotal);
        Assert.Equal(13m, resultado.Venta.Impuesto);
        Assert.Equal(113m, resultado.Venta.Total);
        Assert.Equal(99, producto!.StockActual);
    }

    [Fact]
    public async Task VentaCredito_CreaCuentaPorCobrar()
    {
        var resultado = await RegistrarVentaCreditoAsync(1);

        Assert.NotNull(resultado.CuentaPorCobrar);
        Assert.Equal(113m, resultado.CuentaPorCobrar!.Saldo);
        Assert.Equal(EstadoCuentaCobrar.Pendiente, resultado.CuentaPorCobrar.Estado);
        Assert.Equal(EstadoVenta.AlCredito, resultado.Venta.Estado);
    }

    [Fact]
    public async Task Abono_ActualizaCuentaYRegistraCajaEnLaMismaTransaccion()
    {
        var venta = await RegistrarVentaCreditoAsync(1);
        var caja = await _cajaService.CrearCajaAsync(new Caja
        {
            SucursalId = _sucursalId,
            Nombre = "Caja principal"
        });
        var sesion = await _cajaService.AbrirCajaAsync(new AbrirCajaRequest
        {
            CajaId = caja.Id,
            UsuarioId = _usuarioId,
            MontoInicial = 100m
        });

        var abono = await _creditoService.RegistrarAbonoAsync(new RegistrarAbonoRequest
        {
            CuentaPorCobrarId = venta.CuentaPorCobrar!.Id,
            Monto = 50m,
            MetodoPago = MetodoPago.Efectivo,
            CajaSesionId = sesion.Id,
            UsuarioId = _usuarioId
        });

        var cuenta = await _credito.GetByIdAsync(venta.CuentaPorCobrar.Id);
        var movimientos = (await _cash.GetMovimientosAsync(sesion.Id)).ToList();

        Assert.Equal(63m, abono.Cuenta.Saldo);
        Assert.Equal(EstadoCuentaCobrar.Parcial, cuenta!.Estado);
        Assert.Contains(movimientos, m => m.Tipo == TipoMovimientoCaja.Cobro && m.Monto == 50m);
    }

    [Fact]
    public async Task AnularVentaCredito_RestauraStockYAnulaCuenta()
    {
        var venta = await RegistrarVentaCreditoAsync(1);

        await _saleService.AnularVentaAsync(venta.Venta.Id, _usuarioId, "Prueba de anulación");

        var producto = await _products.GetByIdAsync(_productoId);
        var cuenta = await _credito.GetByIdAsync(venta.CuentaPorCobrar!.Id);

        Assert.Equal(100, producto!.StockActual);
        Assert.Equal(EstadoCuentaCobrar.Anulada, cuenta!.Estado);
        Assert.Equal(0m, cuenta.Saldo);
    }

    [Fact]
    public async Task AnularVentaConAbonos_LanzaError()
    {
        var venta = await RegistrarVentaCreditoAsync(1);
        await _creditoService.RegistrarAbonoAsync(new RegistrarAbonoRequest
        {
            CuentaPorCobrarId = venta.CuentaPorCobrar!.Id,
            Monto = 10m,
            MetodoPago = MetodoPago.Efectivo,
            UsuarioId = _usuarioId
        });

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _saleService.AnularVentaAsync(venta.Venta.Id, _usuarioId, "No permitida"));
    }

    [Fact]
    public async Task CompraCredito_GeneraNumeroYCuentaPorPagar()
    {
        var resultado = await _compraService.RegistrarCompraAsync(new CompraCreateRequest
        {
            SucursalId = _sucursalId,
            AlmacenId = _almacenId,
            ProveedorId = _proveedorId,
            UsuarioId = _usuarioId,
            Items = new List<CompraItemRequest>
            {
                new() { ProductoId = _productoId, Cantidad = 5, CostoUnitario = 100m }
            },
            MontoPagado = 0m,
            MetodoPago = MetodoPago.Efectivo
        });

        using var connection = new SqliteConnection(_connectionString);
        var cuenta = await connection.QuerySingleAsync<CuentaPorPagar>(
            "SELECT * FROM CuentasPorPagar WHERE CompraId = @CompraId",
            new { CompraId = resultado.Compra.Id });

        Assert.Equal($"C-{DateTime.Now.Year}-000001", resultado.Compra.NumeroDocumento);
        Assert.Equal(565m, cuenta.Saldo);
        Assert.Equal(EstadoCuentaPagar.Pendiente, cuenta.Estado);
    }

    [Fact]
    public async Task CierreCaja_CalculaMontoEsperadoDeFormaAtomica()
    {
        var caja = await _cajaService.CrearCajaAsync(new Caja
        {
            SucursalId = _sucursalId,
            Nombre = "Caja principal"
        });
        var sesion = await _cajaService.AbrirCajaAsync(new AbrirCajaRequest
        {
            CajaId = caja.Id,
            UsuarioId = _usuarioId,
            MontoInicial = 100m
        });

        await _saleService.RegistrarVentaAsync(new VentaCreateRequest
        {
            SucursalId = _sucursalId,
            AlmacenId = _almacenId,
            UsuarioId = _usuarioId,
            CajaSesionId = sesion.Id,
            Items = new List<VentaItemRequest> { new() { ProductoId = _productoId, Cantidad = 1 } },
            MontoPagado = 113m,
            MetodoPago = MetodoPago.Efectivo
        });

        var cierre = await _cajaService.CerrarCajaAsync(new CerrarCajaRequest
        {
            CajaSesionId = sesion.Id,
            MontoDeclarado = 213m
        });

        Assert.Equal(213m, cierre.MontoEsperado);
        Assert.Equal(0m, cierre.Diferencia);
        Assert.Equal(EstadoCajaSesion.Cerrada, cierre.Sesion.Estado);
    }

    private async Task<VentaResult> RegistrarVentaContadoAsync(int cantidad)
    {
        return await _saleService.RegistrarVentaAsync(new VentaCreateRequest
        {
            SucursalId = _sucursalId,
            AlmacenId = _almacenId,
            UsuarioId = _usuarioId,
            Items = new List<VentaItemRequest> { new() { ProductoId = _productoId, Cantidad = cantidad } },
            MontoPagado = 113m * cantidad,
            MetodoPago = MetodoPago.Efectivo
        });
    }

    private async Task<VentaResult> RegistrarVentaCreditoAsync(int cantidad)
    {
        return await _saleService.RegistrarVentaAsync(new VentaCreateRequest
        {
            SucursalId = _sucursalId,
            AlmacenId = _almacenId,
            UsuarioId = _usuarioId,
            ClienteId = _clienteId,
            Items = new List<VentaItemRequest> { new() { ProductoId = _productoId, Cantidad = cantidad } },
            MontoPagado = 0m,
            MetodoPago = MetodoPago.Credito,
            DiasCredito = 30
        });
    }

    private async Task SeedAsync()
    {
        await _products.InsertarAsync(new Product
        {
            Id = _productoId,
            CodigoBarras = $"TEST-{Guid.NewGuid():N}",
            Nombre = "Producto de prueba",
            PrecioCompra = 10m,
            PrecioVenta = 113m,
            ImpuestoPorcentaje = 13m,
            StockActual = 0,
            StockMinimo = 2,
            Activo = true,
            FechaCreacion = DateTime.Now
        });

        await _inventory.InsertarAlmacenAsync(new Almacen
        {
            Id = _almacenId,
            SucursalId = _sucursalId,
            Nombre = "Almacén principal",
            Codigo = "A"
        });

        await _inventory.RegistrarMovimientoAsync(new MovimientoInventario
        {
            AlmacenId = _almacenId,
            ProductoId = _productoId,
            Tipo = TipoMovimientoInventario.Entrada,
            Cantidad = 100,
            CostoUnitario = 10m,
            Referencia = "SEED"
        });

        await _clientes.InsertarAsync(new Cliente
        {
            Id = _clienteId,
            NegocioId = "NEG-1",
            Nombre = "Cliente de prueba",
            TipoDocumento = "CI",
            NumeroDocumento = "1234567",
            LimiteCredito = 1000m,
            Activo = true
        });

        await _proveedores.InsertarAsync(new Proveedor
        {
            Id = _proveedorId,
            NegocioId = "NEG-1",
            Nombre = "Proveedor de prueba",
            Ruc = "123456789",
            Activo = true
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
