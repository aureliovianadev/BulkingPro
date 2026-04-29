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
    public DbSet<TreinoExercicio> TreinosExercicios { get; set; }
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

        builder.Entity<Exercicio>().HasData(

            // ================= PEITO =================
            new Exercicio { Id = 1, Nome = "Supino Reto", GrupoMuscularId = peitoId, Ativo = true, DataCriacao = new DateTime(2026, 4, 2) },
            new Exercicio { Id = 2, Nome = "Supino Inclinado", GrupoMuscularId = peitoId, Ativo = true, DataCriacao = new DateTime(2026, 4, 2) },
            new Exercicio { Id = 3, Nome = "Supino Declinado", GrupoMuscularId = peitoId, Ativo = true, DataCriacao = new DateTime(2026, 4, 2) },
            new Exercicio { Id = 4, Nome = "Supino Vertical", GrupoMuscularId = peitoId, Ativo = true, DataCriacao = new DateTime(2026, 4, 2) },
            new Exercicio { Id = 5, Nome = "Supino Barra Guiada", GrupoMuscularId = peitoId, Ativo = true, DataCriacao = new DateTime(2026, 4, 2) },
            new Exercicio { Id = 6, Nome = "Pull Over", GrupoMuscularId = peitoId, Ativo = true, DataCriacao = new DateTime(2026, 4, 2) },
            new Exercicio { Id = 7, Nome = "Crucifixo Reto Máquina", GrupoMuscularId = peitoId, Ativo = true, DataCriacao = new DateTime(2026, 4, 2) },
            new Exercicio { Id = 8, Nome = "Crucifixo Inclinado Máquina", GrupoMuscularId = peitoId, Ativo = true, DataCriacao = new DateTime(2026, 4, 2) },
            new Exercicio { Id = 9, Nome = "Cross Over", GrupoMuscularId = peitoId, Ativo = true, DataCriacao = new DateTime(2026, 4, 2) },
            new Exercicio { Id = 10, Nome = "Peck Deck", GrupoMuscularId = peitoId, Ativo = true, DataCriacao = new DateTime(2026, 4, 2) },

            // ================= COSTAS =================
            new Exercicio { Id = 11, Nome = "Puxada Frente", GrupoMuscularId = costasId, Ativo = true, DataCriacao = new DateTime(2026, 4, 2) },
            new Exercicio { Id = 12, Nome = "Puxador Multi Pegadas", GrupoMuscularId = costasId, Ativo = true, DataCriacao = new DateTime(2026, 4, 2) },
            new Exercicio { Id = 13, Nome = "Puxada Cruzada", GrupoMuscularId = costasId, Ativo = true, DataCriacao = new DateTime(2026, 4, 2) },
            new Exercicio { Id = 14, Nome = "Puxada Articulada", GrupoMuscularId = costasId, Ativo = true, DataCriacao = new DateTime(2026, 4, 2) },
            new Exercicio { Id = 15, Nome = "Lat Machine", GrupoMuscularId = costasId, Ativo = true, DataCriacao = new DateTime(2026, 4, 2) },
            new Exercicio { Id = 16, Nome = "Peck Deck Inverso", GrupoMuscularId = costasId, Ativo = true, DataCriacao = new DateTime(2026, 4, 2) },
            new Exercicio { Id = 17, Nome = "Remada Máquina", GrupoMuscularId = costasId, Ativo = true, DataCriacao = new DateTime(2026, 4, 2) },
            new Exercicio { Id = 18, Nome = "Remada Baixa Polia", GrupoMuscularId = costasId, Ativo = true, DataCriacao = new DateTime(2026, 4, 2) },
            new Exercicio { Id = 19, Nome = "Remada Cavalinho", GrupoMuscularId = costasId, Ativo = true, DataCriacao = new DateTime(2026, 4, 2) },
            new Exercicio { Id = 20, Nome = "Remada 45°", GrupoMuscularId = costasId, Ativo = true, DataCriacao = new DateTime(2026, 4, 2) },
            new Exercicio { Id = 21, Nome = "Graviton", GrupoMuscularId = costasId, Ativo = true, DataCriacao = new DateTime(2026, 4, 2) },

            // ================= OMBRO =================
            new Exercicio { Id = 22, Nome = "Elevação Lateral", GrupoMuscularId = ombroId, Ativo = true, DataCriacao = new DateTime(2026, 4, 2) },
            new Exercicio { Id = 23, Nome = "Elevação Frontal", GrupoMuscularId = ombroId, Ativo = true, DataCriacao = new DateTime(2026, 4, 2) },
            new Exercicio { Id = 24, Nome = "Desenvolvimento Máquina", GrupoMuscularId = ombroId, Ativo = true, DataCriacao = new DateTime(2026, 4, 2) },
            new Exercicio { Id = 25, Nome = "Desenvolvimento Halter", GrupoMuscularId = ombroId, Ativo = true, DataCriacao = new DateTime(2026, 4, 2) },
            new Exercicio { Id = 26, Nome = "Shoulder Press", GrupoMuscularId = ombroId, Ativo = true, DataCriacao = new DateTime(2026, 4, 2) },
            new Exercicio { Id = 27, Nome = "Crucifixo Inverso", GrupoMuscularId = ombroId, Ativo = true, DataCriacao = new DateTime(2026, 4, 2) },
            new Exercicio { Id = 28, Nome = "Remada Alta", GrupoMuscularId = ombroId, Ativo = true, DataCriacao = new DateTime(2026, 4, 2) },
            new Exercicio { Id = 29, Nome = "Encolhimento", GrupoMuscularId = ombroId, Ativo = true, DataCriacao = new DateTime(2026, 4, 2) },

            // ================= BÍCEPS =================
            new Exercicio { Id = 30, Nome = "Rosca Scott", GrupoMuscularId = bicepsId, Ativo = true, DataCriacao = new DateTime(2026, 4, 2) },
            new Exercicio { Id = 31, Nome = "Rosca Martelo", GrupoMuscularId = bicepsId, Ativo = true, DataCriacao = new DateTime(2026, 4, 2) },
            new Exercicio { Id = 32, Nome = "Rosca Direta", GrupoMuscularId = bicepsId, Ativo = true, DataCriacao = new DateTime(2026, 4, 2) },
            new Exercicio { Id = 33, Nome = "Rosca Concentrada", GrupoMuscularId = bicepsId, Ativo = true, DataCriacao = new DateTime(2026, 4, 2) },
            new Exercicio { Id = 34, Nome = "Rosca Inversa", GrupoMuscularId = bicepsId, Ativo = true, DataCriacao = new DateTime(2026, 4, 2) },
            new Exercicio { Id = 35, Nome = "Rosca Alternada", GrupoMuscularId = bicepsId, Ativo = true, DataCriacao = new DateTime(2026, 4, 2) },

            // ================= TRÍCEPS =================
            new Exercicio { Id = 36, Nome = "Tríceps Corda", GrupoMuscularId = tricepsId, Ativo = true, DataCriacao = new DateTime(2026, 4, 2) },
            new Exercicio { Id = 37, Nome = "Tríceps Barra", GrupoMuscularId = tricepsId, Ativo = true, DataCriacao = new DateTime(2026, 4, 2) },
            new Exercicio { Id = 38, Nome = "Tríceps Barra Inversa", GrupoMuscularId = tricepsId, Ativo = true, DataCriacao = new DateTime(2026, 4, 2) },
            new Exercicio { Id = 39, Nome = "Francês Máquina", GrupoMuscularId = tricepsId, Ativo = true, DataCriacao = new DateTime(2026, 4, 2) },
            new Exercicio { Id = 40, Nome = "Paralela Máquina", GrupoMuscularId = tricepsId, Ativo = true, DataCriacao = new DateTime(2026, 4, 2) },
            new Exercicio { Id = 41, Nome = "Tríceps Testa", GrupoMuscularId = tricepsId, Ativo = true, DataCriacao = new DateTime(2026, 4, 2) },

            // ================= PERNA =================
            new Exercicio { Id = 42, Nome = "Leg Press 45°", GrupoMuscularId = pernaId, Ativo = true, DataCriacao = new DateTime(2026, 4, 2) },
            new Exercicio { Id = 43, Nome = "Leg Press Horizontal", GrupoMuscularId = pernaId, Ativo = true, DataCriacao = new DateTime(2026, 4, 2) },
            new Exercicio { Id = 44, Nome = "Cadeira Extensora", GrupoMuscularId = pernaId, Ativo = true, DataCriacao = new DateTime(2026, 4, 2) },
            new Exercicio { Id = 45, Nome = "Hack Machine", GrupoMuscularId = pernaId, Ativo = true, DataCriacao = new DateTime(2026, 4, 2) },
            new Exercicio { Id = 46, Nome = "Agachamento Smith", GrupoMuscularId = pernaId, Ativo = true, DataCriacao = new DateTime(2026, 4, 2) },
            new Exercicio { Id = 47, Nome = "Agachamento Livre", GrupoMuscularId = pernaId, Ativo = true, DataCriacao = new DateTime(2026, 4, 2) },
            new Exercicio { Id = 48, Nome = "Cadeira Flexora", GrupoMuscularId = pernaId, Ativo = true, DataCriacao = new DateTime(2026, 4, 2) },
            new Exercicio { Id = 49, Nome = "Stiff", GrupoMuscularId = pernaId, Ativo = true, DataCriacao = new DateTime(2026, 4, 2) },

            // ================= GLÚTEO =================
            new Exercicio { Id = 50, Nome = "Elevação Pélvica", GrupoMuscularId = gluteoId, Ativo = true, DataCriacao = new DateTime(2026, 4, 2) },
            new Exercicio { Id = 51, Nome = "Agachamento Búlgaro", GrupoMuscularId = gluteoId, Ativo = true, DataCriacao = new DateTime(2026, 4, 2) },
            new Exercicio { Id = 52, Nome = "Glúteo Máquina", GrupoMuscularId = gluteoId, Ativo = true, DataCriacao = new DateTime(2026, 4, 2) },
            new Exercicio { Id = 53, Nome = "Abdutora", GrupoMuscularId = gluteoId, Ativo = true, DataCriacao = new DateTime(2026, 4, 2) },

            // ================= PANTURRILHA =================
            new Exercicio { Id = 54, Nome = "Gêmeos em Pé", GrupoMuscularId = panturrilhaId, Ativo = true, DataCriacao = new DateTime(2026, 4, 2) },
            new Exercicio { Id = 55, Nome = "Gêmeos Sentado", GrupoMuscularId = panturrilhaId, Ativo = true, DataCriacao = new DateTime(2026, 4, 2) },

            // ================= ABDÔMEN =================
            new Exercicio { Id = 56, Nome = "Abdominal Máquina", GrupoMuscularId = abdomenId, Ativo = true, DataCriacao = new DateTime(2026, 4, 2) },
            new Exercicio { Id = 57, Nome = "Abdominal Supra", GrupoMuscularId = abdomenId, Ativo = true, DataCriacao = new DateTime(2026, 4, 2) },
            new Exercicio { Id = 58, Nome = "Abdominal Oblíquo", GrupoMuscularId = abdomenId, Ativo = true, DataCriacao = new DateTime(2026, 4, 2) },
            new Exercicio { Id = 59, Nome = "Prancha", GrupoMuscularId = abdomenId, Ativo = true, DataCriacao = new DateTime(2026, 4, 2) },

            // ================= AERÓBICO =================
            new Exercicio { Id = 60, Nome = "Esteira", GrupoMuscularId = cardioId, Ativo = true, DataCriacao = new DateTime(2026, 4, 2) },
            new Exercicio { Id = 61, Nome = "Bike", GrupoMuscularId = cardioId, Ativo = true, DataCriacao = new DateTime(2026, 4, 2) },
            new Exercicio { Id = 62, Nome = "Air Bike", GrupoMuscularId = cardioId, Ativo = true, DataCriacao = new DateTime(2026, 4, 2) },
            new Exercicio { Id = 63, Nome = "Escada", GrupoMuscularId = cardioId, Ativo = true, DataCriacao = new DateTime(2026, 4, 2) },
            new Exercicio { Id = 64, Nome = "Elíptico", GrupoMuscularId = cardioId, Ativo = true, DataCriacao = new DateTime(2026, 4, 2) },
            new Exercicio { Id = 65, Nome = "Remo", GrupoMuscularId = cardioId, Ativo = true, DataCriacao = new DateTime(2026, 4, 2) }
        );

        // ===================== ROLES E USUÁRIOS =====================

        // 🔹 IDs FIXOS (NUNCA ALTERE DEPOIS DE CRIAR MIGRATION)
        var adminRoleId = "cc05b9bc-cc87-43ec-83f7-a889fd1de657";
        var modRoleId = "8b22cffb-ec0e-4617-896e-364eb078f8b2";
        var userRoleId = "c8d02904-cb9a-4242-90d9-a47c8b1cf750";

        var adminId = "6bd24189-05ed-4c32-80f8-85cb56e3d60b";
        var moderadorId = "5579ffe6-8226-4fca-acb0-bf5b223ff429";
        var usuarioId = "7f670255-b112-4737-ae61-2ea6ed4d9cbd";

        // ===================== ROLES =====================
        builder.Entity<IdentityRole>().HasData(
            new IdentityRole
            {
                Id = adminRoleId,
                Name = "Administrador",
                NormalizedName = "ADMINISTRADOR"
            },
            new IdentityRole
            {
                Id = modRoleId,
                Name = "Moderador",
                NormalizedName = "MODERADOR"
            },
            new IdentityRole
            {
                Id = userRoleId,
                Name = "Usuario",
                NormalizedName = "USUARIO"
            }
        );

        // ===================== USUÁRIOS =====================
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

        var usuario = new Usuario { Id = usuarioId, NomeCompleto = "Usuario", UserName = "usuario@bulking.com", NormalizedUserName = "USUARIO@BULKING.COM", Email = "usuario@bulking.com", NormalizedEmail = "USUARIO@BULKING.COM", EmailConfirmed = true, LockoutEnabled = false, SecurityStamp = Guid.NewGuid().ToString() };

        usuario.PasswordHash = hasher.HashPassword(usuario, "123456");

        builder.Entity<Usuario>().HasData(admin, moderador, usuario);

        // ===================== RELAÇÃO USER ↔ ROLE =====================
        builder.Entity<IdentityUserRole<string>>().HasData(
            new IdentityUserRole<string>
            {
                UserId = adminId,
                RoleId = adminRoleId
            },
            new IdentityUserRole<string>
            {
                UserId = moderadorId,
                RoleId = modRoleId
            },
            new IdentityUserRole<string>
            {
                UserId = usuarioId,
                RoleId = userRoleId
            }
        );

    }


}