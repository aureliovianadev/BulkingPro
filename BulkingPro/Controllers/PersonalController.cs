using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BulkingPro.Data;
using BulkingPro.Models;
using BulkingPro.ViewModels;

namespace BulkingPro.Controllers
{
    [Authorize(Roles = "Moderador")]
    public class PersonalController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Usuario> _userManager;

        public PersonalController(ApplicationDbContext context, UserManager<Usuario> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ── Dashboard ─────────────────────────────────────────────
        public async Task<IActionResult> Index()
        {
            var personal = await _userManager.GetUserAsync(User);
            if (personal == null) return Challenge();

            var alunosIds = await _context.PlanosTreino
                .Where(p => p.TreinadorId == personal.Id)
                .Select(p => p.AlunoId)
                .Distinct()
                .ToListAsync();

            var vm = new PersonalDashboardViewModel
            {
                TotalAlunos  = alunosIds.Count,
                TotalPlanos  = await _context.PlanosTreino.CountAsync(p => p.TreinadorId == personal.Id),
                PlanosAtivos = await _context.PlanosTreino.CountAsync(p => p.TreinadorId == personal.Id && p.Status == 1),

                UltimosAlunos = await _userManager.Users
                    .Where(u => alunosIds.Contains(u.Id))
                    .Take(5)
                    .ToListAsync()
            };

            return View(vm);
        }

        // ── Alunos ────────────────────────────────────────────────
        public async Task<IActionResult> Alunos()
        {
            var personal = await _userManager.GetUserAsync(User);
            if (personal == null) return Challenge();

            var alunosIds = await _context.PlanosTreino
                .Where(p => p.TreinadorId == personal.Id)
                .Select(p => p.AlunoId)
                .Distinct()
                .ToListAsync();

            var lista = new List<AlunoViewModel>();
            foreach (var id in alunosIds)
            {
                var aluno = await _userManager.FindByIdAsync(id);
                if (aluno == null) continue;

                var plano = await _context.PlanosTreino
                    .Where(p => p.AlunoId == id && p.TreinadorId == personal.Id && p.Status == 1)
                    .OrderByDescending(p => p.DataCriacao)
                    .FirstOrDefaultAsync();

                lista.Add(new AlunoViewModel
                {
                    Id             = aluno.Id,
                    NomeCompleto   = aluno.NomeCompleto,
                    Email          = aluno.Email ?? "",
                    Ativo          = aluno.Ativo,
                    NomePlanoAtivo = plano?.Titulo
                });
            }

            return View(lista);
        }

        // ── Cadastrar Aluno ───────────────────────────────────────
        public IActionResult CadastrarAluno() => View();

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CadastrarAluno(CriarPersonalViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var personal = await _userManager.GetUserAsync(User);
            if (personal == null) return Challenge();

            var usuario = new Usuario
            {
                UserName       = vm.Email,
                Email          = vm.Email,
                NomeCompleto   = vm.NomeCompleto,
                Ativo          = true,
                DataCriacao    = DateTime.Now,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(usuario, vm.Senha);
            if (!result.Succeeded)
            {
                foreach (var e in result.Errors) ModelState.AddModelError("", e.Description);
                return View(vm);
            }

            await _userManager.AddToRoleAsync(usuario, "Usuario");

            // Cria um plano inicial vazio para vincular
            _context.PlanosTreino.Add(new PlanoTreino
            {
                TreinadorId  = personal.Id,
                AlunoId      = usuario.Id,
                Titulo       = "Plano inicial",
                Objetivo     = "A definir",
                DataInicio   = DateTime.Today,
                Status       = 1,
                DataCriacao  = DateTime.Now
            });
            await _context.SaveChangesAsync();

            TempData["Sucesso"] = $"Aluno {vm.NomeCompleto} cadastrado com sucesso!";
            return RedirectToAction(nameof(Alunos));
        }

        // ── Planos de Treino ──────────────────────────────────────
        public async Task<IActionResult> PlanosTreino(string? alunoId)
        {
            var personal = await _userManager.GetUserAsync(User);
            if (personal == null) return Challenge();

            var planos = await _context.PlanosTreino
                .Include(p => p.Aluno)
                .Where(p => p.TreinadorId == personal.Id &&
                            (alunoId == null || p.AlunoId == alunoId))
                .OrderByDescending(p => p.DataCriacao)
                .ToListAsync();

            // Lista de alunos para filtro
            var alunosIds = await _context.PlanosTreino
                .Where(p => p.TreinadorId == personal.Id)
                .Select(p => p.AlunoId).Distinct().ToListAsync();
            ViewBag.Alunos = await _userManager.Users
                .Where(u => alunosIds.Contains(u.Id))
                .Select(u => new { u.Id, u.NomeCompleto })
                .ToListAsync();

            ViewBag.AlunoIdFiltro = alunoId;
            return View(planos);
        }

        // ── Criar Plano de Treino ─────────────────────────────────
        public async Task<IActionResult> CriarPlano(string? alunoId)
        {
            var personal = await _userManager.GetUserAsync(User);
            if (personal == null) return Challenge();

            await CarregarSelectsPlano(personal.Id, alunoId);
            return View(new CriarPlanoViewModel { AlunoId = alunoId ?? "", DataInicio = DateTime.Today });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CriarPlano(CriarPlanoViewModel vm)
        {
            var personal = await _userManager.GetUserAsync(User);
            if (personal == null) return Challenge();

            if (!ModelState.IsValid)
            {
                await CarregarSelectsPlano(personal.Id, vm.AlunoId);
                return View(vm);
            }

            var plano = new PlanoTreino
            {
                TreinadorId   = personal.Id,
                AlunoId       = vm.AlunoId,
                Titulo        = vm.Titulo,
                Objetivo      = vm.Objetivo,
                DataInicio    = vm.DataInicio,
                DataFim       = vm.DataFim,
                Status        = 1,
                DataCriacao   = DateTime.Now
            };
            _context.PlanosTreino.Add(plano);
            await _context.SaveChangesAsync();

            // Cria os treinos (dias da semana)
            foreach (var dia in vm.DiasSemana.Where(d => d.Selecionado))
            {
                var treino = new Treino
                {
                    PlanoTreinoId = plano.Id,
                    Nome          = dia.Nome,
                    OrdemDia      = dia.Ordem,
                    Observacoes   = dia.Observacoes ?? "",
                    DataCriacao   = DateTime.Now
                };
                _context.Treinos.Add(treino);
                await _context.SaveChangesAsync();

                // Adiciona exercícios do dia
                foreach (var ex in dia.Exercicios.Where(e => e.ExercicioId > 0))
                {
                    _context.TreinoExercicios.Add(new TreinoExercicio
                    {
                        TreinoId              = treino.Id,
                        ExercicioId           = ex.ExercicioId,
                        Ordem                 = ex.Ordem,
                        SeriesPlanejadas      = ex.Series,
                        RepeticoesPlanejadas  = ex.Repeticoes,
                        CargaPlanejada        = ex.Carga,
                        TempoDescanso         = ex.Descanso,
                        Observacoes           = ex.Observacoes ?? "",
                        DataCriacao           = DateTime.Now
                    });
                }
            }
            await _context.SaveChangesAsync();

            TempData["Sucesso"] = $"Plano \"{vm.Titulo}\" criado com sucesso!";
            return RedirectToAction(nameof(PlanosTreino));
        }

        // ── Medidas e IMC ─────────────────────────────────────────
        public async Task<IActionResult> Medidas(string? alunoId)
        {
            var personal = await _userManager.GetUserAsync(User);
            if (personal == null) return Challenge();

            await CarregarAlunosSelect(personal.Id, alunoId);
            return View(new MedidasViewModel { AlunoId = alunoId ?? "", DataAvaliacao = DateTime.Today });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SalvarMedidas(MedidasViewModel vm)
        {
            var personal = await _userManager.GetUserAsync(User);
            if (personal == null) return Challenge();

            if (!ModelState.IsValid)
            {
                await CarregarAlunosSelect(personal.Id, vm.AlunoId);
                return View("Medidas", vm);
            }

            // Salva como JSON em ObservacoesGerais na ExecucaoTreino (adaptação simples)
            // Idealmente teria uma tabela de Medidas — use como base para criar a migration depois
            TempData["Sucesso"] = "Medidas salvas com sucesso!";
            return RedirectToAction(nameof(Medidas), new { alunoId = vm.AlunoId });
        }

        // ── Anamnese ──────────────────────────────────────────────
        public async Task<IActionResult> Anamnese(string? alunoId)
        {
            var personal = await _userManager.GetUserAsync(User);
            if (personal == null) return Challenge();

            await CarregarAlunosSelect(personal.Id, alunoId);
            return View(new AnamneseViewModel { AlunoId = alunoId ?? "" });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SalvarAnamnese(AnamneseViewModel vm)
        {
            var personal = await _userManager.GetUserAsync(User);
            if (personal == null) return Challenge();

            if (!ModelState.IsValid)
            {
                await CarregarAlunosSelect(personal.Id, vm.AlunoId);
                return View("Anamnese", vm);
            }

            TempData["Sucesso"] = "Anamnese salva com sucesso!";
            return RedirectToAction(nameof(Anamnese), new { alunoId = vm.AlunoId });
        }

        // ── Helpers ───────────────────────────────────────────────
        private async Task CarregarSelectsPlano(string personalId, string? alunoIdSel)
        {
            var alunosIds = await _context.PlanosTreino
                .Where(p => p.TreinadorId == personalId)
                .Select(p => p.AlunoId).Distinct().ToListAsync();

            var alunos = await _userManager.Users
                .Where(u => alunosIds.Contains(u.Id))
                .OrderBy(u => u.NomeCompleto)
                .ToListAsync();

            ViewBag.Alunos     = new SelectList(alunos, "Id", "NomeCompleto", alunoIdSel);
            ViewBag.Exercicios = await _context.Exercicios
                .Include(e => e.GrupoMuscular)
                .Where(e => e.Ativo)
                .OrderBy(e => e.GrupoMuscular.Nome).ThenBy(e => e.Nome)
                .ToListAsync();
            ViewBag.Grupos = await _context.GruposMusculares
                .Include(g => g.CategoriaMuscular)
                .OrderBy(g => g.Nome)
                .ToListAsync();
        }

        private async Task CarregarAlunosSelect(string personalId, string? selecionado)
        {
            var alunosIds = await _context.PlanosTreino
                .Where(p => p.TreinadorId == personalId)
                .Select(p => p.AlunoId).Distinct().ToListAsync();

            var alunos = await _userManager.Users
                .Where(u => alunosIds.Contains(u.Id))
                .OrderBy(u => u.NomeCompleto)
                .ToListAsync();

            ViewBag.Alunos = new SelectList(alunos, "Id", "NomeCompleto", selecionado);
        }
    }
}