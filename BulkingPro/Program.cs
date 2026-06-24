using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using BulkingPro.Data;
using BulkingPro.Models;
using BulkingPro.Services;

var builder = WebApplication.CreateBuilder(args);

// ── Banco de dados (Pomelo MySQL) ────────────────────────────
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// ── Identity ─────────────────────────────────────────────────
builder.Services.AddIdentity<Usuario, IdentityRole>(options =>
{
    options.Password.RequireDigit           = false;
    options.Password.RequireLowercase       = false;
    options.Password.RequireUppercase       = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength         = 6;
    options.SignIn.RequireConfirmedAccount  = false;
    options.SignIn.RequireConfirmedEmail    = false;
    options.User.RequireUniqueEmail         = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// ── Cookie / Redirecionamentos ───────────────────────────────
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath         = "/Account/Login";
    options.AccessDeniedPath  = "/Account/Login";
    options.LogoutPath        = "/Account/Logout";
    options.ExpireTimeSpan    = TimeSpan.FromDays(7);
    options.SlidingExpiration = true;
});

// ── Serviços de E-mail (Recuperação de Senha) ────────────────
builder.Services.AddScoped<IEmailService, EmailService>();

// ── MVC + Razor Pages ────────────────────────────────────────
builder.Services.AddControllersWithViews(options =>
{
    options.ModelBindingMessageProvider.SetValueIsInvalidAccessor(
        (x) => $"O valor '{x}' é inválido.");
    options.ModelBindingMessageProvider.SetValueMustNotBeNullAccessor(
        (x) => "Este campo é obrigatório.");
    options.ModelBindingMessageProvider.SetAttemptedValueIsInvalidAccessor(
        (x, y) => $"O valor '{x}' não é válido para o campo {y}.");
    options.ModelBindingMessageProvider.SetNonPropertyAttemptedValueIsInvalidAccessor(
        (x) => $"O valor '{x}' é inválido.");
    options.ModelBindingMessageProvider.SetUnknownValueIsInvalidAccessor(
        (x) => $"O valor fornecido é inválido para {x}.");
    options.ModelBindingMessageProvider.SetMissingBindRequiredValueAccessor(
        (x) => $"O campo {x} é obrigatório.");
    options.ModelBindingMessageProvider.SetMissingKeyOrValueAccessor(
        () => "Chave ou valor obrigatório.");
});
builder.Services.AddRazorPages();

// ─────────────────────────────────────────────────────────────
var app = builder.Build();

