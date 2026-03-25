using System;
using System.Collections.Generic;

namespace BulkingPro.Models;
public class PlanoTreino
{
    public Guid Id { get; set; }

    public Guid TreinadorId { get; set; }
    public Guid AlunoId { get; set; }

    public string Titulo { get; set; }
    public string? Objetivo { get; set; }

    public DateTime DataInicio { get; set; }
    public DateTime? DataFim { get; set; }

    public int Status { get; set; }

    public DateTime DataCriacao { get; set; }
    public DateTime? DataAtualizacao { get; set; }

    public Usuario Treinador { get; set; }
    public Usuario Aluno { get; set; }

    public ICollection<Treino> Treinos { get; set; } = new List<Treino>();
}