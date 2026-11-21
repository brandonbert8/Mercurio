using System.Windows.Controls;
using System.Windows.Data;
using Mercurio.Domain.Entities;

namespace Mercurio.WPF.Views;

public partial class SalesView : UserControl
{
    public SalesView()
    {
        InitializeComponent();
    }
}

public sealed class ProductoVendibleConverter : IValueConverter
{
    public static ProductoVendibleConverter Instance { get; } = new();

    public object Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        => value is Product producto && producto.Activo && (producto.EsServicio || producto.StockActual > 0);

    public object ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        => Binding.DoNothing;
}
