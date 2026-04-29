namespace BulkingPro.Models;

public class ExecucaoTreinoExercicio
{
    public int Id { get; set; }
    public int ExecucaoTreinoId { get; set; }
    public int TreinoExercicioId { get; set; }
    public int? SeriesFeitas { get; set; }
    public string RepeticoesFeitas { get; set; } = null!;
    public decimal? CargaUsada { get; set; }
    public bool Concluido { get; set; }
    public string Observacoes { get; set; } = null!;
    public ExecucaoTreino ExecucaoTreino { get; set; } = null!;
    public TreinoExercicio TreinoExercicio { get; set; } = null!;
}