using Mercurio.Application.Common;
using Mercurio.Application.DTOs;
using Mercurio.Application.Interfaces;
using Mercurio.Domain.Entities;
using Mercurio.Domain.Interfaces;

namespace Mercurio.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IPasswordHasher _passwordHasher;

        public AuthService(IUsuarioRepository usuarioRepository, IPasswordHasher passwordHasher)
        {
            _usuarioRepository = usuarioRepository;
            _passwordHasher = passwordHasher;
        }

        public async Task<LoginResult> LoginAsync(LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.NegocioId) ||
                string.IsNullOrWhiteSpace(request.NombreUsuario) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                return new LoginResult { Exitoso = false, Mensaje = "Debe indicar el negocio, el usuario y la contraseña." };
            }

            var usuario = await _usuarioRepository.GetByNombreUsuarioAsync(request.NegocioId, request.NombreUsuario.Trim());

            if (usuario is null)
                return new LoginResult { Exitoso = false, Mensaje = "Usuario o contraseña incorrectos." };

            if (!usuario.Activo)
                return new LoginResult { Exitoso = false, Mensaje = "El usuario está inactivo." };

            if (!_passwordHasher.Verify(request.Password, usuario.PasswordSalt, usuario.PasswordHash))
                return new LoginResult { Exitoso = false, Mensaje = "Usuario o contraseña incorrectos." };

            usuario.UltimoAcceso = DateTime.Now;
            await _usuarioRepository.ActualizarUltimoAccesoAsync(usuario.Id, usuario.UltimoAcceso.Value);
            LimpiarCredenciales(usuario);

            return new LoginResult { Exitoso = true, Mensaje = "Inicio de sesión correcto.", Usuario = usuario };
        }

        public async Task<Usuario> CrearUsuarioAsync(CrearUsuarioRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.NegocioId))
                throw new BusinessException("El negocio es obligatorio.");
            if (string.IsNullOrWhiteSpace(request.NombreUsuario))
                throw new BusinessException("El nombre de usuario es obligatorio.");
            if (string.IsNullOrWhiteSpace(request.NombreCompleto))
                throw new BusinessException("El nombre completo es obligatorio.");
            if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 6)
                throw new BusinessException("La contraseña debe tener al menos 6 caracteres.");

            var existente = await _usuarioRepository.GetByNombreUsuarioAsync(request.NegocioId, request.NombreUsuario.Trim());
            if (existente is not null)
                throw new BusinessException("Ya existe un usuario con ese nombre en el negocio.");

            var salt = _passwordHasher.CreateSalt();
            var usuario = new Usuario
            {
                NegocioId = request.NegocioId,
                SucursalId = request.SucursalId,
                NombreUsuario = request.NombreUsuario.Trim(),
                NombreCompleto = request.NombreCompleto.Trim(),
                Email = request.Email?.Trim() ?? string.Empty,
                Rol = request.Rol,
                PasswordSalt = salt,
                PasswordHash = _passwordHasher.Hash(request.Password, salt)
            };

            await _usuarioRepository.InsertarAsync(usuario);
            LimpiarCredenciales(usuario);
            return usuario;
        }

        public async Task<bool> CambiarPasswordAsync(CambiarPasswordRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.PasswordNuevo) || request.PasswordNuevo.Length < 6)
                throw new BusinessException("La nueva contraseña debe tener al menos 6 caracteres.");

            var usuario = await _usuarioRepository.GetByIdAsync(request.UsuarioId);
            if (usuario is null)
                throw new BusinessException("El usuario no existe.");

            if (!_passwordHasher.Verify(request.PasswordActual, usuario.PasswordSalt, usuario.PasswordHash))
                throw new BusinessException("La contraseña actual es incorrecta.");

            var salt = _passwordHasher.CreateSalt();
            usuario.PasswordSalt = salt;
            usuario.PasswordHash = _passwordHasher.Hash(request.PasswordNuevo, salt);

            await _usuarioRepository.ActualizarAsync(usuario);
            return true;
        }

        public async Task<IEnumerable<Usuario>> GetUsuariosAsync(string negocioId)
        {
            var usuarios = (await _usuarioRepository.GetAllAsync(negocioId)).ToList();
            foreach (var usuario in usuarios)
                LimpiarCredenciales(usuario);

            return usuarios;
        }

        public async Task<Usuario?> GetUsuarioAsync(string usuarioId)
        {
            var usuario = await _usuarioRepository.GetByIdAsync(usuarioId);
            if (usuario is not null)
                LimpiarCredenciales(usuario);

            return usuario;
        }

        public async Task DesactivarUsuarioAsync(string usuarioId)
        {
            var usuario = await _usuarioRepository.GetByIdAsync(usuarioId);
            if (usuario is null)
                throw new BusinessException("El usuario no existe.");

            usuario.Activo = false;
            await _usuarioRepository.ActualizarAsync(usuario);
        }

        private static void LimpiarCredenciales(Usuario usuario)
        {
            usuario.PasswordHash = string.Empty;
            usuario.PasswordSalt = string.Empty;
        }
    }
}
