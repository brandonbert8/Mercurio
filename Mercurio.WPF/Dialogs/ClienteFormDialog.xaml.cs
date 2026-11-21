using MaterialDesignThemes.Wpf;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace Mercurio.WPF.Dialogs;

public partial class ClienteFormDialog : UserControl
{
    public ClienteFormDialog()
    {
        InitializeComponent();
    }

    public ClienteFormData? Datos { get; private set; }

    private void Cancelar_Click(object sender, RoutedEventArgs e)
        => DialogHost.CloseDialogCommand.Execute(null, (Button)sender);

    private void Guardar_Click(object sender, RoutedEventArgs e)
    {
        ErrorText.Text = string.Empty;
        if (string.IsNullOrWhiteSpace(NombreBox.Text))
        {
            ErrorText.Text = "Escribe el nombre del cliente.";
            NombreBox.Focus();
            return;
        }

        const NumberStyles inputStyle = NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign;
        if (!decimal.TryParse(CreditoBox.Text, inputStyle, CultureInfo.CurrentCulture, out var credito)
            && !decimal.TryParse(CreditoBox.Text, inputStyle, CultureInfo.InvariantCulture, out credito))
        {
            ErrorText.Text = "Escribe un límite de crédito válido.";
            CreditoBox.Focus();
            return;
        }
        if (credito < 0)
        {
            ErrorText.Text = "El límite de crédito no puede ser negativo.";
            CreditoBox.Focus();
            return;
        }

        Datos = new ClienteFormData(
            NombreBox.Text.Trim(), DocumentoBox.Text.Trim(), TelefonoBox.Text.Trim(), credito);
        DialogHost.CloseDialogCommand.Execute(Datos, (Button)sender);
    }
}
