#nullable enable
namespace BulkingPro.Models;

public class AnamneseAluno
{
    public int Id { get; set; }
    public string AlunoId { get; set; } = null!;
    public string TreinadorId { get; set; } = null!;
    public DateTime DataAvaliacao { get; set; }

    // Histórico esportivo
    public bool JaTreinouAntes { get; set; }
    public string? TempoTreinando { get; set; }
    public string? TempoSemAtividade { get; set; }
    public string? Objetivo { get; set; }
    public string? FrequenciaSemanal { get; set; }
    public string? TempoPorDia { get; set; }

    // Saúde
    public bool TemDoenca { get; set; }
    public string? QualDoenca { get; set; }
    public bool TemLimitacaoMovimento { get; set; }
    public string? QualLimitacao { get; set; }
    public bool TemDorMovimento { get; set; }
    public string? QualDor { get; set; }
    public bool FezCirurgia { get; set; }
    public string? QualCirurgia { get; set; }
    public bool UsaMedicamento { get; set; }
    public string? QualMedicamento { get; set; }

    // Hábitos
    public bool FazDieta { get; set; }
    public string? TipoDieta { get; set; }
    public string? ConsomeAlcool { get; set; }
    public bool Fuma { get; set; }

    public string? ObservacoesGerais { get; set; }
    public DateTime DataCriacao { get; set; }
    public DateTime? DataAtualizacao { get; set; }

    // Navegação
    public Usuario Aluno { get; set; } = null!;
    public Usuario Treinador { get; set; } = null!;
}