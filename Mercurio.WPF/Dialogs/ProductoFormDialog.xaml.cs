using MaterialDesignThemes.Wpf;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace Mercurio.WPF.Dialogs;

public partial class ProductoFormDialog : UserControl
{
    public ProductoFormDialog()
    {
        InitializeComponent();
    }

    public ProductoFormData? Datos { get; private set; }

    private void ServicioCheckBox_Changed(object sender, RoutedEventArgs e)
    {
        if (ExistenciasPanel is not null)
            ExistenciasPanel.IsEnabled = ServicioCheckBox.IsChecked != true;
    }

    private void Cancelar_Click(object sender, RoutedEventArgs e)
        => DialogHost.CloseDialogCommand.Execute(null, (Button)sender);

    private void Guardar_Click(object sender, RoutedEventArgs e)
    {
        ErrorText.Text = string.Empty;
        if (string.IsNullOrWhiteSpace(NombreBox.Text))
        {
            ErrorText.Text = "Escribe el nombre del producto.";
            NombreBox.Focus();
            return;
        }
        if (!TryDecimal(PrecioCompraBox.Text, out var compra) || compra < 0)
        {
            ErrorText.Text = "El precio de compra debe ser igual o mayor a cero.";
            PrecioCompraBox.Focus();
            return;
        }
        if (!TryDecimal(PrecioVentaBox.Text, out var venta) || venta < 0)
        {
            ErrorText.Text = "El precio de venta debe ser igual o mayor a cero.";
            PrecioVentaBox.Focus();
            return;
        }

        var servicio = ServicioCheckBox.IsChecked == true;
        var stockInicial = 0;
        var stockMinimo = 0;
        if (!servicio && (!int.TryParse(StockInicialBox.Text, out stockInicial) || stockInicial < 0))
        {
            ErrorText.Text = "El stock inicial debe ser un entero igual o mayor a cero.";
            StockInicialBox.Focus();
            return;
        }
        if (!servicio && (!int.TryParse(StockMinimoBox.Text, out stockMinimo) || stockMinimo < 0))
        {
            ErrorText.Text = "El stock mínimo debe ser un entero igual o mayor a cero.";
            StockMinimoBox.Focus();
            return;
        }

        Datos = new ProductoFormData(
            NombreBox.Text.Trim(), CodigoBox.Text.Trim(), compra, venta,
            stockMinimo, stockInicial, servicio);
        DialogHost.CloseDialogCommand.Execute(Datos, (Button)sender);
    }

    private static bool TryDecimal(string? value, out decimal result)
    {
        const NumberStyles inputStyle = NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign;
        return decimal.TryParse(value, inputStyle, CultureInfo.CurrentCulture, out result)
               || decimal.TryParse(value, inputStyle, CultureInfo.InvariantCulture, out result);
    }
}
