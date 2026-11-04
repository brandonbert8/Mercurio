using Mercurio.Domain.Interfaces;
using Mercurio.Infrastructure;
using Mercurio.Infrastructure.Repositories;
using Mercurio.WPF.ViewModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.IO;
using System.Windows;

namespace Mercurio.WPF
{
    public partial class App : System.Windows.Application // Soluciona la ambigüedad
    {
        private readonly IHost _host;

        public App()
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.File("logs/mercurio-log-.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            _host = Host.CreateDefaultBuilder()
                .UseSerilog()
                .ConfigureAppConfiguration((context, configurationBuilder) =>
                {
                    configurationBuilder.SetBasePath(Directory.GetCurrentDirectory());
                    configurationBuilder.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                })
                .ConfigureServices((context, services) =>
                {
                    string connectionString = context.Configuration.GetConnectionString("DefaultConnection")!;
                    services.AddSingleton(connectionString);
                    services.AddScoped<IProductRepository,ProductRepository>();
                    // Registrar capas
                    services.AddTransient<MainViewModel>();
                    services.AddTransient<MainWindow>(provider => new MainWindow
                    {
                        DataContext = provider.GetRequiredService<MainViewModel>()
                    });
                })
                .Build();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            try
            {
                Log.Information("Iniciando POS...");
                await _host.StartAsync();

                // Usamos la capa de Infraestructura para inicializar SQLite
                var connectionString = _host.Services.GetRequiredService<string>();
                DatabaseInitializer.Initialize(connectionString);

                var mainWindow = _host.Services.GetRequiredService<MainWindow>();
                mainWindow.Show();

                base.OnStartup(e);
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Error crítico durante el arranque.");
                MessageBox.Show("Ocurrió un error fatal. Revisa los logs.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Current.Shutdown();
            }
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            await _host.StopAsync();
            _host.Dispose();
            Log.CloseAndFlush();
            base.OnExit(e);
        }
    }
}