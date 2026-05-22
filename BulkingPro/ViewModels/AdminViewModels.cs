#nullable enable
using System.ComponentModel.DataAnnotations;
using BulkingPro.Models;

namespace BulkingPro.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalPersonais  { get; set; }
        public int TotalAlunos     { get; set; }
        public int TotalPlanos     { get; set; }
        public int TotalExercicios { get; set; }

        public List<PlanoTreino>     UltimosPlanos      { get; set; } = new();
        public List<PersonalResumo>  PersonaisComAlunos { get; set; } = new();
    }

    public class PersonalResumo
    {
        public string TreinadorId  { get; set; } = null!;
        public string NomeCompleto { get; set; } = null!;
        public int    TotalAlunos  { get; set; }
    }

    public class PersonalViewModel
    {
        public string Id           { get; set; } = null!;
        public string NomeCompleto { get; set; } = null!;
        public string Email        { get; set; } = null!;
        public bool   Ativo        { get; set; }
        public int    TotalAlunos  { get; set; }
    }

    public class AlunoViewModel
    {
        public string  Id             { get; set; } = null!;
        public string  NomeCompleto   { get; set; } = null!;
        public string  Email          { get; set; } = null!;
        public bool    Ativo          { get; set; }
        public string? NomePlanoAtivo { get; set; }
        public string? NomePersonal   { get; set; }
    }

    public class CriarPersonalViewModel
    {
        [Required(ErrorMessage = "Nome obrigatório")]
        [Display(Name = "Nome completo")]
        public string NomeCompleto { get; set; } = null!;

        [Required(ErrorMessage = "E-mail obrigatório")]
        [EmailAddress(ErrorMessage = "E-mail inválido")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Senha obrigatória")]
        [MinLength(6, ErrorMessage = "Mínimo 6 caracteres")]
        [DataType(DataType.Password)]
        public string Senha { get; set; } = null!;

        public string? Cpf { get; set; }
        
        [Phone(ErrorMessage = "Telefone inválido")]
        public string? Telefone { get; set; }

        public List<HorarioAtendimentoViewModel> HorariosAtendimento { get; set; } = new();
    }

    public class HorarioAtendimentoViewModel
    {
        [Required(ErrorMessage = "Selecione o dia")]
        public DayOfWeek? DiaSemana { get; set; }
        
        [Required(ErrorMessage = "Informe o horário de início")]
        public TimeSpan? HoraInicio { get; set; }
        
        [Required(ErrorMessage = "Informe o horário de fim")]
        public TimeSpan? HoraFim { get; set; }
    }

    // ===================== NOVOS VIEWMODELS PARA EDIÇÃO =====================

    public class EditarAlunoViewModel
    {
        public string Id { get; set; } = null!;
        
        [Required(ErrorMessage = "Nome obrigatório")]
        public string NomeCompleto { get; set; } = null!;

        [Required(ErrorMessage = "E-mail obrigatório")]
        [EmailAddress(ErrorMessage = "E-mail inválido")]
        public string Email { get; set; } = null!;

        public string? Cpf { get; set; }
        
        [Phone(ErrorMessage = "Telefone inválido")]
        public string? Telefone { get; set; }

        public bool Ativo { get; set; } = true;

        public List<HorarioAtendimentoEditViewModel> HorariosAtendimento { get; set; } = new();
        public List<int> HorariosParaRemover { get; set; } = new();
    }

    public class HorarioAtendimentoEditViewModel
    {
        public int Id { get; set; }
        public DayOfWeek? DiaSemana { get; set; }
        public TimeSpan? HoraInicio { get; set; }
        public TimeSpan? HoraFim { get; set; }
        public bool Remover { get; set; }
    }
}