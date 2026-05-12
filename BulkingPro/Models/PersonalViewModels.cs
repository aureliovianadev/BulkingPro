#nullable enable
using System.ComponentModel.DataAnnotations;
using BulkingPro.Models;

namespace BulkingPro.ViewModels
{
    // ── Dashboard ────────────────────────────────────────────────
    public class PersonalDashboardViewModel
    {
        public int TotalAlunos  { get; set; }
        public int TotalPlanos  { get; set; }
        public int PlanosAtivos { get; set; }
        public List<Usuario> UltimosAlunos { get; set; } = new();
    }

    // ── Criar Plano ──────────────────────────────────────────────
    public class CriarPlanoViewModel
    {
        [Required(ErrorMessage = "Selecione o aluno")]
        public string AlunoId { get; set; } = "";

        [Required(ErrorMessage = "Informe o título")]
        public string Titulo { get; set; } = "";

        [Required(ErrorMessage = "Informe o objetivo")]
        public string Objetivo { get; set; } = "";

        public DateTime DataInicio { get; set; } = DateTime.Today;
        public DateTime? DataFim   { get; set; }

        public List<DiaTreinoViewModel> DiasSemana { get; set; } = new()
        {
            new() { Nome = "Segunda-feira",  Ordem = 1 },
            new() { Nome = "Terça-feira",    Ordem = 2 },
            new() { Nome = "Quarta-feira",   Ordem = 3 },
            new() { Nome = "Quinta-feira",   Ordem = 4 },
            new() { Nome = "Sexta-feira",    Ordem = 5 },
            new() { Nome = "Sábado",         Ordem = 6 },
            new() { Nome = "Domingo",        Ordem = 7 },
        };
    }

    public class DiaTreinoViewModel
    {
        public string  Nome        { get; set; } = "";
        public int     Ordem       { get; set; }
        public bool    Selecionado { get; set; }
        public string? Observacoes { get; set; }
        public List<ExercicioDiaViewModel> Exercicios { get; set; } = Enumerable.Range(1, 8)
            .Select(i => new ExercicioDiaViewModel { Ordem = i })
            .ToList();
    }

    public class ExercicioDiaViewModel
    {
        public int      Ordem       { get; set; }
        public int      ExercicioId { get; set; }
        public int      Series      { get; set; } = 3;
        public string   Repeticoes  { get; set; } = "12";
        public decimal? Carga       { get; set; }
        public int?     Descanso    { get; set; } = 60;
        public string?  Observacoes { get; set; }
    }

    // ── Medidas ──────────────────────────────────────────────────
    public class MedidasViewModel
    {
        [Required(ErrorMessage = "Selecione o aluno")]
        public string AlunoId { get; set; } = "";

        public DateTime DataAvaliacao { get; set; } = DateTime.Today;

        [Range(0.5, 2.5, ErrorMessage = "Altura inválida")]
        public decimal? Altura { get; set; }        // metros

        [Range(1, 500, ErrorMessage = "Peso inválido")]
        public decimal? Peso   { get; set; }        // kg

        // Circunferências (cm)
        public decimal? Pescoco      { get; set; }
        public decimal? Ombro        { get; set; }
        public decimal? ToraxContrai { get; set; }
        public decimal? ToraxRelax   { get; set; }
        public decimal? BicepsDireito{ get; set; }
        public decimal? BicepsEsquerdo{ get; set; }
        public decimal? Cintura      { get; set; }
        public decimal? Abdomen      { get; set; }
        public decimal? Quadril      { get; set; }
        public decimal? CoxaDireita  { get; set; }
        public decimal? CoxaEsquerda { get; set; }
        public decimal? PanturrilhaDireita  { get; set; }
        public decimal? PanturrilhaEsquerda { get; set; }

        // Calculado
        public decimal? IMC => (Peso.HasValue && Altura.HasValue && Altura > 0)
            ? Math.Round(Peso.Value / (Altura.Value * Altura.Value), 2)
            : null;

        public string ClassificacaoIMC => IMC switch
        {
            < 18.5m  => "Abaixo do peso",
            < 25.0m  => "Peso normal",
            < 30.0m  => "Sobrepeso",
            < 35.0m  => "Obesidade grau I",
            < 40.0m  => "Obesidade grau II",
            >= 40.0m => "Obesidade grau III",
            _        => "—"
        };

        public string? Observacoes { get; set; }
    }

    // ── Anamnese ─────────────────────────────────────────────────
    public class AnamneseViewModel
    {
        [Required(ErrorMessage = "Selecione o aluno")]
        public string AlunoId { get; set; } = "";

        // Histórico esportivo
        public bool    JaTreinouAntes     { get; set; }
        public string? TempoTreinando     { get; set; }
        public string? TempoSemAtividade  { get; set; }
        public string? Objetivo           { get; set; }
        public string? FrequenciaSemanal  { get; set; }
        public string? TempoPorDia        { get; set; }

        // Saúde
        public bool    TemDoenca          { get; set; }
        public string? QualDoenca         { get; set; }
        public bool    TemLimitacaoMovimento { get; set; }
        public string? QualLimitacao      { get; set; }
        public bool    TemDorMovimento    { get; set; }
        public string? QualDor            { get; set; }
        public bool    FezCirurgia        { get; set; }
        public string? QualCirurgia       { get; set; }
        public bool    UsaMedicamento     { get; set; }
        public string? QualMedicamento    { get; set; }

        // Hábitos
        public bool    FazDieta           { get; set; }
        public string? TipoDieta          { get; set; }
        public string? ConsomeAlcool      { get; set; }   // Não / Socialmente / Frequentemente
        public bool    Fuma               { get; set; }

        public string? ObservacoesGerais  { get; set; }
        public DateTime DataAvaliacao     { get; set; } = DateTime.Today;
    }
}