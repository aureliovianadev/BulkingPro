using Microsoft.AspNetCore.Identity;

namespace BulkingPro.Models;

public class Usuario : IdentityUser
{
    public string NomeCompleto { get; set; } = null!;
    public string? Cpf { get; set; }
    public string? Telefone { get; set; }
    public bool Ativo { get; set; }
    public DateTime DataCriacao { get; set; }
    public DateTime? DataAtualizacao { get; set; }
    
    // Horários de atendimento do aluno
    public ICollection<AlunoHorarioAtendimento> HorariosAtendimento { get; set; } = new List<AlunoHorarioAtendimento>();
    
    public ICollection<PlanoTreino> PlanosComoTreinador { get; set; } = new List<PlanoTreino>();
    public ICollection<PlanoTreino> PlanosComoAluno { get; set; } = new List<PlanoTreino>();
}