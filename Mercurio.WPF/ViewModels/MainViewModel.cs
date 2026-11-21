using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mercurio.Application.Common;
using Mercurio.Application.DTOs;
using Mercurio.Application.Interfaces;
using Mercurio.Domain.Entities;
using System.Collections.ObjectModel;
using Mercurio.Domain.Enums;
using Mercurio.WPF.Dialogs;
using Mercurio.WPF.Services;
using Mercurio.Application.DTOs.Reports;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using System.Globalization;

namespace Mercurio.WPF.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly INegocioService _negocioService;
        private readonly ICatalogoService _catalogoService;
        private readonly IInventoryService _inventoryService;
        private readonly IReporteService _reporteService;
        private readonly IAuthService _authService;
        private readonly ISaleService _saleService;
        private readonly IClienteService _clienteService;
        private readonly IProveedorService _proveedorService;
        private readonly ICompraService _compraService;
        private readonly ICajaService _cajaService;
        private readonly IConfiguracionService _configuracionService;
        private readonly ICreditoService _creditoService;
        private readonly IDialogService _dialogService;

        [ObservableProperty] private string _paginaActual = "Resumen";
        [ObservableProperty] private string _nombreNegocio = "Mercurio";
        [ObservableProperty] private string _nombreSucursal = "Sucursal principal";
        [ObservableProperty] private bool _isAuthenticated;
        [ObservableProperty] private bool _modoRegistro;
        [ObservableProperty] private int _pasoOnboarding = 1;
        [ObservableProperty] private string _nombreUsuario = string.Empty;
        [ObservableProperty] private string _password = string.Empty;
        [ObservableProperty] private string _nombreCompleto = string.Empty;
        [ObservableProperty] private string _nombreNegocioNuevo = string.Empty;
        [ObservableProperty] private Negocio? _negocioSeleccionado;
        [ObservableProperty] private string _busqueda = string.Empty;
        [ObservableProperty] private Product? _productoStockSeleccionado;
        [ObservableProperty] private int _stockActualSeleccionado;
        [ObservableProperty] private int _cantidadEntradaStock;
        [ObservableProperty] private decimal _costoEntradaStock;
        [ObservableProperty] private string _estado = "Listo";
        [ObservableProperty] private bool _cargando;
        [ObservableProperty] private decimal _ventasHoy;
        [ObservableProperty] private decimal _ventasUltimosSieteDias;
        [ObservableProperty] private bool _hayVentasEnPeriodo;
        [ObservableProperty] private int _cantidadVentasHoy;
        [ObservableProperty] private decimal _valorInventario;
        [ObservableProperty] private int _productosActivos;
        [ObservableProperty] private int _productosStockBajo;
        [ObservableProperty] private decimal _montoCajaInicial;
        [ObservableProperty] private decimal _montoCajaDeclarado;
        [ObservableProperty] private string _busquedaVenta = string.Empty;
        [ObservableProperty] private Product? _productoSeleccionadoVenta;
        [ObservableProperty] private decimal _montoPagadoVenta;
        [ObservableProperty] private decimal _descuentoVenta;
        [ObservableProperty] private MetodoPago _metodoPagoVenta = MetodoPago.Efectivo;
        [ObservableProperty] private Cliente? _clienteVenta;
        [ObservableProperty] private decimal _totalVenta;
        [ObservableProperty] private decimal _vueltoVenta;
        [ObservableProperty] private CajaSesion? _sesionCajaAbierta;
        [ObservableProperty] private bool _cajaEstaAbierta;
        [ObservableProperty] private string _busquedaCompra = string.Empty;
        [ObservableProperty] private Proveedor? _proveedorCompra;
        [ObservableProperty] private MetodoPago _metodoPagoCompra = MetodoPago.Efectivo;
        [ObservableProperty] private decimal _montoPagadoCompra;
        [ObservableProperty] private decimal _totalCompra;
        [ObservableProperty] private CuentaPorCobrarReporteDto? _cuentaSeleccionada;
        [ObservableProperty] private decimal _montoAbono;
        [ObservableProperty] private MetodoPago _metodoPagoAbono = MetodoPago.Efectivo;
        [ObservableProperty] private DateTime? _fechaReporteDesde = DateTime.Today.AddDays(-30);
        [ObservableProperty] private DateTime? _fechaReporteHasta = DateTime.Today;
        [ObservableProperty] private decimal _totalVentasReporte;
        [ObservableProperty] private Negocio? _negocioConfiguracion;

        public string EstadoCajaTexto => CajaEstaAbierta ? "Caja abierta" : "Caja cerrada";

        public string EstadoCajaVenta => CajaEstaAbierta
            ? "Caja abierta · lista para cobrar"
            : "Caja cerrada · abre la caja antes de cobrar en efectivo";

        public string ResumenClientes => Clientes.Count == 1
            ? "1 cliente registrado"
            : $"{Clientes.Count} clientes registrados";

        public string ResumenProveedores => Proveedores.Count == 1
            ? "1 proveedor registrado"
            : $"{Proveedores.Count} proveedores registrados";

        public bool CompraAlCredito => MetodoPagoCompra == MetodoPago.Credito;
        public bool PuedeEditarPagoCompra => !CompraAlCredito;
        public bool CompraPagoNecesitaCaja => MetodoPagoCompra == MetodoPago.Efectivo &&
            MontoPagadoCompra > 0 && !CajaEstaAbierta;

        public bool PuedeCobrar => CarritoVenta.Count > 0 &&
            (CajaEstaAbierta || (MetodoPagoVenta != MetodoPago.Efectivo &&
                                 !(MetodoPagoVenta == MetodoPago.Credito && MontoPagadoVenta > 0)));

        public bool HayProductoSeleccionadoVenta => ProductoSeleccionadoVenta is not null;

        public ObservableCollection<Product> Productos { get; } = new();
        public ObservableCollection<StockBajoView> StockBajo { get; } = new();
        public ObservableCollection<Negocio> Negocios { get; } = new();
        public ObservableCollection<Venta> Ventas { get; } = new();
        public ObservableCollection<Cliente> Clientes { get; } = new();
        public ObservableCollection<Proveedor> Proveedores { get; } = new();
        public ObservableCollection<Compra> Compras { get; } = new();
        public ObservableCollection<Caja> Cajas { get; } = new();
        public ObservableCollection<CajaSesion> SesionesCaja { get; } = new();
        public ObservableCollection<LineaVentaView> CarritoVenta { get; } = new();
        public ObservableCollection<Product> ProductosCompra { get; } = new();
        public ObservableCollection<LineaCompraView> CarritoCompra { get; } = new();
        public ObservableCollection<CuentaPorCobrarReporteDto> CuentasPorCobrar { get; } = new();
        public ObservableCollection<ResumenVentasDto> VentasReporte { get; } = new();
        public ObservableCollection<VentasPorProductoDto> ProductosMasVendidosReporte { get; } = new();
        public ObservableCollection<ValorizacionInventarioDto> ValorizacionReporte { get; } = new();
        public ObservableCollection<double> VentasSemana { get; } = new();
        public ObservableCollection<string> EtiquetasVentasSemana { get; } = new();
        public ISeries[] VentasChartSeries { get; }
        public Axis[] VentasChartAxes { get; }
        public Axis[] VentasChartYAxes { get; }
        public IEnumerable<MetodoPago> MetodosPago { get; } = Enum.GetValues<MetodoPago>();

        public MainViewModel(
            INegocioService negocioService,
            ICatalogoService catalogoService,
            IInventoryService inventoryService,
            IReporteService reporteService,
            IAuthService authService,
            ISaleService saleService,
            IClienteService clienteService,
            IProveedorService proveedorService,
            ICompraService compraService,
            ICajaService cajaService,
            IConfiguracionService configuracionService,
            IDialogService dialogService,
            ICreditoService creditoService)
        {
            _negocioService = negocioService;
            _catalogoService = catalogoService;
            _inventoryService = inventoryService;
            _reporteService = reporteService;
            _authService = authService;
            _saleService = saleService;
            _clienteService = clienteService;
            _proveedorService = proveedorService;
            _compraService = compraService;
            _cajaService = cajaService;
            _configuracionService = configuracionService;
            _dialogService = dialogService;
            _creditoService = creditoService;

            VentasChartSeries =
            [
                new ColumnSeries<double>
                {
                    Name = "Ingresos",
                    Values = VentasSemana,
                    MaxBarWidth = 34,
                    Fill = new SolidColorPaint(new SKColor(79, 93, 235)),
                    Stroke = null
                }
            ];
            VentasChartAxes =
            [
                new Axis
                {
                    Labels = EtiquetasVentasSemana,
                    TextSize = 11,
                    LabelsRotation = 0,
                    SeparatorsPaint = new SolidColorPaint(new SKColor(239, 240, 243))
                }
            ];
            VentasChartYAxes =
            [
                new Axis
                {
                    TextSize = 10,
                    MinLimit = 0,
                    Labeler = value => $"Bs. {value:N0}",
                    SeparatorsPaint = new SolidColorPaint(new SKColor(231, 232, 236))
                }
            ];
        }

        public string NegocioId { get; private set; } = string.Empty;
        public string SucursalId { get; private set; } = string.Empty;
        public string AlmacenId { get; private set; } = string.Empty;
        public Usuario? UsuarioActual { get; private set; }

        [RelayCommand]
        public async Task InicializarAsync()
        {
            try
            {
                Cargando = true;
                Estado = "Cargando...";
                var negocios = (await _negocioService.GetNegociosAsync()).ToList();
                Negocios.Clear();
                foreach (var negocio in negocios)
                    Negocios.Add(negocio);

                if (negocios.Count > 0)
                {
                    NegocioSeleccionado = negocios[0];
                    ModoRegistro = false;
                }
                else
                {
                    ModoRegistro = true;
                }

                Estado = "Listo";
            }
            catch (Exception ex)
            {
                Estado = "No fue posible cargar los datos";
                await _dialogService.MostrarErrorAsync("No fue posible cargar los datos al iniciar. Verifica la base de datos.");
                System.Diagnostics.Debug.WriteLine(ex);
            }
            finally
            {
                Cargando = false;
            }
        }

        [RelayCommand]
        private async Task NavegarAsync(string pagina)
        {
            if (string.IsNullOrWhiteSpace(pagina)) return;
            PaginaActual = pagina;


            // Actualizar estado activo del menú lateral
            if (System.Windows.Application.Current.MainWindow is MainWindow mw)
                mw.ActualizarNavActivo(pagina);

            if (pagina is "Resumen" or "Inventario")
                await CargarDatosAsync();
            else if (pagina == "Ventas")
                await CargarVentasAsync();
            else if (pagina == "Clientes")
                await CargarClientesAsync();
            else if (pagina == "Proveedores")
                await CargarProveedoresAsync();
            else if (pagina == "Compras")
                await CargarComprasAsync();
            else if (pagina == "Caja")
                await CargarCajaAsync();
            else if (pagina == "Creditos")
                await CargarCreditosAsync();
            else if (pagina == "Reportes")
                await CargarReportesAsync();
            else if (pagina == "Configuracion")
                await CargarConfiguracionAsync();
        }

        [RelayCommand]
        private async Task CargarVentasAsync()
        {
            await EjecutarAsync(async () =>
            {
                if (string.IsNullOrWhiteSpace(SucursalId)) return;
                var desde = DateTime.Today.AddDays(-30);
                var ventas = await _saleService.GetVentasAsync(SucursalId, desde, DateTime.Now);
                Ventas.Clear();
                foreach (var venta in ventas.OrderByDescending(x => x.Fecha))
                    Ventas.Add(venta);
                await ActualizarSesionCajaAbiertaAsync();
                await ActualizarCatalogoVentaAsync();
                if (!string.IsNullOrWhiteSpace(NegocioId))
                {
                    var clientes = await _clienteService.GetClientesAsync(NegocioId);
                    Clientes.Clear();
                    foreach (var cliente in clientes)
                        Clientes.Add(cliente);
                }
                Estado = $"{Ventas.Count} ventas recientes";
            });
        }

        [RelayCommand]
        private async Task BuscarVentaAsync()
        {
            await ActualizarCatalogoVentaAsync();
            if (string.IsNullOrWhiteSpace(BusquedaVenta))
                return;

            var termino = BusquedaVenta.Trim();
            var codigoEscaneado = Productos.FirstOrDefault(producto =>
                string.Equals(producto.CodigoBarras, termino, StringComparison.OrdinalIgnoreCase));

            if (codigoEscaneado is not null)
            {
                if (!codigoEscaneado.EsServicio && codigoEscaneado.StockActual <= 0)
                {
                    await _dialogService.MostrarAdvertenciaAsync(
                        $"{codigoEscaneado.Nombre} no tiene existencias. Registra una compra o una entrada de stock antes de venderlo.");
                    return;
                }

                await AgregarAlCarritoAsync(codigoEscaneado.Id);
                BusquedaVenta = string.Empty;
                await ActualizarCatalogoVentaAsync();
                return;
            }

            ProductoSeleccionadoVenta = Productos.FirstOrDefault(producto =>
                producto.Activo && (producto.EsServicio || producto.StockActual > 0))
                ?? Productos.FirstOrDefault();
        }

        [RelayCommand]
        private async Task AgregarProductoSeleccionadoVentaAsync()
        {
            if (ProductoSeleccionadoVenta is null)
            {
                await _dialogService.MostrarInfoAsync("Selecciona un producto de la lista para agregarlo a la venta.");
                return;
            }

            await AgregarAlCarritoAsync(ProductoSeleccionadoVenta.Id);
        }

        partial void OnProductoSeleccionadoVentaChanged(Product? value)
        {
            OnPropertyChanged(nameof(HayProductoSeleccionadoVenta));
        }

        [RelayCommand]
        private void LimpiarCarrito()
        {
            CarritoVenta.Clear();
            MontoPagadoVenta = 0;
            DescuentoVenta = 0;
            ClienteVenta = null;
            ActualizarTotalVenta();
            Estado = "Carrito vaciado";
        }

        [RelayCommand]
        private async Task AgregarAlCarritoAsync(string productoId)
        {
            await EjecutarAsync(async () =>
            {
                if (string.IsNullOrWhiteSpace(productoId))
                    return;

                var producto = await _catalogoService.GetProductoAsync(productoId);
                if (producto is null)
                {
                    await _dialogService.MostrarErrorAsync("El producto ya no está disponible en el catálogo.");
                    return;
                }

                var linea = CarritoVenta.FirstOrDefault(x => x.ProductoId == producto.Id);
                if (linea is not null)
                {
                    linea.Cantidad++;
                    linea.ActualizarTotales();
                }
                else
                {
                    var nuevaLinea = new LineaVentaView(
                        producto.Id,
                        producto.CodigoBarras,
                        producto.Nombre,
                        1,
                        producto.PrecioVenta);
                    nuevaLinea.CantidadCambiada += () => ActualizarTotalVenta();
                    CarritoVenta.Add(nuevaLinea);
                }

                ActualizarTotalVenta();
            });
        }

        [RelayCommand]
        private void QuitarDelCarrito(LineaVentaView? linea)
        {
            if (linea is null) return;
            CarritoVenta.Remove(linea);
            ActualizarTotalVenta();
        }

        [RelayCommand]
        private void CambiarCantidad(LineaVentaView? linea)
        {
            if (linea is null) return;
            linea.ActualizarTotales();
            ActualizarTotalVenta();
        }

        [RelayCommand]
        private async Task RegistrarVentaRapidaAsync()
        {
            if (CarritoVenta.Count == 0)
            {
                await _dialogService.MostrarAdvertenciaAsync("Agrega al menos un producto al carrito antes de cobrar.");
                return;
            }
            if (string.IsNullOrWhiteSpace(SucursalId) ||
                string.IsNullOrWhiteSpace(AlmacenId) ||
                string.IsNullOrWhiteSpace(UsuarioActual?.Id))
            {
                await _dialogService.MostrarErrorAsync("No hay una sesión activa válida. Cierra sesión y vuelve a ingresar.");
                return;
            }
            if (MetodoPagoVenta == MetodoPago.Credito && ClienteVenta is null)
            {
                await _dialogService.MostrarAdvertenciaAsync("Para registrar una venta a crédito debes seleccionar un cliente.");
                return;
            }
            if ((MetodoPagoVenta == MetodoPago.Efectivo ||
                 (MetodoPagoVenta == MetodoPago.Credito && MontoPagadoVenta > 0)) &&
                SesionCajaAbierta is null)
            {
                await _dialogService.MostrarAdvertenciaAsync("La caja está cerrada. Ve a la sección Caja y ábrela antes de cobrar en efectivo.");
                return;
            }

            try
            {
                foreach (var linea in CarritoVenta)
                {
                    var producto = await _catalogoService.GetProductoAsync(linea.ProductoId);
                    if (producto is null || producto.EsServicio)
                        continue;

                    var stock = await _inventoryService.GetStockProductoAsync(AlmacenId, producto.Id);
                    var disponible = stock?.Cantidad ?? 0;
                    if (disponible < linea.Cantidad)
                    {
                        await _dialogService.MostrarAdvertenciaAsync(
                            $"No se puede vender {producto.Nombre}: hay {disponible} en stock y se necesitan {linea.Cantidad}. Selecciónalo en Inventario y registra una entrada de stock o una compra.");
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                await _dialogService.MostrarErrorAsync("No se pudo consultar el inventario. Intenta nuevamente antes de cobrar.");
                return;
            }

            // ── Diálogo de confirmación de venta ──────────────────────────────
            var resumen = new ResumenVentaDialog
            {
                Items = CarritoVenta.Select(x => new ItemVentaResumen
                {
                    Nombre         = x.Nombre,
                    Cantidad       = x.Cantidad,
                    PrecioUnitario = x.PrecioUnitario,
                    Total          = x.Total
                }).ToList(),
                Subtotal     = CarritoVenta.Sum(x => x.Total),
                Descuento    = DescuentoVenta,
                Total        = TotalVenta,
                MontoPagado  = MontoPagadoVenta,
                Vuelto       = VueltoVenta,
                MetodoPago   = MetodoPagoVenta.ToString(),
                Cliente      = ClienteVenta?.Nombre ?? string.Empty
            };

            var confirmado = await _dialogService.ConfirmarVentaAsync(resumen);
            if (!confirmado) return;

            await EjecutarAsync(async () =>
            {
                var request = new VentaCreateRequest
                {
                    SucursalId    = SucursalId,
                    AlmacenId     = AlmacenId,
                    UsuarioId     = UsuarioActual!.Id,
                    ClienteId     = ClienteVenta?.Id,
                    CajaSesionId  = SesionCajaAbierta?.Id,
                    TipoDocumento = TipoDocumento.Ticket,
                    Descuento     = DescuentoVenta,
                    MetodoPago    = MetodoPagoVenta,
                    MontoPagado   = MontoPagadoVenta,
                    Items = CarritoVenta.Select(x => new VentaItemRequest
                    {
                        ProductoId     = x.ProductoId,
                        Cantidad       = x.Cantidad,
                        PrecioUnitario = x.PrecioUnitario,
                        Descuento      = 0
                    }).ToList()
                };

                var resultado = await _saleService.RegistrarVentaAsync(request);
                var cambio = resultado.Vuelto;

                CarritoVenta.Clear();
                MontoPagadoVenta = 0;
                DescuentoVenta = 0;
                ClienteVenta = null;
                VueltoVenta = resultado.Vuelto;
                ActualizarTotalVenta();

                var msg = cambio > 0
                    ? $"Venta {resultado.Venta.NumeroDocumento} registrada · Vuelto: Bs. {cambio:N2}"
                    : $"Venta {resultado.Venta.NumeroDocumento} registrada correctamente.";

                await _dialogService.MostrarExitoAsync(msg);
                Estado = $"Venta registrada · {resultado.Venta.NumeroDocumento}";
                await CargarVentasAsync();
            });
        }

        partial void OnMontoPagadoVentaChanged(decimal value)
        {
            VueltoVenta = MetodoPagoVenta == MetodoPago.Efectivo
                ? Math.Max(0, value - TotalVenta)
                : 0;
            OnPropertyChanged(nameof(PuedeCobrar));
        }

        partial void OnSesionCajaAbiertaChanged(CajaSesion? value)
        {
            CajaEstaAbierta = value is not null;
            OnPropertyChanged(nameof(EstadoCajaVenta));
            OnPropertyChanged(nameof(EstadoCajaTexto));
            OnPropertyChanged(nameof(PuedeCobrar));
            OnPropertyChanged(nameof(CompraPagoNecesitaCaja));
        }

        partial void OnDescuentoVentaChanged(decimal value)
        {
            ActualizarTotalVenta();
        }

        partial void OnMetodoPagoVentaChanged(MetodoPago value)
        {
            if (value != MetodoPago.Efectivo)
                VueltoVenta = 0;
            OnPropertyChanged(nameof(PuedeCobrar));
        }

        private async Task ActualizarCatalogoVentaAsync()
        {
            var productos = string.IsNullOrWhiteSpace(BusquedaVenta)
                ? await _catalogoService.GetProductosAsync()
                : await _catalogoService.BuscarProductosAsync(BusquedaVenta.Trim());

            var lista = productos.ToList();
            ProductoSeleccionadoVenta = null;
            Productos.Clear();
            foreach (var producto in lista)
            {
                if (!producto.EsServicio)
                {
                    var stock = await _inventoryService.GetStockProductoAsync(AlmacenId, producto.Id);
                    producto.StockActual = stock?.Cantidad ?? 0;
                }
                Productos.Add(producto);
            }
        }

        private void ActualizarTotalVenta()
        {
            var subtotal = CarritoVenta.Sum(x => x.Total);
            TotalVenta = Math.Max(0, Math.Round(subtotal - DescuentoVenta, 2));
            if (MetodoPagoVenta == MetodoPago.Efectivo)
                VueltoVenta = Math.Max(0, MontoPagadoVenta - TotalVenta);
            OnPropertyChanged(nameof(PuedeCobrar));
        }

        [RelayCommand]
        private async Task CargarClientesAsync()
        {
            await EjecutarAsync(async () =>
            {
                if (string.IsNullOrWhiteSpace(NegocioId)) return;
                var clientes = await _clienteService.GetClientesAsync(NegocioId);
                Clientes.Clear();
                foreach (var cliente in clientes)
                    Clientes.Add(cliente);
                OnPropertyChanged(nameof(ResumenClientes));
                Estado = $"{Clientes.Count} clientes";
            });
        }

        [RelayCommand]
        private async Task AbrirNuevoClienteAsync()
        {
            var datos = await _dialogService.AbrirFormularioClienteAsync();
            if (datos is null)
                return;

            await EjecutarAsync(async () =>
            {
                await _clienteService.CrearClienteAsync(new Cliente
                {
                    NegocioId       = NegocioId,
                    Nombre          = datos.Nombre,
                    NumeroDocumento = datos.NumeroDocumento,
                    Telefono        = datos.Telefono,
                    LimiteCredito   = datos.LimiteCredito,
                    Activo          = true
                });
                await _dialogService.MostrarExitoAsync("Cliente guardado correctamente.");
                await CargarClientesAsync();
            });
        }

        [RelayCommand]
        private async Task CargarComprasAsync()
        {
            await EjecutarAsync(async () =>
            {
                if (string.IsNullOrWhiteSpace(SucursalId)) return;
                var compras = await _compraService.GetComprasAsync(SucursalId, DateTime.Today.AddDays(-30), DateTime.Now);
                Compras.Clear();
                foreach (var compra in compras.OrderByDescending(x => x.Fecha))
                    Compras.Add(compra);

                var proveedores = await _proveedorService.GetProveedoresAsync(NegocioId);
                Proveedores.Clear();
                foreach (var proveedor in proveedores)
                    Proveedores.Add(proveedor);
                OnPropertyChanged(nameof(ResumenProveedores));
                await ActualizarCatalogoCompraAsync();
                Estado = $"{Compras.Count} compras recientes";
            });
        }

        [RelayCommand]
        private async Task CargarProveedoresAsync()
        {
            await EjecutarAsync(async () =>
            {
                var proveedores = await _proveedorService.GetProveedoresAsync(NegocioId);
                Proveedores.Clear();
                foreach (var proveedor in proveedores.OrderBy(x => x.Nombre))
                    Proveedores.Add(proveedor);
                OnPropertyChanged(nameof(ResumenProveedores));
                Estado = $"{Proveedores.Count} proveedor(es)";
            });
        }

        [RelayCommand]
        private async Task BuscarCompraAsync()
        {
            await ActualizarCatalogoCompraAsync();
        }

        [RelayCommand]
        private async Task AgregarProductoCompraAsync(string productoId)
        {
            var producto = ProductosCompra.FirstOrDefault(x => x.Id == productoId)
                ?? await _catalogoService.GetProductoAsync(productoId);
            if (producto is null || !producto.Activo)
            {
                await _dialogService.MostrarErrorAsync("No se encontró el producto seleccionado.");
                return;
            }

            var linea = CarritoCompra.FirstOrDefault(x => x.ProductoId == producto.Id);
            if (linea is not null)
            {
                linea.Cantidad++;
            }
            else
            {
                var nuevaLinea = new LineaCompraView(producto.Id, producto.Nombre, 1, producto.PrecioCompra);
                nuevaLinea.TotalCambiado += ActualizarTotalCompra;
                CarritoCompra.Add(nuevaLinea);
            }
            ActualizarTotalCompra();
        }

        [RelayCommand]
        private void QuitarProductoCompra(LineaCompraView? linea)
        {
            if (linea is null) return;
            CarritoCompra.Remove(linea);
            ActualizarTotalCompra();
        }

        [RelayCommand]
        private async Task RegistrarCompraRapidaAsync()
        {
            if (CarritoCompra.Count == 0)
            {
                await _dialogService.MostrarAdvertenciaAsync("Agrega al menos un producto a la compra.");
                return;
            }
            if (ProveedorCompra is null)
            {
                await _dialogService.MostrarAdvertenciaAsync("Selecciona un proveedor antes de registrar la compra.");
                return;
            }
            if (CarritoCompra.Any(x => x.Cantidad <= 0 || x.CostoUnitario < 0))
            {
                await _dialogService.MostrarAdvertenciaAsync("Revisa las cantidades y costos: la cantidad debe ser mayor a cero y el costo no puede ser negativo.");
                return;
            }
            if (MetodoPagoCompra != MetodoPago.Credito &&
                (MontoPagadoCompra < 0 || MontoPagadoCompra > TotalCompra))
            {
                await _dialogService.MostrarAdvertenciaAsync("El monto pagado debe estar entre cero y el total de la compra.");
                return;
            }
            if (MetodoPagoCompra == MetodoPago.Efectivo && MontoPagadoCompra > 0 && !CajaEstaAbierta)
            {
                await _dialogService.MostrarAdvertenciaAsync("Abre la caja antes de registrar un pago en efectivo.");
                return;
            }

            var montoPagado = MetodoPagoCompra == MetodoPago.Credito ? 0 : MontoPagadoCompra;
            var saldoPendiente = Math.Max(0, TotalCompra - montoPagado);
            var confirmado = await _dialogService.ConfirmarAsync(
                "Registrar compra",
                $"Proveedor: {ProveedorCompra.Nombre}\nProductos: {CarritoCompra.Count}\nTotal: Bs. {TotalCompra:N2}\nPagado ahora: Bs. {montoPagado:N2}\nSaldo pendiente: Bs. {saldoPendiente:N2}");
            if (!confirmado) return;

            await EjecutarAsync(async () =>
            {
                await _compraService.RegistrarCompraAsync(new CompraCreateRequest
                {
                    SucursalId = SucursalId,
                    AlmacenId = AlmacenId,
                    ProveedorId = ProveedorCompra.Id,
                    UsuarioId = UsuarioActual!.Id,
                    CajaSesionId = MetodoPagoCompra == MetodoPago.Efectivo && montoPagado > 0 ? SesionCajaAbierta?.Id : null,
                    MetodoPago = MetodoPagoCompra,
                    MontoPagado = montoPagado,
                    Items = CarritoCompra.Select(x => new CompraItemRequest
                    {
                        ProductoId = x.ProductoId,
                        Cantidad = x.Cantidad,
                        CostoUnitario = x.CostoUnitario
                    }).ToList()
                });

                CarritoCompra.Clear();
                ProveedorCompra = null;
                MontoPagadoCompra = 0;
                ActualizarTotalCompra();
                await _dialogService.MostrarExitoAsync("Compra registrada e inventario actualizado.");
                await CargarComprasAsync();
                await CargarDatosAsync();
            });
        }

        private async Task ActualizarCatalogoCompraAsync()
        {
            var productos = string.IsNullOrWhiteSpace(BusquedaCompra)
                ? await _catalogoService.GetProductosAsync()
                : await _catalogoService.BuscarProductosAsync(BusquedaCompra.Trim());
            ProductosCompra.Clear();
            foreach (var producto in productos)
                ProductosCompra.Add(producto);
        }

        private void ActualizarTotalCompra()
        {
            var totalAnterior = TotalCompra;
            TotalCompra = Math.Round(CarritoCompra.Sum(x => x.Total), 2);
            if (MetodoPagoCompra != MetodoPago.Credito && MontoPagadoCompra == totalAnterior)
                MontoPagadoCompra = TotalCompra;
        }

        partial void OnMetodoPagoCompraChanged(MetodoPago value)
        {
            if (value == MetodoPago.Credito)
                MontoPagadoCompra = 0;
            else if (MontoPagadoCompra == 0)
                MontoPagadoCompra = TotalCompra;
            OnPropertyChanged(nameof(CompraAlCredito));
            OnPropertyChanged(nameof(PuedeEditarPagoCompra));
            OnPropertyChanged(nameof(CompraPagoNecesitaCaja));
        }

        partial void OnMontoPagadoCompraChanged(decimal value)
        {
            OnPropertyChanged(nameof(CompraPagoNecesitaCaja));
        }

        partial void OnBusquedaCompraChanged(string value)
        {
            _ = ActualizarCatalogoCompraAsync();
        }

        [RelayCommand]
        private async Task CargarCreditosAsync()
        {
            await EjecutarAsync(async () =>
            {
                var cuentas = await _reporteService.GetCuentasPorCobrarAsync(NegocioId, DateTime.Today, soloVencidas: false);
                CuentasPorCobrar.Clear();
                foreach (var cuenta in cuentas)
                    CuentasPorCobrar.Add(cuenta);
                Estado = $"{CuentasPorCobrar.Count} cuenta(s) pendientes";
            });
        }

        [RelayCommand]
        private async Task RegistrarCobroAsync()
        {
            if (CuentaSeleccionada is null)
            {
                await _dialogService.MostrarAdvertenciaAsync("Selecciona una cuenta pendiente de cobro.");
                return;
            }
            if (MontoAbono <= 0 || MontoAbono > CuentaSeleccionada.Saldo)
            {
                await _dialogService.MostrarAdvertenciaAsync("Ingresa un monto mayor a cero y no superior al saldo pendiente.");
                return;
            }
            if (MetodoPagoAbono == MetodoPago.Efectivo && SesionCajaAbierta is null)
            {
                await _dialogService.MostrarAdvertenciaAsync("Abre la caja antes de registrar un cobro en efectivo.");
                return;
            }

            await EjecutarAsync(async () =>
            {
                await _creditoService.RegistrarAbonoAsync(new RegistrarAbonoRequest
                {
                    CuentaPorCobrarId = CuentaSeleccionada.CuentaId,
                    Monto = MontoAbono,
                    MetodoPago = MetodoPagoAbono,
                    UsuarioId = UsuarioActual?.Id,
                    CajaSesionId = MetodoPagoAbono == MetodoPago.Efectivo ? SesionCajaAbierta?.Id : null
                });
                CuentaSeleccionada = null;
                MontoAbono = 0;
                await _dialogService.MostrarExitoAsync("Cobro registrado y saldo actualizado.");
                await CargarCreditosAsync();
                await CargarCajaAsync();
            });
        }

        [RelayCommand]
        private async Task CargarReportesAsync()
        {
            if (FechaReporteDesde is null || FechaReporteHasta is null || FechaReporteDesde > FechaReporteHasta)
            {
                await _dialogService.MostrarAdvertenciaAsync("El rango de fechas no es válido.");
                return;
            }

            await EjecutarAsync(async () =>
            {
                var desde = FechaReporteDesde.Value.Date;
                var hasta = FechaReporteHasta.Value.Date.AddDays(1).AddTicks(-1);
                var ventas = (await _reporteService.GetResumenVentasAsync(NegocioId, SucursalId, desde, hasta)).ToList();
                VentasReporte.Clear();
                foreach (var item in ventas.OrderByDescending(x => x.Fecha))
                    VentasReporte.Add(item);
                TotalVentasReporte = ventas.Sum(x => x.Total);

                var productos = await _reporteService.GetVentasPorProductoAsync(NegocioId, SucursalId, desde, hasta);
                ProductosMasVendidosReporte.Clear();
                foreach (var item in productos.OrderByDescending(x => x.Cantidad).Take(10))
                    ProductosMasVendidosReporte.Add(item);

                var valorizacion = await _reporteService.GetValorizacionInventarioAsync(NegocioId, SucursalId);
                ValorizacionReporte.Clear();
                foreach (var item in valorizacion)
                    ValorizacionReporte.Add(item);
                Estado = "Reportes actualizados";
            });
        }

        [RelayCommand]
        private async Task CargarConfiguracionAsync()
        {
            await EjecutarAsync(async () =>
            {
                NegocioConfiguracion = await _negocioService.GetNegocioAsync(NegocioId);
                if (NegocioConfiguracion is null)
                    await _dialogService.MostrarErrorAsync("No se encontró la configuración del negocio.");
            });
        }

        [RelayCommand]
        private async Task GuardarConfiguracionAsync()
        {
            if (NegocioConfiguracion is null) return;

            await EjecutarAsync(async () =>
            {
                await _negocioService.ActualizarNegocioAsync(NegocioConfiguracion);
                NombreNegocio = NegocioConfiguracion.Nombre;
                await _dialogService.MostrarExitoAsync("Configuración guardada.");
            });
        }

        [RelayCommand]
        private async Task AbrirNuevoProveedorAsync()
        {
            var datos = await _dialogService.AbrirFormularioProveedorAsync();
            if (datos is null)
                return;

            await EjecutarAsync(async () =>
            {
                var proveedor = await _proveedorService.CrearProveedorAsync(new Proveedor
                {
                    NegocioId = NegocioId,
                    Nombre    = datos.Nombre,
                    Ruc       = datos.Ruc,
                    Contacto  = datos.Contacto,
                    Telefono  = datos.Telefono,
                    Email     = datos.Email,
                    Activo    = true
                });
                await _dialogService.MostrarExitoAsync("Proveedor guardado correctamente.");
                await CargarProveedoresAsync();
                if (PaginaActual == "Compras")
                    ProveedorCompra = Proveedores.FirstOrDefault(x => x.Id == proveedor.Id);
            });
        }

        [RelayCommand]
        private async Task CargarCajaAsync()
        {
            await EjecutarAsync(async () =>
            {
                var cajas = await _cajaService.GetCajasAsync(SucursalId);
                Cajas.Clear();
                foreach (var caja in cajas)
                    Cajas.Add(caja);

                var desde = DateTime.Today.AddDays(-7);
                var sesiones = await _cajaService.GetSesionesAsync(SucursalId, desde, DateTime.Now);
                SesionesCaja.Clear();
                foreach (var sesion in sesiones.OrderByDescending(x => x.FechaApertura))
                    SesionesCaja.Add(sesion);
                await ActualizarSesionCajaAbiertaAsync();
                Estado = $"{Cajas.Count} caja(s), {SesionesCaja.Count} sesiones";
            });
        }

        private async Task ActualizarSesionCajaAbiertaAsync()
        {
            SesionCajaAbierta = null;
            var cajas = await _cajaService.GetCajasAsync(SucursalId);
            foreach (var caja in cajas.Where(x => x.Activa))
            {
                var sesion = await _cajaService.GetSesionAbiertaAsync(caja.Id);
                if (sesion is null)
                    continue;

                SesionCajaAbierta = sesion;
                break;
            }
        }

        [RelayCommand]
        private async Task AbrirCajaRapidaAsync()
        {
            await EjecutarAsync(async () =>
            {
                var caja = Cajas.FirstOrDefault();
                if (caja is null)
                {
                    caja = await _cajaService.CrearCajaAsync(new Caja
                    {
                        SucursalId = SucursalId,
                        Nombre     = "Caja principal",
                        Activa     = true
                    });
                }

                await _cajaService.AbrirCajaAsync(new AbrirCajaRequest
                {
                    CajaId      = caja.Id,
                    UsuarioId   = UsuarioActual?.Id ?? string.Empty,
                    MontoInicial = MontoCajaInicial,
                    Observacion = "Apertura desde Mercurio"
                });

                var montoStr = $"Bs. {MontoCajaInicial:N2}";
                MontoCajaInicial = 0;
                await CargarCajaAsync();
                await _dialogService.MostrarExitoAsync($"Caja abierta con fondo inicial de {montoStr}.");
            });
        }

        [RelayCommand]
        private async Task CerrarCajaRapidaAsync()
        {
            var sesion = SesionCajaAbierta;
            if (sesion is null)
            {
                await _dialogService.MostrarAdvertenciaAsync("No hay ninguna caja abierta en este momento.");
                return;
            }

            // Calcular los montos para el resumen
            // Usamos los datos de la sesión + los movimientos para construir el resumen
            var movimientos = await _cajaService.GetMovimientosAsync(sesion.Id);
            var ventasEf  = movimientos.Where(m => m.Tipo == TipoMovimientoCaja.Venta).Sum(m => m.Monto);
            var cobrosEf  = movimientos.Where(m => m.Tipo == TipoMovimientoCaja.Cobro).Sum(m => m.Monto);
            var ingresos  = movimientos.Where(m => m.Tipo == TipoMovimientoCaja.Ingreso).Sum(m => m.Monto);
            var egresos   = movimientos.Where(m => m.Tipo == TipoMovimientoCaja.Egreso).Sum(m => m.Monto);
            var esperado  = sesion.MontoInicial + ventasEf + cobrosEf + ingresos - egresos;
            var diferencia = MontoCajaDeclarado - esperado;

            var resumenCierre = new ResumenCierreCajaDialog
            {
                MontoInicial    = sesion.MontoInicial,
                VentasEfectivo  = ventasEf,
                CobrosEfectivo  = cobrosEf,
                Ingresos        = ingresos,
                Egresos         = egresos,
                MontoEsperado   = esperado,
                MontoDeclarado  = MontoCajaDeclarado,
                Diferencia      = diferencia
            };

            var confirmado = await _dialogService.ConfirmarCierreCajaAsync(resumenCierre);
            if (!confirmado) return;

            await EjecutarAsync(async () =>
            {
                var resultado = await _cajaService.CerrarCajaAsync(new CerrarCajaRequest
                {
                    CajaSesionId  = sesion.Id,
                    MontoDeclarado = MontoCajaDeclarado,
                    Observacion   = "Cierre desde Mercurio"
                });

                MontoCajaDeclarado = 0;
                await CargarCajaAsync();

                var difTexto = resultado.Diferencia == 0
                    ? "sin diferencias"
                    : resultado.Diferencia > 0
                        ? $"sobrante Bs. {resultado.Diferencia:N2}"
                        : $"faltante Bs. {Math.Abs(resultado.Diferencia):N2}";

                await _dialogService.MostrarExitoAsync($"Caja cerrada correctamente — {difTexto}.");
                Estado = "Caja cerrada";
            });
        }

        [RelayCommand]
        private async Task CargarProductosAsync()
        {
            await CargarDatosAsync();
        }

        [RelayCommand]
        private async Task IniciarSesionAsync()
        {
            if (NegocioSeleccionado is null)
            {
                await _dialogService.MostrarAdvertenciaAsync("Selecciona un negocio antes de continuar.");
                return;
            }

            await EjecutarAsync(async () =>
            {
                var resultado = await _authService.LoginAsync(new Mercurio.Application.DTOs.LoginRequest
                {
                    NegocioId    = NegocioSeleccionado.Id,
                    NombreUsuario = NombreUsuario,
                    Password     = Password
                });

                if (!resultado.Exitoso || resultado.Usuario is null)
                {
                    await _dialogService.MostrarErrorAsync(resultado.Mensaje);
                    Estado = resultado.Mensaje;
                    return;
                }

                UsuarioActual = resultado.Usuario;
                NombreNegocio = NegocioSeleccionado.Nombre;
                await PrepararContextoAsync(NegocioSeleccionado);
                await CargarDatosAsync();
                Password      = string.Empty;
                IsAuthenticated = true;
                Estado = $"Sesión iniciada · {UsuarioActual.NombreCompleto}";
            });
        }

        [RelayCommand]
        private void MostrarRegistro()
        {
            ModoRegistro  = true;
            PasoOnboarding = 1;
            Estado = "Complete los datos de su negocio";
        }

        [RelayCommand]
        private void CancelarRegistro()
        {
            ModoRegistro  = false;
            PasoOnboarding = 1;
            Estado = Negocios.Count > 0 ? "Listo" : "Complete el registro inicial";
        }

        [RelayCommand]
        private async Task SiguientePasoAsync()
        {
            if (PasoOnboarding == 1)
            {
                if (string.IsNullOrWhiteSpace(NombreNegocioNuevo))
                {
                    await _dialogService.MostrarAdvertenciaAsync("Escribe el nombre de tu negocio para continuar.");
                    return;
                }
                PasoOnboarding = 2;
                Estado = "Ahora dinos tu nombre";
                return;
            }

            if (PasoOnboarding == 2)
            {
                if (string.IsNullOrWhiteSpace(NombreCompleto))
                {
                    await _dialogService.MostrarAdvertenciaAsync("Escribe tu nombre para continuar.");
                    return;
                }
                PasoOnboarding = 3;
                Estado = "Crea tu usuario y contraseña";
            }
        }

        [RelayCommand]
        private void PasoAnterior()
        {
            if (PasoOnboarding > 1)
                PasoOnboarding--;
        }

        [RelayCommand]
        private async Task CrearCuentaAsync()
        {
            if (string.IsNullOrWhiteSpace(NombreNegocioNuevo))
            {
                await _dialogService.MostrarAdvertenciaAsync("Ingresa el nombre del negocio.");
                return;
            }
            if (string.IsNullOrWhiteSpace(NombreCompleto))
            {
                await _dialogService.MostrarAdvertenciaAsync("Ingresa tu nombre completo.");
                return;
            }
            if (string.IsNullOrWhiteSpace(NombreUsuario) || Password.Length < 6)
            {
                await _dialogService.MostrarAdvertenciaAsync("El usuario es obligatorio y la contraseña debe tener al menos 6 caracteres.");
                return;
            }

            await EjecutarAsync(async () =>
            {
                var negocio = await _negocioService.CrearNegocioAsync(new Negocio
                {
                    Nombre              = NombreNegocioNuevo.Trim(),
                    Moneda              = "BOB",
                    ZonaHoraria         = "America/La_Paz",
                    ImpuestoPorcentaje  = 13m
                });

                var sucursal = await _negocioService.CrearSucursalAsync(new Sucursal
                {
                    NegocioId = negocio.Id,
                    Nombre    = "Sucursal principal",
                    Codigo    = "001",
                    Activa    = true
                });

                await _inventoryService.CrearAlmacenAsync(new Almacen
                {
                    SucursalId = sucursal.Id,
                    Nombre     = "Almacén principal",
                    Codigo     = "ALM-001",
                    Activo     = true
                });

                var usuario = await _authService.CrearUsuarioAsync(new Mercurio.Application.DTOs.CrearUsuarioRequest
                {
                    NegocioId     = negocio.Id,
                    SucursalId    = sucursal.Id,
                    NombreUsuario = NombreUsuario.Trim(),
                    NombreCompleto = NombreCompleto.Trim(),
                    Password      = Password,
                    Rol           = Mercurio.Domain.Enums.RolUsuario.Administrador
                });

                Negocios.Add(negocio);
                NegocioSeleccionado = negocio;
                UsuarioActual       = usuario;
                NombreNegocio       = negocio.Nombre;
                await PrepararContextoAsync(negocio);
                await CargarDatosAsync();

                Password           = string.Empty;
                NombreUsuario      = string.Empty;
                NombreCompleto     = string.Empty;
                NombreNegocioNuevo = string.Empty;
                ModoRegistro       = false;
                IsAuthenticated    = true;
                Estado = "Negocio creado correctamente";
                await _dialogService.MostrarExitoAsync($"¡Bienvenido a Mercurio, {negocio.Nombre}!");
            });
        }

        [RelayCommand]
        private void CerrarSesion()
        {
            UsuarioActual   = null;
            Password        = string.Empty;
            IsAuthenticated = false;
            PaginaActual    = "Resumen";
            Estado          = "Sesión cerrada";
        }

        [RelayCommand]
        private async Task BuscarAsync()
        {
            await EjecutarAsync(async () =>
            {
                var productos = string.IsNullOrWhiteSpace(Busqueda)
                    ? await _catalogoService.GetProductosAsync()
                    : await _catalogoService.BuscarProductosAsync(Busqueda.Trim());

                Productos.Clear();
                foreach (var producto in productos)
                {
                    if (!producto.EsServicio)
                    {
                        var stock = await _inventoryService.GetStockProductoAsync(AlmacenId, producto.Id);
                        producto.StockActual = stock?.Cantidad ?? 0;
                    }
                    Productos.Add(producto);
                }

                Estado = $"{Productos.Count} producto(s)";
            });
        }

        [RelayCommand]
        private async Task AbrirNuevoProductoAsync()
        {
            var datos = await _dialogService.AbrirFormularioProductoAsync();
            if (datos is null)
                return;

            await EjecutarAsync(async () =>
            {
                var stockInicial = datos.StockInicial;
                var producto = await _catalogoService.CrearProductoAsync(new Product
                {
                    CodigoBarras = string.IsNullOrWhiteSpace(datos.CodigoBarras)
                        ? $"P{Guid.NewGuid():N}"[..9].ToUpperInvariant()
                        : datos.CodigoBarras,
                    Nombre       = datos.Nombre,
                    PrecioCompra = datos.PrecioCompra,
                    PrecioVenta  = datos.PrecioVenta,
                    StockMinimo  = datos.StockMinimo,
                    EsServicio   = datos.EsServicio,
                    Activo       = true
                });

                if (!producto.EsServicio && stockInicial > 0)
                {
                    await _inventoryService.RegistrarEntradaAsync(
                        AlmacenId,
                        producto.Id,
                        stockInicial,
                        datos.PrecioCompra,
                        "STOCK-INICIAL",
                        UsuarioActual?.Id,
                        "Stock inicial registrado al crear el producto.");
                }

                await _dialogService.MostrarExitoAsync(producto.EsServicio
                    ? "Servicio agregado al catálogo. No requiere control de stock."
                    : stockInicial > 0
                        ? "Producto agregado con stock inicial."
                        : "Producto agregado al catálogo. Registra una entrada o compra antes de vender.");
                await CargarDatosAsync();
            });
        }

        partial void OnProductoStockSeleccionadoChanged(Product? value)
        {
            _ = ActualizarStockSeleccionadoAsync(value);
        }

        private async Task ActualizarStockSeleccionadoAsync(Product? producto)
        {
            if (producto is null)
            {
                StockActualSeleccionado = 0;
                CostoEntradaStock = 0;
                return;
            }

            CostoEntradaStock = producto.PrecioCompra;
            try
            {
                var stock = await _inventoryService.GetStockProductoAsync(AlmacenId, producto.Id);
                StockActualSeleccionado = stock?.Cantidad ?? 0;
            }
            catch
            {
                StockActualSeleccionado = 0;
            }
        }

        [RelayCommand]
        private async Task RegistrarEntradaStockAsync()
        {
            if (ProductoStockSeleccionado is null)
            {
                await _dialogService.MostrarAdvertenciaAsync("Selecciona primero un producto en la lista.");
                return;
            }
            if (ProductoStockSeleccionado.EsServicio)
            {
                await _dialogService.MostrarInfoAsync("Los servicios no manejan existencias.");
                return;
            }
            if (CantidadEntradaStock <= 0)
            {
                await _dialogService.MostrarAdvertenciaAsync("La cantidad de entrada debe ser mayor a cero.");
                return;
            }
            if (CostoEntradaStock < 0)
            {
                await _dialogService.MostrarAdvertenciaAsync("El costo unitario no puede ser negativo.");
                return;
            }

            await EjecutarAsync(async () =>
            {
                await _inventoryService.RegistrarEntradaAsync(
                    AlmacenId,
                    ProductoStockSeleccionado.Id,
                    CantidadEntradaStock,
                    CostoEntradaStock,
                    "ENTRADA-MANUAL",
                    UsuarioActual?.Id,
                    "Entrada de stock desde Inventario.");

                var nombre = ProductoStockSeleccionado.Nombre;
                CantidadEntradaStock = 0;
                await ActualizarStockSeleccionadoAsync(ProductoStockSeleccionado);
                await _dialogService.MostrarExitoAsync($"Stock agregado para {nombre}. Ya puedes venderlo.");
                await CargarDatosAsync();
            });
        }

        partial void OnBusquedaChanged(string value)
        {
            if (string.IsNullOrEmpty(value))
                _ = BuscarAsync();
        }

        private async Task PrepararContextoAsync(Negocio negocio)
        {
            NegocioId     = negocio.Id;
            NombreNegocio = negocio.Nombre;

            var sucursales = (await _negocioService.GetSucursalesAsync(NegocioId)).ToList();
            if (sucursales.Count == 0)
            {
                sucursales.Add(await _negocioService.CrearSucursalAsync(new Sucursal
                {
                    NegocioId = NegocioId,
                    Nombre    = "Sucursal principal",
                    Codigo    = "001",
                    Activa    = true
                }));
            }

            var sucursal = sucursales[0];
            SucursalId    = sucursal.Id;
            NombreSucursal = sucursal.Nombre;

            var almacenes = (await _inventoryService.GetAlmacenesAsync(SucursalId)).ToList();
            if (almacenes.Count == 0)
            {
                almacenes.Add(await _inventoryService.CrearAlmacenAsync(new Almacen
                {
                    SucursalId = SucursalId,
                    Nombre     = "Almacén principal",
                    Codigo     = "ALM-001",
                    Activo     = true
                }));
            }

            AlmacenId = almacenes[0].Id;
        }

        private async Task CargarDatosAsync()
        {
            if (string.IsNullOrWhiteSpace(NegocioId) ||
                string.IsNullOrWhiteSpace(SucursalId) ||
                string.IsNullOrWhiteSpace(AlmacenId))
                return;

            await EjecutarAsync(async () =>
            {
                var productos = await _catalogoService.GetProductosAsync();
                var lista = productos.ToList();
                Productos.Clear();
                foreach (var producto in lista)
                {
                    if (!producto.EsServicio)
                    {
                        var stock = await _inventoryService.GetStockProductoAsync(AlmacenId, producto.Id);
                        producto.StockActual = stock?.Cantidad ?? 0;
                    }
                    Productos.Add(producto);
                }

                ProductosActivos = lista.Count;

                var desde = DateTime.Today.AddDays(-6);
                var hasta = DateTime.Today.AddDays(1).AddTicks(-1);
                var resumenVentas = (await _reporteService.GetResumenVentasAsync(
                    NegocioId, SucursalId, desde, hasta)).ToList();

                var ventasPorDia = resumenVentas
                    .GroupBy(x => x.Fecha.Date)
                    .ToDictionary(g => g.Key, g => new
                    {
                        Total = g.Sum(x => x.Total),
                        Cantidad = g.Sum(x => x.CantidadVentas)
                    });
                VentasSemana.Clear();
                EtiquetasVentasSemana.Clear();
                for (var dia = desde.Date; dia <= DateTime.Today; dia = dia.AddDays(1))
                {
                    var totalDia = ventasPorDia.TryGetValue(dia, out var resumenDia) ? resumenDia.Total : 0m;
                    VentasSemana.Add((double)totalDia);
                    EtiquetasVentasSemana.Add(dia.ToString("ddd dd", CultureInfo.GetCultureInfo("es-BO")).ToUpperInvariant());
                }

                VentasUltimosSieteDias = Math.Round((decimal)VentasSemana.Sum(), 2);
                HayVentasEnPeriodo = VentasSemana.Any(x => x > 0);

                VentasHoy = ventasPorDia.TryGetValue(DateTime.Today, out var resumenHoy)
                    ? resumenHoy.Total
                    : 0m;
                CantidadVentasHoy = resumenHoy?.Cantidad ?? 0;

                var valorizacion = await _reporteService.GetValorizacionInventarioAsync(
                    NegocioId, SucursalId);
                ValorInventario = valorizacion.Sum(x => x.Valor);

                var stockBajo = (await _reporteService.GetStockBajoAsync(
                    NegocioId, SucursalId)).ToList();
                ProductosStockBajo = stockBajo.Count;
                StockBajo.Clear();
                foreach (var item in stockBajo)
                {
                    StockBajo.Add(new StockBajoView(
                        item.ProductoNombre,
                        item.AlmacenNombre,
                        item.Cantidad,
                        item.StockMinimo));
                }

                Estado = "Información actualizada";
            });
        }

        private async Task EjecutarAsync(Func<Task> accion)
        {
            try
            {
                Cargando = true;
                await accion();
            }
            catch (BusinessException ex)
            {
                Estado = ex.Message;
                await _dialogService.MostrarErrorAsync(ex.Message);
            }
            catch (Exception ex)
            {
                var msg = "Ocurrió un error inesperado. Intenta nuevamente.";
                Estado = msg;
                await _dialogService.MostrarErrorAsync(msg);
                System.Diagnostics.Debug.WriteLine(ex);
            }
            finally
            {
                Cargando = false;
            }
        }
    }

    public sealed record StockBajoView(
        string Producto,
        string Almacen,
        int Cantidad,
        int StockMinimo);

    public sealed partial class LineaCompraView : ObservableObject
    {
        [ObservableProperty] private int _cantidad;
        [ObservableProperty] private decimal _costoUnitario;
        public event Action? TotalCambiado;

        public LineaCompraView(string productoId, string nombre, int cantidad, decimal costoUnitario)
        {
            ProductoId = productoId;
            Nombre = nombre;
            Cantidad = cantidad;
            CostoUnitario = costoUnitario;
        }

        public string ProductoId { get; }
        public string Nombre { get; }
        public decimal Total => Math.Round(Cantidad * CostoUnitario, 2);

        partial void OnCantidadChanged(int value)
        {
            if (value < 1) Cantidad = 1;
            OnPropertyChanged(nameof(Total));
            TotalCambiado?.Invoke();
        }

        partial void OnCostoUnitarioChanged(decimal value)
        {
            OnPropertyChanged(nameof(Total));
            TotalCambiado?.Invoke();
        }
    }

    public sealed partial class LineaVentaView : ObservableObject
    {
        [ObservableProperty] private int _cantidad;
        [ObservableProperty] private decimal _precioUnitario;
        public event Action? CantidadCambiada;

        public LineaVentaView(
            string productoId,
            string codigo,
            string nombre,
            int cantidad,
            decimal precioUnitario)
        {
            ProductoId     = productoId;
            Codigo         = codigo;
            Nombre         = nombre;
            Cantidad       = cantidad;
            PrecioUnitario = precioUnitario;
        }

        public string ProductoId { get; }
        public string Codigo { get; }
        public string Nombre { get; }
        public decimal Total => Math.Round(Cantidad * PrecioUnitario, 2);

        public void ActualizarTotales()
        {
            OnPropertyChanged(nameof(Total));
        }

        partial void OnCantidadChanged(int value)
        {
            if (value < 1)
                Cantidad = 1;
            OnPropertyChanged(nameof(Total));
            CantidadCambiada?.Invoke();
        }

        partial void OnPrecioUnitarioChanged(decimal value)
        {
            OnPropertyChanged(nameof(Total));
            CantidadCambiada?.Invoke();
        }
    }
}
