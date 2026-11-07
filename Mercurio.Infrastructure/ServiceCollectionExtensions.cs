using Mercurio.Application.Interfaces;
using Mercurio.Application.Security;
using Mercurio.Application.Services;
using Mercurio.Domain.Interfaces;
using Mercurio.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Mercurio.Infrastructure
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMercurioBusinessLogic(this IServiceCollection services, string connectionString)
        {
            services.AddSingleton(connectionString);

            services.AddSingleton<IPasswordHasher, PasswordHasher>();

            services.AddScoped<INegocioRepository, NegocioRepository>();
            services.AddScoped<ISucursalRepository, SucursalRepository>();
            services.AddScoped<IUsuarioRepository, UsuarioRepository>();
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<ICategoriaRepository, CategoriaRepository>();
            services.AddScoped<IInventoryRepository, InventoryRepository>();
            services.AddScoped<IProveedorRepository, ProveedorRepository>();
            services.AddScoped<ICompraRepository, CompraRepository>();
            services.AddScoped<ISaleRepository, SaleRepository>();
            services.AddScoped<IPagoRepository, PagoRepository>();
            services.AddScoped<IClienteRepository, ClienteRepository>();
            services.AddScoped<ICreditoRepository, CreditoRepository>();
            services.AddScoped<ICashRepository, CashRepository>();
            services.AddScoped<IConfiguracionRepository, ConfiguracionRepository>();
            services.AddScoped<IReporteRepository, ReporteRepository>();

            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<INegocioService, NegocioService>();
            services.AddScoped<ICatalogoService, CatalogoService>();
            services.AddScoped<IInventoryService, InventoryService>();
            services.AddScoped<IProveedorService, ProveedorService>();
            services.AddScoped<ICompraService, CompraService>();
            services.AddScoped<ISaleService, SaleService>();
            services.AddScoped<IClienteService, ClienteService>();
            services.AddScoped<ICreditoService, CreditoService>();
            services.AddScoped<ICajaService, CajaService>();
            services.AddScoped<IConfiguracionService, ConfiguracionService>();
            services.AddScoped<IReporteService, ReporteService>();

            return services;
        }
    }
}
