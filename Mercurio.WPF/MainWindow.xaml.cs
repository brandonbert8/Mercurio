using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using Mercurio.WPF.ViewModels;
using Wpf.Ui.Controls;
using Wpf.Ui;

namespace Mercurio.WPF;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        ActualizarGlyphMaximizar();
    }

    internal void ConfigureUiServices(ISnackbarService snackbars)
    {
        snackbars.SetSnackbarPresenter(SnackbarPresenter);
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel viewModel)
        {
            ActualizarNavActivo(viewModel.PaginaActual);
            await viewModel.InicializarAsync();
        }
    }

    private void MinimizeWindow(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void ToggleMaximizeWindow(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState == WindowState.Maximized
            ? WindowState.Normal
            : WindowState.Maximized;
    }

    private void CloseWindow(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void Window_StateChanged(object? sender, EventArgs e) => ActualizarGlyphMaximizar();

    private void ActualizarGlyphMaximizar()
    {
        if (MaximizeWindowGlyph is not null)
            MaximizeWindowGlyph.Text = WindowState == WindowState.Maximized ? "\uE923" : "\uE922";
    }

    internal void ActualizarNavActivo(string pagina)
    {
        NavigationSidebar?.ActualizarNavActivo(pagina);
    }
}

public sealed class PaginaVisibilityConverter : IValueConverter
{
    public static PaginaVisibilityConverter Instance { get; } = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => string.Equals(value?.ToString(), parameter?.ToString(), StringComparison.OrdinalIgnoreCase)
            ? Visibility.Visible
            : Visibility.Collapsed;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => Binding.DoNothing;
}

public sealed class BoolVisibilityConverter : IValueConverter
{
    public static BoolVisibilityConverter Instance { get; } = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is true ? Visibility.Visible : Visibility.Collapsed;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => Binding.DoNothing;
}

public sealed class InverseBoolVisibilityConverter : IValueConverter
{
    public static InverseBoolVisibilityConverter Instance { get; } = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is true ? Visibility.Collapsed : Visibility.Visible;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => Binding.DoNothing;
}

public sealed class EqualsOneVisibilityConverter : IValueConverter
{
    public static EqualsOneVisibilityConverter Instance { get; } = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => Equals(value, 1) ? Visibility.Visible : Visibility.Collapsed;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => Binding.DoNothing;
}

public sealed class EqualsTwoVisibilityConverter : IValueConverter
{
    public static EqualsTwoVisibilityConverter Instance { get; } = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => Equals(value, 2) ? Visibility.Visible : Visibility.Collapsed;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => Binding.DoNothing;
}

public sealed class EqualsThreeVisibilityConverter : IValueConverter
{
    public static EqualsThreeVisibilityConverter Instance { get; } = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => Equals(value, 3) ? Visibility.Visible : Visibility.Collapsed;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => Binding.DoNothing;
}

public sealed class NotThreeVisibilityConverter : IValueConverter
{
    public static NotThreeVisibilityConverter Instance { get; } = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => Equals(value, 3) ? Visibility.Collapsed : Visibility.Visible;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => Binding.DoNothing;
}

public sealed class TituloRegistroConverter : IValueConverter
{
    public static TituloRegistroConverter Instance { get; } = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is true ? "Configura tu negocio" : "Bienvenido a Mercurio";

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => Binding.DoNothing;
}

public sealed class SubtituloRegistroConverter : IValueConverter
{
    public static SubtituloRegistroConverter Instance { get; } = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is true
            ? "Crea tu acceso y deja listo tu negocio para comenzar."
            : "Inicia sesi\u00f3n para continuar.";

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => Binding.DoNothing;
}

public sealed class FechaVencimientoColorConverter : IValueConverter
{
    public static FechaVencimientoColorConverter Instance { get; } = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not DateTime fecha)
            return Brushes.Gray;

        if (fecha.Date < DateTime.Today)
            return new SolidColorBrush(Color.FromRgb(0xDC, 0x26, 0x26));

        if (fecha.Date <= DateTime.Today.AddDays(7))
            return new SolidColorBrush(Color.FromRgb(0xD9, 0x77, 0x06));

        return new SolidColorBrush(Color.FromRgb(0x74, 0x77, 0x84));
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => Binding.DoNothing;
}

public sealed class CajaBadgeColorConverter : IValueConverter
{
    public static CajaBadgeColorConverter Instance { get; } = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is true
            ? new SolidColorBrush(Color.FromRgb(0x16, 0xA3, 0x4A))
            : new SolidColorBrush(Color.FromRgb(0xDC, 0x26, 0x26));

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => Binding.DoNothing;
}
