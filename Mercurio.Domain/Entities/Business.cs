using Mercurio.Domain.Enums;

namespace Mercurio.Domain.Entities
{
    public class Negocio
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Nombre { get; set; } = string.Empty;
        public string RazonSocial { get; set; } = string.Empty;
        public string Ruc { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Moneda { get; set; } = "PEN";
        public string ZonaHoraria { get; set; } = "America/Lima";
        public bool Activo { get; set; } = true;
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
    }

    public class Sucursal
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string NegocioId { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Codigo { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public bool Activa { get; set; } = true;
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
    }

    public class Usuario
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string NegocioId { get; set; } = string.Empty;
        public string? SucursalId { get; set; }
        public string NombreUsuario { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string PasswordSalt { get; set; } = string.Empty;
        public RolUsuario Rol { get; set; } = RolUsuario.Cajero;
        public bool Activo { get; set; } = true;
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        public DateTime? UltimoAcceso { get; set; }
    }
}
