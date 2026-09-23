using Mercurio.Views;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Mercurio
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            MainContent.Content = new AuthView();
        }

       
    }
}