using System;
using System.Collections.Generic;

namespace BulkingPro.Models;

public class TreinoExercicio
{
    public int Id { get; set; } // pode continuar int

    public int TreinoId { get; set; } // continua int (se Treino for int)
    public int ExercicioId { get; set; } 

    public int Ordem { get; set; }

    public int SeriesPlanejadas { get; set; }
    public string RepeticoesPlanejadas { get; set; }

    public decimal? CargaPlanejada { get; set; }
    public int? TempoDescanso { get; set; }

    public string? Observacoes { get; set; }

    public DateTime DataCriacao { get; set; }
    public DateTime? DataAtualizacao { get; set; }

    public Treino Treino { get; set; }
    public Exercicio Exercicio { get; set; }

    public ICollection<ExecucaoTreinoExercicio> Execucoes { get; set; } = new List<ExecucaoTreinoExercicio>();
}