using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using BulkingPro.Data;
using BulkingPro.Models;

var builder = WebApplication.CreateBuilder(args);

// ── Banco de dados ──────────────────────────────────────────
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySQL(builder.Configuration.GetConnectionString("DefaultConnection")));

// ── Identity ────────────────────────────────────────────────
builder.Services.AddIdentity<Usuario, IdentityRole>(options =>
{
    // Senha
    options.Password.RequireDigit           = false;
    options.Password.RequireLowercase       = false;
    options.Password.RequireUppercase       = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength         = 6;

    // Login
    options.SignIn.RequireConfirmedAccount = false;
    options.SignIn.RequireConfirmedEmail = false;
    
    // Usuário
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders()
.AddRoles<IdentityRole>();

// ── Redirecionar para login customizado ─────────────────────
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath        = "/Account/Login";
    options.AccessDeniedPath = "/Account/Login";
    options.LogoutPath       = "/Account/Logout";
    options.ExpireTimeSpan   = TimeSpan.FromDays(7);
    options.SlidingExpiration = true;
});

// ── MVC + Razor Pages ───────────────────────────────────────
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();

// ── Seed / Garante banco e cria usuários padrão ─────────────
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var dbContext = services.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.EnsureCreatedAsync();
        
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<Usuario>>();
        
        // Criar roles
        string[] roles = { "Administrador", "Moderador", "Usuario" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }
        
        // Criar Admin
        var adminEmail = "admin@bulkingpro.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new Usuario
            {
                UserName = adminEmail,
                Email = adminEmail,
                NomeCompleto = "Administrador Master",
                Ativo = true,
                DataCriacao = DateTime.Now,
                EmailConfirmed = true
            };
            var result = await userManager.CreateAsync(adminUser, "admin123");
            if (result.Succeeded)
                await userManager.AddToRoleAsync(adminUser, "Administrador");
        }
        
        // Criar Personal
        var personalEmail = "personal@bulkingpro.com";
        var personalUser = await userManager.FindByEmailAsync(personalEmail);
        if (personalUser == null)
        {
            personalUser = new Usuario
            {
                UserName = personalEmail,
                Email = personalEmail,
                NomeCompleto = "Carlos Personal",
                Ativo = true,
                DataCriacao = DateTime.Now,
                EmailConfirmed = true
            };
            var result = await userManager.CreateAsync(personalUser, "personal123");
            if (result.Succeeded)
                await userManager.AddToRoleAsync(personalUser, "Moderador");
        }
        
        // Criar Aluno
        var alunoEmail = "aluno@bulkingpro.com";
        var alunoUser = await userManager.FindByEmailAsync(alunoEmail);
        if (alunoUser == null)
        {
            alunoUser = new Usuario
            {
                UserName = alunoEmail,
                Email = alunoEmail,
                NomeCompleto = "João Aluno",
                Ativo = true,
                DataCriacao = DateTime.Now,
                EmailConfirmed = true
            };
            var result = await userManager.CreateAsync(alunoUser, "aluno123");
            if (result.Succeeded)
                await userManager.AddToRoleAsync(alunoUser, "Usuario");
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Erro ao criar o banco de dados ou usuários padrão");
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

// ── Rotas ───────────────────────────────────────────────────
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();