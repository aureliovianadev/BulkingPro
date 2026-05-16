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
    }
}