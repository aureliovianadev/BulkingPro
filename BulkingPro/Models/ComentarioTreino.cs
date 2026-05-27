#nullable enable
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BulkingPro.Models;

public class ComentarioTreino
{
    [Key]
    public int Id { get; set; }
    
    public int TreinoExercicioId { get; set; }
    public string AlunoId { get; set; } = null!;
    public string? Comentario { get; set; }
    public bool Lido { get; set; } = false;
    public DateTime DataCriacao { get; set; } = DateTime.Now;
    
    [ForeignKey("TreinoExercicioId")]
    public TreinoExercicio? TreinoExercicio { get; set; }
    
    [ForeignKey("AlunoId")]
    public Usuario? Aluno { get; set; }
}