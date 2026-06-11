#nullable enable
using System.ComponentModel.DataAnnotations;

namespace BulkingPro.ViewModels
{
    // ================================================================
    // VIEWMODEL PARA EDIÇÃO DE PLANOS DE TREINO
    // ================================================================

    public class EditarPlanoViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Selecione o aluno")]
        public string AlunoId { get; set; } = "";

        [Required(ErrorMessage = "Informe o título")]
        public string Titulo { get; set; } = "";

        [Required(ErrorMessage = "Informe o objetivo")]
        public string Objetivo { get; set; } = "";

        public DateTime DataInicio { get; set; } = DateTime.Today;
        public DateTime? DataFim { get; set; }

        public List<DiaTreinoEditViewModel> DiasSemana { get; set; } = new();
    }

    // ================================================================
    // VIEWMODEL PARA CADA DIA DA SEMANA NA EDIÇÃO
    // ================================================================

    public class DiaTreinoEditViewModel
    {
        public int? TreinoId { get; set; }
        public string Nome { get; set; } = "";
        public int Ordem { get; set; }
        public bool Selecionado { get; set; }
        public string? Observacoes { get; set; }
        public List<ExercicioEditViewModel> Exercicios { get; set; } = new();
    }

    // ================================================================
    // VIEWMODEL PARA CADA EXERCÍCIO NA EDIÇÃO
    // ================================================================

    public class ExercicioEditViewModel
    {
        public int? TreinoExercicioId { get; set; }
        public int Ordem { get; set; }

        [Required(ErrorMessage = "Selecione um exercício")]
        public int ExercicioId { get; set; }

        [Range(1, 50, ErrorMessage = "Séries deve ser entre 1 e 50")]
        public int Series { get; set; } = 3;

        public string? Repeticoes { get; set; }

        [Range(0, 3600, ErrorMessage = "Tempo máximo 60 minutos")]
        public int? TempoExecucaoSegundos { get; set; }

        [Range(0, 500, ErrorMessage = "Carga máxima 500kg")]
        public decimal? Carga { get; set; }

        [Range(0, 300, ErrorMessage = "Descanso máximo 300 segundos")]
        public int? Descanso { get; set; }

        public string? Observacoes { get; set; }
    }
}