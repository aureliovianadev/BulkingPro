using System;
using System.Collections.Generic;

namespace BulkingPro.Models;

public class GrupoMuscular
{
    public Guid Id { get; set; }
    public string Nome { get; set; }

    public Guid CategoriaMuscularId { get; set; }
    public CategoriaMuscular CategoriaMuscular { get; set; }

    public ICollection<Exercicio> Exercicios { get; set; } = new List<Exercicio>();
}