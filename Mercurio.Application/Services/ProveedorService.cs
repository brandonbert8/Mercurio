using Mercurio.Application.Common;
using Mercurio.Application.Interfaces;
using Mercurio.Domain.Entities;
using Mercurio.Domain.Interfaces;

namespace Mercurio.Application.Services
{
    public class ProveedorService : IProveedorService
    {
        private readonly IProveedorRepository _proveedorRepository;

        public ProveedorService(IProveedorRepository proveedorRepository)
        {
            _proveedorRepository = proveedorRepository;
        }

        public Task<IEnumerable<Proveedor>> GetProveedoresAsync(string negocioId)
        {
            return _proveedorRepository.GetAllAsync(negocioId);
        }

        public Task<Proveedor?> GetProveedorAsync(string id)
        {
            return _proveedorRepository.GetByIdAsync(id);
        }

        public Task<IEnumerable<Proveedor>> BuscarProveedoresAsync(string negocioId, string termino)
        {
            if (string.IsNullOrWhiteSpace(termino))
                return _proveedorRepository.GetAllAsync(negocioId);

            return _proveedorRepository.BuscarAsync(negocioId, termino.Trim());
        }

        public async Task<Proveedor> CrearProveedorAsync(Proveedor proveedor)
        {
            if (string.IsNullOrWhiteSpace(proveedor.NegocioId))
                throw new BusinessException("El negocio es obligatorio.");
            if (string.IsNullOrWhiteSpace(proveedor.Nombre))
                throw new BusinessException("El nombre del proveedor es obligatorio.");

            await _proveedorRepository.InsertarAsync(proveedor);
            return proveedor;
        }

        public async Task ActualizarProveedorAsync(Proveedor proveedor)
        {
            var existente = await _proveedorRepository.GetByIdAsync(proveedor.Id);
            if (existente is null)
                throw new BusinessException("El proveedor no existe.");
            if (string.IsNullOrWhiteSpace(proveedor.Nombre))
                throw new BusinessException("El nombre del proveedor es obligatorio.");

            await _proveedorRepository.ActualizarAsync(proveedor);
        }

        public async Task DesactivarProveedorAsync(string id)
        {
            var existente = await _proveedorRepository.GetByIdAsync(id);
            if (existente is null)
                throw new BusinessException("El proveedor no existe.");

            await _proveedorRepository.DesactivarAsync(id);
        }
    }
}
