namespace BulkingPro.Models;

public class CategoriaMuscular
{
    public int Id { get; set; }
    public string Nome { get; set; } = null!; // Inferior, Superior, Aeróbico
    public ICollection<GrupoMuscular> Grupos { get; set; } = new List<GrupoMuscular>();
}