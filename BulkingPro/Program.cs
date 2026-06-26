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

        // ── Aluno ──
        if (personal != null)
        {
            var aluno = await CriarUsuario(userManager, "aluno@bulkingpro.com", "João Aluno", "aluno123", "Usuario");

            if (aluno != null)
            {
                var planoExistente = await db.PlanosTreino
                    .AnyAsync(p => p.AlunoId == aluno.Id && p.TreinadorId == personal.Id);

                if (!planoExistente)
                {
                    // ═══════════════════════════════════════════════════════════════
                    // DEFINIÇÃO DOS PLANOS DE TREINO COM DATAS FIXAS
                    // ═══════════════════════════════════════════════════════════════

                    var hoje = DateTime.Today;

                    // ═══════════════════════════════════════════════════════════════
                    // DATAS FIXAS PARA CADA PLANO (começando no dia 01 de cada mês)
                    // ═══════════════════════════════════════════════════════════════

                    var planos = new[]
                    {
                        new { 
                            Id = 1, 
                            Titulo = "Ciclo 1 - Adaptação", 
                            Inicio = new DateTime(2026, 6, 1), 
                            Fim = new DateTime(2026, 6, 14) 
                        },
                        new { 
                            Id = 2, 
                            Titulo = "Ciclo 2 - Volume", 
                            Inicio = new DateTime(2026, 6, 15), 
                            Fim = new DateTime(2026, 6, 28) 
                        },
                        new { 
                            Id = 3, 
                            Titulo = "Ciclo 3 - Intensidade", 
                            Inicio = new DateTime(2026, 6, 29), 
                            Fim = new DateTime(2026, 7, 12) 
                        },
                        new { 
                            Id = 4, 
                            Titulo = "Ciclo 4 - Progressão", 
                            Inicio = new DateTime(2026, 7, 13), 
                            Fim = new DateTime(2026, 7, 26) 
                        },
                        new { 
                            Id = 5, 
                            Titulo = "Ciclo 5 - Pico", 
                            Inicio = new DateTime(2026, 7, 27), 
                            Fim = new DateTime(2026, 8, 9) 
                        }
                    };

                    // Mapeamento dos dias da semana (1 = Segunda, 2 = Terça, ..., 6 = Sábado)
                    var diasSemana = new[]
                    {
                        new { Nome = "Segunda-feira", Ordem = 1, Dia = DayOfWeek.Monday },
                        new { Nome = "Terça-feira",   Ordem = 2, Dia = DayOfWeek.Tuesday },
                        new { Nome = "Quarta-feira",  Ordem = 3, Dia = DayOfWeek.Wednesday },
                        new { Nome = "Quinta-feira",  Ordem = 4, Dia = DayOfWeek.Thursday },
                        new { Nome = "Sexta-feira",   Ordem = 5, Dia = DayOfWeek.Friday },
                        new { Nome = "Sábado",        Ordem = 6, Dia = DayOfWeek.Saturday }
                    };

                    // Exercícios Superiores (dias ímpares)
                    var exerciciosSuperiores = new (int Id, string Nome, int ExercicioId, int Series, string Repeticoes, decimal CargaBase)[]
                    {
                        (1, "Supino Reto com Barra", 1, 4, "12", 40m),
                        (2, "Supino com Halteres", 3, 4, "12", 30m),
                        (3, "Puxada Frente", 7, 4, "12", 50m),
                        (4, "Remada Unilateral", 9, 3, "15", 20m),
                        (5, "Rosca Direta com Barra", 13, 3, "15", 15m),
                        (6, "Tríceps Corda", 17, 3, "15", 20m),
                        (7, "Desenvolvimento com Barra", 21, 4, "12", 25m)
                    };

                    // Exercícios Inferiores (dias pares)
                    var exerciciosInferiores = new (int Id, string Nome, int ExercicioId, int Series, string Repeticoes, decimal CargaBase)[]
                    {
                        (1, "Agachamento Livre", 29, 4, "10", 60m),
                        (2, "Leg Press 45°", 30, 4, "12", 80m),
                        (3, "Cadeira Extensora", 31, 4, "15", 40m),
                        (4, "Mesa Flexora", 32, 4, "15", 35m),
                        (5, "Elevação Pélvica", 35, 3, "15", 50m),
                        (6, "Gêmeos em Pé", 38, 4, "20", 30m)
                    };

                    int planoIndex = 0;
                    foreach (var planoInfo in planos)
                    {
                        planoIndex++;
                        decimal incrementoCarga = (planoIndex - 1) * 2.5m;

                        Console.WriteLine($"📋 Plano {planoInfo.Id}: {planoInfo.Titulo} - {planoInfo.Inicio:dd/MM/yyyy} a {planoInfo.Fim:dd/MM/yyyy}");

                        int statusPlano = planoInfo.Fim >= hoje ? 1 : 2;

                        var plano = new PlanoTreino
                        {
                            TreinadorId = personal.Id,
                            AlunoId = aluno.Id,
                            Titulo = planoInfo.Titulo,
                            Objetivo = "Hipertrofia (ganho de massa muscular)",
                            DataInicio = planoInfo.Inicio,
                            DataFim = planoInfo.Fim,
                            Status = statusPlano,
                            DataCriacao = planoInfo.Inicio
                        };
                        db.PlanosTreino.Add(plano);
                        await db.SaveChangesAsync();

                        // ═══════════════════════════════════════════════════════════════
                        // CRIAR OS DIAS DE TREINO DENTRO DO PERÍODO DO PLANO
                        // ═══════════════════════════════════════════════════════════════

                        foreach (var diaInfo in diasSemana)
                        {
                            // ═══════════════════════════════════════════════════════════════
                            // ENCONTRAR A PRIMEIRA OCORRÊNCIA DESTE DIA DA SEMANA NO PLANO
                            // ═══════════════════════════════════════════════════════════════
                            
                            var dataDia = planoInfo.Inicio;
                            
                            // Avançar até encontrar o dia da semana correto
                            while (dataDia.DayOfWeek != diaInfo.Dia)
                            {
                                dataDia = dataDia.AddDays(1);
                            }

                            // Se a data calculada for anterior à data de início, avançar uma semana
                            if (dataDia < planoInfo.Inicio)
                            {
                                dataDia = dataDia.AddDays(7);
                            }

                            Console.WriteLine($"   📅 {diaInfo.Nome}: {dataDia:dd/MM/yyyy}");

                            var treino = new Treino
                            {
                                PlanoTreinoId = plano.Id,
                                Nome = diaInfo.Nome,
                                OrdemDia = diaInfo.Ordem,
                                Observacoes = diaInfo.Ordem % 2 == 0 ? "Foco em membros inferiores" : "Foco em membros superiores",
                                DataCriacao = planoInfo.Inicio
                            };
                            db.Treinos.Add(treino);
                            await db.SaveChangesAsync();

                            // Selecionar exercícios conforme o dia
                            var exerciciosDoDia = diaInfo.Ordem % 2 == 1 
                                ? exerciciosSuperiores 
                                : exerciciosInferiores;

                            var treinoExerciciosCriados = new List<TreinoExercicio>();
                            
                            foreach (var ex in exerciciosDoDia)
                            {
                                var te = new TreinoExercicio
                                {
                                    TreinoId = treino.Id,
                                    ExercicioId = ex.ExercicioId,
                                    Ordem = ex.Id,
                                    SeriesPlanejadas = ex.Series,
                                    RepeticoesPlanejadas = ex.Repeticoes,
                                    CargaPlanejada = ex.CargaBase + incrementoCarga,
                                    TempoDescanso = ex.Id % 2 == 0 ? 45 : 60,
                                    DataCriacao = planoInfo.Inicio
                                };
                                db.TreinoExercicios.Add(te);
                                treinoExerciciosCriados.Add(te);
                            }
                            await db.SaveChangesAsync();

                            // ═══════════════════════════════════════════════════════════════
                            // GERAR EXECUÇÕES PARA CADA SEMANA DO PLANO
                            // ═══════════════════════════════════════════════════════════════

                            var dataExecucao = dataDia;
                            while (dataExecucao <= planoInfo.Fim)
                            {
                                // ⭐⭐⭐ SÓ CRIA EXECUÇÃO SE A DATA FOR MENOR OU IGUAL A HOJE
                                if (dataExecucao <= hoje)
                                {
                                    var execucao = new ExecucaoTreino
                                    {
                                        TreinoId = treino.Id,
                                        AlunoId = aluno.Id,
                                        DataExecucao = dataExecucao,
                                        DuracaoMinutos = 55 + (planoIndex * 2),
                                        EsforcoPercebido = Math.Min(6 + planoIndex, 10),
                                        ObservacoesGerais = "Treino concluído conforme planejado.",
                                        Concluido = true,
                                        DataCriacao = dataExecucao
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

                                    Console.WriteLine($"      ✅ Execução criada para {dataExecucao:dd/MM/yyyy}");
                                }

                                // Avançar para a próxima semana (mesmo dia da semana)
                                dataExecucao = dataExecucao.AddDays(7);
                            }
                        }

                        Console.WriteLine($"✅ Plano '{plano.Titulo}' criado ({planoInfo.Inicio:dd/MM} a {planoInfo.Fim:dd/MM}) - status {(statusPlano == 1 ? "Ativo" : "Concluído")}");
                    }

                    // ── Horários de Atendimento ──
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

                    // ── Avaliações Físicas ──
                    var avaliacoes = new[]
                    {
                        new AvaliacaoFisica
                        {
                            AlunoId = aluno.Id,
                            TreinadorId = personal.Id,
                            DataAvaliacao = new DateTime(2026, 6, 15),
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
                            Observacoes = "Avaliação inicial do Ciclo 2.",
                            DataCriacao = new DateTime(2026, 6, 15)
                        },
                        new AvaliacaoFisica
                        {
                            AlunoId = aluno.Id,
                            TreinadorId = personal.Id,
                            DataAvaliacao = new DateTime(2026, 6, 29),
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
                            Observacoes = "Avaliação do Ciclo 3.",
                            DataCriacao = new DateTime(2026, 6, 29)
                        },
                        new AvaliacaoFisica
                        {
                            AlunoId = aluno.Id,
                            TreinadorId = personal.Id,
                            DataAvaliacao = new DateTime(2026, 7, 13),
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
                            Observacoes = "Avaliação do Ciclo 4.",
                            DataCriacao = new DateTime(2026, 7, 13)
                        }
                    };

                    db.AvaliacoesFisicas.AddRange(avaliacoes);
                    await db.SaveChangesAsync();

                    // ── Anamnese ──
                    var anamnese = new AnamneseAluno
                    {
                        AlunoId = aluno.Id,
                        TreinadorId = personal.Id,
                        DataAvaliacao = new DateTime(2026, 6, 1),
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
                        TipoDieta = "Dieta hipercalórica com foco em proteína",
                        ConsomeAlcool = "Socialmente, finais de semana",
                        Fuma = false,
                        ObservacoesGerais = "Aluno motivado, sem restrições médicas.",
                        DataCriacao = new DateTime(2026, 6, 1)
                    };
                    db.Anamneses.Add(anamnese);
                    await db.SaveChangesAsync();

                    Console.WriteLine($"✅ Aluno '{aluno.Email}' vinculado ao Personal '{personal.Email}' com 5 planos de treino!");
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