namespace Mercurio.Domain.Entities
{
    public class Configuracion
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string NegocioId { get; set; } = string.Empty;
        public string Clave { get; set; } = string.Empty;
        public string Valor { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Grupo { get; set; } = "General";
        public DateTime FechaActualizacion { get; set; } = DateTime.Now;
    }
}
