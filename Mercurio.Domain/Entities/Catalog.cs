namespace Mercurio.Domain.Entities
{
    public class Categoria
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string NegocioId { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public bool Activo { get; set; } = true;
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
    }
}
