using Microsoft.AspNetCore.Identity;

namespace BulkingPro.Models;
public class Usuario : IdentityUser<Guid>
{
    public string NomeCompleto { get; set; } = null!;
    public bool Ativo { get; set; }

    public DateTime DataCriacao { get; set; }
    public DateTime? DataAtualizacao { get; set; }

    public ICollection<PlanoTreino> PlanosComoTreinador { get; set; } = new List<PlanoTreino>();
    public ICollection<PlanoTreino> PlanosComoAluno { get; set; } = new List<PlanoTreino>();
    public string Nome { get; internal set; }
}