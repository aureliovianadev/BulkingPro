using System;
using System.Collections.Generic;

#nullable enable

namespace BulkingPro.Models;

public class ExecucaoTreino
{
    public int Id { get; set; }

    public int TreinoId { get; set; }
    public int AlunoId { get; set; }

    public DateTime DataExecucao { get; set; }

    public int? DuracaoMinutos { get; set; }
    public int? EsforcoPercebido { get; set; }

    public string? ObservacoesGerais { get; set; }

    public bool Concluido { get; set; }

    public DateTime DataCriacao { get; set; }

    public Treino Treino { get; set; } = null!;
    public Usuario Aluno { get; set; } = null!;

    public ICollection<ExecucaoTreinoExercicio> Exercicios { get; set; } = new List<ExecucaoTreinoExercicio>();
}