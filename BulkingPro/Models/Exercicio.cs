using System;
using System.Collections.Generic;

namespace BulkingPro.Models;

public class Exercicio
{
    public int Id { get; set; }

    public string Nome { get; set; } = null!;
    public string Descricao { get; set; } = null!;

    // 🔥 Relacionamento correto com GrupoMuscular
    public int GrupoMuscularId { get; set; }
    public GrupoMuscular GrupoMuscular { get; set; } = null!;

    public string InstrucoesExecucao { get; set; } = null!;

    public bool Ativo { get; set; }

    public DateTime DataCriacao { get; set; }
    public DateTime? DataAtualizacao { get; set; }

    public ICollection<TreinoExercicio> TreinoExercicios { get; set; } = new List<TreinoExercicio>();
}