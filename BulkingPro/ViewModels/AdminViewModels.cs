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

        // Lista de horários (cada horário pode ter múltiplos dias)
        public List<HorarioComDiasViewModel> HorariosAtendimento { get; set; } = new();
    }

    public class HorarioComDiasViewModel
    {
        public TimeSpan? HoraInicio { get; set; }
        public TimeSpan? HoraFim { get; set; }
        
        // Dias selecionados
        public bool Domingo { get; set; }
        public bool Segunda { get; set; }
        public bool Terca { get; set; }
        public bool Quarta { get; set; }
        public bool Quinta { get; set; }
        public bool Sexta { get; set; }
        public bool Sabado { get; set; }

        public List<DayOfWeek> DiasSelecionados()
        {
            var dias = new List<DayOfWeek>();
            if (Domingo) dias.Add(DayOfWeek.Sunday);
            if (Segunda) dias.Add(DayOfWeek.Monday);
            if (Terca) dias.Add(DayOfWeek.Tuesday);
            if (Quarta) dias.Add(DayOfWeek.Wednesday);
            if (Quinta) dias.Add(DayOfWeek.Thursday);
            if (Sexta) dias.Add(DayOfWeek.Friday);
            if (Sabado) dias.Add(DayOfWeek.Saturday);
            return dias;
        }
    }

    // ===================== VIEWMODELS PARA EDIÇÃO =====================

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

        public List<HorarioComDiasEditViewModel> HorariosAtendimento { get; set; } = new();
        public List<int> HorariosParaRemover { get; set; } = new();
    }

    public class HorarioComDiasEditViewModel
    {
        public int Id { get; set; }
        public TimeSpan? HoraInicio { get; set; }
        public TimeSpan? HoraFim { get; set; }
        
        public bool Domingo { get; set; }
        public bool Segunda { get; set; }
        public bool Terca { get; set; }
        public bool Quarta { get; set; }
        public bool Quinta { get; set; }
        public bool Sexta { get; set; }
        public bool Sabado { get; set; }

        public List<DayOfWeek> DiasSelecionados()
        {
            var dias = new List<DayOfWeek>();
            if (Domingo) dias.Add(DayOfWeek.Sunday);
            if (Segunda) dias.Add(DayOfWeek.Monday);
            if (Terca) dias.Add(DayOfWeek.Tuesday);
            if (Quarta) dias.Add(DayOfWeek.Wednesday);
            if (Quinta) dias.Add(DayOfWeek.Thursday);
            if (Sexta) dias.Add(DayOfWeek.Friday);
            if (Sabado) dias.Add(DayOfWeek.Saturday);
            return dias;
        }
    }
}