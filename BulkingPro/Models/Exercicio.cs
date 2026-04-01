using System;
using System.Collections.Generic;

namespace BulkingPro.Models;

public class Exercicio
{
    public Guid Id { get; set; }

    public string Nome { get; set; }
    public string? Descricao { get; set; }

    // 🔥 Relacionamento correto com GrupoMuscular
    public Guid GrupoMuscularId { get; set; }
    public GrupoMuscular GrupoMuscular { get; set; }

    public string? InstrucoesExecucao { get; set; }

    public bool Ativo { get; set; }

    public DateTime DataCriacao { get; set; }
    public DateTime? DataAtualizacao { get; set; }

    public ICollection<TreinoExercicio> TreinoExercicios { get; set; } = new List<TreinoExercicio>();
}