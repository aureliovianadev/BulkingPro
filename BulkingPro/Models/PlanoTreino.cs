using System;
using System.Collections.Generic;

namespace BulkingPro.Models;
public class PlanoTreino
{
    public int Id { get; set; }

    public int TreinadorId { get; set; }
    public int AlunoId { get; set; }

    public string Titulo { get; set; } = null!;
    public string Objetivo { get; set; } = null!;

    public DateTime DataInicio { get; set; }
    public DateTime? DataFim { get; set; }

    public int Status { get; set; }

    public DateTime DataCriacao { get; set; }
    public DateTime? DataAtualizacao { get; set; }

    public Usuario Treinador { get; set; } = null!;
    public Usuario Aluno { get; set; } = null!;

    public ICollection<Treino> Treinos { get; set; } = new List<Treino>();
}