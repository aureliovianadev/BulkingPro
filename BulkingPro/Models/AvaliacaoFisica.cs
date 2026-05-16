#nullable enable
namespace BulkingPro.Models;

public class AvaliacaoFisica
{
    public int    Id           { get; set; }
    public string AlunoId      { get; set; } = null!;
    public string TreinadorId  { get; set; } = null!;
    public DateTime DataAvaliacao { get; set; }

    // Principais
    public decimal? Altura { get; set; }   // metros
    public decimal? Peso   { get; set; }   // kg

    // Circunferências (cm)
    public decimal? Pescoco              { get; set; }
    public decimal? Ombro                { get; set; }
    public decimal? ToraxContrai         { get; set; }
    public decimal? ToraxRelax           { get; set; }
    public decimal? BicepsDireito        { get; set; }
    public decimal? BicepsEsquerdo       { get; set; }
    public decimal? Cintura              { get; set; }
    public decimal? Abdomen              { get; set; }
    public decimal? Quadril              { get; set; }
    public decimal? CoxaDireita          { get; set; }
    public decimal? CoxaEsquerda         { get; set; }
    public decimal? PanturrilhaDireita   { get; set; }
    public decimal? PanturrilhaEsquerda  { get; set; }

    public string?  Observacoes  { get; set; }
    public DateTime DataCriacao  { get; set; }

    // Navegação
    public Usuario Aluno     { get; set; } = null!;
    public Usuario Treinador { get; set; } = null!;
}