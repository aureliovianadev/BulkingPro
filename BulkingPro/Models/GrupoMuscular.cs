namespace BulkingPro.Models;

public class GrupoMuscular
{
    public int Id { get; set; }
    public string Nome { get; set; } = null!;
    public int CategoriaMuscularId { get; set; }
    public CategoriaMuscular CategoriaMuscular { get; set; } = null!;
    public ICollection<Exercicio> Exercicios { get; set; } = new List<Exercicio>();
}