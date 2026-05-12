using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BulkingPro.Data;
using BulkingPro.Models;

namespace BulkingPro.Controllers
{
    [Authorize(Roles = "Administrador,Moderador")]
    public class ExerciciosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ExerciciosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Exercicios
        public async Task<IActionResult> Index()
        {
            var exercicios = await _context.Exercicios
                .Include(e => e.GrupoMuscular)
                    .ThenInclude(g => g.CategoriaMuscular)
                .OrderBy(e => e.GrupoMuscular.Nome)
                .ThenBy(e => e.Nome)
                .ToListAsync();

            return View(exercicios);
        }

        // GET: Exercicios/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var exercicio = await _context.Exercicios
                .Include(e => e.GrupoMuscular)
                    .ThenInclude(g => g.CategoriaMuscular)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (exercicio == null) return NotFound();

            return View(exercicio);
        }

        // GET: Exercicios/Create
        public IActionResult Create()
        {
            CarregarGruposMusculares();
            return View();
        }

        // POST: Exercicios/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Nome,Descricao,GrupoMuscularId,InstrucoesExecucao,Ativo,DataCriacao,DataAtualizacao")] Exercicio exercicio)
        {
            if (ModelState.IsValid)
            {
                exercicio.DataCriacao = DateTime.Now;
                _context.Add(exercicio);
                await _context.SaveChangesAsync();

                TempData["Sucesso"] = $"Exercício \"{exercicio.Nome}\" criado com sucesso!";
                return RedirectToAction(nameof(Index));
            }

            CarregarGruposMusculares(exercicio.GrupoMuscularId);
            return View(exercicio);
        }

        // GET: Exercicios/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var exercicio = await _context.Exercicios.FindAsync(id);
            if (exercicio == null) return NotFound();

            CarregarGruposMusculares(exercicio.GrupoMuscularId);
            return View(exercicio);
        }

        // POST: Exercicios/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nome,Descricao,GrupoMuscularId,InstrucoesExecucao,Ativo,DataCriacao,DataAtualizacao")] Exercicio exercicio)
        {
            if (id != exercicio.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    exercicio.DataAtualizacao = DateTime.Now;
                    _context.Update(exercicio);
                    await _context.SaveChangesAsync();

                    TempData["Sucesso"] = $"Exercício \"{exercicio.Nome}\" atualizado!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ExercicioExists(exercicio.Id)) return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }

            CarregarGruposMusculares(exercicio.GrupoMuscularId);
            return View(exercicio);
        }

        // GET: Exercicios/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var exercicio = await _context.Exercicios
                .Include(e => e.GrupoMuscular)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (exercicio == null) return NotFound();

            return View(exercicio);
        }

        // POST: Exercicios/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var exercicio = await _context.Exercicios.FindAsync(id);
            if (exercicio != null)
            {
                _context.Exercicios.Remove(exercicio);
                await _context.SaveChangesAsync();
                TempData["Sucesso"] = $"Exercício \"{exercicio.Nome}\" excluído.";
            }

            return RedirectToAction(nameof(Index));
        }

        // ── Helpers ──────────────────────────────────────────────────
        private bool ExercicioExists(int id) =>
            _context.Exercicios.Any(e => e.Id == id);

        private void CarregarGruposMusculares(int? selecionado = null)
        {
            ViewBag.GrupoMuscularId = new SelectList(
                _context.GruposMusculares.OrderBy(g => g.Nome),
                "Id",
                "Nome",       // ← mostra o nome, não o Id
                selecionado
            );
        }
    }
}
