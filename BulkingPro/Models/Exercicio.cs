namespace BulkingPro.Models;

public enum TipoExecucao
{
    Repeticoes = 0,
    Tempo      = 1
}

public class Exercicio
{
    public int    Id              { get; set; }
    public string Nome            { get; set; } = null!;
    public string Descricao       { get; set; } = null!;
    public int    GrupoMuscularId { get; set; }
    public GrupoMuscular GrupoMuscular { get; set; } = null!;
    public string InstrucoesExecucao { get; set; } = null!;

    // Tipo: Repetições ou Tempo
    public TipoExecucao TipoExecucao { get; set; } = TipoExecucao.Repeticoes;

    public bool     Ativo           { get; set; }
    public DateTime DataCriacao     { get; set; }
    public DateTime? DataAtualizacao { get; set; }

    public ICollection<TreinoExercicio> TreinoExercicios { get; set; } = new List<TreinoExercicio>();
}
