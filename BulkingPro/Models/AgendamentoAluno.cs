#nullable enable
namespace BulkingPro.Models;

public class AgendamentoAluno
{
    public int Id { get; set; }
    public string PersonalId { get; set; } = null!;
    public string AlunoId { get; set; } = null!;
    public DateTime DataAgendamento { get; set; }
    public TimeSpan HoraInicio { get; set; }
    public TimeSpan HoraFim { get; set; }
    public int Status { get; set; }
    public string? Observacoes { get; set; }
    public DateTime DataCriacao { get; set; }
    public DateTime? DataAtualizacao { get; set; }
    
    public Usuario Personal { get; set; } = null!;
    public Usuario Aluno { get; set; } = null!;
}