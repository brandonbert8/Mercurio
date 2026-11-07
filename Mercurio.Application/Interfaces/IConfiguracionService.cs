using Mercurio.Domain.Entities;

namespace Mercurio.Application.Interfaces
{
    public interface IConfiguracionService
    {
        Task<IEnumerable<Configuracion>> ObtenerTodasAsync(string negocioId);
        Task<IEnumerable<Configuracion>> ObtenerPorGrupoAsync(string negocioId, string grupo);
        Task<string?> ObtenerValorAsync(string negocioId, string clave);
        Task<T> ObtenerValorAsync<T>(string negocioId, string clave, T valorPorDefecto);
        Task EstablecerValorAsync(string negocioId, string clave, string valor, string descripcion = "", string grupo = "General");
        Task EliminarAsync(string negocioId, string clave);
    }
}
