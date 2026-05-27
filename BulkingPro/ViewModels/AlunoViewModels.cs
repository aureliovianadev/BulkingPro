using BulkingPro.Models;
#nullable enable
using System.ComponentModel.DataAnnotations;

namespace BulkingPro.ViewModels;

public class AlunoDashboardViewModel
{
    public int TotalTreinosRealizados { get; set; }
    public int TotalTreinosPrevistos { get; set; }
    public int DiasConsecutivos { get; set; }
    public decimal? EvolucaoPeso { get; set; }
    public int AumentoCargaTotal { get; set; }
    
    public List<PesoEvolucao> EvolucaoPesoData { get; set; } = new();
    public List<CargaEvolucao> EvolucaoCargaData { get; set; } = new();
    public List<TreinoSemana> TreinosSemana { get; set; } = new();
}

public class PesoEvolucao
{
    public DateTime Data { get; set; }
    public decimal Peso { get; set; }
}

public class CargaEvolucao
{
    public string ExercicioNome { get; set; } = "";
    public decimal CargaInicial { get; set; }
    public decimal CargaAtual { get; set; }
    public decimal Percentual => CargaInicial > 0 ? (CargaAtual - CargaInicial) / CargaInicial * 100 : 0;
}

public class TreinoSemana
{
    public DayOfWeek Dia { get; set; }
    public string Nome { get; set; } = "";
    public bool Realizado { get; set; }
    public DateTime? DataRealizacao { get; set; }
}

public class TreinoAlunoViewModel
{
    public PlanoTreino? PlanoAtivo { get; set; }
    public List<DiaTreinoAlunoViewModel> DiasTreino { get; set; } = new();
    public DateTime DataAtual { get; set; } = DateTime.Today;
}

public class DiaTreinoAlunoViewModel
{
    public int TreinoId { get; set; }
    public string Nome { get; set; } = "";
    public int OrdemDia { get; set; }
    public DayOfWeek DiaSemana { get; set; }
    public bool Hoje { get; set; }
    public bool Realizado { get; set; }
    public DateTime? DataRealizacao { get; set; }
    public List<ExercicioTreinoAlunoViewModel> Exercicios { get; set; } = new();
}

public class ExercicioTreinoAlunoViewModel
{
    public int TreinoExercicioId { get; set; }
    public int Ordem { get; set; }
    public string NomeExercicio { get; set; } = "";
    public string GrupoMuscular { get; set; } = "";
    public int Series { get; set; }
    public string RepeticoesOuTempo { get; set; } = "";
    public decimal? Carga { get; set; }
    public int? Descanso { get; set; }
    public string? Observacoes { get; set; }
    public string? MeuComentario { get; set; }
    public bool JaComentou { get; set; }
}

public class ComentarioViewModel
{
    public int TreinoExercicioId { get; set; }
    public string NomeExercicio { get; set; } = "";
    public string? Comentario { get; set; }
}

public class ComentarioEnviarViewModel
{
    [Required(ErrorMessage = "Digite seu comentário")]
    [StringLength(500, ErrorMessage = "Máximo 500 caracteres")]
    public string Comentario { get; set; } = "";
    public int TreinoExercicioId { get; set; }
}

public class ExecucaoTreinoViewModel
{
    public int TreinoId { get; set; }
    public string TreinoNome { get; set; } = "";
}