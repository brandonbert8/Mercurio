using Mercurio.Infrastructure;
using Mercurio.WPF.Services;
using Mercurio.WPF.ViewModels;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Windows;

namespace Mercurio.WPF
{
    public partial class App : System.Windows.Application
    {
        private readonly IHost _host;
        private IServiceScope? _scope;

        public App()
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.File("logs/mercurio-log-.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            _host = Host.CreateDefaultBuilder()
                .UseSerilog()
                .ConfigureAppConfiguration((context, builder) =>
                {
                    builder.SetBasePath(AppContext.BaseDirectory);
                    builder.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                })
                .ConfigureServices((context, services) =>
                {
                    string connectionString = context.Configuration.GetConnectionString("DefaultConnection")!;
                    services.AddMercurioBusinessLogic(connectionString);

                    // IDialogService como Singleton lazy: se inicializa la primera vez que se pide,
                    // después de que MainWindow haya sido creada manualmente.
                    services.AddSingleton<IDialogService, LazyDialogService>();
                    services.AddTransient<MainViewModel>();
                })
                .Build();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            try
            {
                LiveCharts.Configure(settings => settings.AddSkiaSharp().AddDefaultMappers().AddDefaultTheme());
                Log.Information("Iniciando POS Mercurio...");
                await _host.StartAsync();

                var connectionString = _host.Services.GetRequiredService<string>();
                DatabaseInitializer.Initialize(connectionString);

                _scope = _host.Services.CreateScope();
                var sp = _scope.ServiceProvider;

                // Crear ViewModel (ya tiene IDialogService inyectado)
                var vm = sp.GetRequiredService<MainViewModel>();

                // Crear la ventana y pasarle el ViewModel
                var mainWindow = new MainWindow { DataContext = vm };

                // Ahora que la ventana existe, inicializar el DialogService con ella
                var lazy = (LazyDialogService)sp.GetRequiredService<IDialogService>();
                lazy.Initialize(mainWindow);

                mainWindow.Show();
                base.OnStartup(e);
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Error crítico durante el arranque.");
                MessageBox.Show("Ocurrió un error fatal. Revisa los logs.", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Current.Shutdown();
            }
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            _scope?.Dispose();
            await _host.StopAsync();
            _host.Dispose();
            Log.CloseAndFlush();
            base.OnExit(e);
        }
    }
}
