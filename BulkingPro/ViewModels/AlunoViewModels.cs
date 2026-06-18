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

// ── VIEWMODEL ANTIGO (MANTER PARA COMPATIBILIDADE) ──
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

// ═══════════════════════════════════════════════════════════════════
// NOVOS VIEWMODELS PARA CONTROLE DE PERÍODOS E HISTÓRICO
// ═══════════════════════════════════════════════════════════════════

public class TreinosAlunoViewModel
{
    public List<PlanoPeriodoViewModel> PlanosDisponiveis { get; set; } = new();
    public string PlanoSelecionadoId { get; set; } = "";
    public PlanoPeriodoViewModel? PlanoAtual { get; set; }
    public List<DiaTreinoComDataViewModel> DiasTreino { get; set; } = new();
    public DateTime DataReferencia { get; set; } = DateTime.Today;
    public bool TemPlanoAtivo { get; set; }
    public string? Mensagem { get; set; }
}

public class PlanoPeriodoViewModel
{
    public string Id { get; set; } = "";
    public string Titulo { get; set; } = "";
    public DateTime DataInicio { get; set; }
    public DateTime? DataFim { get; set; }
    public bool IsAtivo { get; set; }
    
    public string PeriodoFormatado => DataFim.HasValue 
        ? $"{DataInicio:dd/MM/yyyy} - {DataFim:dd/MM/yyyy}"
        : $"{DataInicio:dd/MM/yyyy} - (em andamento)";
    
    public string Label => IsAtivo 
        ? $"🟢 {Titulo} ({PeriodoFormatado})" 
        : $"📋 {Titulo} ({PeriodoFormatado})";
}

public class DiaTreinoComDataViewModel
{
    public int TreinoId { get; set; }
    public string Nome { get; set; } = "";
    public int OrdemDia { get; set; }
    public DayOfWeek DiaSemana { get; set; }
    public DateTime DataReferencia { get; set; }
    public bool Hoje { get; set; }
    public bool Realizado { get; set; }
    public DateTime? DataRealizacao { get; set; }
    public List<ExercicioTreinoAlunoViewModel> Exercicios { get; set; } = new();
}

// ═══════════════════════════════════════════════════════════════════
// VIEWMODELS EXISTENTES (COM A NOVA PROPRIEDADE Concluido)
// ═══════════════════════════════════════════════════════════════════

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
    public bool Concluido { get; set; } = false; // ⭐ NOVO
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