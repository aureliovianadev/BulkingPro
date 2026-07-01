#nullable enable
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
                TotalAlunos = alunosIds.Count,
                TotalPlanos = await _context.PlanosTreino.CountAsync(p => p.TreinadorId == personal.Id),
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
                    Id = aluno.Id,
                    NomeCompleto = aluno.NomeCompleto,
                    Email = aluno.Email ?? "",
                    Ativo = aluno.Ativo,
                    NomePlanoAtivo = plano?.Titulo
                });
            }

            return View(lista);
        }

        // ── Verificar Conflito de Horário (AJAX) ───────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerificarConflitoHorario([FromBody] List<HorarioFrontend> horarios)
        {
            var personal = await _userManager.GetUserAsync(User);
            if (personal == null) return Json(new { temConflito = false });

            foreach (var h in horarios)
            {
                if (string.IsNullOrEmpty(h.HoraInicio) || string.IsNullOrEmpty(h.HoraFim)) continue;

                if (!TimeSpan.TryParse(h.HoraInicio, out var horaInicio)) continue;
                if (!TimeSpan.TryParse(h.HoraFim, out var horaFim)) continue;

                var diasSelecionados = new List<DayOfWeek>();
                if (h.Domingo) diasSelecionados.Add(DayOfWeek.Sunday);
                if (h.Segunda) diasSelecionados.Add(DayOfWeek.Monday);
                if (h.Terca) diasSelecionados.Add(DayOfWeek.Tuesday);
                if (h.Quarta) diasSelecionados.Add(DayOfWeek.Wednesday);
                if (h.Quinta) diasSelecionados.Add(DayOfWeek.Thursday);
                if (h.Sexta) diasSelecionados.Add(DayOfWeek.Friday);
                if (h.Sabado) diasSelecionados.Add(DayOfWeek.Saturday);

                foreach (var dia in diasSelecionados)
                {
                    var conflito = await _context.AlunosHorariosAtendimento
                        .AnyAsync(a => a.PersonalId == personal.Id 
                            && a.DiaSemana == dia
                            && a.Ativo == true
                            && ((a.HoraInicio <= horaInicio && a.HoraFim > horaInicio)
                                || (a.HoraInicio < horaFim && a.HoraFim >= horaFim)
                                || (a.HoraInicio >= horaInicio && a.HoraFim <= horaFim)));

                    if (conflito) return Json(new { temConflito = true });
                }
            }

            return Json(new { temConflito = false });
        }

        // ── Verifica se existe conflito (método auxiliar) ───────────
        private async Task<bool> ExisteConflitoHorario(string personalId, string alunoIdIgnorar, List<HorarioComDiasViewModel> horarios)
        {
            foreach (var h in horarios)
            {
                if (!h.HoraInicio.HasValue || !h.HoraFim.HasValue) continue;
                
                var diasSelecionados = h.DiasSelecionados();
                
                foreach (var dia in diasSelecionados)
                {
                    var conflito = await _context.AlunosHorariosAtendimento
                        .AnyAsync(a => a.PersonalId == personalId 
                            && a.AlunoId != alunoIdIgnorar
                            && a.DiaSemana == dia
                            && a.Ativo == true
                            && ((a.HoraInicio <= h.HoraInicio.Value && a.HoraFim > h.HoraInicio.Value)
                                || (a.HoraInicio < h.HoraFim.Value && a.HoraFim >= h.HoraFim.Value)
                                || (a.HoraInicio >= h.HoraInicio.Value && a.HoraFim <= h.HoraFim.Value)));
                    
                    if (conflito) return true;
                }
            }
            return false;
        }

        // ── Cadastrar Aluno ───────────────────────────────────────
        public IActionResult CadastrarAluno() => View();

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CadastrarAluno(CriarPersonalViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var personal = await _userManager.GetUserAsync(User);
            if (personal == null) return Challenge();

            if (vm.HorariosAtendimento != null && vm.HorariosAtendimento.Any())
            {
                var temConflito = await ExisteConflitoHorario(personal.Id, "", vm.HorariosAtendimento);
                if (temConflito)
                {
                    ModelState.AddModelError("", "❌ Conflito de horário! Já existe um aluno agendado neste mesmo dia e horário.");
                    return View(vm);
                }
            }

            var usuario = new Usuario
            {
                UserName = vm.Email,
                Email = vm.Email,
                NomeCompleto = vm.NomeCompleto,
                Cpf = vm.Cpf,
                Telefone = vm.Telefone,
                Ativo = true,
                DataCriacao = DateTime.Now,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(usuario, vm.Senha);
            if (!result.Succeeded)
            {
                foreach (var e in result.Errors) ModelState.AddModelError("", e.Description);
                return View(vm);
            }

            await _userManager.AddToRoleAsync(usuario, "Usuario");

            _context.PlanosTreino.Add(new PlanoTreino
            {
                TreinadorId = personal.Id,
                AlunoId = usuario.Id,
                Titulo = "Plano inicial",
                Objetivo = "A definir",
                DataInicio = DateTime.Today,
                Status = 1,
                DataCriacao = DateTime.Now
            });

            if (vm.HorariosAtendimento != null && vm.HorariosAtendimento.Any())
            {
                foreach (var h in vm.HorariosAtendimento)
                {
                    if (!h.HoraInicio.HasValue || !h.HoraFim.HasValue) continue;
                    
                    var diasSelecionados = h.DiasSelecionados();
                    
                    foreach (var dia in diasSelecionados)
                    {
                        _context.AlunosHorariosAtendimento.Add(new AlunoHorarioAtendimento
                        {
                            PersonalId = personal.Id,
                            AlunoId = usuario.Id,
                            DiaSemana = dia,
                            HoraInicio = h.HoraInicio.Value,
                            HoraFim = h.HoraFim.Value,
                            Ativo = true,
                            DataCriacao = DateTime.Now
                        });
                    }
                }
            }

            await _context.SaveChangesAsync();

            TempData["Sucesso"] = $"Aluno {vm.NomeCompleto} cadastrado com sucesso!";
            return RedirectToAction(nameof(Alunos));
        }

        // ── Editar Aluno ─────────────────────────────────────────────
        public async Task<IActionResult> EditarAluno(string id)
        {
            var personal = await _userManager.GetUserAsync(User);
            if (personal == null) return Challenge();

            var alunoVinculado = await _context.PlanosTreino
                .AnyAsync(p => p.TreinadorId == personal.Id && p.AlunoId == id);
            
            if (!alunoVinculado) return Forbid();

            var aluno = await _userManager.FindByIdAsync(id);
            if (aluno == null) return NotFound();

            var horarios = await _context.AlunosHorariosAtendimento
                .Where(h => h.PersonalId == personal.Id && h.AlunoId == id && h.Ativo)
                .ToListAsync();

            var horariosAgrupados = horarios
                .GroupBy(h => new { h.HoraInicio, h.HoraFim })
                .Select(g => new HorarioComDiasEditViewModel
                {
                    HoraInicio = g.Key.HoraInicio,
                    HoraFim = g.Key.HoraFim,
                    Domingo = g.Any(h => h.DiaSemana == DayOfWeek.Sunday),
                    Segunda = g.Any(h => h.DiaSemana == DayOfWeek.Monday),
                    Terca = g.Any(h => h.DiaSemana == DayOfWeek.Tuesday),
                    Quarta = g.Any(h => h.DiaSemana == DayOfWeek.Wednesday),
                    Quinta = g.Any(h => h.DiaSemana == DayOfWeek.Thursday),
                    Sexta = g.Any(h => h.DiaSemana == DayOfWeek.Friday),
                    Sabado = g.Any(h => h.DiaSemana == DayOfWeek.Saturday)
                })
                .ToList();

            var vm = new EditarAlunoViewModel
            {
                Id = aluno.Id,
                NomeCompleto = aluno.NomeCompleto,
                Email = aluno.Email ?? "",
                Cpf = aluno.Cpf,
                Telefone = aluno.Telefone,
                Ativo = aluno.Ativo,
                HorariosAtendimento = horariosAgrupados
            };

            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarAluno(EditarAlunoViewModel vm)
        {
            var personal = await _userManager.GetUserAsync(User);
            if (personal == null) return Challenge();

            if (!ModelState.IsValid) return View(vm);

            var aluno = await _userManager.FindByIdAsync(vm.Id);
            if (aluno == null) return NotFound();

            if (vm.HorariosAtendimento != null && vm.HorariosAtendimento.Any())
            {
                var horariosParaValidar = vm.HorariosAtendimento.Select(h => new HorarioComDiasViewModel
                {
                    HoraInicio = h.HoraInicio,
                    HoraFim = h.HoraFim,
                    Domingo = h.Domingo,
                    Segunda = h.Segunda,
                    Terca = h.Terca,
                    Quarta = h.Quarta,
                    Quinta = h.Quinta,
                    Sexta = h.Sexta,
                    Sabado = h.Sabado
                }).ToList();

                var temConflito = await ExisteConflitoHorario(personal.Id, vm.Id, horariosParaValidar);
                if (temConflito)
                {
                    ModelState.AddModelError("", "❌ Conflito de horário! Já existe outro aluno agendado neste mesmo dia e horário.");
                    return View(vm);
                }
            }

            aluno.NomeCompleto = vm.NomeCompleto;
            aluno.Email = vm.Email;
            aluno.UserName = vm.Email;
            aluno.NormalizedEmail = vm.Email.ToUpper();
            aluno.NormalizedUserName = vm.Email.ToUpper();
            aluno.Cpf = vm.Cpf;
            aluno.Telefone = vm.Telefone;
            aluno.Ativo = vm.Ativo;
            aluno.DataAtualizacao = DateTime.Now;

            var result = await _userManager.UpdateAsync(aluno);
            if (!result.Succeeded)
            {
                foreach (var e in result.Errors)
                    ModelState.AddModelError("", e.Description);
                return View(vm);
            }

            var horariosAntigos = await _context.AlunosHorariosAtendimento
                .Where(h => h.PersonalId == personal.Id && h.AlunoId == aluno.Id)
                .ToListAsync();
            _context.AlunosHorariosAtendimento.RemoveRange(horariosAntigos);

            foreach (var h in vm.HorariosAtendimento.Where(h => h.HoraInicio.HasValue && h.HoraFim.HasValue))
            {
                var diasSelecionados = h.DiasSelecionados();
                foreach (var dia in diasSelecionados)
                {
                    _context.AlunosHorariosAtendimento.Add(new AlunoHorarioAtendimento
                    {
                        PersonalId = personal.Id,
                        AlunoId = aluno.Id,
                        DiaSemana = dia,
                        HoraInicio = h.HoraInicio.Value,
                        HoraFim = h.HoraFim.Value,
                        Ativo = true,
                        DataCriacao = DateTime.Now
                    });
                }
            }

            await _context.SaveChangesAsync();

            TempData["Sucesso"] = $"Aluno {vm.NomeCompleto} atualizado com sucesso!";
            return RedirectToAction(nameof(Alunos));
        }

        // ── Inativar Aluno ────────────────────────────────────────
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> InativarAluno(string id)
        {
            var personal = await _userManager.GetUserAsync(User);
            if (personal == null) return Challenge();

            var alunoVinculado = await _context.PlanosTreino
                .AnyAsync(p => p.TreinadorId == personal.Id && p.AlunoId == id);
            
            if (!alunoVinculado) return Forbid();

            var aluno = await _userManager.FindByIdAsync(id);
            if (aluno == null) return NotFound();

            aluno.Ativo = false;
            aluno.DataAtualizacao = DateTime.Now;
            await _userManager.UpdateAsync(aluno);

            TempData["Sucesso"] = $"Aluno {aluno.NomeCompleto} foi inativado.";
            return RedirectToAction(nameof(Alunos));
        }

        // ── Reativar Aluno ────────────────────────────────────────
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ReativarAluno(string id)
        {
            var personal = await _userManager.GetUserAsync(User);
            if (personal == null) return Challenge();

            var alunoVinculado = await _context.PlanosTreino
                .AnyAsync(p => p.TreinadorId == personal.Id && p.AlunoId == id);
            
            if (!alunoVinculado) return Forbid();

            var aluno = await _userManager.FindByIdAsync(id);
            if (aluno == null) return NotFound();

            aluno.Ativo = true;
            aluno.DataAtualizacao = DateTime.Now;
            await _userManager.UpdateAsync(aluno);

            TempData["Sucesso"] = $"Aluno {aluno.NomeCompleto} foi reativado.";
            return RedirectToAction(nameof(Alunos));
        }

        // ── Desvincular Aluno ─────────────────────────────────────
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DesvincularAluno(string id)
        {
            var personal = await _userManager.GetUserAsync(User);
            if (personal == null) return Challenge();

            var planos = await _context.PlanosTreino
                .Where(p => p.TreinadorId == personal.Id && p.AlunoId == id)
                .ToListAsync();

            if (!planos.Any()) return NotFound();

            var aluno = await _userManager.FindByIdAsync(id);
            var alunoNome = aluno?.NomeCompleto ?? "Aluno";

            _context.PlanosTreino.RemoveRange(planos);
            await _context.SaveChangesAsync();

            TempData["Sucesso"] = $"Aluno {alunoNome} foi desvinculado de você. Ele ainda existe no sistema, mas não está mais sob sua orientação.";
            return RedirectToAction(nameof(Alunos));
        }

        // ── Excluir Aluno (exclusão definitiva) ─────────────────────
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ExcluirAluno(string id)
        {
            var personal = await _userManager.GetUserAsync(User);
            if (personal == null) return Challenge();

            var alunoVinculado = await _context.PlanosTreino
                .AnyAsync(p => p.TreinadorId == personal.Id && p.AlunoId == id);

            if (!alunoVinculado) return Forbid();

            var aluno = await _userManager.FindByIdAsync(id);
            if (aluno == null) return NotFound();

            var alunoNome = aluno.NomeCompleto;

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var planosIds = await _context.PlanosTreino
                    .Where(p => p.AlunoId == id)
                    .Select(p => p.Id)
                    .ToListAsync();

                var treinosIds = await _context.Treinos
                    .Where(t => planosIds.Contains(t.PlanoTreinoId))
                    .Select(t => t.Id)
                    .ToListAsync();

                var treinoExerciciosIds = await _context.TreinoExercicios
                    .Where(te => treinosIds.Contains(te.TreinoId))
                    .Select(te => te.Id)
                    .ToListAsync();

                var execucoesTreinoIds = await _context.ExecucoesTreino
                    .Where(et => et.AlunoId == id || treinosIds.Contains(et.TreinoId))
                    .Select(et => et.Id)
                    .ToListAsync();

                // Comentários vinculados aos exercícios do treino do aluno
                var comentarios = await _context.ComentariosTreino
                    .Where(c => treinoExerciciosIds.Contains(c.TreinoExercicioId))
                    .ToListAsync();
                _context.ComentariosTreino.RemoveRange(comentarios);

                // Execuções de exercício (ligadas à execução do treino e/ou ao exercício do treino)
                var execucoesExercicios = await _context.ExecucoesTreinoExercicios
                    .Where(ete => execucoesTreinoIds.Contains(ete.ExecucaoTreinoId) || treinoExerciciosIds.Contains(ete.TreinoExercicioId))
                    .ToListAsync();
                _context.ExecucoesTreinoExercicios.RemoveRange(execucoesExercicios);

                var execucoesTreino = await _context.ExecucoesTreino
                    .Where(et => execucoesTreinoIds.Contains(et.Id))
                    .ToListAsync();
                _context.ExecucoesTreino.RemoveRange(execucoesTreino);

                var treinoExercicios = await _context.TreinoExercicios
                    .Where(te => treinoExerciciosIds.Contains(te.Id))
                    .ToListAsync();
                _context.TreinoExercicios.RemoveRange(treinoExercicios);

                var treinos = await _context.Treinos
                    .Where(t => treinosIds.Contains(t.Id))
                    .ToListAsync();
                _context.Treinos.RemoveRange(treinos);

                var planos = await _context.PlanosTreino
                    .Where(p => planosIds.Contains(p.Id))
                    .ToListAsync();
                _context.PlanosTreino.RemoveRange(planos);

                var avaliacoes = await _context.AvaliacoesFisicas
                    .Where(a => a.AlunoId == id)
                    .ToListAsync();
                _context.AvaliacoesFisicas.RemoveRange(avaliacoes);

                var anamneses = await _context.Anamneses
                    .Where(a => a.AlunoId == id)
                    .ToListAsync();
                _context.Anamneses.RemoveRange(anamneses);

                var agendamentos = await _context.AgendamentosAlunos
                    .Where(a => a.AlunoId == id)
                    .ToListAsync();
                _context.AgendamentosAlunos.RemoveRange(agendamentos);

                var horariosAtendimento = await _context.AlunosHorariosAtendimento
                    .Where(h => h.AlunoId == id)
                    .ToListAsync();
                _context.AlunosHorariosAtendimento.RemoveRange(horariosAtendimento);

                await _context.SaveChangesAsync();

                var resultadoExclusao = await _userManager.DeleteAsync(aluno);
                if (!resultadoExclusao.Succeeded)
                {
                    await transaction.RollbackAsync();
                    TempData["Erro"] = "Não foi possível excluir o aluno. Tente novamente.";
                    return RedirectToAction(nameof(Alunos));
                }

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                TempData["Erro"] = "Não foi possível excluir o aluno. Tente novamente.";
                return RedirectToAction(nameof(Alunos));
            }

            TempData["Sucesso"] = $"Aluno {alunoNome} foi excluído permanentemente. Caso ele queira voltar, será necessário se cadastrar novamente.";
            return RedirectToAction(nameof(Alunos));
        }

        // ═════════════════════════════════════════════════════════════════
        // AGENDA SEMANAL - CORRIGIDA
        // ═════════════════════════════════════════════════════════════════

        public async Task<IActionResult> Agenda(DateTime? data)
        {
            var personal = await _userManager.GetUserAsync(User);
            if (personal == null) return Challenge();

            var dataRef = data ?? DateTime.Today;
            
            var inicioSemana = dataRef.AddDays(-(int)dataRef.DayOfWeek + (int)DayOfWeek.Monday);
            if (dataRef.DayOfWeek == DayOfWeek.Sunday)
                inicioSemana = dataRef.AddDays(-6);
            
            var fimSemana = inicioSemana.AddDays(6);

            var agendamentos = await _context.AlunosHorariosAtendimento
                .Include(a => a.Aluno)
                .Where(a => a.PersonalId == personal.Id && a.Ativo)
                .ToListAsync();

            var dias = new List<DiaAgendaViewModel>();
            var totalAulas = 0;
            var totalHoras = 0;
            var proximoAtendimento = "Nenhum agendamento";

            for (int i = 0; i < 7; i++)
            {
                var dia = inicioSemana.AddDays(i);
                var agendamentosDoDia = agendamentos
                    .Where(a => a.DiaSemana == dia.DayOfWeek)
                    .OrderBy(a => a.HoraInicio)
                    .ToList();

                var cards = new List<AgendamentoCardViewModel>();

                foreach (var ag in agendamentosDoDia)
                {
                    // ════════════════════════════════════════════════════════════
                    // CORREÇÃO 1: Busca o plano ATIVO e com vigência para a data
                    // ════════════════════════════════════════════════════════════
                    
                    var planoAtivo = await _context.PlanosTreino
                        .Include(p => p.Treinos)
                            .ThenInclude(t => t.TreinoExercicios)
                                .ThenInclude(te => te.Exercicio)
                                    .ThenInclude(e => e.GrupoMuscular)
                        .FirstOrDefaultAsync(p => 
                            p.AlunoId == ag.AlunoId && 
                            p.Status == 1 &&
                            p.DataInicio.Date <= dia.Date && 
                            (p.DataFim == null || p.DataFim.Value.Date >= dia.Date)
                        );

                    // ════════════════════════════════════════════════════════════
                    // CORREÇÃO 2: Mapeamento correto do dia da semana
                    // Usando o dia da semana da data do agendamento (dia)
                    // ════════════════════════════════════════════════════════════
                    
                    // Obtém o dia da semana da data atual (0 = Domingo, 1 = Segunda, ..., 6 = Sábado)
                    var diaSemana = (int)dia.DayOfWeek;
                    
                    // Converte para o formato do sistema (1 = Segunda, 2 = Terça, ..., 7 = Domingo)
                    var ordemDia = diaSemana == 0 ? 7 : diaSemana;
                    
                    // Busca o treino correspondente ao dia da semana
                    var treinoDoDia = planoAtivo?.Treinos
                        .FirstOrDefault(t => t.OrdemDia == ordemDia);

                    var exercicios = new List<ExercicioResumoViewModel>();

                    // ════════════════════════════════════════════════════════════
                    // CORREÇÃO 3: Só carrega exercícios se houver plano E treino
                    // ════════════════════════════════════════════════════════════

                    var planoValidoParaData = planoAtivo != null && treinoDoDia != null;

                    if (planoValidoParaData)
                    {
                        foreach (var te in treinoDoDia!.TreinoExercicios.OrderBy(te => te.Ordem))
                        {
                            string repsOuTempo;
                            if (te.TempoExecucaoSegundos.HasValue && te.TempoExecucaoSegundos.Value > 0)
                            {
                                var minutos = te.TempoExecucaoSegundos.Value / 60;
                                var segundos = te.TempoExecucaoSegundos.Value % 60;
                                repsOuTempo = minutos > 0 ? $"{minutos}min {segundos}s" : $"{segundos}s";
                            }
                            else
                            {
                                repsOuTempo = string.IsNullOrEmpty(te.RepeticoesPlanejadas) ? "—" : te.RepeticoesPlanejadas;
                            }

                            exercicios.Add(new ExercicioResumoViewModel
                            {
                                Nome = te.Exercicio?.Nome ?? "—",
                                GrupoMuscular = te.Exercicio?.GrupoMuscular?.Nome ?? "—",
                                Series = te.SeriesPlanejadas,
                                RepeticoesOuTempo = repsOuTempo,
                                Carga = te.CargaPlanejada,
                                Descanso = te.TempoDescanso,
                                Observacoes = te.Observacoes
                            });
                        }
                    }

                    var duracao = (ag.HoraFim - ag.HoraInicio).TotalHours;
                    totalAulas++;
                    totalHoras += (int)Math.Round(duracao);

                    var nomeCompleto = ag.Aluno?.NomeCompleto ?? "Aluno";
                    var iniciais = nomeCompleto.Length >= 2
                        ? $"{nomeCompleto[0]}{nomeCompleto.Split(' ').Last()[0]}".ToUpper()
                        : nomeCompleto[0].ToString().ToUpper();

                    // ════════════════════════════════════════════════════════════
                    // CORREÇÃO 4: Define a mensagem correta baseada no motivo
                    // ════════════════════════════════════════════════════════════
                    string mensagemSemPlano;
                    if (planoAtivo == null)
                    {
                        mensagemSemPlano = "Não há plano de treino ativo para esta data.";
                    }
                    else if (treinoDoDia == null)
                    {
                        mensagemSemPlano = $"Não há treino configurado para {dia.DayOfWeek}.";
                    }
                    else
                    {
                        mensagemSemPlano = null!;
                    }

                    cards.Add(new AgendamentoCardViewModel
                    {
                        Id = ag.Id,
                        AlunoId = ag.AlunoId,
                        AlunoNome = nomeCompleto,
                        Iniciais = iniciais,
                        HoraInicio = ag.HoraInicio,
                        HoraFim = ag.HoraFim,
                        Status = "Confirmado",
                        Objetivo = planoValidoParaData ? planoAtivo!.Objetivo : null,
                        PlanoTreinoId = planoValidoParaData ? planoAtivo!.Id : null,
                        TreinoDiaNome = planoValidoParaData ? treinoDoDia!.Nome : null,
                        Exercicios = exercicios,
                        TemPlanoAtivo = planoValidoParaData,
                        MensagemSemPlano = mensagemSemPlano
                    });

                    if (proximoAtendimento == "Nenhum agendamento" && 
                        (dia > DateTime.Today || 
                         (dia == DateTime.Today && ag.HoraInicio > DateTime.Now.TimeOfDay)))
                    {
                        proximoAtendimento = $"{dia:dd/MM} às {ag.HoraInicio:hh\\:mm} - {ag.Aluno?.NomeCompleto}";
                    }
                }

                dias.Add(new DiaAgendaViewModel
                {
                    DiaSemana = dia.DayOfWeek,
                    NomeDia = ObterNomeDiaCompleto(dia.DayOfWeek),
                    NomeDiaAbreviado = ObterNomeDiaAbreviado(dia.DayOfWeek),
                    Data = dia,
                    Hoje = dia.Date == DateTime.Today.Date,
                    Agendamentos = cards
                });
            }

            var vm = new AgendaSemanalViewModel
            {
                DataReferencia = dataRef,
                InicioSemana = inicioSemana,
                FimSemana = fimSemana,
                Dias = dias,
                Resumo = new ResumoAgendaViewModel
                {
                    TotalAulas = totalAulas,
                    TotalHoras = totalHoras,
                    ProximoAtendimento = proximoAtendimento
                }
            };

            return View(vm);
        }

        private string ObterNomeDiaCompleto(DayOfWeek dia)
        {
            return dia switch
            {
                DayOfWeek.Sunday => "Domingo",
                DayOfWeek.Monday => "Segunda-feira",
                DayOfWeek.Tuesday => "Terça-feira",
                DayOfWeek.Wednesday => "Quarta-feira",
                DayOfWeek.Thursday => "Quinta-feira",
                DayOfWeek.Friday => "Sexta-feira",
                DayOfWeek.Saturday => "Sábado",
                _ => ""
            };
        }

        private string ObterNomeDiaAbreviado(DayOfWeek dia)
        {
            return dia switch
            {
                DayOfWeek.Sunday => "DOM",
                DayOfWeek.Monday => "SEG",
                DayOfWeek.Tuesday => "TER",
                DayOfWeek.Wednesday => "QUA",
                DayOfWeek.Thursday => "QUI",
                DayOfWeek.Friday => "SEX",
                DayOfWeek.Saturday => "SÁB",
                _ => ""
            };
        }

        // ═════════════════════════════════════════════════════════════════
        // VALIDAÇÃO DE CONFLITO DE PLANOS
        // ═════════════════════════════════════════════════════════════════

        private async Task<bool> ValidarConflitoPlanos(string alunoId, DateTime dataInicio, DateTime dataFim, int? planoIdIgnorar = null)
        {
            // Dois períodos [A, B] e [C, D] se sobrepõem quando: A <= D e C <= B
            // Trata plano sem data fim como permanente (DateTime.MaxValue)
            var conflito = await _context.PlanosTreino
                .AnyAsync(p =>
                    p.AlunoId == alunoId &&
                    p.Status == 1 &&
                    (planoIdIgnorar == null || p.Id != planoIdIgnorar) &&
                    p.DataInicio <= dataFim &&
                    dataInicio <= (p.DataFim ?? DateTime.MaxValue)
                );

            return conflito;
        }

        // ═════════════════════════════════════════════════════════════════
        // PLANOS DE TREINO
        // ═════════════════════════════════════════════════════════════════

        public async Task<IActionResult> PlanosTreino()
        {
            var personal = await _userManager.GetUserAsync(User);
            if (personal == null) return Challenge();

            var planos = await _context.PlanosTreino
                .Include(p => p.Aluno)
                .Where(p => p.TreinadorId == personal.Id)
                .ToListAsync();

            var alunosResumo = planos
                .GroupBy(p => new { p.AlunoId, p.Aluno!.NomeCompleto })
                .Select(g => new AlunoPlanosResumoViewModel
                {
                    AlunoId = g.Key.AlunoId,
                    NomeCompleto = g.Key.NomeCompleto,
                    UltimaAtualizacao = g.Max(p => p.DataCriacao),
                    TotalPlanos = g.Count(),
                    PlanoAtivoNome = g.FirstOrDefault(p => p.Status == 1)?.Titulo,
                    PlanoAtivoId = g.FirstOrDefault(p => p.Status == 1)?.Id
                })
                .OrderByDescending(a => a.UltimaAtualizacao)
                .ToList();

            return View(alunosResumo);
        }

        public async Task<IActionResult> PlanosDoAluno(string id)
        {
            var personal = await _userManager.GetUserAsync(User);
            if (personal == null) return Challenge();

            var alunoVinculado = await _context.PlanosTreino
                .AnyAsync(p => p.TreinadorId == personal.Id && p.AlunoId == id);
            
            if (!alunoVinculado) return Forbid();

            var aluno = await _userManager.FindByIdAsync(id);
            if (aluno == null) return NotFound();

            var planos = await _context.PlanosTreino
                .Where(p => p.TreinadorId == personal.Id && p.AlunoId == id)
                .OrderByDescending(p => p.DataCriacao)
                .Select(p => new PlanoTreinoDetailViewModel
                {
                    Id = p.Id,
                    Titulo = p.Titulo,
                    Objetivo = p.Objetivo,
                    DataCriacao = p.DataCriacao,
                    Status = p.Status
                })
                .ToListAsync();

            var vm = new PlanosDoAlunoViewModel
            {
                AlunoId = id,
                AlunoNome = aluno.NomeCompleto,
                Planos = planos
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DuplicarPlano(int id)
        {
            var personal = await _userManager.GetUserAsync(User);
            if (personal == null) return Challenge();

            var planoOriginal = await _context.PlanosTreino
                .Include(p => p.Treinos)
                    .ThenInclude(t => t.TreinoExercicios)
                .FirstOrDefaultAsync(p => p.Id == id && p.TreinadorId == personal.Id);

            if (planoOriginal == null) return NotFound();

            var novoPlano = new PlanoTreino
            {
                TreinadorId = planoOriginal.TreinadorId,
                AlunoId = planoOriginal.AlunoId,
                Titulo = $"{planoOriginal.Titulo} (Cópia)",
                Objetivo = planoOriginal.Objetivo,
                DataInicio = DateTime.Today,
                DataFim = null,
                Status = 1,
                DataCriacao = DateTime.Now
            };
            _context.PlanosTreino.Add(novoPlano);
            await _context.SaveChangesAsync();

            foreach (var treinoOriginal in planoOriginal.Treinos)
            {
                var novoTreino = new Treino
                {
                    PlanoTreinoId = novoPlano.Id,
                    Nome = treinoOriginal.Nome,
                    OrdemDia = treinoOriginal.OrdemDia,
                    Observacoes = treinoOriginal.Observacoes,
                    DataCriacao = DateTime.Now
                };
                _context.Treinos.Add(novoTreino);
                await _context.SaveChangesAsync();

                foreach (var exOriginal in treinoOriginal.TreinoExercicios)
                {
                    _context.TreinoExercicios.Add(new TreinoExercicio
                    {
                        TreinoId = novoTreino.Id,
                        ExercicioId = exOriginal.ExercicioId,
                        Ordem = exOriginal.Ordem,
                        SeriesPlanejadas = exOriginal.SeriesPlanejadas,
                        RepeticoesPlanejadas = exOriginal.RepeticoesPlanejadas,
                        TempoExecucaoSegundos = exOriginal.TempoExecucaoSegundos,
                        CargaPlanejada = exOriginal.CargaPlanejada.HasValue ? Math.Round(exOriginal.CargaPlanejada.Value, 2) : null,
                        TempoDescanso = exOriginal.TempoDescanso,
                        Observacoes = exOriginal.Observacoes,
                        DataCriacao = DateTime.Now
                    });
                }
            }
            await _context.SaveChangesAsync();

            TempData["Sucesso"] = $"Plano \"{novoPlano.Titulo}\" duplicado com sucesso!";
            return RedirectToAction(nameof(PlanosDoAluno), new { id = planoOriginal.AlunoId });
        }

        // ── Criar Plano de Treino (GET) ───────────────────────────────
        public async Task<IActionResult> CriarPlano(string? alunoId)
        {
            var personal = await _userManager.GetUserAsync(User);
            if (personal == null) return Challenge();

            await CarregarSelectsPlano(personal.Id, alunoId);
            return View(new CriarPlanoViewModel { AlunoId = alunoId ?? "", DataInicio = DateTime.Today });
        }

        // ── Criar Plano de Treino (POST) ──────────────────────────────
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

            // ════════════════════════════════════════════════════════════
            // CORREÇÃO: Validar conflito de datas antes de salvar
            // ════════════════════════════════════════════════════════════

            if (vm.DataFim.HasValue && vm.DataFim.Value < vm.DataInicio)
            {
                ModelState.AddModelError("", "A data de fim não pode ser anterior à data de início.");
                await CarregarSelectsPlano(personal.Id, vm.AlunoId);
                return View(vm);
            }

            var temConflito = await ValidarConflitoPlanos(vm.AlunoId, vm.DataInicio, vm.DataFim ?? DateTime.MaxValue);
            if (temConflito)
            {
                ModelState.AddModelError("", "Já existe um treino ativo para este aluno dentro do período informado. Edite o treino existente ou altere as datas do novo treino.");
                await CarregarSelectsPlano(personal.Id, vm.AlunoId);
                return View(vm);
            }

            var temExercicio = vm.DiasSemana
                .Where(d => d.Selecionado)
                .Any(d => d.Exercicios.Any(e => e.ExercicioId > 0));

            if (!temExercicio)
            {
                ModelState.AddModelError("", "Adicione pelo menos 1 exercício em algum dia da semana antes de salvar.");
                await CarregarSelectsPlano(personal.Id, vm.AlunoId);
                return View(vm);
            }

            foreach (var dia in vm.DiasSemana.Where(d => d.Selecionado))
            {
                foreach (var ex in dia.Exercicios.Where(e => e.ExercicioId > 0))
                {
                    var temRepeticoes = !string.IsNullOrWhiteSpace(ex.Repeticoes);
                    var temTempo = ex.TempoExecucao.HasValue && ex.TempoExecucao.Value > 0;
                    if (!temRepeticoes && !temTempo)
                    {
                        ModelState.AddModelError("", $"Em {dia.Nome}: um exercício está sem Repetições nem Tempo de execução. Preencha exatamente um dos dois.");
                        await CarregarSelectsPlano(personal.Id, vm.AlunoId);
                        return View(vm);
                    }
                    if (temRepeticoes && temTempo)
                    {
                        ModelState.AddModelError("", $"Em {dia.Nome}: um exercício tem Repetições E Tempo preenchidos ao mesmo tempo. Preencha apenas um dos dois.");
                        await CarregarSelectsPlano(personal.Id, vm.AlunoId);
                        return View(vm);
                    }
                }
            }

            var plano = new PlanoTreino
            {
                TreinadorId = personal.Id,
                AlunoId = vm.AlunoId,
                Titulo = vm.Titulo,
                Objetivo = vm.Objetivo,
                DataInicio = vm.DataInicio,
                DataFim = vm.DataFim,
                Status = 1,
                DataCriacao = DateTime.Now
            };
            _context.PlanosTreino.Add(plano);
            await _context.SaveChangesAsync();

            foreach (var dia in vm.DiasSemana.Where(d => d.Selecionado))
            {
                var treino = new Treino
                {
                    PlanoTreinoId = plano.Id,
                    Nome = dia.Nome,
                    OrdemDia = dia.Ordem,
                    Observacoes = dia.Observacoes ?? "",
                    DataCriacao = DateTime.Now
                };
                _context.Treinos.Add(treino);
                await _context.SaveChangesAsync();

                foreach (var ex in dia.Exercicios.Where(e => e.ExercicioId > 0))
                {
                    int? tempoSegundos = null;
                    string repeticoes = "";

                    if (ex.TempoExecucao.HasValue && ex.TempoExecucao.Value > 0)
                    {
                        tempoSegundos = (int)(ex.TempoExecucao.Value * 60);
                        repeticoes = "";
                    }
                    else if (!string.IsNullOrWhiteSpace(ex.Repeticoes))
                    {
                        tempoSegundos = null;
                        repeticoes = ex.Repeticoes;
                    }

                    _context.TreinoExercicios.Add(new TreinoExercicio
                    {
                        TreinoId = treino.Id,
                        ExercicioId = ex.ExercicioId,
                        Ordem = ex.Ordem,
                        SeriesPlanejadas = ex.Series ?? 3,
                        RepeticoesPlanejadas = repeticoes,
                        TempoExecucaoSegundos = tempoSegundos,
                        CargaPlanejada = ex.Carga.HasValue ? (decimal)ex.Carga.Value : (decimal?)null,
                        TempoDescanso = ex.Descanso,
                        Observacoes = ex.Observacoes ?? "",
                        DataCriacao = DateTime.Now
                    });
                }
            }
            await _context.SaveChangesAsync();

            TempData["Sucesso"] = $"Plano \"{vm.Titulo}\" criado com sucesso!";
            return RedirectToAction(nameof(PlanosTreino));
        }

        public async Task<IActionResult> EditarPlano(int id)
        {
            var personal = await _userManager.GetUserAsync(User);
            if (personal == null) return Challenge();

            var plano = await _context.PlanosTreino
                .Include(p => p.Treinos)
                    .ThenInclude(t => t.TreinoExercicios)
                .FirstOrDefaultAsync(p => p.Id == id && p.TreinadorId == personal.Id);

            if (plano == null) return NotFound();

            var vm = new EditarPlanoViewModel
            {
                Id = plano.Id,
                AlunoId = plano.AlunoId,
                Titulo = plano.Titulo,
                Objetivo = plano.Objetivo,
                DataInicio = plano.DataInicio,
                DataFim = plano.DataFim,
                DiasSemana = new List<DiaTreinoEditViewModel>()
            };

            for (int i = 1; i <= 7; i++)
            {
                var treino = plano.Treinos.FirstOrDefault(t => t.OrdemDia == i);
                var diaVM = new DiaTreinoEditViewModel
                {
                    TreinoId = treino?.Id,
                    Nome = ObterNomeDia(i),
                    Ordem = i,
                    Selecionado = treino != null,
                    Observacoes = treino?.Observacoes ?? "",
                    Exercicios = new List<ExercicioEditViewModel>()
                };

                if (treino != null)
                {
                    foreach (var te in treino.TreinoExercicios.OrderBy(te => te.Ordem))
                    {
                        diaVM.Exercicios.Add(new ExercicioEditViewModel
                        {
                            TreinoExercicioId = te.Id,
                            Ordem = te.Ordem,
                            ExercicioId = te.ExercicioId,
                            Series = te.SeriesPlanejadas,
                            Repeticoes = te.RepeticoesPlanejadas,
                            TempoExecucaoSegundos = te.TempoExecucaoSegundos,
                            Carga = te.CargaPlanejada.HasValue ? (int?)((int)te.CargaPlanejada.Value) : null,
                            Descanso = te.TempoDescanso,
                            Observacoes = te.Observacoes
                        });
                    }
                }

                vm.DiasSemana.Add(diaVM);
            }

            await CarregarSelectsPlano(personal.Id, vm.AlunoId);
            return View("EditarPlano", vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarPlano(EditarPlanoViewModel vm)
        {
            var personal = await _userManager.GetUserAsync(User);
            if (personal == null) return Challenge();

            var plano = await _context.PlanosTreino
                .Include(p => p.Treinos)
                    .ThenInclude(t => t.TreinoExercicios)
                .FirstOrDefaultAsync(p => p.Id == vm.Id && p.TreinadorId == personal.Id);

            if (plano == null) return NotFound();

            if (!ModelState.IsValid)
            {
                await CarregarSelectsPlano(personal.Id, vm.AlunoId);
                return View("EditarPlano", vm);
            }

            // ════════════════════════════════════════════════════════════
            // CORREÇÃO: Validar conflito de datas na edição (ignorando o próprio plano)
            // ════════════════════════════════════════════════════════════

            if (vm.DataFim.HasValue && vm.DataFim.Value < vm.DataInicio)
            {
                ModelState.AddModelError("", "A data de fim não pode ser anterior à data de início.");
                await CarregarSelectsPlano(personal.Id, vm.AlunoId);
                return View("EditarPlano", vm);
            }

            var temConflito = await ValidarConflitoPlanos(vm.AlunoId, vm.DataInicio, vm.DataFim ?? DateTime.MaxValue, vm.Id);
            if (temConflito)
            {
                ModelState.AddModelError("", "Já existe outro treino ativo para este aluno dentro do período informado. Edite o treino existente ou altere as datas do novo treino.");
                await CarregarSelectsPlano(personal.Id, vm.AlunoId);
                return View("EditarPlano", vm);
            }

            var temExercicio = vm.DiasSemana
                .Where(d => d.Selecionado)
                .Any(d => d.Exercicios.Any(e => e.ExercicioId > 0));

            if (!temExercicio)
            {
                ModelState.AddModelError("", "Adicione pelo menos 1 exercício em algum dia da semana antes de salvar.");
                await CarregarSelectsPlano(personal.Id, vm.AlunoId);
                return View("EditarPlano", vm);
            }

            foreach (var dia in vm.DiasSemana.Where(d => d.Selecionado))
            {
                foreach (var ex in dia.Exercicios.Where(e => e.ExercicioId > 0))
                {
                    var temRepeticoes = !string.IsNullOrWhiteSpace(ex.Repeticoes);
                    var temTempo = ex.TempoExecucaoSegundos.HasValue && ex.TempoExecucaoSegundos.Value > 0;
                    if (!temRepeticoes && !temTempo)
                    {
                        ModelState.AddModelError("", $"Em {dia.Nome}: um exercício está sem Repetições nem Tempo de execução. Preencha exatamente um dos dois.");
                        await CarregarSelectsPlano(personal.Id, vm.AlunoId);
                        return View("EditarPlano", vm);
                    }
                    if (temRepeticoes && temTempo)
                    {
                        ModelState.AddModelError("", $"Em {dia.Nome}: um exercício tem Repetições E Tempo preenchidos ao mesmo tempo. Preencha apenas um dos dois.");
                        await CarregarSelectsPlano(personal.Id, vm.AlunoId);
                        return View("EditarPlano", vm);
                    }
                }
            }

            plano.Titulo = vm.Titulo;
            plano.Objetivo = vm.Objetivo;
            plano.DataInicio = vm.DataInicio;
            plano.DataFim = vm.DataFim;
            plano.DataAtualizacao = DateTime.Now;

            foreach (var treino in plano.Treinos)
            {
                _context.TreinoExercicios.RemoveRange(treino.TreinoExercicios);
            }
            _context.Treinos.RemoveRange(plano.Treinos);
            await _context.SaveChangesAsync();

            foreach (var dia in vm.DiasSemana.Where(d => d.Selecionado))
            {
                var treino = new Treino
                {
                    PlanoTreinoId = plano.Id,
                    Nome = dia.Nome,
                    OrdemDia = dia.Ordem,
                    Observacoes = dia.Observacoes ?? "",
                    DataCriacao = DateTime.Now
                };
                _context.Treinos.Add(treino);
                await _context.SaveChangesAsync();

                foreach (var ex in dia.Exercicios.Where(e => e.ExercicioId > 0))
                {
                    int? tempoSegundos = ex.TempoExecucaoSegundos;
                    string repeticoes = ex.Repeticoes ?? "";

                    if (tempoSegundos.HasValue && tempoSegundos.Value > 0)
                    {
                        repeticoes = "";
                    }

                    _context.TreinoExercicios.Add(new TreinoExercicio
                    {
                        TreinoId = treino.Id,
                        ExercicioId = ex.ExercicioId,
                        Ordem = ex.Ordem,
                        SeriesPlanejadas = ex.Series,
                        RepeticoesPlanejadas = repeticoes,
                        TempoExecucaoSegundos = tempoSegundos,
                        CargaPlanejada = ex.Carga.HasValue ? (decimal)ex.Carga.Value : (decimal?)null,
                        TempoDescanso = ex.Descanso,
                        Observacoes = ex.Observacoes ?? "",
                        DataCriacao = DateTime.Now
                    });
                }
            }
            await _context.SaveChangesAsync();

            TempData["Sucesso"] = $"Plano \"{plano.Titulo}\" atualizado com sucesso!";
            return RedirectToAction(nameof(PlanosDoAluno), new { id = plano.AlunoId });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ExcluirPlano(int id)
        {
            var personal = await _userManager.GetUserAsync(User);
            if (personal == null) return Challenge();

            var plano = await _context.PlanosTreino
                .Include(p => p.Treinos)
                    .ThenInclude(t => t.TreinoExercicios)
                .FirstOrDefaultAsync(p => p.Id == id && p.TreinadorId == personal.Id);

            if (plano == null) return NotFound();

            var alunoId = plano.AlunoId;
            var titulo = plano.Titulo;

            foreach (var treino in plano.Treinos)
            {
                _context.TreinoExercicios.RemoveRange(treino.TreinoExercicios);
            }
            _context.Treinos.RemoveRange(plano.Treinos);
            _context.PlanosTreino.Remove(plano);

            await _context.SaveChangesAsync();

            TempData["Sucesso"] = $"Plano \"{titulo}\" foi excluído com sucesso!";
            return RedirectToAction(nameof(PlanosDoAluno), new { id = alunoId });
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

            if (string.IsNullOrEmpty(vm.AlunoId))
            {
                await CarregarAlunosSelect(personal.Id, vm.AlunoId);
                ModelState.AddModelError("AlunoId", "Selecione o aluno.");
                return View("Medidas", vm);
            }

            var avaliacao = new AvaliacaoFisica
            {
                AlunoId = vm.AlunoId,
                TreinadorId = personal.Id,
                DataAvaliacao = vm.DataAvaliacao,
                Altura = vm.Altura,
                Peso = vm.Peso,
                Pescoco = vm.Pescoco,
                Ombro = vm.Ombro,
                ToraxContrai = vm.ToraxContrai,
                ToraxRelax = vm.ToraxRelax,
                BicepsDireito = vm.BicepsDireito,
                BicepsEsquerdo = vm.BicepsEsquerdo,
                Cintura = vm.Cintura,
                Abdomen = vm.Abdomen,
                Quadril = vm.Quadril,
                CoxaDireita = vm.CoxaDireita,
                CoxaEsquerda = vm.CoxaEsquerda,
                PanturrilhaDireita = vm.PanturrilhaDireita,
                PanturrilhaEsquerda = vm.PanturrilhaEsquerda,
                Observacoes = vm.Observacoes,
                DataCriacao = DateTime.Now
            };

            _context.AvaliacoesFisicas.Add(avaliacao);
            await _context.SaveChangesAsync();

            TempData["Sucesso"] = "Medidas salvas com sucesso!";
            return RedirectToAction(nameof(HistoricoMedidas), new { alunoId = vm.AlunoId });
        }

        public async Task<IActionResult> HistoricoMedidas(string? alunoId)
        {
            var personal = await _userManager.GetUserAsync(User);
            if (personal == null) return Challenge();

            await CarregarAlunosSelect(personal.Id, alunoId);

            var historico = string.IsNullOrEmpty(alunoId)
                ? new List<AvaliacaoFisica>()
                : await _context.AvaliacoesFisicas
                    .Where(a => a.AlunoId == alunoId && a.TreinadorId == personal.Id)
                    .OrderByDescending(a => a.DataAvaliacao)
                    .ToListAsync();

            ViewBag.AlunoId = alunoId;
            ViewBag.AlunoNome = alunoId != null
                ? (await _userManager.FindByIdAsync(alunoId))?.NomeCompleto
                : null;

            return View(historico);
        }

        public async Task<IActionResult> EditarMedida(int id)
        {
            var personal = await _userManager.GetUserAsync(User);
            if (personal == null) return Challenge();

            var av = await _context.AvaliacoesFisicas
                .FirstOrDefaultAsync(a => a.Id == id && a.TreinadorId == personal.Id);
            if (av == null) return NotFound();

            await CarregarAlunosSelect(personal.Id, av.AlunoId);

            var vm = new MedidasViewModel
            {
                AlunoId = av.AlunoId,
                DataAvaliacao = av.DataAvaliacao,
                Altura = av.Altura,
                Peso = av.Peso,
                Pescoco = av.Pescoco,
                Ombro = av.Ombro,
                ToraxContrai = av.ToraxContrai,
                ToraxRelax = av.ToraxRelax,
                BicepsDireito = av.BicepsDireito,
                BicepsEsquerdo = av.BicepsEsquerdo,
                Cintura = av.Cintura,
                Abdomen = av.Abdomen,
                Quadril = av.Quadril,
                CoxaDireita = av.CoxaDireita,
                CoxaEsquerda = av.CoxaEsquerda,
                PanturrilhaDireita = av.PanturrilhaDireita,
                PanturrilhaEsquerda = av.PanturrilhaEsquerda,
                Observacoes = av.Observacoes
            };

            ViewBag.AvaliacaoId = id;
            return View("Medidas", vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AtualizarMedida(int id, MedidasViewModel vm)
        {
            var personal = await _userManager.GetUserAsync(User);
            if (personal == null) return Challenge();

            var av = await _context.AvaliacoesFisicas
                .FirstOrDefaultAsync(a => a.Id == id && a.TreinadorId == personal.Id);
            if (av == null) return NotFound();

            av.DataAvaliacao = vm.DataAvaliacao;
            av.Altura = vm.Altura;
            av.Peso = vm.Peso;
            av.Pescoco = vm.Pescoco;
            av.Ombro = vm.Ombro;
            av.ToraxContrai = vm.ToraxContrai;
            av.ToraxRelax = vm.ToraxRelax;
            av.BicepsDireito = vm.BicepsDireito;
            av.BicepsEsquerdo = vm.BicepsEsquerdo;
            av.Cintura = vm.Cintura;
            av.Abdomen = vm.Abdomen;
            av.Quadril = vm.Quadril;
            av.CoxaDireita = vm.CoxaDireita;
            av.CoxaEsquerda = vm.CoxaEsquerda;
            av.PanturrilhaDireita = vm.PanturrilhaDireita;
            av.PanturrilhaEsquerda = vm.PanturrilhaEsquerda;
            av.Observacoes = vm.Observacoes;

            await _context.SaveChangesAsync();
            TempData["Sucesso"] = "Medidas atualizadas!";
            return RedirectToAction(nameof(HistoricoMedidas), new { alunoId = av.AlunoId });
        }

        // ── Anamnese ──────────────────────────────────────────────
        public async Task<IActionResult> Anamnese(string? alunoId)
        {
            var personal = await _userManager.GetUserAsync(User);
            if (personal == null) return Challenge();

            await CarregarAlunosSelect(personal.Id, alunoId);
            return View(new AnamneseViewModel { AlunoId = alunoId ?? "", DataAvaliacao = DateTime.Today });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SalvarAnamnese(AnamneseViewModel vm)
        {
            var personal = await _userManager.GetUserAsync(User);
            if (personal == null) return Challenge();

            if (string.IsNullOrEmpty(vm.AlunoId))
            {
                await CarregarAlunosSelect(personal.Id, vm.AlunoId);
                ModelState.AddModelError("AlunoId", "Selecione o aluno.");
                return View("Anamnese", vm);
            }

            var anamnese = new AnamneseAluno
            {
                AlunoId = vm.AlunoId,
                TreinadorId = personal.Id,
                DataAvaliacao = vm.DataAvaliacao,
                JaTreinouAntes = vm.JaTreinouAntes,
                TempoTreinando = vm.TempoTreinando,
                TempoSemAtividade = vm.TempoSemAtividade,
                Objetivo = vm.Objetivo,
                FrequenciaSemanal = vm.FrequenciaSemanal,
                TempoPorDia = vm.TempoPorDia,
                TemDoenca = vm.TemDoenca,
                QualDoenca = vm.QualDoenca,
                TemLimitacaoMovimento = vm.TemLimitacaoMovimento,
                QualLimitacao = vm.QualLimitacao,
                TemDorMovimento = vm.TemDorMovimento,
                QualDor = vm.QualDor,
                FezCirurgia = vm.FezCirurgia,
                QualCirurgia = vm.QualCirurgia,
                UsaMedicamento = vm.UsaMedicamento,
                QualMedicamento = vm.QualMedicamento,
                FazDieta = vm.FazDieta,
                TipoDieta = vm.TipoDieta,
                ConsomeAlcool = vm.ConsomeAlcool,
                Fuma = vm.Fuma,
                ObservacoesGerais = vm.ObservacoesGerais,
                DataCriacao = DateTime.Now
            };

            _context.Anamneses.Add(anamnese);
            await _context.SaveChangesAsync();

            TempData["Sucesso"] = "Anamnese salva com sucesso!";
            return RedirectToAction(nameof(VisualizarAnamnese), new { alunoId = vm.AlunoId });
        }

        public async Task<IActionResult> VisualizarAnamnese(string? alunoId)
        {
            var personal = await _userManager.GetUserAsync(User);
            if (personal == null) return Challenge();

            await CarregarAlunosSelect(personal.Id, alunoId);

            var anamnese = string.IsNullOrEmpty(alunoId) ? null
                : await _context.Anamneses
                    .Where(a => a.AlunoId == alunoId && a.TreinadorId == personal.Id)
                    .OrderByDescending(a => a.DataAvaliacao)
                    .FirstOrDefaultAsync();

            ViewBag.AlunoId = alunoId;
            ViewBag.AlunoNome = alunoId != null
                ? (await _userManager.FindByIdAsync(alunoId))?.NomeCompleto
                : null;

            return View(anamnese);
        }

        public async Task<IActionResult> EditarAnamnese(int id)
        {
            var personal = await _userManager.GetUserAsync(User);
            if (personal == null) return Challenge();

            var a = await _context.Anamneses
                .FirstOrDefaultAsync(x => x.Id == id && x.TreinadorId == personal.Id);
            if (a == null) return NotFound();

            await CarregarAlunosSelect(personal.Id, a.AlunoId);

            var vm = new AnamneseViewModel
            {
                AlunoId = a.AlunoId,
                DataAvaliacao = a.DataAvaliacao,
                JaTreinouAntes = a.JaTreinouAntes,
                TempoTreinando = a.TempoTreinando,
                TempoSemAtividade = a.TempoSemAtividade,
                Objetivo = a.Objetivo,
                FrequenciaSemanal = a.FrequenciaSemanal,
                TempoPorDia = a.TempoPorDia,
                TemDoenca = a.TemDoenca,
                QualDoenca = a.QualDoenca,
                TemLimitacaoMovimento = a.TemLimitacaoMovimento,
                QualLimitacao = a.QualLimitacao,
                TemDorMovimento = a.TemDorMovimento,
                QualDor = a.QualDor,
                FezCirurgia = a.FezCirurgia,
                QualCirurgia = a.QualCirurgia,
                UsaMedicamento = a.UsaMedicamento,
                QualMedicamento = a.QualMedicamento,
                FazDieta = a.FazDieta,
                TipoDieta = a.TipoDieta,
                ConsomeAlcool = a.ConsomeAlcool,
                Fuma = a.Fuma,
                ObservacoesGerais = a.ObservacoesGerais
            };

            ViewBag.AnamneseId = id;
            return View("Anamnese", vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AtualizarAnamnese(int id, AnamneseViewModel vm)
        {
            var personal = await _userManager.GetUserAsync(User);
            if (personal == null) return Challenge();

            var anamnese = await _context.Anamneses
                .FirstOrDefaultAsync(a => a.Id == id && a.TreinadorId == personal.Id);
            if (anamnese == null) return NotFound();

            anamnese.DataAvaliacao = vm.DataAvaliacao;
            anamnese.JaTreinouAntes = vm.JaTreinouAntes;
            anamnese.TempoTreinando = vm.TempoTreinando;
            anamnese.TempoSemAtividade = vm.TempoSemAtividade;
            anamnese.Objetivo = vm.Objetivo;
            anamnese.FrequenciaSemanal = vm.FrequenciaSemanal;
            anamnese.TempoPorDia = vm.TempoPorDia;
            anamnese.TemDoenca = vm.TemDoenca;
            anamnese.QualDoenca = vm.QualDoenca;
            anamnese.TemLimitacaoMovimento = vm.TemLimitacaoMovimento;
            anamnese.QualLimitacao = vm.QualLimitacao;
            anamnese.TemDorMovimento = vm.TemDorMovimento;
            anamnese.QualDor = vm.QualDor;
            anamnese.FezCirurgia = vm.FezCirurgia;
            anamnese.QualCirurgia = vm.QualCirurgia;
            anamnese.UsaMedicamento = vm.UsaMedicamento;
            anamnese.QualMedicamento = vm.QualMedicamento;
            anamnese.FazDieta = vm.FazDieta;
            anamnese.TipoDieta = vm.TipoDieta;
            anamnese.ConsomeAlcool = vm.ConsomeAlcool;
            anamnese.Fuma = vm.Fuma;
            anamnese.ObservacoesGerais = vm.ObservacoesGerais;
            anamnese.DataAtualizacao = DateTime.Now;

            await _context.SaveChangesAsync();

            TempData["Sucesso"] = "Anamnese atualizada com sucesso!";
            return RedirectToAction(nameof(VisualizarAnamnese), new { alunoId = anamnese.AlunoId });
        }

        public async Task<IActionResult> VisualizarPlano(int id)
        {
            var personal = await _userManager.GetUserAsync(User);
            if (personal == null) return Challenge();

            var plano = await _context.PlanosTreino
                .Include(p => p.Aluno)
                .Include(p => p.Treinos)
                    .ThenInclude(t => t.TreinoExercicios)
                        .ThenInclude(te => te.Exercicio)
                            .ThenInclude(e => e.GrupoMuscular)
                .FirstOrDefaultAsync(p => p.Id == id && p.TreinadorId == personal.Id);

            if (plano == null) return NotFound();

            return View(plano);
        }

        // ═════════════════════════════════════════════════════════════════
        // HELPERS - CARREGAMENTO DE ALUNOS
        // ═════════════════════════════════════════════════════════════════

        // ── Carregar alunos para o select de planos de treino ──
        private async Task CarregarSelectsPlano(string personalId, string? alunoIdSel)
        {
            var alunosIds = await _context.PlanosTreino
                .Where(p => p.TreinadorId == personalId)
                .Select(p => p.AlunoId)
                .Distinct()
                .ToListAsync();

            var alunos = await _userManager.Users
                .Where(u => alunosIds.Contains(u.Id))
                .OrderBy(u => u.NomeCompleto)
                .ToListAsync();

            var lista = new List<SelectListItem>();
            
            if (alunos.Any())
            {
                lista.Add(new SelectListItem 
                { 
                    Value = "", 
                    Text = "— selecione o aluno —", 
                    Selected = string.IsNullOrEmpty(alunoIdSel) 
                });
            }
            
            lista.AddRange(alunos.Select(a => new SelectListItem
            {
                Value = a.Id,
                Text = a.NomeCompleto,
                Selected = a.Id == alunoIdSel
            }));

            ViewBag.Alunos = new SelectList(lista, "Value", "Text", alunoIdSel);
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

        // ── Carregar alunos para outras telas ──
        private async Task CarregarAlunosSelect(string personalId, string? selecionado)
        {
            var alunosIds = await _context.PlanosTreino
                .Where(p => p.TreinadorId == personalId)
                .Select(p => p.AlunoId)
                .Distinct()
                .ToListAsync();

            var alunos = await _userManager.Users
                .Where(u => alunosIds.Contains(u.Id))
                .OrderBy(u => u.NomeCompleto)
                .ToListAsync();

            if (!string.IsNullOrEmpty(selecionado) && !alunos.Any(a => a.Id == selecionado))
            {
                selecionado = null;
            }

            var lista = new List<SelectListItem>();
            
            if (alunos.Any())
            {
                lista.Add(new SelectListItem 
                { 
                    Value = "", 
                    Text = "— selecione o aluno —", 
                    Selected = string.IsNullOrEmpty(selecionado) 
                });
            }
            
            lista.AddRange(alunos.Select(a => new SelectListItem
            {
                Value = a.Id,
                Text = a.NomeCompleto,
                Selected = a.Id == selecionado
            }));

            ViewBag.Alunos = new SelectList(lista, "Value", "Text", selecionado);
        }

        // ── Método auxiliar para obter nome do dia ─────────────────
        private string ObterNomeDia(int ordem)
        {
            return ordem switch
            {
                1 => "Segunda-feira",
                2 => "Terça-feira",
                3 => "Quarta-feira",
                4 => "Quinta-feira",
                5 => "Sexta-feira",
                6 => "Sábado",
                7 => "Domingo",
                _ => ""
            };
        }
    }

    // ── Classe auxiliar para validação via AJAX ───────────────────
    public class HorarioFrontend
    {
        public string HoraInicio { get; set; } = "";
        public string HoraFim { get; set; } = "";
        public bool Domingo { get; set; }
        public bool Segunda { get; set; }
        public bool Terca { get; set; }
        public bool Quarta { get; set; }
        public bool Quinta { get; set; }
        public bool Sexta { get; set; }
        public bool Sabado { get; set; }
    }
}