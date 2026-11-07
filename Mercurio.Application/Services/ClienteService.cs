using Mercurio.Application.Common;
using Mercurio.Application.Interfaces;
using Mercurio.Domain.Entities;
using Mercurio.Domain.Interfaces;

namespace Mercurio.Application.Services
{
    public class ClienteService : IClienteService
    {
        private readonly IClienteRepository _clienteRepository;
        private readonly ICreditoRepository _creditoRepository;

        public ClienteService(IClienteRepository clienteRepository, ICreditoRepository creditoRepository)
        {
            _clienteRepository = clienteRepository;
            _creditoRepository = creditoRepository;
        }

        public Task<IEnumerable<Cliente>> GetClientesAsync(string negocioId)
        {
            return _clienteRepository.GetAllAsync(negocioId);
        }

        public Task<Cliente?> GetClienteAsync(string id)
        {
            return _clienteRepository.GetByIdAsync(id);
        }

        public Task<Cliente?> GetClientePorDocumentoAsync(string negocioId, string numeroDocumento)
        {
            if (string.IsNullOrWhiteSpace(numeroDocumento))
                return Task.FromResult<Cliente?>(null);

            return _clienteRepository.GetByDocumentoAsync(negocioId, numeroDocumento.Trim());
        }

        public Task<IEnumerable<Cliente>> BuscarClientesAsync(string negocioId, string termino)
        {
            if (string.IsNullOrWhiteSpace(termino))
                return _clienteRepository.GetAllAsync(negocioId);

            return _clienteRepository.BuscarAsync(negocioId, termino.Trim());
        }

        public async Task<Cliente> CrearClienteAsync(Cliente cliente)
        {
            ValidarCliente(cliente);

            if (!string.IsNullOrWhiteSpace(cliente.NumeroDocumento))
            {
                var existente = await _clienteRepository.GetByDocumentoAsync(cliente.NegocioId, cliente.NumeroDocumento.Trim());
                if (existente is not null)
                    throw new BusinessException("Ya existe un cliente con ese documento.");
            }

            await _clienteRepository.InsertarAsync(cliente);
            return cliente;
        }

        public async Task ActualizarClienteAsync(Cliente cliente)
        {
            var existente = await _clienteRepository.GetByIdAsync(cliente.Id);
            if (existente is null)
                throw new BusinessException("El cliente no existe.");

            ValidarCliente(cliente);
            await _clienteRepository.ActualizarAsync(cliente);
        }

        public async Task DesactivarClienteAsync(string id)
        {
            var existente = await _clienteRepository.GetByIdAsync(id);
            if (existente is null)
                throw new BusinessException("El cliente no existe.");

            var saldo = await _creditoRepository.GetSaldoClienteAsync(id);
            if (saldo > 0)
                throw new BusinessException("No se puede desactivar un cliente con saldo pendiente.");

            await _clienteRepository.DesactivarAsync(id);
        }

        public async Task<bool> PuedeComprarAlCreditoAsync(string clienteId, decimal monto)
        {
            if (monto <= 0)
                return true;

            var cliente = await _clienteRepository.GetByIdAsync(clienteId);
            if (cliente is null || !cliente.Activo || cliente.LimiteCredito <= 0)
                return false;

            var saldo = await _creditoRepository.GetSaldoClienteAsync(clienteId);
            return saldo + monto <= cliente.LimiteCredito;
        }

        private static void ValidarCliente(Cliente cliente)
        {
            if (string.IsNullOrWhiteSpace(cliente.NegocioId))
                throw new BusinessException("El negocio es obligatorio.");
            if (string.IsNullOrWhiteSpace(cliente.Nombre))
                throw new BusinessException("El nombre del cliente es obligatorio.");
            if (cliente.LimiteCredito < 0)
                throw new BusinessException("El límite de crédito no puede ser negativo.");
            if (string.IsNullOrWhiteSpace(cliente.TipoDocumento))
                cliente.TipoDocumento = "DNI";
        }
    }
}
