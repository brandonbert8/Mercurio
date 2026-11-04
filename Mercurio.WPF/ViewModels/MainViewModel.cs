// Mercurio.WPF/ViewModels/MainViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mercurio.Application.Interfaces;
using Mercurio.Application.Services;
using Mercurio.Domain.Entities;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Mercurio.WPF.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly ISaleService _saleService;

        [ObservableProperty]
        private ObservableCollection<Product> _productos = new();

        [ObservableProperty]
        private string _nuevoNombre = string.Empty;

        [ObservableProperty]
        private decimal _nuevoPrecio;

        // Inyección de dependencias directa al ViewModel
        public MainViewModel(ISaleService saleService)
        {
            _saleService = saleService;
        }

        [RelayCommand]
        private async Task CargarProductosAsync()
        {
            var inventario = await _saleService.ObtenerInventarioAsync();
            Productos.Clear();
            foreach (var item in inventario)
            {
                Productos.Add(item);
            }
        }

        [RelayCommand]
        private async Task GuardarProductoAsync()
        {
            if (string.IsNullOrWhiteSpace(NuevoNombre) || NuevoPrecio <= 0) return;

            var nuevo = new Product
            {
                Nombre = NuevoNombre,
                PrecioVenta = NuevoPrecio,
                CodigoBarras = Guid.NewGuid().ToString().Substring(0, 8)
            };

            await _saleService.CreateProductAsync(nuevo);
            await CargarProductosAsync();

            NuevoNombre = string.Empty;
            NuevoPrecio = 0;
        }
    }
}