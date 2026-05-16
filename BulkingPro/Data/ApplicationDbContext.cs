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

    // Tabelas
    public DbSet<Exercicio> Exercicios { get; set; }
    public DbSet<GrupoMuscular> GruposMusculares { get; set; }
    public DbSet<CategoriaMuscular> CategoriasMusculares { get; set; }
    public DbSet<PlanoTreino> PlanosTreino { get; set; }
    public DbSet<Treino> Treinos { get; set; }
    public DbSet<TreinoExercicio> TreinoExercicios { get; set; }
    public DbSet<ExecucaoTreino> ExecucoesTreino { get; set; }
    public DbSet<ExecucaoTreinoExercicio> ExecucoesTreinoExercicios { get; set; }
    public DbSet<Usuario> Usuarios { get; set; }

    // Novas tabelas
    public DbSet<AvaliacaoFisica> AvaliacoesFisicas { get; set; }
    public DbSet<AnamneseAluno>   Anamneses          { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // ── Relacionamentos ──────────────────────────────────────

        builder.Entity<PlanoTreino>()
            .HasOne(p => p.Treinador)
            .WithMany(u => u.PlanosComoTreinador)
            .HasForeignKey(p => p.TreinadorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<PlanoTreino>()
            .HasOne(p => p.Aluno)
            .WithMany(u => u.PlanosComoAluno)
            .HasForeignKey(p => p.AlunoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Treino>()
            .HasOne(t => t.PlanoTreino)
            .WithMany(p => p.Treinos)
            .HasForeignKey(t => t.PlanoTreinoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<TreinoExercicio>()
            .HasOne(te => te.Treino)
            .WithMany(t => t.TreinoExercicios)
            .HasForeignKey(te => te.TreinoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<TreinoExercicio>()
            .HasOne(te => te.Exercicio)
            .WithMany(e => e.TreinoExercicios)
            .HasForeignKey(te => te.ExercicioId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Exercicio>()
            .HasOne(e => e.GrupoMuscular)
            .WithMany(g => g.Exercicios)
            .HasForeignKey(e => e.GrupoMuscularId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<GrupoMuscular>()
            .HasOne(g => g.CategoriaMuscular)
            .WithMany(c => c.Grupos)
            .HasForeignKey(g => g.CategoriaMuscularId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ExecucaoTreino>()
            .HasOne(et => et.Treino)
            .WithMany(t => t.Execucoes)
            .HasForeignKey(et => et.TreinoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ExecucaoTreino>()
            .HasOne(et => et.Aluno)
            .WithMany()
            .HasForeignKey(et => et.AlunoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ExecucaoTreinoExercicio>()
            .HasOne(ete => ete.ExecucaoTreino)
            .WithMany(et => et.Exercicios)
            .HasForeignKey(ete => ete.ExecucaoTreinoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ExecucaoTreinoExercicio>()
            .HasOne(ete => ete.TreinoExercicio)
            .WithMany(te => te.Execucoes)
            .HasForeignKey(ete => ete.TreinoExercicioId)
            .OnDelete(DeleteBehavior.Restrict);

        // AvaliacaoFisica
        builder.Entity<AvaliacaoFisica>()
            .HasOne(a => a.Aluno)
            .WithMany()
            .HasForeignKey(a => a.AlunoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<AvaliacaoFisica>()
            .HasOne(a => a.Treinador)
            .WithMany()
            .HasForeignKey(a => a.TreinadorId)
            .OnDelete(DeleteBehavior.Restrict);

        // AnamneseAluno
        builder.Entity<AnamneseAluno>()
            .HasOne(a => a.Aluno)
            .WithMany()
            .HasForeignKey(a => a.AlunoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<AnamneseAluno>()
            .HasOne(a => a.Treinador)
            .WithMany()
            .HasForeignKey(a => a.TreinadorId)
            .OnDelete(DeleteBehavior.Restrict);

        // ── SEED: Categorias ─────────────────────────────────────
        builder.Entity<CategoriaMuscular>().HasData(
            new CategoriaMuscular { Id = 1, Nome = "Superior" },
            new CategoriaMuscular { Id = 2, Nome = "Inferior" },
            new CategoriaMuscular { Id = 3, Nome = "Aeróbico" }
        );

        // ── SEED: Grupos musculares ───────────────────────────────
        builder.Entity<GrupoMuscular>().HasData(
            new GrupoMuscular { Id = 1,  Nome = "Peito",       CategoriaMuscularId = 1 },
            new GrupoMuscular { Id = 2,  Nome = "Costas",      CategoriaMuscularId = 1 },
            new GrupoMuscular { Id = 3,  Nome = "Bíceps",      CategoriaMuscularId = 1 },
            new GrupoMuscular { Id = 4,  Nome = "Tríceps",     CategoriaMuscularId = 1 },
            new GrupoMuscular { Id = 5,  Nome = "Ombro",       CategoriaMuscularId = 1 },
            new GrupoMuscular { Id = 6,  Nome = "Abdômen",     CategoriaMuscularId = 1 },
            new GrupoMuscular { Id = 7,  Nome = "Perna",       CategoriaMuscularId = 2 },
            new GrupoMuscular { Id = 8,  Nome = "Glúteo",      CategoriaMuscularId = 2 },
            new GrupoMuscular { Id = 9,  Nome = "Panturrilha", CategoriaMuscularId = 2 },
            new GrupoMuscular { Id = 10, Nome = "Cardio",      CategoriaMuscularId = 3 }
        );

        // ── SEED: Exercícios ─────────────────────────────────────
        builder.Entity<Exercicio>().HasData(
            // PEITO (GrupoId = 1)
            new Exercicio { Id = 1,  Nome = "Supino Reto com Barra",      Descricao = "Exercício clássico para peitoral",    GrupoMuscularId = 1, InstrucoesExecucao = "Deite no banco, desça a barra até o peito e empurre.", Ativo = true, DataCriacao = new DateTime(2026, 4, 1) },
            new Exercicio { Id = 2,  Nome = "Supino Inclinado com Barra",  Descricao = "Foco na parte superior do peitoral", GrupoMuscularId = 1, InstrucoesExecucao = "Banco inclinado a 45°, mesmo movimento do supino reto.", Ativo = true, DataCriacao = new DateTime(2026, 4, 1) },
            new Exercicio { Id = 3,  Nome = "Supino com Halteres",         Descricao = "Maior amplitude de movimento",       GrupoMuscularId = 1, InstrucoesExecucao = "Desça os halteres até sentir o alongamento e empurre.", Ativo = true, DataCriacao = new DateTime(2026, 4, 1) },
            new Exercicio { Id = 4,  Nome = "Crossover",                   Descricao = "Isolamento do peitoral",              GrupoMuscularId = 1, InstrucoesExecucao = "Puxe os cabos em arco na frente do corpo.", Ativo = true, DataCriacao = new DateTime(2026, 4, 1) },
            new Exercicio { Id = 5,  Nome = "Peck Deck (Borboleta)",        Descricao = "Isolamento peitoral na máquina",      GrupoMuscularId = 1, InstrucoesExecucao = "Una os braços à frente contraindo o peitoral.", Ativo = true, DataCriacao = new DateTime(2026, 4, 1) },
            new Exercicio { Id = 6,  Nome = "Flexão de Braço",              Descricao = "Exercício com peso corporal",         GrupoMuscularId = 1, InstrucoesExecucao = "Corpo reto, desça o peito até próximo ao chão.", Ativo = true, DataCriacao = new DateTime(2026, 4, 1) },

            // COSTAS (GrupoId = 2)
            new Exercicio { Id = 7,  Nome = "Puxada Frente",               Descricao = "Dorsais e bíceps",                   GrupoMuscularId = 2, InstrucoesExecucao = "Puxe a barra até a altura do queixo.", Ativo = true, DataCriacao = new DateTime(2026, 4, 1) },
            new Exercicio { Id = 8,  Nome = "Remada Curvada",               Descricao = "Espessura das costas",                GrupoMuscularId = 2, InstrucoesExecucao = "Curvado, puxe a barra em direção ao abdômen.", Ativo = true, DataCriacao = new DateTime(2026, 4, 1) },
            new Exercicio { Id = 9,  Nome = "Remada Unilateral",            Descricao = "Trabalha os dorsais isoladamente",   GrupoMuscularId = 2, InstrucoesExecucao = "Apoie um joelho no banco, puxe o halter.", Ativo = true, DataCriacao = new DateTime(2026, 4, 1) },
            new Exercicio { Id = 10, Nome = "Puxada Triângulo",             Descricao = "Ênfase na parte baixa das costas",   GrupoMuscularId = 2, InstrucoesExecucao = "Use o triângulo no cabo e puxe até o abdômen.", Ativo = true, DataCriacao = new DateTime(2026, 4, 1) },
            new Exercicio { Id = 11, Nome = "Barra Fixa",                   Descricao = "Exercício com peso corporal",         GrupoMuscularId = 2, InstrucoesExecucao = "Puxe o corpo até o queixo ultrapassar a barra.", Ativo = true, DataCriacao = new DateTime(2026, 4, 1) },
            new Exercicio { Id = 12, Nome = "Levantamento Terra",           Descricao = "Exercício composto para costas",     GrupoMuscularId = 2, InstrucoesExecucao = "Levante a barra do chão com as costas retas.", Ativo = true, DataCriacao = new DateTime(2026, 4, 1) },

            // BÍCEPS (GrupoId = 3)
            new Exercicio { Id = 13, Nome = "Rosca Direta com Barra",       Descricao = "Exercício básico de bíceps",          GrupoMuscularId = 3, InstrucoesExecucao = "Curl com barra reta mantendo os cotovelos fixos.", Ativo = true, DataCriacao = new DateTime(2026, 4, 1) },
            new Exercicio { Id = 14, Nome = "Rosca Alternada",               Descricao = "Bíceps com halteres alternando",     GrupoMuscularId = 3, InstrucoesExecucao = "Curl com halteres alternando os braços.", Ativo = true, DataCriacao = new DateTime(2026, 4, 1) },
            new Exercicio { Id = 15, Nome = "Rosca Concentrada",             Descricao = "Isolamento do bíceps",                GrupoMuscularId = 3, InstrucoesExecucao = "Cotovelo apoiado na coxa, faça o curl.", Ativo = true, DataCriacao = new DateTime(2026, 4, 1) },
            new Exercicio { Id = 16, Nome = "Rosca Scott",                   Descricao = "Bíceps no banco scott",               GrupoMuscularId = 3, InstrucoesExecucao = "Braços apoiados no banco inclinado.", Ativo = true, DataCriacao = new DateTime(2026, 4, 1) },

            // TRÍCEPS (GrupoId = 4)
            new Exercicio { Id = 17, Nome = "Tríceps Corda",                 Descricao = "Isolamento do tríceps no cabo",      GrupoMuscularId = 4, InstrucoesExecucao = "Puxe a corda para baixo abrindo as pontas.", Ativo = true, DataCriacao = new DateTime(2026, 4, 1) },
            new Exercicio { Id = 18, Nome = "Tríceps Testa",                 Descricao = "Exercício com barra para tríceps",   GrupoMuscularId = 4, InstrucoesExecucao = "Deite, abaixe a barra até a testa e empurre.", Ativo = true, DataCriacao = new DateTime(2026, 4, 1) },
            new Exercicio { Id = 19, Nome = "Tríceps Francês",               Descricao = "Haltere sobre a cabeça",              GrupoMuscularId = 4, InstrucoesExecucao = "Segure halter, abaixe atrás da cabeça.", Ativo = true, DataCriacao = new DateTime(2026, 4, 1) },
            new Exercicio { Id = 20, Nome = "Mergulho (Paralelas)",          Descricao = "Peso corporal para tríceps",          GrupoMuscularId = 4, InstrucoesExecucao = "Desça entre as barras paralelas e empurre.", Ativo = true, DataCriacao = new DateTime(2026, 4, 1) },

            // OMBRO (GrupoId = 5)
            new Exercicio { Id = 21, Nome = "Desenvolvimento com Barra",    Descricao = "Exercício composto para ombros",     GrupoMuscularId = 5, InstrucoesExecucao = "Empurre a barra acima da cabeça.", Ativo = true, DataCriacao = new DateTime(2026, 4, 1) },
            new Exercicio { Id = 22, Nome = "Elevação Lateral",             Descricao = "Feixe médio do deltoide",             GrupoMuscularId = 5, InstrucoesExecucao = "Eleve os halteres lateralmente até a altura dos ombros.", Ativo = true, DataCriacao = new DateTime(2026, 4, 1) },
            new Exercicio { Id = 23, Nome = "Elevação Frontal",             Descricao = "Feixe anterior do deltoide",          GrupoMuscularId = 5, InstrucoesExecucao = "Eleve os halteres à frente até a altura dos ombros.", Ativo = true, DataCriacao = new DateTime(2026, 4, 1) },
            new Exercicio { Id = 24, Nome = "Crucifixo Invertido",          Descricao = "Feixe posterior do deltoide",         GrupoMuscularId = 5, InstrucoesExecucao = "Curvado, abra os braços para trás.", Ativo = true, DataCriacao = new DateTime(2026, 4, 1) },

            // ABDÔMEN (GrupoId = 6)
            new Exercicio { Id = 25, Nome = "Prancha",                       Descricao = "Estabilização do core",               GrupoMuscularId = 6, InstrucoesExecucao = "Mantenha o corpo reto apoiado nos antebraços.", Ativo = true, DataCriacao = new DateTime(2026, 4, 1) },
            new Exercicio { Id = 26, Nome = "Abdominal Supra",               Descricao = "Crunch básico",                       GrupoMuscularId = 6, InstrucoesExecucao = "Eleve o tronco contraindo o abdômen.", Ativo = true, DataCriacao = new DateTime(2026, 4, 1) },
            new Exercicio { Id = 27, Nome = "Abdominal Infra",               Descricao = "Elevação de pernas",                  GrupoMuscularId = 6, InstrucoesExecucao = "Deitado, eleve as pernas estendidas.", Ativo = true, DataCriacao = new DateTime(2026, 4, 1) },
            new Exercicio { Id = 28, Nome = "Abdominal Máquina",             Descricao = "Abdominal em máquina",                GrupoMuscularId = 6, InstrucoesExecucao = "Flexione o tronco na máquina de forma controlada.", Ativo = true, DataCriacao = new DateTime(2026, 4, 1) },

            // PERNA (GrupoId = 7)
            new Exercicio { Id = 29, Nome = "Agachamento Livre",             Descricao = "Exercício composto para pernas",     GrupoMuscularId = 7, InstrucoesExecucao = "Pés na largura dos ombros, desça até 90°.", Ativo = true, DataCriacao = new DateTime(2026, 4, 1) },
            new Exercicio { Id = 30, Nome = "Leg Press 45°",                 Descricao = "Quadríceps e glúteos",                GrupoMuscularId = 7, InstrucoesExecucao = "Empurre a plataforma sem travar os joelhos.", Ativo = true, DataCriacao = new DateTime(2026, 4, 1) },
            new Exercicio { Id = 31, Nome = "Cadeira Extensora",             Descricao = "Isolamento do quadríceps",           GrupoMuscularId = 7, InstrucoesExecucao = "Estenda os joelhos na máquina de forma controlada.", Ativo = true, DataCriacao = new DateTime(2026, 4, 1) },
            new Exercicio { Id = 32, Nome = "Mesa Flexora",                  Descricao = "Isquiotibiais",                       GrupoMuscularId = 7, InstrucoesExecucao = "Flexione os joelhos puxando os calcanhares.", Ativo = true, DataCriacao = new DateTime(2026, 4, 1) },
            new Exercicio { Id = 33, Nome = "Avanço (Lunge)",                Descricao = "Quadríceps e glúteos",                GrupoMuscularId = 7, InstrucoesExecucao = "Dê um passo à frente e desça o joelho traseiro.", Ativo = true, DataCriacao = new DateTime(2026, 4, 1) },
            new Exercicio { Id = 34, Nome = "Stiff",                         Descricao = "Isquiotibiais e glúteos",             GrupoMuscularId = 7, InstrucoesExecucao = "Incline o tronco com as pernas semi-estendidas.", Ativo = true, DataCriacao = new DateTime(2026, 4, 1) },

            // GLÚTEO (GrupoId = 8)
            new Exercicio { Id = 35, Nome = "Elevação Pélvica (Hip Thrust)", Descricao = "Glúteo máximo",                       GrupoMuscularId = 8, InstrucoesExecucao = "Apoie os ombros no banco e empurre o quadril.", Ativo = true, DataCriacao = new DateTime(2026, 4, 1) },
            new Exercicio { Id = 36, Nome = "Abdução de Quadril",            Descricao = "Glúteo médio na máquina",             GrupoMuscularId = 8, InstrucoesExecucao = "Abra as pernas contra a resistência.", Ativo = true, DataCriacao = new DateTime(2026, 4, 1) },
            new Exercicio { Id = 37, Nome = "Glúteo no Cabo",                Descricao = "Isolamento do glúteo",                GrupoMuscularId = 8, InstrucoesExecucao = "Coloque o cabo no tornozelo e estenda a perna para trás.", Ativo = true, DataCriacao = new DateTime(2026, 4, 1) },

            // PANTURRILHA (GrupoId = 9)
            new Exercicio { Id = 38, Nome = "Gêmeos em Pé",                  Descricao = "Panturrilha em pé",                   GrupoMuscularId = 9, InstrucoesExecucao = "Eleve os calcanhares o máximo possível.", Ativo = true, DataCriacao = new DateTime(2026, 4, 1) },
            new Exercicio { Id = 39, Nome = "Gêmeos Sentado",                Descricao = "Sóleo (panturrilha sentado)",         GrupoMuscularId = 9, InstrucoesExecucao = "Na máquina sentado, eleve os calcanhares.", Ativo = true, DataCriacao = new DateTime(2026, 4, 1) },

            // CARDIO (GrupoId = 10)
            new Exercicio { Id = 40, Nome = "Esteira",                       Descricao = "Cardio na esteira",                   GrupoMuscularId = 10, InstrucoesExecucao = "Caminhe ou corra em ritmo moderado.", Ativo = true, DataCriacao = new DateTime(2026, 4, 1) },
            new Exercicio { Id = 41, Nome = "Bicicleta Ergométrica",         Descricao = "Cardio de baixo impacto",             GrupoMuscularId = 10, InstrucoesExecucao = "Pedale em cadência constante.", Ativo = true, DataCriacao = new DateTime(2026, 4, 1) },
            new Exercicio { Id = 42, Nome = "Elíptico",                      Descricao = "Cardio com baixo impacto articular", GrupoMuscularId = 10, InstrucoesExecucao = "Movimento elíptico contínuo.", Ativo = true, DataCriacao = new DateTime(2026, 4, 1) },
            new Exercicio { Id = 43, Nome = "Corda (Jump Rope)",             Descricao = "Cardio de alta intensidade",          GrupoMuscularId = 10, InstrucoesExecucao = "Pule a corda em ritmo constante.", Ativo = true, DataCriacao = new DateTime(2026, 4, 1) }
        );

        // ── SEED: Roles ──────────────────────────────────────────
        // Roles e usuários são criados via UserManager no Program.cs
        // NÃO duplicar aqui para evitar conflito com o seed do Program.cs
    }
}
