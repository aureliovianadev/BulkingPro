using System;

namespace BulkingPro.Models;

public class ExecucaoTreinoExercicio
{
    public Guid Id { get; set; }

    public Guid ExecucaoTreinoId { get; set; }
    public Guid TreinoExercicioId { get; set; }

    public int? SeriesFeitas { get; set; }
    public string? RepeticoesFeitas { get; set; }

    public decimal? CargaUsada { get; set; }

    public bool Concluido { get; set; }

    public string? Observacoes { get; set; }

    public ExecucaoTreino ExecucaoTreino { get; set; }
    public TreinoExercicio TreinoExercicio { get; set; }
}