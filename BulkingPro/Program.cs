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
                    // ── 1. Cria o Plano de Treino ──
                    var plano = new PlanoTreino
                    {
                        TreinadorId = personal.Id,
                        AlunoId = aluno.Id,
                        Titulo = "Plano Inicial - Hipertrofia",
                        Objetivo = "Hipertrofia (ganho de massa muscular)",
                        DataInicio = DateTime.Today,
                        Status = 1,
                        DataCriacao = DateTime.Now
                    };
                    db.PlanosTreino.Add(plano);
                    await db.SaveChangesAsync();

                    // ── 2. Cria os Treinos para cada dia da semana ──
                    var diasSemana = new[]
                    {
                        new { Nome = "Segunda-feira", Ordem = 1 },
                        new { Nome = "Terça-feira", Ordem = 2 },
                        new { Nome = "Quarta-feira", Ordem = 3 },
                        new { Nome = "Quinta-feira", Ordem = 4 },
                        new { Nome = "Sexta-feira", Ordem = 5 },
                        new { Nome = "Sábado", Ordem = 6 }
                    };

                    foreach (var diaInfo in diasSemana)
                    {
                        var treino = new Treino
                        {
                            PlanoTreinoId = plano.Id,
                            Nome = diaInfo.Nome,
                            OrdemDia = diaInfo.Ordem,
                            Observacoes = diaInfo.Ordem % 2 == 0 ? "Foco em membros inferiores" : "Foco em membros superiores",
                            DataCriacao = DateTime.Now
                        };
                        db.Treinos.Add(treino);
                        await db.SaveChangesAsync();

                        // ── 3. Adiciona exercícios para cada treino ──
                        // Usando lista de tuplas com tipos explícitos
                        var exerciciosDoDia = new List<(int ExercicioId, int Ordem, int Series, string Repeticoes, decimal? Carga, int? Descanso)>();

                        if (diaInfo.Ordem % 2 == 1) // Dias ímpares: Superiores
                        {
                            exerciciosDoDia.AddRange(new (int, int, int, string, decimal?, int?)[]
                            {
                                (1, 1, 4, "12", 40.0m, 60),
                                (3, 2, 4, "12", 30.0m, 60),
                                (7, 3, 4, "12", 50.0m, 60),
                                (9, 4, 3, "15", 20.0m, 45),
                                (13, 5, 3, "15", 15.0m, 45),
                                (17, 6, 3, "15", 20.0m, 45),
                                (21, 7, 4, "12", 25.0m, 60)
                            });
                        }
                        else // Dias pares: Inferiores
                        {
                            exerciciosDoDia.AddRange(new (int, int, int, string, decimal?, int?)[]
                            {
                                (29, 1, 4, "10", 60.0m, 90),
                                (30, 2, 4, "12", 80.0m, 60),
                                (31, 3, 4, "15", 40.0m, 45),
                                (32, 4, 4, "15", 35.0m, 45),
                                (35, 5, 3, "15", 50.0m, 60),
                                (38, 6, 4, "20", 30.0m, 30)
                            });
                        }

                        foreach (var ex in exerciciosDoDia)
                        {
                            db.TreinoExercicios.Add(new TreinoExercicio
                            {
                                TreinoId = treino.Id,
                                ExercicioId = ex.ExercicioId,
                                Ordem = ex.Ordem,
                                SeriesPlanejadas = ex.Series,
                                RepeticoesPlanejadas = ex.Repeticoes,
                                CargaPlanejada = ex.Carga,
                                TempoDescanso = ex.Descanso,
                                DataCriacao = DateTime.Now
                            });
                        }
                        await db.SaveChangesAsync();
                    }

                    // ── 4. Cria os Horários de Atendimento ──
                    var horariosAtendimento = new[]
                    {
                        new { Dia = DayOfWeek.Monday, HoraInicio = new TimeSpan(8, 0, 0), HoraFim = new TimeSpan(9, 0, 0) },
                        new { Dia = DayOfWeek.Wednesday, HoraInicio = new TimeSpan(10, 0, 0), HoraFim = new TimeSpan(11, 0, 0) },
                        new { Dia = DayOfWeek.Friday, HoraInicio = new TimeSpan(14, 0, 0), HoraFim = new TimeSpan(15, 0, 0) }
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

                    Console.WriteLine($"✅ Aluno '{aluno.Email}' vinculado ao Personal '{personal.Email}' com plano de treino completo!");
                    Console.WriteLine($"📋 Total de treinos criados: 6 (Segunda a Sábado)");
                    Console.WriteLine($"📋 Total de exercícios criados: 39");
                    Console.WriteLine($"📋 Horários de atendimento: Segunda 08:00, Quarta 10:00, Sexta 14:00");
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
