using System;
using System.Collections.Generic;

namespace BulkingPro.Models;

public class CategoriaMuscular
{
    public Guid Id { get; set; }
    public string Nome { get; set; } // Inferior, Superior, Aeróbico

    public ICollection<GrupoMuscular> Grupos { get; set; } = new List<GrupoMuscular>();
}