using Mercurio.WPF.Dialogs;
using MaterialDesignThemes.Wpf;
using System.Windows;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace Mercurio.WPF.Services;

public sealed class LazyDialogService : IDialogService
{
    private MainWindow? _window;
    private ISnackbarService? _snackbars;

    private MainWindow Window => _window
        ?? throw new InvalidOperationException("El servicio visual todavía no se inicializó.");

    private ISnackbarService Snackbars => _snackbars
        ?? throw new InvalidOperationException("El servicio visual todavía no se inicializó.");

    public void Initialize(MainWindow window)
    {
        _window = window;
        _snackbars = new SnackbarService();
        window.ConfigureUiServices(_snackbars);
    }

    public Task MostrarExitoAsync(string mensaje)
        => MostrarToastAsync("Listo", mensaje, ControlAppearance.Success, TimeSpan.FromSeconds(4));

    public Task MostrarErrorAsync(string mensaje)
        => MostrarToastAsync("No se pudo completar", mensaje, ControlAppearance.Danger, TimeSpan.FromSeconds(8));

    public Task MostrarInfoAsync(string mensaje)
        => MostrarToastAsync("Información", mensaje, ControlAppearance.Info, TimeSpan.FromSeconds(5));

    public Task MostrarAdvertenciaAsync(string mensaje)
        => MostrarToastAsync("Revisa estos datos", mensaje, ControlAppearance.Caution, TimeSpan.FromSeconds(7));

    private Task MostrarToastAsync(string titulo, string mensaje, ControlAppearance appearance, TimeSpan duration)
    {
        Window.Dispatcher.Invoke(() =>
            Snackbars.Show(titulo, mensaje, appearance, null!, duration));
        return Task.CompletedTask;
    }

    public async Task<ProductoFormData?> AbrirFormularioProductoAsync()
    {
        var dialog = new ProductoFormDialog();
        var result = await DialogHost.Show(dialog, "MercurioDialogHost");
        return result as ProductoFormData;
    }

    public async Task<ClienteFormData?> AbrirFormularioClienteAsync()
    {
        var dialog = new ClienteFormDialog();
        var result = await DialogHost.Show(dialog, "MercurioDialogHost");
        return result as ClienteFormData;
    }

    public async Task<ProveedorFormData?> AbrirFormularioProveedorAsync()
    {
        var dialog = new ProveedorFormDialog();
        var result = await DialogHost.Show(dialog, "MercurioDialogHost");
        return result as ProveedorFormData;
    }

    public Task<bool> ConfirmarAsync(string titulo, string mensaje)
        => MostrarVentanaAsync(() => new ConfirmDialog(titulo, mensaje));

    public Task<bool> ConfirmarVentaAsync(ResumenVentaDialog resumen)
        => MostrarVentanaAsync(() => new VentaConfirmDialog(resumen));

    public Task<bool> ConfirmarCierreCajaAsync(ResumenCierreCajaDialog resumen)
        => MostrarVentanaAsync(() => new CierreCajaDialog(resumen));

    private Task<bool> MostrarVentanaAsync<TWindow>(Func<TWindow> crear)
        where TWindow : Window
    {
        var resultado = false;
        Window.Dispatcher.Invoke(() =>
        {
            var dialog = crear();
            dialog.Owner = Window;
            resultado = dialog.ShowDialog() == true;
        });
        return Task.FromResult(resultado);
    }
}
