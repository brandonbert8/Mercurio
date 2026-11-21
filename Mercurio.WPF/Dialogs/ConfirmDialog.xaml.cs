using System.Windows;
using System.Windows.Media.Effects;

namespace Mercurio.WPF
{
    public partial class ConfirmDialog : Window
    {
        public ConfirmDialog(string titulo, string mensaje)
        {
            InitializeComponent();
            TxtTitulo.Text = titulo;
            TxtMensaje.Text = mensaje;
        }

        private void Confirmar_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void Cancelar_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            // Fade in
            Opacity = 0;
            var fadeIn = new System.Windows.Media.Animation.DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(150));
            BeginAnimation(OpacityProperty, fadeIn);
        }
    }
}
