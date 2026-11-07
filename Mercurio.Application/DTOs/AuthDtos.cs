namespace Mercurio.Application.DTOs
{
    public class LoginRequest
    {
        public string NegocioId { get; set; } = string.Empty;
        public string NombreUsuario { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class LoginResult
    {
        public bool Exitoso { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public Domain.Entities.Usuario? Usuario { get; set; }
    }

    public class CrearUsuarioRequest
    {
        public string NegocioId { get; set; } = string.Empty;
        public string? SucursalId { get; set; }
        public string NombreUsuario { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public Domain.Enums.RolUsuario Rol { get; set; } = Domain.Enums.RolUsuario.Cajero;
    }

    public class CambiarPasswordRequest
    {
        public string UsuarioId { get; set; } = string.Empty;
        public string PasswordActual { get; set; } = string.Empty;
        public string PasswordNuevo { get; set; } = string.Empty;
    }
}
