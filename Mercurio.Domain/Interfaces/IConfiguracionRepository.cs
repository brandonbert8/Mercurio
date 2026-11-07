using Mercurio.Domain.Entities;

namespace Mercurio.Domain.Interfaces
{
    public interface IConfiguracionRepository
    {
        Task<IEnumerable<Configuracion>> GetAllAsync(string negocioId);
        Task<IEnumerable<Configuracion>> GetByGrupoAsync(string negocioId, string grupo);
        Task<Configuracion?> GetByClaveAsync(string negocioId, string clave);
        Task UpsertAsync(Configuracion configuracion);
        Task EliminarAsync(string negocioId, string clave);
    }
}
