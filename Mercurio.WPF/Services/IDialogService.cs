using Mercurio.WPF.Dialogs;

namespace Mercurio.WPF.Services
{
    /// <summary>
    /// Servicio de diálogos y notificaciones visuales para la UI de Mercurio.
    /// Reemplaza los MessageBox nativos y los errores silenciosos.
    /// </summary>
    public interface IDialogService
    {
        /// <summary>Muestra un toast verde de éxito que desaparece solo.</summary>
        Task MostrarExitoAsync(string mensaje);

        /// <summary>Muestra un toast rojo de error persistente con botón para cerrar.</summary>
        Task MostrarErrorAsync(string mensaje);

        /// <summary>Muestra un toast azul informativo.</summary>
        Task MostrarInfoAsync(string mensaje);

        /// <summary>Muestra un banner de advertencia amarillo persistente.</summary>
        Task MostrarAdvertenciaAsync(string mensaje);

        Task<ProductoFormData?> AbrirFormularioProductoAsync();

        Task<ClienteFormData?> AbrirFormularioClienteAsync();

        Task<ProveedorFormData?> AbrirFormularioProveedorAsync();

        /// <summary>Diálogo de confirmación simple.</summary>
        Task<bool> ConfirmarAsync(string titulo, string mensaje);

        /// <summary>Diálogo de confirmación de venta con resumen de items y totales.</summary>
        Task<bool> ConfirmarVentaAsync(ResumenVentaDialog resumen);

        /// <summary>Diálogo de resumen de cierre de caja.</summary>
        Task<bool> ConfirmarCierreCajaAsync(ResumenCierreCajaDialog resumen);
    }
}
