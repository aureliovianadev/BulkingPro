using System;
using System.Collections.Generic;

namespace BulkingPro.Models;

public class ExecucaoTreino
{
    public Guid Id { get; set; }

    public Guid TreinoId { get; set; }
    public Guid AlunoId { get; set; }

    public DateTime DataExecucao { get; set; }

    public int? DuracaoMinutos { get; set; }
    public int? EsforcoPercebido { get; set; }

    public string? ObservacoesGerais { get; set; }

    public bool Concluido { get; set; }

    public DateTime DataCriacao { get; set; }

    public Treino Treino { get; set; }
    public Usuario Aluno { get; set; }

    public ICollection<ExecucaoTreinoExercicio> Exercicios { get; set; } = new List<ExecucaoTreinoExercicio>();
}