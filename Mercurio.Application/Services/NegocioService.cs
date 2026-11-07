using Mercurio.Application.Common;
using Mercurio.Application.Interfaces;
using Mercurio.Domain.Entities;
using Mercurio.Domain.Interfaces;

namespace Mercurio.Application.Services
{
    public class NegocioService : INegocioService
    {
        private readonly INegocioRepository _negocioRepository;
        private readonly ISucursalRepository _sucursalRepository;

        public NegocioService(INegocioRepository negocioRepository, ISucursalRepository sucursalRepository)
        {
            _negocioRepository = negocioRepository;
            _sucursalRepository = sucursalRepository;
        }

        public Task<IEnumerable<Negocio>> GetNegociosAsync()
        {
            return _negocioRepository.GetAllAsync();
        }

        public Task<Negocio?> GetNegocioAsync(string id)
        {
            return _negocioRepository.GetByIdAsync(id);
        }

        public async Task<Negocio> CrearNegocioAsync(Negocio negocio)
        {
            if (string.IsNullOrWhiteSpace(negocio.Nombre))
                throw new BusinessException("El nombre del negocio es obligatorio.");

            if (string.IsNullOrWhiteSpace(negocio.Moneda))
                negocio.Moneda = "PEN";

            await _negocioRepository.InsertarAsync(negocio);
            return negocio;
        }

        public async Task ActualizarNegocioAsync(Negocio negocio)
        {
            var existente = await _negocioRepository.GetByIdAsync(negocio.Id);
            if (existente is null)
                throw new BusinessException("El negocio no existe.");

            if (string.IsNullOrWhiteSpace(negocio.Nombre))
                throw new BusinessException("El nombre del negocio es obligatorio.");

            await _negocioRepository.ActualizarAsync(negocio);
        }

        public Task<IEnumerable<Sucursal>> GetSucursalesAsync(string negocioId)
        {
            return _sucursalRepository.GetAllAsync(negocioId);
        }

        public Task<Sucursal?> GetSucursalAsync(string id)
        {
            return _sucursalRepository.GetByIdAsync(id);
        }

        public async Task<Sucursal> CrearSucursalAsync(Sucursal sucursal)
        {
            if (string.IsNullOrWhiteSpace(sucursal.NegocioId))
                throw new BusinessException("El negocio es obligatorio.");
            if (string.IsNullOrWhiteSpace(sucursal.Nombre))
                throw new BusinessException("El nombre de la sucursal es obligatorio.");
            if (string.IsNullOrWhiteSpace(sucursal.Codigo))
                throw new BusinessException("El código de la sucursal es obligatorio.");

            var negocio = await _negocioRepository.GetByIdAsync(sucursal.NegocioId);
            if (negocio is null)
                throw new BusinessException("El negocio no existe.");

            await _sucursalRepository.InsertarAsync(sucursal);
            return sucursal;
        }

        public async Task ActualizarSucursalAsync(Sucursal sucursal)
        {
            var existente = await _sucursalRepository.GetByIdAsync(sucursal.Id);
            if (existente is null)
                throw new BusinessException("La sucursal no existe.");

            if (string.IsNullOrWhiteSpace(sucursal.Nombre))
                throw new BusinessException("El nombre de la sucursal es obligatorio.");

            await _sucursalRepository.ActualizarAsync(sucursal);
        }
    }
}
