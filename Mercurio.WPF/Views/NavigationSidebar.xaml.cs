using System.Windows;
using System.Windows.Controls;

namespace Mercurio.WPF.Views;

public partial class NavigationSidebar : UserControl
{
    public NavigationSidebar()
    {
        InitializeComponent();
    }

    public void ActualizarNavActivo(string pagina)
    {
        var navButtons = new[]
        {
            ("Resumen", NavResumen),
            ("Ventas", NavVentas),
            ("Compras", NavCompras),
            ("Proveedores", NavProveedores),
            ("Inventario", NavInventario),
            ("Clientes", NavClientes),
            ("Creditos", NavCreditos),
            ("Caja", NavCaja),
            ("Reportes", NavReportes),
            ("Configuracion", NavConfiguracion),
        };

        foreach (var (nombre, boton) in navButtons)
        {
            if (boton is null)
                continue;

            boton.Style = string.Equals(nombre, pagina, StringComparison.OrdinalIgnoreCase)
                ? (Style)TryFindResource("NavItemActivo")!
                : (Style)TryFindResource("NavItem")!;
        }
    }
}
