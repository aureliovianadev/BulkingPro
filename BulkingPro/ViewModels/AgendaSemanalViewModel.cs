// ViewModels/AgendaSemanalViewModel.cs
#nullable enable
using BulkingPro.Models;

namespace BulkingPro.ViewModels
{
    public class AgendaSemanalViewModel
    {
        public DateTime DataReferencia { get; set; } = DateTime.Today;
        public DateTime InicioSemana { get; set; }
        public DateTime FimSemana { get; set; }
        public List<DiaAgendaViewModel> Dias { get; set; } = new();
        public ResumoAgendaViewModel Resumo { get; set; } = new();
    }

    public class DiaAgendaViewModel
    {
        public DayOfWeek DiaSemana { get; set; }
        public string NomeDia { get; set; } = "";
        public string NomeDiaAbreviado { get; set; } = "";
        public DateTime Data { get; set; }
        public bool Hoje { get; set; }
        public List<AgendamentoCardViewModel> Agendamentos { get; set; } = new();
        public bool TemAgendamentos => Agendamentos.Any();
        public int TotalAgendamentos => Agendamentos.Count;
    }

    public class AgendamentoCardViewModel
    {
        public int Id { get; set; }
        public string AlunoId { get; set; } = "";
        public string AlunoNome { get; set; } = "";
        public string? AlunoFoto { get; set; }
        public string Iniciais { get; set; } = "";
        public TimeSpan HoraInicio { get; set; }
        public TimeSpan HoraFim { get; set; }
        public string Status { get; set; } = "Confirmado";
        public string? Objetivo { get; set; }
        public int? PlanoTreinoId { get; set; }
        public string? TreinoDiaNome { get; set; }
        public List<ExercicioResumoViewModel> Exercicios { get; set; } = new();
        public bool TemExercicios => Exercicios.Any();
        
        // ════════════════════════════════════════════════════════════════
        // PROPRIEDADES PARA CORREÇÃO DO BUG DA AGENDA
        // ════════════════════════════════════════════════════════════════
        public bool TemPlanoAtivo { get; set; }
        public string? MensagemSemPlano { get; set; }
    }

    public class ExercicioResumoViewModel
    {
        public string Nome { get; set; } = "";
        public string GrupoMuscular { get; set; } = "";
        public int Series { get; set; }
        public string RepeticoesOuTempo { get; set; } = "";
        public decimal? Carga { get; set; }
        public int? Descanso { get; set; }
        public string? Observacoes { get; set; }
    }

    public class ResumoAgendaViewModel
    {
        public int TotalAulas { get; set; }
        public int TotalHoras { get; set; }
        public string ProximoAtendimento { get; set; } = "Nenhum agendamento";
        public bool TemProximo => ProximoAtendimento != "Nenhum agendamento";
    }
}
