using Mercurio.WPF.Dialogs;
using System.Windows;
using System.Windows.Media;

namespace Mercurio.WPF
{
    public partial class CierreCajaDialog : Window
    {
        public CierreCajaDialog(ResumenCierreCajaDialog resumen)
        {
            InitializeComponent();

            TxtInicial.Text    = $"Bs. {resumen.MontoInicial:N2}";
            TxtVentas.Text     = $"+ Bs. {resumen.VentasEfectivo:N2}";
            TxtEsperado.Text   = $"Bs. {resumen.MontoEsperado:N2}";
            TxtDeclarado.Text  = $"Bs. {resumen.MontoDeclarado:N2}";

            if (resumen.CobrosEfectivo > 0)
            {
                TxtCobros.Text = $"+ Bs. {resumen.CobrosEfectivo:N2}";
                FilaCobros.Visibility = Visibility.Visible;
            }
            if (resumen.Ingresos > 0)
            {
                TxtIngresos.Text = $"+ Bs. {resumen.Ingresos:N2}";
                FilaIngresos.Visibility = Visibility.Visible;
            }
            if (resumen.Egresos > 0)
            {
                TxtEgresos.Text = $"- Bs. {resumen.Egresos:N2}";
                FilaEgresos.Visibility = Visibility.Visible;
            }

            // Diferencia con colores semafóricos
            var dif = resumen.Diferencia;
            if (dif == 0)
            {
                BorderDiferencia.Background = new SolidColorBrush(Color.FromRgb(0xEC, 0xFD, 0xF5));
                TxtDiferenciaLabel.Text = "✓  Cuadre perfecto";
                TxtDiferenciaLabel.Foreground = new SolidColorBrush(Color.FromRgb(0x16, 0xA3, 0x4A));
                TxtDiferencia.Text = "Bs. 0.00";
                TxtDiferencia.Foreground = new SolidColorBrush(Color.FromRgb(0x16, 0xA3, 0x4A));
            }
            else if (dif > 0)
            {
                BorderDiferencia.Background = new SolidColorBrush(Color.FromRgb(0xEC, 0xFD, 0xF5));
                TxtDiferenciaLabel.Text = "Sobrante";
                TxtDiferenciaLabel.Foreground = new SolidColorBrush(Color.FromRgb(0x16, 0xA3, 0x4A));
                TxtDiferencia.Text = $"+ Bs. {dif:N2}";
                TxtDiferencia.Foreground = new SolidColorBrush(Color.FromRgb(0x16, 0xA3, 0x4A));
            }
            else
            {
                BorderDiferencia.Background = new SolidColorBrush(Color.FromRgb(0xFE, 0xF2, 0xF2));
                TxtDiferenciaLabel.Text = "⚠  Faltante";
                TxtDiferenciaLabel.Foreground = new SolidColorBrush(Color.FromRgb(0xDC, 0x26, 0x26));
                TxtDiferencia.Text = $"- Bs. {Math.Abs(dif):N2}";
                TxtDiferencia.Foreground = new SolidColorBrush(Color.FromRgb(0xDC, 0x26, 0x26));

                // Advertencia de faltante
                BorderAdvertencia.Visibility = Visibility.Visible;
                TxtAdvertencia.Text = $"Hay un faltante de Bs. {Math.Abs(dif):N2}. Verifica los billetes antes de confirmar el cierre.";
            }
        }

        private void CerrarCaja_Click(object sender, RoutedEventArgs e)
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
