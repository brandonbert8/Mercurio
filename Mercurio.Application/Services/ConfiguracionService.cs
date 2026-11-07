using System.Globalization;
using Mercurio.Application.Common;
using Mercurio.Application.Interfaces;
using Mercurio.Domain.Entities;
using Mercurio.Domain.Interfaces;

namespace Mercurio.Application.Services
{
    public class ConfiguracionService : IConfiguracionService
    {
        private readonly IConfiguracionRepository _configuracionRepository;

        public ConfiguracionService(IConfiguracionRepository configuracionRepository)
        {
            _configuracionRepository = configuracionRepository;
        }

        public Task<IEnumerable<Configuracion>> ObtenerTodasAsync(string negocioId)
        {
            return _configuracionRepository.GetAllAsync(negocioId);
        }

        public Task<IEnumerable<Configuracion>> ObtenerPorGrupoAsync(string negocioId, string grupo)
        {
            return _configuracionRepository.GetByGrupoAsync(negocioId, grupo);
        }

        public async Task<string?> ObtenerValorAsync(string negocioId, string clave)
        {
            if (string.IsNullOrWhiteSpace(clave))
                throw new BusinessException("La clave de configuración es obligatoria.");

            var configuracion = await _configuracionRepository.GetByClaveAsync(negocioId, clave);
            return configuracion?.Valor;
        }

        public async Task<T> ObtenerValorAsync<T>(string negocioId, string clave, T valorPorDefecto)
        {
            var valor = await ObtenerValorAsync(negocioId, clave);
            if (string.IsNullOrWhiteSpace(valor))
                return valorPorDefecto;

            try
            {
                var tipo = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);
                if (tipo.IsEnum)
                    return (T)Enum.Parse(tipo, valor, true);

                var convertido = Convert.ChangeType(valor, tipo, CultureInfo.InvariantCulture);
                return (T)convertido;
            }
            catch
            {
                return valorPorDefecto;
            }
        }

        public async Task EstablecerValorAsync(string negocioId, string clave, string valor, string descripcion = "", string grupo = "General")
        {
            if (string.IsNullOrWhiteSpace(negocioId))
                throw new BusinessException("El negocio es obligatorio.");
            if (string.IsNullOrWhiteSpace(clave))
                throw new BusinessException("La clave de configuración es obligatoria.");

            var existente = await _configuracionRepository.GetByClaveAsync(negocioId, clave);
            var configuracion = existente ?? new Configuracion
            {
                NegocioId = negocioId,
                Clave = clave.Trim()
            };

            configuracion.Valor = valor ?? string.Empty;
            configuracion.Descripcion = descripcion ?? string.Empty;
            configuracion.Grupo = string.IsNullOrWhiteSpace(grupo) ? "General" : grupo;
            configuracion.FechaActualizacion = DateTime.Now;

            await _configuracionRepository.UpsertAsync(configuracion);
        }

        public async Task EliminarAsync(string negocioId, string clave)
        {
            if (string.IsNullOrWhiteSpace(clave))
                throw new BusinessException("La clave de configuración es obligatoria.");

            await _configuracionRepository.EliminarAsync(negocioId, clave);
        }
    }
}
