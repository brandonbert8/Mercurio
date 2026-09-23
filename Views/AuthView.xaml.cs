using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Mercurio.Views
{
    /// <summary>
    /// Interaction logic for AuthView.xaml
    /// </summary>
    public partial class AuthView : UserControl
    {
        private bool _isSignup;
        public AuthView()
        {
            InitializeComponent();
        }

        private void ShowSignup_Click(
       object sender,
       RoutedEventArgs e)
        {
            if (_isSignup)
                return;

            _isSignup = true;

            AnimateSwitch(
                LoginPanel,
                SignupPanel,
                LoginTransform,
                SignupTransform
            );
        }

        private void ShowLogin_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (!_isSignup)
                return;

            _isSignup = false;

            AnimateSwitch(
                SignupPanel,
                LoginPanel,
                SignupTransform,
                LoginTransform
            );
        }
        private static void AnimateSwitch(
       UIElement current,
       UIElement next,
       TranslateTransform currentTransform,
       TranslateTransform nextTransform)
        {
            next.Visibility = Visibility.Visible;

            nextTransform.X = 18;

            var fadeOut = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(120)
            };

            var fadeIn = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(220)
            };

            var slide = new DoubleAnimation
            {
                From = 18,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(220),
                EasingFunction = new CubicEase
                {
                    EasingMode = EasingMode.EaseOut
                }
            };

            current.BeginAnimation(
                OpacityProperty,
                fadeOut
            );

            next.BeginAnimation(
                OpacityProperty,
                fadeIn
            );

            nextTransform.BeginAnimation(
                TranslateTransform.XProperty,
                slide
            );

            fadeOut.Completed += (_, _) =>
            {
                current.Visibility = Visibility.Collapsed;
                current.Opacity = 1;
            };
        }
        private void SignIn_Click(object sender, RoutedEventArgs e)
        {
            string username = txtLoginUsername.Text;
            string password = txtLoginPassword.Password;

            // TODO: Agregar lógica de autenticación con la base de datos
        }

        // 3. Evento del botón para crear cuenta (en la vista de registro)
        private void CreateAccount_Click(object sender, RoutedEventArgs e)
        {
            string displayName = txtSignupDisplayName.Text;
            string username = txtSignupUsername.Text;
            string password = txtSignupPassword.Password;

            // TODO: Agregar lógica de registro de usuario en la base de datos
        }
        private void txtLoginPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                // Aquí puedes llamar a tu lógica de inicio de sesión
                SignIn_Click(btnLogin, new RoutedEventArgs());
            }
        }
    }
}
