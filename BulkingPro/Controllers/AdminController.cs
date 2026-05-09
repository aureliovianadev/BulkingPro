using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BulkingPro.Data;
using BulkingPro.Models;
using BulkingPro.ViewModels;

namespace BulkingPro.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Usuario> _userManager;

        public AdminController(ApplicationDbContext context, UserManager<Usuario> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Admin/Dashboard
        public async Task<IActionResult> Index()
        {
            var personais = await _userManager.GetUsersInRoleAsync("Moderador");
            var alunos    = await _userManager.GetUsersInRoleAsync("Usuario");

            var vm = new AdminDashboardViewModel
            {
                TotalPersonais  = personais.Count,
                TotalAlunos     = alunos.Count,
                TotalPlanos     = await _context.PlanosTreino.CountAsync(),
                TotalExercicios = await _context.Exercicios.CountAsync(e => e.Ativo),

                UltimosPlanos = await _context.PlanosTreino
                    .Include(p => p.Aluno)
                    .Include(p => p.Treinador)
                    .OrderByDescending(p => p.DataCriacao)
                    .Take(5)
                    .ToListAsync(),

                PersonaisComAlunos = await _context.PlanosTreino
                    .Include(p => p.Treinador)
                    .GroupBy(p => new { p.TreinadorId, p.Treinador.NomeCompleto })
                    .Select(g => new PersonalResumo
                    {
                        TreinadorId   = g.Key.TreinadorId,
                        NomeCompleto  = g.Key.NomeCompleto,
                        TotalAlunos   = g.Select(p => p.AlunoId).Distinct().Count()
                    })
                    .ToListAsync()
            };

            return View(vm);
        }

        // GET: Admin/Personais
        public async Task<IActionResult> Personais()
        {
            var personais = await _userManager.GetUsersInRoleAsync("Moderador");

            var lista = new List<PersonalViewModel>();
            foreach (var p in personais)
            {
                var qtdAlunos = await _context.PlanosTreino
                    .Where(pt => pt.TreinadorId == p.Id)
                    .Select(pt => pt.AlunoId)
                    .Distinct()
                    .CountAsync();

                lista.Add(new PersonalViewModel
                {
                    Id           = p.Id,
                    NomeCompleto = p.NomeCompleto,
                    Email        = p.Email ?? "",
                    Ativo        = p.Ativo,
                    TotalAlunos  = qtdAlunos
                });
            }

            return View(lista);
        }

        // GET: Admin/CriarPersonal
        public IActionResult CriarPersonal() => View();

        // POST: Admin/CriarPersonal
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CriarPersonal(CriarPersonalViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var usuario = new Usuario
            {
                UserName      = vm.Email,
                Email         = vm.Email,
                NomeCompleto  = vm.NomeCompleto,
                Ativo         = true,
                DataCriacao   = DateTime.Now,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(usuario, vm.Senha);
            if (!result.Succeeded)
            {
                foreach (var e in result.Errors)
                    ModelState.AddModelError("", e.Description);
                return View(vm);
            }

            await _userManager.AddToRoleAsync(usuario, "Moderador");
            TempData["Sucesso"] = $"Personal {vm.NomeCompleto} criado com sucesso!";
            return RedirectToAction(nameof(Personais));
        }

        // POST: Admin/TogglePersonal
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TogglePersonal(string id)
        {
            var usuario = await _userManager.FindByIdAsync(id);
            if (usuario == null) return NotFound();

            usuario.Ativo = !usuario.Ativo;
            await _userManager.UpdateAsync(usuario);
            return RedirectToAction(nameof(Personais));
        }

        // GET: Admin/Alunos
        public async Task<IActionResult> Alunos()
        {
            var alunos = await _userManager.GetUsersInRoleAsync("Usuario");

            var lista = new List<AlunoViewModel>();
            foreach (var a in alunos)
            {
                var plano = await _context.PlanosTreino
                    .Include(p => p.Treinador)
                    .Where(p => p.AlunoId == a.Id && p.Status == 1)
                    .OrderByDescending(p => p.DataCriacao)
                    .FirstOrDefaultAsync();

                lista.Add(new AlunoViewModel
                {
                    Id              = a.Id,
                    NomeCompleto    = a.NomeCompleto,
                    Email           = a.Email ?? "",
                    Ativo           = a.Ativo,
                    NomePlanoAtivo  = plano?.Titulo,
                    NomePersonal    = plano?.Treinador?.NomeCompleto
                });
            }

            return View(lista);
        }

        // GET: Admin/Exercicios
        public async Task<IActionResult> Exercicios()
        {
            var exercicios = await _context.Exercicios
                .Include(e => e.GrupoMuscular)
                    .ThenInclude(g => g.CategoriaMuscular)
                .OrderBy(e => e.GrupoMuscular.Nome)
                .ThenBy(e => e.Nome)
                .ToListAsync();

            return View(exercicios);
        }

        // GET: Admin/Planos
        public async Task<IActionResult> Planos()
        {
            var planos = await _context.PlanosTreino
                .Include(p => p.Aluno)
                .Include(p => p.Treinador)
                .OrderByDescending(p => p.DataCriacao)
                .ToListAsync();

            return View(planos);
        }

        // GET: Admin/Permissoes
        public IActionResult Permissoes() => View();
    }
}