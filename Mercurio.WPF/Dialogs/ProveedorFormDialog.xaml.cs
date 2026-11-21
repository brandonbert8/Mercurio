using MaterialDesignThemes.Wpf;
using System.Windows;
using System.Windows.Controls;

namespace Mercurio.WPF.Dialogs;

public partial class ProveedorFormDialog : UserControl
{
    public ProveedorFormDialog()
    {
        InitializeComponent();
    }

    public ProveedorFormData? Datos { get; private set; }

    private void Cancelar_Click(object sender, RoutedEventArgs e)
        => DialogHost.CloseDialogCommand.Execute(null, (Button)sender);

    private void Guardar_Click(object sender, RoutedEventArgs e)
    {
        ErrorText.Text = string.Empty;
        if (string.IsNullOrWhiteSpace(NombreBox.Text))
        {
            ErrorText.Text = "Escribe el nombre del proveedor.";
            NombreBox.Focus();
            return;
        }

        Datos = new ProveedorFormData(
            NombreBox.Text.Trim(), ContactoBox.Text.Trim(), TelefonoBox.Text.Trim(),
            RucBox.Text.Trim(), EmailBox.Text.Trim());
        DialogHost.CloseDialogCommand.Execute(Datos, (Button)sender);
    }
}
