using System;
using System.Collections.Generic;

namespace BulkingPro.Models;
public class Treino
{
    public int Id { get; set; }

    public int PlanoTreinoId { get; set; }

    public string Nome { get; set; }
    public int OrdemDia { get; set; }
    public string? Observacoes { get; set; }

    public DateTime DataCriacao { get; set; }
    public DateTime? DataAtualizacao { get; set; }

    public PlanoTreino PlanoTreino { get; set; }

    public ICollection<TreinoExercicio> TreinoExercicios { get; set; } = new List<TreinoExercicio>();
    public ICollection<ExecucaoTreino> Execucoes { get; set; } = new List<ExecucaoTreino>();
}