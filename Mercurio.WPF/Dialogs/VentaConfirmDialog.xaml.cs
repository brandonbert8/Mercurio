using Mercurio.WPF.Dialogs;
using System.Windows;

namespace Mercurio.WPF
{
    public partial class VentaConfirmDialog : Window
    {
        public VentaConfirmDialog(ResumenVentaDialog resumen)
        {
            InitializeComponent();

            // Cliente
            TxtCliente.Text = string.IsNullOrWhiteSpace(resumen.Cliente)
                ? "Cliente de mostrador"
                : resumen.Cliente;

            // Items
            ItemsLista.ItemsSource = resumen.Items;

            // Totales
            TxtTotal.Text = $"Bs. {resumen.Total:N2}";
            TxtMetodo.Text = resumen.MetodoPago;

            if (resumen.Descuento > 0)
            {
                TxtDescuento.Text = $"-Bs. {resumen.Descuento:N2}";
                FilaDescuento.Visibility = Visibility.Visible;
            }

            if (resumen.MontoPagado > 0)
            {
                TxtMontoPagado.Text = $"Bs. {resumen.MontoPagado:N2}";
                FilaMontoPagado.Visibility = Visibility.Visible;
            }

            if (resumen.Vuelto > 0)
            {
                TxtVuelto.Text = $"Bs. {resumen.Vuelto:N2}";
                FilaVuelto.Visibility = Visibility.Visible;
            }
        }

        private void Confirmar_Click(object sender, RoutedEventArgs e)
            => DialogResult = true;

        private void Cancelar_Click(object sender, RoutedEventArgs e)
            => DialogResult = false;

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            Opacity = 0;
            var fadeIn = new System.Windows.Media.Animation.DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(150));
            BeginAnimation(OpacityProperty, fadeIn);
        }
    }
}
