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

            // Cria plano inicial
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

            // Salva os horários de atendimento do aluno
            if (vm.HorariosAtendimento != null && vm.HorariosAtendimento.Any())
            {
                foreach (var h in vm.HorariosAtendimento.Where(h => h.DiaSemana.HasValue && h.HoraInicio.HasValue && h.HoraFim.HasValue))
                {
                    _context.AlunosHorariosAtendimento.Add(new AlunoHorarioAtendimento
                    {
                        PersonalId = personal.Id,
                        AlunoId = usuario.Id,
                        DiaSemana = h.DiaSemana.Value,
                        HoraInicio = h.HoraInicio.Value,
                        HoraFim = h.HoraFim.Value,
                        Ativo = true,
                        DataCriacao = DateTime.Now
                    });
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

            // Verifica se o aluno pertence ao personal
            var alunoVinculado = await _context.PlanosTreino
                .AnyAsync(p => p.TreinadorId == personal.Id && p.AlunoId == id);
            
            if (!alunoVinculado) return Forbid();

            var aluno = await _userManager.FindByIdAsync(id);
            if (aluno == null) return NotFound();

            // Busca os horários de atendimento do aluno
            var horarios = await _context.AlunosHorariosAtendimento
                .Where(h => h.PersonalId == personal.Id && h.AlunoId == id && h.Ativo)
                .ToListAsync();

            var vm = new EditarAlunoViewModel
            {
                Id = aluno.Id,
                NomeCompleto = aluno.NomeCompleto,
                Email = aluno.Email ?? "",
                Cpf = aluno.Cpf,
                Telefone = aluno.Telefone,
                Ativo = aluno.Ativo,
                HorariosAtendimento = horarios.Select(h => new HorarioAtendimentoEditViewModel
                {
                    Id = h.Id,
                    DiaSemana = h.DiaSemana,
                    HoraInicio = h.HoraInicio,
                    HoraFim = h.HoraFim
                }).ToList()
            };

            // Adiciona linhas vazias para novos horários (máximo 7)
            for (int i = vm.HorariosAtendimento.Count; i < 7; i++)
            {
                vm.HorariosAtendimento.Add(new HorarioAtendimentoEditViewModel());
            }

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

            // Atualiza dados do aluno
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

            // Remove horários marcados para exclusão
            if (vm.HorariosParaRemover != null && vm.HorariosParaRemover.Any())
            {
                var horariosToRemove = await _context.AlunosHorariosAtendimento
                    .Where(h => vm.HorariosParaRemover.Contains(h.Id) && h.PersonalId == personal.Id)
                    .ToListAsync();
                _context.AlunosHorariosAtendimento.RemoveRange(horariosToRemove);
            }

            // Atualiza ou adiciona horários
            foreach (var h in vm.HorariosAtendimento.Where(h => h.DiaSemana.HasValue && h.HoraInicio.HasValue && h.HoraFim.HasValue))
            {
                if (h.Id > 0)
                {
                    // Atualiza horário existente
                    var horarioExistente = await _context.AlunosHorariosAtendimento
                        .FirstOrDefaultAsync(hx => hx.Id == h.Id && hx.PersonalId == personal.Id);
                    if (horarioExistente != null)
                    {
                        horarioExistente.DiaSemana = h.DiaSemana.Value;
                        horarioExistente.HoraInicio = h.HoraInicio.Value;
                        horarioExistente.HoraFim = h.HoraFim.Value;
                        horarioExistente.DataAtualizacao = DateTime.Now;
                    }
                }
                else
                {
                    // Adiciona novo horário
                    _context.AlunosHorariosAtendimento.Add(new AlunoHorarioAtendimento
                    {
                        PersonalId = personal.Id,
                        AlunoId = aluno.Id,
                        DiaSemana = h.DiaSemana.Value,
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
                    if (ex.TempoExecucao.HasValue && ex.TempoExecucao.Value > 0)
                    {
                        tempoSegundos = ex.TempoExecucao.Value * 60;
                    }

                    _context.TreinoExercicios.Add(new TreinoExercicio
                    {
                        TreinoId = treino.Id,
                        ExercicioId = ex.ExercicioId,
                        Ordem = ex.Ordem,
                        SeriesPlanejadas = ex.Series,
                        RepeticoesPlanejadas = ex.Repeticoes ?? "",
                        TempoExecucaoSegundos = tempoSegundos,
                        CargaPlanejada = ex.Carga,
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

        // ── Agenda / Horários de Atendimento ───────────────────────
        public async Task<IActionResult> Agenda(DateTime? data)
        {
            var personal = await _userManager.GetUserAsync(User);
            if (personal == null) return Challenge();

            var dataSelecionada = data ?? DateTime.Today;
            
            var horarios = await _context.HorariosTrabalhoPersonal
                .Where(h => h.PersonalId == personal.Id && h.Ativo)
                .OrderBy(h => h.DiaSemana)
                .ThenBy(h => h.HoraInicio)
                .ToListAsync();

            var agendamentos = await _context.AgendamentosAlunos
                .Include(a => a.Aluno)
                .Where(a => a.PersonalId == personal.Id && a.Status == 1)
                .ToListAsync();

            var alunosIds = await _context.PlanosTreino
                .Where(p => p.TreinadorId == personal.Id)
                .Select(p => p.AlunoId)
                .Distinct()
                .ToListAsync();
            
            var alunos = await _userManager.Users
                .Where(u => alunosIds.Contains(u.Id) && u.Ativo)
                .ToListAsync();

            ViewBag.DataSelecionada = dataSelecionada;
            ViewBag.Horarios = horarios;
            ViewBag.Agendamentos = agendamentos;
            ViewBag.Alunos = alunos;
            
            return View();
        }

        public IActionResult DefinirHorarios()
        {
            return View(new DefinirHorarioViewModel());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DefinirHorarios(DefinirHorarioViewModel vm)
        {
            var personal = await _userManager.GetUserAsync(User);
            if (personal == null) return Challenge();

            if (!ModelState.IsValid) return View(vm);

            var existe = await _context.HorariosTrabalhoPersonal
                .AnyAsync(h => h.PersonalId == personal.Id && h.DiaSemana == vm.DiaSemana);

            if (existe)
            {
                ModelState.AddModelError("", "Já existe um horário definido para este dia da semana.");
                return View(vm);
            }

            var horario = new HorarioTrabalhoPersonal
            {
                PersonalId = personal.Id,
                DiaSemana = vm.DiaSemana,
                HoraInicio = vm.HoraInicio,
                HoraFim = vm.HoraFim,
                Ativo = vm.Ativo,
                DataCriacao = DateTime.Now
            };

            _context.HorariosTrabalhoPersonal.Add(horario);
            await _context.SaveChangesAsync();

            TempData["Sucesso"] = "Horário de atendimento adicionado com sucesso!";
            return RedirectToAction(nameof(Agenda));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoverHorario(int id)
        {
            var personal = await _userManager.GetUserAsync(User);
            if (personal == null) return Challenge();

            var horario = await _context.HorariosTrabalhoPersonal
                .FirstOrDefaultAsync(h => h.Id == id && h.PersonalId == personal.Id);
            
            if (horario == null) return NotFound();

            _context.HorariosTrabalhoPersonal.Remove(horario);
            await _context.SaveChangesAsync();

            TempData["Sucesso"] = "Horário removido com sucesso!";
            return RedirectToAction(nameof(Agenda));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AgendarAula(AgendarAulaViewModel vm)
        {
            var personal = await _userManager.GetUserAsync(User);
            if (personal == null) return Challenge();

            if (!ModelState.IsValid)
            {
                TempData["Erro"] = "Preencha todos os campos corretamente.";
                return RedirectToAction(nameof(Agenda));
            }

            var conflito = await _context.AgendamentosAlunos
                .AnyAsync(a => a.PersonalId == personal.Id && 
                               a.DiaSemana == vm.DataAgendamento.DayOfWeek &&
                               a.Status == 1 &&
                               ((vm.HoraInicio >= a.HoraInicio && vm.HoraInicio < a.HoraFim) ||
                                (vm.HoraFim > a.HoraInicio && vm.HoraFim <= a.HoraFim)));

            if (conflito)
            {
                TempData["Erro"] = "Já existe um agendamento neste horário.";
                return RedirectToAction(nameof(Agenda));
            }

            var agendamento = new AgendamentoAluno
            {
                PersonalId = personal.Id,
                AlunoId = vm.AlunoId,
                HorarioTrabalhoId = 0,
                DiaSemana = vm.DataAgendamento.DayOfWeek,
                HoraInicio = vm.HoraInicio,
                HoraFim = vm.HoraFim,
                Status = 1,
                DataCriacao = DateTime.Now
            };

            _context.AgendamentosAlunos.Add(agendamento);
            await _context.SaveChangesAsync();

            TempData["Sucesso"] = $"Aula agendada com sucesso para {vm.DataAgendamento:dd/MM/yyyy} às {vm.HoraInicio:hh\\:mm}";
            return RedirectToAction(nameof(Agenda));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelarAgendamento(int id)
        {
            var personal = await _userManager.GetUserAsync(User);
            if (personal == null) return Challenge();

            var agendamento = await _context.AgendamentosAlunos
                .FirstOrDefaultAsync(a => a.Id == id && a.PersonalId == personal.Id);
            
            if (agendamento == null) return NotFound();

            agendamento.Status = 0;
            agendamento.DataAtualizacao = DateTime.Now;
            await _context.SaveChangesAsync();

            TempData["Sucesso"] = "Agendamento cancelado com sucesso!";
            return RedirectToAction(nameof(Agenda));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ConcluirAgendamento(int id)
        {
            var personal = await _userManager.GetUserAsync(User);
            if (personal == null) return Challenge();

            var agendamento = await _context.AgendamentosAlunos
                .FirstOrDefaultAsync(a => a.Id == id && a.PersonalId == personal.Id);
            
            if (agendamento == null) return NotFound();

            agendamento.Status = 2;
            agendamento.DataAtualizacao = DateTime.Now;
            await _context.SaveChangesAsync();

            TempData["Sucesso"] = "Atendimento concluído!";
            return RedirectToAction(nameof(Agenda));
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

            ViewBag.Alunos = new SelectList(alunos, "Id", "NomeCompleto", alunoIdSel);
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