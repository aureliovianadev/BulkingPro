#nullable enable
namespace BulkingPro.Models;

public class AlunoHorarioAtendimento
{
    public int Id { get; set; }
    public string PersonalId { get; set; } = null!;
    public string AlunoId { get; set; } = null!;
    public DayOfWeek DiaSemana { get; set; }
    public TimeSpan HoraInicio { get; set; }
    public TimeSpan HoraFim { get; set; }
    public bool Ativo { get; set; } = true;
    public DateTime DataCriacao { get; set; }
    public DateTime? DataAtualizacao { get; set; }
    
    public Usuario Personal { get; set; } = null!;
    public Usuario Aluno { get; set; } = null!;
}