using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using BulkingPro.Models;

namespace BulkingPro.Data;

public class ApplicationDbContext : IdentityDbContext<Usuario>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    // 🔥 Tabelas
    public DbSet<Exercicio> Exercicios { get; set; }
    public DbSet<GrupoMuscular> GruposMusculares { get; set; }
    public DbSet<CategoriaMuscular> CategoriasMusculares { get; set; }
    public DbSet<PlanoTreino> PlanosTreino { get; set; }
    public DbSet<Treino> Treinos { get; set; }
    public DbSet<TreinoExercicio> TreinoExercicios { get; set; }  // ← NOME CORRETO
    public DbSet<ExecucaoTreino> ExecucoesTreino { get; set; }
    public DbSet<ExecucaoTreinoExercicio> ExecucoesTreinoExercicios { get; set; }
    public DbSet<Usuario> Usuarios { get; set; }

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
        var superiorId = 1;
        var inferiorId = 2;
        var aerobicoId = 3;

        builder.Entity<CategoriaMuscular>().HasData(
            new CategoriaMuscular { Id = superiorId, Nome = "Superior" },
            new CategoriaMuscular { Id = inferiorId, Nome = "Inferior" },
            new CategoriaMuscular { Id = aerobicoId, Nome = "Aeróbico" }
        );

        // 🔹 Grupos
        var peitoId = 1;
        var costasId = 2;
        var bicepsId = 3;
        var tricepsId = 4;
        var ombroId = 5;
        var abdomenId = 6;
        var pernaId = 7;
        var gluteoId = 8;
        var panturrilhaId = 9;
        var cardioId = 10;

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

        // ===================== EXERCÍCIOS (mantenha seus 65 exercícios) =====================
        // ... seus exercícios aqui (não vou repetir para não ficar enorme)
        // ... mas mantenha TODO o seed de exercícios que você já tem

        // ===================== ROLES E USUÁRIOS =====================

        // 🔹 IDs FIXOS
        var adminRoleId = "cc05b9bc-cc87-43ec-83f7-a889fd1de657";
        var modRoleId = "8b22cffb-ec0e-4617-896e-364eb078f8b2";
        var userRoleId = "c8d02904-cb9a-4242-90d9-a47c8b1cf750";

        var adminId = "6bd24189-05ed-4c32-80f8-85cb56e3d60b";
        var moderadorId = "5579ffe6-8226-4fca-acb0-bf5b223ff429";
        var usuarioId = "7f670255-b112-4737-ae61-2ea6ed4d9cbd";

        // Roles
        builder.Entity<IdentityRole>().HasData(
            new IdentityRole { Id = adminRoleId, Name = "Administrador", NormalizedName = "ADMINISTRADOR" },
            new IdentityRole { Id = modRoleId, Name = "Moderador", NormalizedName = "MODERADOR" },
            new IdentityRole { Id = userRoleId, Name = "Usuario", NormalizedName = "USUARIO" }
        );

        // Usuários
        var hasher = new PasswordHasher<Usuario>();

        var admin = new Usuario
        {
            Id = adminId,
            NomeCompleto = "Administrador",
            UserName = "admin@bulking.com",
            NormalizedUserName = "ADMIN@BULKING.COM",
            Email = "admin@bulking.com",
            NormalizedEmail = "ADMIN@BULKING.COM",
            EmailConfirmed = true,
            LockoutEnabled = false,
            SecurityStamp = Guid.NewGuid().ToString()
        };
        admin.PasswordHash = hasher.HashPassword(admin, "123456");

        var moderador = new Usuario
        {
            Id = moderadorId,
            NomeCompleto = "Moderador",
            UserName = "moderador@bulking.com",
            NormalizedUserName = "MODERADOR@BULKING.COM",
            Email = "moderador@bulking.com",
            NormalizedEmail = "MODERADOR@BULKING.COM",
            EmailConfirmed = true,
            LockoutEnabled = false,
            SecurityStamp = Guid.NewGuid().ToString()
        };
        moderador.PasswordHash = hasher.HashPassword(moderador, "123456");

        var usuario = new Usuario
        {
            Id = usuarioId,
            NomeCompleto = "Usuario",
            UserName = "usuario@bulking.com",
            NormalizedUserName = "USUARIO@BULKING.COM",
            Email = "usuario@bulking.com",
            NormalizedEmail = "USUARIO@BULKING.COM",
            EmailConfirmed = true,
            LockoutEnabled = false,
            SecurityStamp = Guid.NewGuid().ToString()
        };
        usuario.PasswordHash = hasher.HashPassword(usuario, "123456");

        builder.Entity<Usuario>().HasData(admin, moderador, usuario);

        // UserRoles
        builder.Entity<IdentityUserRole<string>>().HasData(
            new IdentityUserRole<string> { UserId = adminId, RoleId = adminRoleId },
            new IdentityUserRole<string> { UserId = moderadorId, RoleId = modRoleId },
            new IdentityUserRole<string> { UserId = usuarioId, RoleId = userRoleId }
        );
    }
}