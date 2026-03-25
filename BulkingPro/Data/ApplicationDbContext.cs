using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using BulkingPro.Models;

namespace BulkingPro.Data;

public class ApplicationDbContext : IdentityDbContext<Usuario, IdentityRole<Guid>, Guid>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // Tabelas
    public DbSet<Exercicio> Exercicios { get; set; }
    public DbSet<PlanoTreino> PlanosTreino { get; set; }
    public DbSet<Treino> Treinos { get; set; }
    public DbSet<TreinoExercicio> TreinosExercicios { get; set; }
    public DbSet<ExecucaoTreino> ExecucoesTreino { get; set; }
    public DbSet<ExecucaoTreinoExercicio> ExecucoesTreinoExercicios { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // 🔥 PlanoTreino → Treinador
        builder.Entity<PlanoTreino>()
            .HasOne(p => p.Treinador)
            .WithMany(u => u.PlanosComoTreinador)
            .HasForeignKey(p => p.TreinadorId)
            .OnDelete(DeleteBehavior.Restrict);

        // 🔥 PlanoTreino → Aluno
        builder.Entity<PlanoTreino>()
            .HasOne(p => p.Aluno)
            .WithMany(u => u.PlanosComoAluno)
            .HasForeignKey(p => p.AlunoId)
            .OnDelete(DeleteBehavior.Restrict);

        // Treino → PlanoTreino
        builder.Entity<Treino>()
            .HasOne(t => t.PlanoTreino)
            .WithMany(p => p.Treinos)
            .HasForeignKey(t => t.PlanoTreinoId);

        // TreinoExercicio → Treino
        builder.Entity<TreinoExercicio>()
            .HasOne(te => te.Treino)
            .WithMany(t => t.TreinoExercicios)
            .HasForeignKey(te => te.TreinoId);

        // TreinoExercicio → Exercicio
        builder.Entity<TreinoExercicio>()
            .HasOne(te => te.Exercicio)
            .WithMany(e => e.TreinoExercicios)
            .HasForeignKey(te => te.ExercicioId);

        // ExecucaoTreino → Treino
        builder.Entity<ExecucaoTreino>()
            .HasOne(et => et.Treino)
            .WithMany(t => t.Execucoes)
            .HasForeignKey(et => et.TreinoId);

        // ExecucaoTreino → Aluno
        builder.Entity<ExecucaoTreino>()
            .HasOne(et => et.Aluno)
            .WithMany()
            .HasForeignKey(et => et.AlunoId)
            .OnDelete(DeleteBehavior.Restrict);

        // ExecucaoTreinoExercicio → ExecucaoTreino
        builder.Entity<ExecucaoTreinoExercicio>()
            .HasOne(ete => ete.ExecucaoTreino)
            .WithMany(et => et.Exercicios)
            .HasForeignKey(ete => ete.ExecucaoTreinoId);

        // ExecucaoTreinoExercicio → TreinoExercicio
        builder.Entity<ExecucaoTreinoExercicio>()
            .HasOne(ete => ete.TreinoExercicio)
            .WithMany(te => te.Execucoes)
            .HasForeignKey(ete => ete.TreinoExercicioId);
    }
}