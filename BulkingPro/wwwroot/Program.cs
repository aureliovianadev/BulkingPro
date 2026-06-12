using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using BulkingPro.Data;
using BulkingPro.Models;
using System.Globalization;

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

// ── MVC + Razor Pages ────────────────────────────────────────
builder.Services.AddControllersWithViews(options =>
{
    // Mensagens de erro de model binding em português
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
    try
    {
        var db = services.GetRequiredService<ApplicationDbContext>();
        await db.Database.EnsureCreatedAsync();

        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<Usuario>>();

        // Roles
        string[] roles = { "Administrador", "Moderador", "Usuario" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        // Admin
        await CriarUsuario(userManager, "admin@bulkingpro.com",    "Administrador Master", "admin123",    "Administrador");
        // Personal
        await CriarUsuario(userManager, "personal@bulkingpro.com", "Carlos Personal",      "personal123", "Moderador");
        // Aluno
        await CriarUsuario(userManager, "aluno@bulkingpro.com",    "João Aluno",           "aluno123",    "Usuario");
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

// ── Cultura invariante: garante que input[type=number] (ponto decimal) ──────
// seja corretamente interpretado pelo model binder, independente do SO do servidor
var invariantCulture = CultureInfo.InvariantCulture;
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture(invariantCulture),
    SupportedCultures = new[] { invariantCulture },
    SupportedUICultures = new[] { invariantCulture }
});

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();

static async Task CriarUsuario(
    UserManager<Usuario> userManager,
    string email, string nome, string senha, string role)
{
    if (await userManager.FindByEmailAsync(email) != null) return;

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
        await userManager.AddToRoleAsync(user, role);
}
