using System;
using System.Collections.Generic;

namespace BulkingPro.Models;
public class Treino
{
    public int Id { get; set; }

    public int PlanoTreinoId { get; set; }

    public string Nome { get; set; } = null!;
    public int OrdemDia { get; set; }
    public string Observacoes { get; set; } = null!;

    public DateTime DataCriacao { get; set; }
    public DateTime? DataAtualizacao { get; set; }

    public PlanoTreino PlanoTreino { get; set; } = null!;

    public ICollection<TreinoExercicio> TreinoExercicios { get; set; } = new List<TreinoExercicio>();
    public ICollection<ExecucaoTreino> Execucoes { get; set; } = new List<ExecucaoTreino>();
}