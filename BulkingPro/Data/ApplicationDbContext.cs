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

    // 🔥 Tabelas
    public DbSet<Exercicio> Exercicios { get; set; }
    public DbSet<GrupoMuscular> GruposMusculares { get; set; }
    public DbSet<CategoriaMuscular> CategoriasMusculares { get; set; }

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

        // 🔹 Treino → PlanoTreino
        builder.Entity<Treino>()
            .HasOne(t => t.PlanoTreino)
            .WithMany(p => p.Treinos)
            .HasForeignKey(t => t.PlanoTreinoId)
            .OnDelete(DeleteBehavior.Restrict);

        // 🔹 TreinoExercicio → Treino
        builder.Entity<TreinoExercicio>()
    .HasOne(te => te.Treino)
    .WithMany(t => t.TreinoExercicios)
    .HasForeignKey(te => te.TreinoId)
    .OnDelete(DeleteBehavior.Restrict);

        // 🔹 TreinoExercicio → Exercicio
        builder.Entity<TreinoExercicio>()
    .HasOne(te => te.Exercicio)
    .WithMany(e => e.TreinoExercicios)
    .HasForeignKey(te => te.ExercicioId)
    .OnDelete(DeleteBehavior.Restrict);

        // 🔥 Exercicio → GrupoMuscular
        builder.Entity<Exercicio>()
            .HasOne(e => e.GrupoMuscular)
            .WithMany(g => g.Exercicios)
            .HasForeignKey(e => e.GrupoMuscularId)
            .OnDelete(DeleteBehavior.Restrict);

        // 🔥 GrupoMuscular → CategoriaMuscular
        builder.Entity<GrupoMuscular>()
            .HasOne(g => g.CategoriaMuscular)
            .WithMany(c => c.Grupos)
            .HasForeignKey(g => g.CategoriaMuscularId)
            .OnDelete(DeleteBehavior.Restrict);

        // 🔹 ExecucaoTreino → Treino
        builder.Entity<ExecucaoTreino>()
            .HasOne(et => et.Treino)
            .WithMany(t => t.Execucoes)
            .HasForeignKey(et => et.TreinoId)
            .OnDelete(DeleteBehavior.Restrict);

        // 🔹 ExecucaoTreino → Aluno
        builder.Entity<ExecucaoTreino>()
            .HasOne(et => et.Aluno)
            .WithMany()
            .HasForeignKey(et => et.AlunoId)
            .OnDelete(DeleteBehavior.Restrict);

        // 🔹 ExecucaoTreinoExercicio → ExecucaoTreino
        builder.Entity<ExecucaoTreinoExercicio>()
            .HasOne(ete => ete.ExecucaoTreino)
            .WithMany(et => et.Exercicios)
            .HasForeignKey(ete => ete.ExecucaoTreinoId)
            .OnDelete(DeleteBehavior.Restrict);

        // 🔹 ExecucaoTreinoExercicio → TreinoExercicio
        builder.Entity<ExecucaoTreinoExercicio>()
            .HasOne(ete => ete.TreinoExercicio)
            .WithMany(te => te.Execucoes)
            .HasForeignKey(ete => ete.TreinoExercicioId)
            .OnDelete(DeleteBehavior.Restrict);
// ===================== SEED =====================

// 🔹 Categorias
var superiorId = Guid.Parse("11111111-1111-1111-1111-111111111111");
var inferiorId = Guid.Parse("22222222-2222-2222-2222-222222222222");
var aerobicoId = Guid.Parse("33333333-3333-3333-3333-333333333333");

builder.Entity<CategoriaMuscular>().HasData(
    new CategoriaMuscular { Id = superiorId, Nome = "Superior" },
    new CategoriaMuscular { Id = inferiorId, Nome = "Inferior" },
    new CategoriaMuscular { Id = aerobicoId, Nome = "Aeróbico" }
);

// 🔹 Grupos
var peitoId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
var costasId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
var bicepsId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");
var tricepsId = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd");
var ombroId = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee");
var abdomenId = Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff");

var pernaId = Guid.Parse("12121212-1212-1212-1212-121212121212");
var gluteoId = Guid.Parse("13131313-1313-1313-1313-131313131313");
var panturrilhaId = Guid.Parse("14141414-1414-1414-1414-141414141414");

var cardioId = Guid.Parse("15151515-1515-1515-1515-151515151515");

builder.Entity<GrupoMuscular>().HasData(
    // SUPERIOR
    new GrupoMuscular { Id = peitoId, Nome = "Peito", CategoriaMuscularId = superiorId },
    new GrupoMuscular { Id = costasId, Nome = "Costas", CategoriaMuscularId = superiorId },
    new GrupoMuscular { Id = bicepsId, Nome = "Bíceps", CategoriaMuscularId = superiorId },
    new GrupoMuscular { Id = tricepsId, Nome = "Tríceps", CategoriaMuscularId = superiorId },
    new GrupoMuscular { Id = ombroId, Nome = "Ombro", CategoriaMuscularId = superiorId },
    new GrupoMuscular { Id = abdomenId, Nome = "Abdômen", CategoriaMuscularId = superiorId },

    // INFERIOR
    new GrupoMuscular { Id = pernaId, Nome = "Perna", CategoriaMuscularId = inferiorId },
    new GrupoMuscular { Id = gluteoId, Nome = "Glúteo", CategoriaMuscularId = inferiorId },
    new GrupoMuscular { Id = panturrilhaId, Nome = "Panturrilha", CategoriaMuscularId = inferiorId },

    // AERÓBICO
    new GrupoMuscular { Id = cardioId, Nome = "Cardio", CategoriaMuscularId = aerobicoId }
);

// 🔹 Exercícios
builder.Entity<Exercicio>().HasData(

    // PEITO
    new Exercicio { Id = Guid.NewGuid(), Nome = "Supino Reto", GrupoMuscularId = peitoId, Ativo = true, DataCriacao = DateTime.Now },
    new Exercicio { Id = Guid.NewGuid(), Nome = "Supino Inclinado", GrupoMuscularId = peitoId, Ativo = true, DataCriacao = DateTime.Now },

    // COSTAS
    new Exercicio { Id = Guid.NewGuid(), Nome = "Puxada na Frente", GrupoMuscularId = costasId, Ativo = true, DataCriacao = DateTime.Now },

    // BÍCEPS
    new Exercicio { Id = Guid.NewGuid(), Nome = "Rosca Direta", GrupoMuscularId = bicepsId, Ativo = true, DataCriacao = DateTime.Now },

    // PERNA
    new Exercicio { Id = Guid.NewGuid(), Nome = "Agachamento", GrupoMuscularId = pernaId, Ativo = true, DataCriacao = DateTime.Now },

    // ABDÔMEN
    new Exercicio { Id = Guid.NewGuid(), Nome = "Abdominal Crunch", GrupoMuscularId = abdomenId, Ativo = true, DataCriacao = DateTime.Now },

    // CARDIO
    new Exercicio { Id = Guid.NewGuid(), Nome = "Corrida", GrupoMuscularId = cardioId, Ativo = true, DataCriacao = DateTime.Now }
);

// ===================== FIM =====================
            
    }

    
}