#nullable enable
using System.ComponentModel.DataAnnotations;
using BulkingPro.Models;

namespace BulkingPro.ViewModels
{
    public class AgendaViewModel
    {
        public List<HorarioTrabalhoPersonal> Horarios { get; set; } = new();
        public List<AgendamentoAluno> Agendamentos { get; set; } = new();
        public DateTime DataSelecionada { get; set; } = DateTime.Today;
        public List<Usuario> Alunos { get; set; } = new();
    }

    public class DefinirHorarioViewModel
    {
        [Required(ErrorMessage = "Selecione o dia da semana")]
        public DayOfWeek DiaSemana { get; set; }
        
        [Required(ErrorMessage = "Informe o horario de inicio")]
        public TimeSpan HoraInicio { get; set; }
        
        [Required(ErrorMessage = "Informe o horario de fim")]
        public TimeSpan HoraFim { get; set; }
        
        public bool Ativo { get; set; } = true;
    }

    public class AgendarAulaViewModel
    {
        [Required(ErrorMessage = "Selecione o aluno")]
        public string AlunoId { get; set; } = "";
        
        [Required(ErrorMessage = "Selecione a data")]
        public DateTime DataAgendamento { get; set; }
        
        [Required(ErrorMessage = "Selecione o horario")]
        public TimeSpan HoraInicio { get; set; }
        
        public TimeSpan HoraFim { get; set; }
        public string? Observacoes { get; set; }
    }
}