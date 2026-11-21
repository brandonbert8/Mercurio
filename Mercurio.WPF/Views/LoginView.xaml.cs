using System.Windows;
using System.Windows.Controls;
using Mercurio.WPF.ViewModels;

namespace Mercurio.WPF.Views;

public partial class LoginView : UserControl
{
    public LoginView()
    {
        InitializeComponent();
    }

    private void LoginPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel viewModel && sender is PasswordBox passwordBox)
            viewModel.Password = passwordBox.Password;
    }

    private void SetupPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel viewModel && sender is PasswordBox passwordBox)
            viewModel.Password = passwordBox.Password;
    }
}
