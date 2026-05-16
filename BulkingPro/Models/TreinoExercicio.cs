namespace BulkingPro.Models;

public class TreinoExercicio
{
    public int    Id          { get; set; }
    public int    TreinoId    { get; set; }
    public int    ExercicioId { get; set; }
    public int    Ordem       { get; set; }

    public int    SeriesPlanejadas     { get; set; }

    // Quando TipoExecucao == Repeticoes
    public string  RepeticoesPlanejadas { get; set; } = null!;

    // Quando TipoExecucao == Tempo (segundos)
    public int?    TempoExecucaoSegundos { get; set; }

    public decimal? CargaPlanejada    { get; set; }
    public int?     TempoDescanso     { get; set; }
    public string   Observacoes       { get; set; } = null!;
    public DateTime DataCriacao       { get; set; }
    public DateTime? DataAtualizacao  { get; set; }

    public Treino    Treino    { get; set; } = null!;
    public Exercicio Exercicio { get; set; } = null!;
    public ICollection<ExecucaoTreinoExercicio> Execucoes { get; set; } = new List<ExecucaoTreinoExercicio>();
}
