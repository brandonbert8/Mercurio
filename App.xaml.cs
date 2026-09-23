using Mercurio.src.Database;
using System.Configuration;
using System.Data;
using System.Windows;

namespace Mercurio
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
                var configuration = new DatabaseConfiguration();
                var connectionFactory = new SqliteConnectionFactory(configuration);

                var databaseInitilizer = new DatabaseInitializer(configuration, connectionFactory);
                await databaseInitilizer.InitializeAsync();
                var mainWindow = new MainWindow();
                MainWindow = mainWindow;
                mainWindow.Show();
            }
            catch(Exception ex)
            {
                MessageBox.Show($"Error al inicializar la base de datos: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
            }
        }
    }

}