// ── Seed: banco + roles + usuários padrão ────────────────────
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var db = services.GetRequiredService<ApplicationDbContext>();

    // Migrations são críticas: se falharem, o app não deve continuar rodando
    // "quebrado" silenciosamente — melhor falhar alto e visível no startup.
    await db.Database.MigrateAsync();

    try
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<Usuario>>();

        // Roles
        string[] roles = { "Administrador", "Moderador", "Usuario" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        // ── Admin ──
        await CriarUsuario(userManager, "admin@bulkingpro.com", "Administrador Master", "admin123", "Administrador");

        // ── Personal Trainer ──
        var personal = await CriarUsuario(userManager, "personal@bulkingpro.com", "Carlos Personal", "personal123", "Moderador");

        // ── Aluno (com vínculo ao Personal e plano de treino completo) ──
        if (personal != null)
        {
            var aluno = await CriarUsuario(userManager, "aluno@bulkingpro.com", "João Aluno", "aluno123", "Usuario");

            if (aluno != null)
            {
                // Verifica se já existe um plano para este aluno com este personal
                var planoExistente = await db.PlanosTreino
                    .AnyAsync(p => p.AlunoId == aluno.Id && p.TreinadorId == personal.Id);

                if (!planoExistente)
                {
                    // ── Definição dos 5 ciclos de treino (2 semanas cada) ──
                    var ciclos = new[]
                    {
                        new { Inicio = new DateTime(2026, 5, 3),  Fim = new DateTime(2026, 5, 17), Titulo = "Ciclo 1 - Hipertrofia (Adaptação)" },
                        new { Inicio = new DateTime(2026, 5, 18), Fim = new DateTime(2026, 6, 1),  Titulo = "Ciclo 2 - Hipertrofia (Volume)" },
                        new { Inicio = new DateTime(2026, 6, 2),  Fim = new DateTime(2026, 6, 15), Titulo = "Ciclo 3 - Hipertrofia (Intensidade)" },
                        new { Inicio = new DateTime(2026, 6, 16), Fim = new DateTime(2026, 6, 30), Titulo = "Ciclo 4 - Hipertrofia (Progressão de Carga)" },
                        new { Inicio = new DateTime(2026, 7, 1),  Fim = new DateTime(2026, 7, 15), Titulo = "Ciclo 5 - Hipertrofia (Pico)" }
                    };

                    var diasSemana = new[]
                    {
                        new { Nome = "Segunda-feira", Ordem = 1, Dia = DayOfWeek.Monday },
                        new { Nome = "Terça-feira",   Ordem = 2, Dia = DayOfWeek.Tuesday },
                        new { Nome = "Quarta-feira",  Ordem = 3, Dia = DayOfWeek.Wednesday },
                        new { Nome = "Quinta-feira",  Ordem = 4, Dia = DayOfWeek.Thursday },
                        new { Nome = "Sexta-feira",   Ordem = 5, Dia = DayOfWeek.Friday },
                        new { Nome = "Sábado",        Ordem = 6, Dia = DayOfWeek.Saturday }
                    };

                    var hoje = DateTime.Today;
                    int cicloIndex = 0;

                    foreach (var ciclo in ciclos)
                    {
                        cicloIndex++;
                        decimal incrementoCarga = (cicloIndex - 1) * 2.5m; // leve progressão de carga entre ciclos

                        // Plano já concluído (data fim no passado) ou ainda ativo
                        int statusPlano = ciclo.Fim < hoje ? 2 : 1;

                        var plano = new PlanoTreino
                        {
                            TreinadorId = personal.Id,
                            AlunoId = aluno.Id,
                            Titulo = ciclo.Titulo,
                            Objetivo = "Hipertrofia (ganho de massa muscular)",
                            DataInicio = ciclo.Inicio,
                            DataFim = ciclo.Fim,
                            Status = statusPlano,
                            DataCriacao = ciclo.Inicio
                        };
                        db.PlanosTreino.Add(plano);
                        await db.SaveChangesAsync();

                        foreach (var diaInfo in diasSemana)
                        {
                            var treino = new Treino
                            {
                                PlanoTreinoId = plano.Id,
                                Nome = diaInfo.Nome,
                                OrdemDia = diaInfo.Ordem,
                                Observacoes = diaInfo.Ordem % 2 == 0 ? "Foco em membros inferiores" : "Foco em membros superiores",
                                DataCriacao = ciclo.Inicio
                            };
                            db.Treinos.Add(treino);
                            await db.SaveChangesAsync();

                            // ── Exercícios do dia (com progressão leve de carga entre ciclos) ──
                            var exerciciosDoDia = new List<(int ExercicioId, int Ordem, int Series, string Repeticoes, decimal? Carga, int? Descanso)>();

                            if (diaInfo.Ordem % 2 == 1) // Dias ímpares: Superiores
                            {
                                exerciciosDoDia.AddRange(new (int, int, int, string, decimal?, int?)[]
                                {
                                    (1, 1, 4, "12", 40.0m + incrementoCarga, 60),
                                    (3, 2, 4, "12", 30.0m + incrementoCarga, 60),
                                    (7, 3, 4, "12", 50.0m + incrementoCarga, 60),
                                    (9, 4, 3, "15", 20.0m + incrementoCarga, 45),
                                    (13, 5, 3, "15", 15.0m + incrementoCarga, 45),
                                    (17, 6, 3, "15", 20.0m + incrementoCarga, 45),
                                    (21, 7, 4, "12", 25.0m + incrementoCarga, 60)
                                });
                            }
                            else // Dias pares: Inferiores
                            {
                                exerciciosDoDia.AddRange(new (int, int, int, string, decimal?, int?)[]
                                {
                                    (29, 1, 4, "10", 60.0m + incrementoCarga, 90),
                                    (30, 2, 4, "12", 80.0m + incrementoCarga, 60),
                                    (31, 3, 4, "15", 40.0m + incrementoCarga, 45),
                                    (32, 4, 4, "15", 35.0m + incrementoCarga, 45),
                                    (35, 5, 3, "15", 50.0m + incrementoCarga, 60),
                                    (38, 6, 4, "20", 30.0m + incrementoCarga, 30)
                                });
                            }

                            var treinoExerciciosCriados = new List<TreinoExercicio>();
                            foreach (var ex in exerciciosDoDia)
                            {
                                var te = new TreinoExercicio
                                {
                                    TreinoId = treino.Id,
                                    ExercicioId = ex.ExercicioId,
                                    Ordem = ex.Ordem,
                                    SeriesPlanejadas = ex.Series,
                                    RepeticoesPlanejadas = ex.Repeticoes,
                                    CargaPlanejada = ex.Carga,
                                    TempoDescanso = ex.Descanso,
                                    DataCriacao = ciclo.Inicio
                                };
                                db.TreinoExercicios.Add(te);
                                treinoExerciciosCriados.Add(te);
                            }
                            await db.SaveChangesAsync();

                            // ── Gera execuções reais nas datas do ciclo que caem nesse dia da semana ──
                            // (só gera execuções para datas que já passaram, até hoje)
                            for (var data = ciclo.Inicio; data <= ciclo.Fim; data = data.AddDays(1))
                            {
                                if (data.DayOfWeek != diaInfo.Dia) continue;
                                if (data > hoje) continue; // não cria execução para o futuro

                                var execucao = new ExecucaoTreino
                                {
                                    TreinoId = treino.Id,
                                    AlunoId = aluno.Id,
                                    DataExecucao = data,
                                    DuracaoMinutos = 55 + (cicloIndex * 2),
                                    EsforcoPercebido = Math.Min(6 + cicloIndex, 10),
                                    ObservacoesGerais = "Treino concluído conforme planejado.",
                                    Concluido = true,
                                    DataCriacao = data
                                };
                                db.ExecucoesTreino.Add(execucao);
                                await db.SaveChangesAsync();

                                foreach (var te in treinoExerciciosCriados)
                                {
                                    db.ExecucoesTreinoExercicios.Add(new ExecucaoTreinoExercicio
                                    {
                                        ExecucaoTreinoId = execucao.Id,
                                        TreinoExercicioId = te.Id,
                                        SeriesFeitas = te.SeriesPlanejadas,
                                        RepeticoesFeitas = te.RepeticoesPlanejadas,
                                        CargaUsada = te.CargaPlanejada,
                                        Concluido = true,
                                        Observacoes = ""
                                    });
                                }
                                await db.SaveChangesAsync();
                            }
                        }

                        Console.WriteLine($"📋 Plano '{plano.Titulo}' criado ({ciclo.Inicio:dd/MM} a {ciclo.Fim:dd/MM}) - status {(statusPlano == 1 ? "Ativo" : "Concluído")}");
                    }

                    // ── Horários de Atendimento (Segunda a Sábado) ──
                    var horariosAtendimento = new[]
                    {
                        new { Dia = DayOfWeek.Monday,    HoraInicio = new TimeSpan(8, 0, 0),  HoraFim = new TimeSpan(9, 0, 0) },
                        new { Dia = DayOfWeek.Tuesday,   HoraInicio = new TimeSpan(8, 0, 0),  HoraFim = new TimeSpan(9, 0, 0) },
                        new { Dia = DayOfWeek.Wednesday, HoraInicio = new TimeSpan(8, 0, 0),  HoraFim = new TimeSpan(9, 0, 0) },
                        new { Dia = DayOfWeek.Thursday,  HoraInicio = new TimeSpan(8, 0, 0),  HoraFim = new TimeSpan(9, 0, 0) },
                        new { Dia = DayOfWeek.Friday,    HoraInicio = new TimeSpan(8, 0, 0),  HoraFim = new TimeSpan(9, 0, 0) },
                        new { Dia = DayOfWeek.Saturday,  HoraInicio = new TimeSpan(10, 0, 0), HoraFim = new TimeSpan(11, 0, 0) }
                    };

                    foreach (var horario in horariosAtendimento)
                    {
                        db.AlunosHorariosAtendimento.Add(new AlunoHorarioAtendimento
                        {
                            PersonalId = personal.Id,
                            AlunoId = aluno.Id,
                            DiaSemana = horario.Dia,
                            HoraInicio = horario.HoraInicio,
                            HoraFim = horario.HoraFim,
                            Ativo = true,
                            DataCriacao = DateTime.Now
                        });
                    }
                    await db.SaveChangesAsync();

                    // ── Avaliações Físicas (medidas + IMC calculado a partir de Altura/Peso) ──
                    var avaliacoes = new[]
                    {
                        new AvaliacaoFisica
                        {
                            AlunoId = aluno.Id,
                            TreinadorId = personal.Id,
                            DataAvaliacao = new DateTime(2026, 5, 18),
                            Altura = 1.78m,
                            Peso = 72.4m,
                            Pescoco = 37.0m,
                            Ombro = 112.0m,
                            ToraxContrai = 98.0m,
                            ToraxRelax = 95.0m,
                            BicepsDireito = 32.0m,
                            BicepsEsquerdo = 31.5m,
                            Cintura = 81.0m,
                            Abdomen = 84.0m,
                            Quadril = 96.0m,
                            CoxaDireita = 54.0m,
                            CoxaEsquerda = 53.5m,
                            PanturrilhaDireita = 36.0m,
                            PanturrilhaEsquerda = 35.5m,
                            Observacoes = "Avaliação inicial do Ciclo 2. Aluno iniciando adaptação ao volume de treino.",
                            DataCriacao = new DateTime(2026, 5, 18)
                        },
                        new AvaliacaoFisica
                        {
                            AlunoId = aluno.Id,
                            TreinadorId = personal.Id,
                            DataAvaliacao = new DateTime(2026, 6, 2),
                            Altura = 1.78m,
                            Peso = 74.1m,
                            Pescoco = 37.2m,
                            Ombro = 113.5m,
                            ToraxContrai = 99.5m,
                            ToraxRelax = 96.5m,
                            BicepsDireito = 33.0m,
                            BicepsEsquerdo = 32.5m,
                            Cintura = 80.0m,
                            Abdomen = 83.0m,
                            Quadril = 96.5m,
                            CoxaDireita = 55.0m,
                            CoxaEsquerda = 54.5m,
                            PanturrilhaDireita = 36.5m,
                            PanturrilhaEsquerda = 36.0m,
                            Observacoes = "Avaliação do Ciclo 3. Ganho de massa muscular visível, leve redução de cintura.",
                            DataCriacao = new DateTime(2026, 6, 2)
                        },
                        new AvaliacaoFisica
                        {
                            AlunoId = aluno.Id,
                            TreinadorId = personal.Id,
                            DataAvaliacao = new DateTime(2026, 6, 16),
                            Altura = 1.78m,
                            Peso = 75.6m,
                            Pescoco = 37.5m,
                            Ombro = 115.0m,
                            ToraxContrai = 101.0m,
                            ToraxRelax = 98.0m,
                            BicepsDireito = 34.0m,
                            BicepsEsquerdo = 33.5m,
                            Cintura = 79.5m,
                            Abdomen = 82.0m,
                            Quadril = 97.0m,
                            CoxaDireita = 56.0m,
                            CoxaEsquerda = 55.5m,
                            PanturrilhaDireita = 37.0m,
                            PanturrilhaEsquerda = 36.5m,
                            Observacoes = "Avaliação do Ciclo 4. Progresso consistente de hipertrofia, ótima evolução geral.",
                            DataCriacao = new DateTime(2026, 6, 16)
                        }
                    };

                    db.AvaliacoesFisicas.AddRange(avaliacoes);
                    await db.SaveChangesAsync();
                    Console.WriteLine("📏 3 avaliações físicas (medidas + IMC) cadastradas: 18/05, 02/06 e 16/06.");

                    // ── Anamnese ──
                    var anamnese = new AnamneseAluno
                    {
                        AlunoId = aluno.Id,
                        TreinadorId = personal.Id,
                        DataAvaliacao = new DateTime(2026, 5, 3),
                        JaTreinouAntes = true,
                        TempoTreinando = "2 anos",
                        TempoSemAtividade = "3 meses",
                        Objetivo = "Hipertrofia e ganho de massa muscular",
                        FrequenciaSemanal = "6x por semana",
                        TempoPorDia = "60 minutos",
                        TemDoenca = false,
                        QualDoenca = null,
                        TemLimitacaoMovimento = false,
                        QualLimitacao = null,
                        TemDorMovimento = false,
                        QualDor = null,
                        FezCirurgia = false,
                        QualCirurgia = null,
                        UsaMedicamento = false,
                        QualMedicamento = null,
                        FazDieta = true,
                        TipoDieta = "Dieta hipercalórica com foco em proteína (acompanhamento nutricional)",
                        ConsomeAlcool = "Socialmente, finais de semana",
                        Fuma = false,
                        ObservacoesGerais = "Aluno motivado, sem restrições médicas. Retomando treinos após período de pausa de 3 meses.",
                        DataCriacao = new DateTime(2026, 5, 3)
                    };
                    db.Anamneses.Add(anamnese);
                    await db.SaveChangesAsync();
                    Console.WriteLine("📝 Anamnese inicial cadastrada para o aluno.");

                    Console.WriteLine($"✅ Aluno '{aluno.Email}' vinculado ao Personal '{personal.Email}' com 5 ciclos de treino completos!");
                }
                else
                {
                    Console.WriteLine($"ℹ️ Aluno '{aluno.Email}' já está vinculado ao Personal '{personal.Email}'.");
                }
            }
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Erro ao criar banco de dados ou usuários padrão.");
    }
}

// ── Pipeline ─────────────────────────────────────────────────
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();

// ── Método auxiliar para criar usuário ──────────────────────
static async Task<Usuario?> CriarUsuario(
    UserManager<Usuario> userManager,
    string email,
    string nome,
    string senha,
    string role)
{
    var existingUser = await userManager.FindByEmailAsync(email);
    if (existingUser != null)
    {
        Console.WriteLine($"ℹ️ Usuário '{email}' já existe.");
        return existingUser;
    }

    var user = new Usuario
    {
        UserName       = email,
        Email          = email,
        NomeCompleto   = nome,
        Ativo          = true,
        DataCriacao    = DateTime.Now,
        EmailConfirmed = true
    };

    var result = await userManager.CreateAsync(user, senha);
    if (result.Succeeded)
    {
        await userManager.AddToRoleAsync(user, role);
        Console.WriteLine($"✅ Usuário '{email}' criado com sucesso com role '{role}'.");
        return user;
    }

    Console.WriteLine($"❌ Erro ao criar usuário '{email}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
    return null;
}
