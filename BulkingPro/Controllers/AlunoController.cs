using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BulkingPro.Data;
using BulkingPro.Models;
using BulkingPro.ViewModels;

namespace BulkingPro.Controllers;

[Authorize(Roles = "Usuario")]
public class AlunoController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<Usuario> _userManager;

    public AlunoController(ApplicationDbContext context, UserManager<Usuario> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // GET: /Aluno/Index
    public async Task<IActionResult> Index()
    {
        var aluno = await _userManager.GetUserAsync(User);
        if (aluno == null) return Challenge();

        // Busca o plano ativo do aluno
        var planoAtivo = await _context.PlanosTreino
            .Include(p => p.Treinos)
                .ThenInclude(t => t.TreinoExercicios)
                    .ThenInclude(te => te.Exercicio)
            .FirstOrDefaultAsync(p => p.AlunoId == aluno.Id && p.Status == 1);

        // --- Evolução de peso ---
        var avaliacoes = await _context.AvaliacoesFisicas
            .Where(a => a.AlunoId == aluno.Id)
            .OrderBy(a => a.DataAvaliacao)
            .ToListAsync();

        var evolucaoPeso = avaliacoes
            .Where(a => a.Peso.HasValue)
            .Select(a => new PesoEvolucao
            {
                Data = a.DataAvaliacao,
                Peso = a.Peso.Value
            }).ToList();

        var pesoInicial = evolucaoPeso.FirstOrDefault()?.Peso;
        var pesoAtual = evolucaoPeso.LastOrDefault()?.Peso;
        decimal? evolucaoPesoKg = (pesoAtual.HasValue && pesoInicial.HasValue) 
            ? pesoAtual.Value - pesoInicial.Value 
            : (decimal?)null;

        // --- Aumento de carga nos exercícios (histórico das execuções) ---
        var cargasEvolucao = new List<CargaEvolucao>();
        
        if (planoAtivo != null)
        {
            var exerciciosComCarga = planoAtivo.Treinos
                .SelectMany(t => t.TreinoExercicios)
                .Where(te => te.CargaPlanejada.HasValue && te.CargaPlanejada > 0)
                .Select(te => te.ExercicioId)
                .Distinct()
                .ToList();

            foreach (var exercicioId in exerciciosComCarga)
            {
                var execucoes = await _context.ExecucoesTreinoExercicios
                    .Include(ete => ete.ExecucaoTreino)
                    .Where(ete => ete.TreinoExercicio.ExercicioId == exercicioId 
                        && ete.ExecucaoTreino.AlunoId == aluno.Id
                        && ete.CargaUsada.HasValue)
                    .OrderBy(ete => ete.ExecucaoTreino.DataExecucao)
                    .ToListAsync();

                if (execucoes.Any())
                {
                    var primeiraCarga = execucoes.First().CargaUsada ?? 0;
                    var ultimaCarga = execucoes.Last().CargaUsada ?? 0;
                    
                    var exercicioNome = await _context.Exercicios
                        .Where(e => e.Id == exercicioId)
                        .Select(e => e.Nome)
                        .FirstOrDefaultAsync();

                    cargasEvolucao.Add(new CargaEvolucao
                    {
                        ExercicioNome = exercicioNome ?? "Exercício",
                        CargaInicial = primeiraCarga,
                        CargaAtual = ultimaCarga
                    });
                }
            }
        }

        // --- Constância nos treinos (últimos 30 dias) ---
        var treinosRealizados = await _context.ExecucoesTreino
            .Where(e => e.AlunoId == aluno.Id && e.Concluido)
            .Select(e => e.DataExecucao.Date)
            .ToListAsync();

        var treinosSemana = new List<TreinoSemana>();
        var hoje = DateTime.Today;
        var inicioSemana = hoje.AddDays(-(int)hoje.DayOfWeek + 1); // Segunda-feira

        for (int i = 0; i < 7; i++)
        {
            var dia = inicioSemana.AddDays(i);
            treinosSemana.Add(new TreinoSemana
            {
                Dia = dia.DayOfWeek,
                Nome = dia.ToString("dddd", new System.Globalization.CultureInfo("pt-BR")),
                Realizado = treinosRealizados.Contains(dia),
                DataRealizacao = treinosRealizados.Contains(dia) ? dia : null
            });
        }

        // Dias consecutivos de treino
        var diasConsecutivos = 0;
        var dataCheck = DateTime.Today;
        while (treinosRealizados.Contains(dataCheck))
        {
            diasConsecutivos++;
            dataCheck = dataCheck.AddDays(-1);
        }

        var vm = new AlunoDashboardViewModel
        {
            TotalTreinosPrevistos = planoAtivo?.Treinos.Sum(t => t.TreinoExercicios.Count) ?? 0,
            TotalTreinosRealizados = await _context.ExecucoesTreinoExercicios
                .Include(ete => ete.ExecucaoTreino)
                .Where(ete => ete.ExecucaoTreino.AlunoId == aluno.Id && ete.Concluido)
                .CountAsync(),
            DiasConsecutivos = diasConsecutivos,
            EvolucaoPeso = evolucaoPesoKg,
            AumentoCargaTotal = cargasEvolucao.Sum(c => (int)(c.CargaAtual - c.CargaInicial)),
            EvolucaoPesoData = evolucaoPeso,
            EvolucaoCargaData = cargasEvolucao,
            TreinosSemana = treinosSemana
        };

        return View(vm);
    }

    // ═══════════════════════════════════════════════════════════════
    // GET: /Aluno/Treinos (VERSÃO ATUALIZADA COM HISTÓRICO)
    // ═══════════════════════════════════════════════════════════════
    public async Task<IActionResult> Treinos(string? planoId)
    {
        var aluno = await _userManager.GetUserAsync(User);
        if (aluno == null) return Challenge();

        var vm = new TreinosAlunoViewModel();
        vm.DataReferencia = DateTime.Today;

        // ── Buscar TODOS os planos do aluno (ordenados por data) ──
        var todosPlanos = await _context.PlanosTreino
            .Include(p => p.Treinos)
                .ThenInclude(t => t.TreinoExercicios)
                    .ThenInclude(te => te.Exercicio)
                        .ThenInclude(e => e.GrupoMuscular)
            .Where(p => p.AlunoId == aluno.Id)
            .OrderByDescending(p => p.DataInicio)
            .ToListAsync();

        if (!todosPlanos.Any())
        {
            vm.Mensagem = "Você ainda não possui nenhum plano de treino. Aguarde seu personal trainer montar um plano para você!";
            return View(vm);
        }

        // ── Popular lista de planos disponíveis ──
        foreach (var plano in todosPlanos)
        {
            vm.PlanosDisponiveis.Add(new PlanoPeriodoViewModel
            {
                Id = plano.Id.ToString(),
                Titulo = plano.Titulo,
                DataInicio = plano.DataInicio,
                DataFim = plano.DataFim,
                IsAtivo = plano.Status == 1
            });
        }

        // ── Determinar qual plano exibir ──
        PlanoTreino? planoSelecionado = null;

        // Se o usuário selecionou um plano específico via query string
        if (!string.IsNullOrEmpty(planoId) && int.TryParse(planoId, out int id))
        {
            planoSelecionado = todosPlanos.FirstOrDefault(p => p.Id == id);
        }

        // Se não selecionou ou o ID é inválido, pega o plano ativo (Status = 1)
        if (planoSelecionado == null)
        {
            planoSelecionado = todosPlanos.FirstOrDefault(p => p.Status == 1);

            // Se não tem plano ativo, pega o mais recente
            if (planoSelecionado == null)
            {
                planoSelecionado = todosPlanos.FirstOrDefault();
            }
        }

        if (planoSelecionado == null)
        {
            vm.Mensagem = "Nenhum plano disponível para visualização.";
            return View(vm);
        }

        // ── Atualizar ViewModel com o plano selecionado ──
        vm.PlanoSelecionadoId = planoSelecionado.Id.ToString();
        vm.PlanoAtual = vm.PlanosDisponiveis.FirstOrDefault(p => p.Id == vm.PlanoSelecionadoId);
        vm.TemPlanoAtivo = planoSelecionado.Status == 1;

        // ── Buscar execuções do aluno para saber o que foi realizado ──
        var execucoes = await _context.ExecucoesTreinoExercicios
            .Include(e => e.ExecucaoTreino)
            .Where(e => e.ExecucaoTreino.AlunoId == aluno.Id && e.Concluido)
            .Select(e => new { e.TreinoExercicioId, e.ExecucaoTreino.DataExecucao })
            .ToListAsync();

        // ── Buscar comentários do aluno ──
        var comentarios = await _context.ComentariosTreino
            .Where(c => c.AlunoId == aluno.Id)
            .ToDictionaryAsync(c => c.TreinoExercicioId, c => c.Comentario);

        // ── Mapear dias da semana com datas reais ──
        var hoje = DateTime.Today;
        var diaSemanaHoje = (int)hoje.DayOfWeek;
        if (diaSemanaHoje == 0) diaSemanaHoje = 7; // Domingo = 7

        var diasTreino = new List<DiaTreinoComDataViewModel>();

        foreach (var treino in planoSelecionado.Treinos.OrderBy(t => t.OrdemDia))
        {
            // Calcula a data real deste dia da semana na semana atual
            var ordemDia = treino.OrdemDia;
            if (ordemDia == 7) ordemDia = 0; // Converter Domingo para 0 para DayOfWeek

            var diaSemana = (DayOfWeek)ordemDia;
            var dataReferencia = hoje.AddDays(-(diaSemanaHoje - (int)diaSemana));

            // Se o dia da semana for Domingo (0) e hoje for Domingo (0), ajusta
            if (diaSemana == DayOfWeek.Sunday && hoje.DayOfWeek == DayOfWeek.Sunday)
            {
                dataReferencia = hoje;
            }

            var isHoje = dataReferencia.Date == hoje.Date;

            var exercicios = new List<ExercicioTreinoAlunoViewModel>();

            foreach (var te in treino.TreinoExercicios.OrderBy(te => te.Ordem))
            {
                // Verifica se o exercício foi concluído
                var foiExecutado = execucoes.Any(e => e.TreinoExercicioId == te.Id);

                // Verifica se tem comentário
                comentarios.TryGetValue(te.Id, out string? comentario);

                // Formata reps ou tempo
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

                exercicios.Add(new ExercicioTreinoAlunoViewModel
                {
                    TreinoExercicioId = te.Id,
                    Ordem = te.Ordem,
                    NomeExercicio = te.Exercicio?.Nome ?? "—",
                    GrupoMuscular = te.Exercicio?.GrupoMuscular?.Nome ?? "—",
                    Series = te.SeriesPlanejadas,
                    RepeticoesOuTempo = repsOuTempo,
                    Carga = te.CargaPlanejada,
                    Descanso = te.TempoDescanso,
                    Observacoes = te.Observacoes,
                    MeuComentario = comentario,
                    JaComentou = !string.IsNullOrEmpty(comentario),
                    Concluido = foiExecutado
                });
            }

            // Verifica se o treino foi realizado (pelo menos um exercício concluído)
            var treinoRealizado = exercicios.Any(e => e.Concluido);
            var dataRealizacao = treinoRealizado ? hoje : (DateTime?)null;

            // Se o treino foi realizado, buscar a data real da execução
            if (treinoRealizado)
            {
                var primeiraExecucao = execucoes
                    .Where(e => treino.TreinoExercicios.Select(te => te.Id).Contains(e.TreinoExercicioId))
                    .OrderBy(e => e.DataExecucao)
                    .FirstOrDefault();
                if (primeiraExecucao != null)
                {
                    dataRealizacao = primeiraExecucao.DataExecucao;
                }
            }

            diasTreino.Add(new DiaTreinoComDataViewModel
            {
                TreinoId = treino.Id,
                Nome = treino.Nome,
                OrdemDia = treino.OrdemDia,
                DiaSemana = diaSemana,
                DataReferencia = dataReferencia,
                Hoje = isHoje,
                Realizado = treinoRealizado,
                DataRealizacao = dataRealizacao,
                Exercicios = exercicios
            });
        }

        vm.DiasTreino = diasTreino;

        return View(vm);
    }

    // POST: /Aluno/EnviarComentario
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EnviarComentario([FromBody] ComentarioEnviarViewModel vm)
    {
        var aluno = await _userManager.GetUserAsync(User);
        if (aluno == null) return Unauthorized();

        var comentario = await _context.ComentariosTreino
            .FirstOrDefaultAsync(c => c.TreinoExercicioId == vm.TreinoExercicioId && c.AlunoId == aluno.Id);

        if (comentario != null)
        {
            comentario.Comentario = vm.Comentario;
            comentario.Lido = false;
            comentario.DataCriacao = DateTime.Now;
        }
        else
        {
            comentario = new ComentarioTreino
            {
                TreinoExercicioId = vm.TreinoExercicioId,
                AlunoId = aluno.Id,
                Comentario = vm.Comentario,
                Lido = false,
                DataCriacao = DateTime.Now
            };
            _context.ComentariosTreino.Add(comentario);
        }

        await _context.SaveChangesAsync();
        return Json(new { sucesso = true });
    }

    // GET: /Aluno/Medidas
    public async Task<IActionResult> Medidas()
    {
        var aluno = await _userManager.GetUserAsync(User);
        if (aluno == null) return Challenge();

        var avaliacoes = await _context.AvaliacoesFisicas
            .Where(a => a.AlunoId == aluno.Id)
            .OrderByDescending(a => a.DataAvaliacao)
            .ToListAsync();

        return View(avaliacoes);
    }

    // GET: /Aluno/Anamnese
    public async Task<IActionResult> Anamnese()
    {
        var aluno = await _userManager.GetUserAsync(User);
        if (aluno == null) return Challenge();

        var anamnese = await _context.Anamneses
            .Where(a => a.AlunoId == aluno.Id)
            .OrderByDescending(a => a.DataAvaliacao)
            .FirstOrDefaultAsync();

        return View(anamnese);
    }

    // POST: /Aluno/MarcarTreinoRealizado
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarcarTreinoRealizado([FromBody] ExecucaoTreinoViewModel vm)
    {
        var aluno = await _userManager.GetUserAsync(User);
        if (aluno == null) return Unauthorized();

        // Carrega o treino com os exercícios e suas informações
        var treino = await _context.Treinos
            .Include(t => t.TreinoExercicios)
                .ThenInclude(te => te.Exercicio)
            .FirstOrDefaultAsync(t => t.Id == vm.TreinoId);

        if (treino == null) return NotFound();

        // Verifica se já registrou hoje
        var jaRegistrou = await _context.ExecucoesTreino
            .AnyAsync(e => e.TreinoId == vm.TreinoId 
                && e.AlunoId == aluno.Id 
                && e.DataExecucao.Date == DateTime.Today);

        if (jaRegistrou)
            return Json(new { sucesso = false, erro = "Você já registrou este treino hoje!" });

        var execucao = new ExecucaoTreino
        {
            TreinoId = vm.TreinoId,
            AlunoId = aluno.Id,
            DataExecucao = DateTime.Now,
            Concluido = true,
            DataCriacao = DateTime.Now,
            ObservacoesGerais = "Registrado automaticamente pelo aluno"
        };
        _context.ExecucoesTreino.Add(execucao);
        await _context.SaveChangesAsync();

        // Registrar cada exercício baseado no que foi salvo (tempo ou repetições)
        foreach (var te in treino.TreinoExercicios)
        {
            string repeticoesFeitas;
            
            // Se tem tempo salvo, registra como tempo
            if (te.TempoExecucaoSegundos.HasValue && te.TempoExecucaoSegundos.Value > 0)
            {
                var minutos = te.TempoExecucaoSegundos.Value / 60;
                var segundos = te.TempoExecucaoSegundos.Value % 60;
                repeticoesFeitas = minutos > 0 ? $"{minutos}min" : $"{segundos}s";
            }
            else
            {
                // Senão, usa as repetições
                repeticoesFeitas = string.IsNullOrEmpty(te.RepeticoesPlanejadas) ? "12" : te.RepeticoesPlanejadas;
            }

            _context.ExecucoesTreinoExercicios.Add(new ExecucaoTreinoExercicio
            {
                ExecucaoTreinoId = execucao.Id,
                TreinoExercicioId = te.Id,
                SeriesFeitas = te.SeriesPlanejadas,
                RepeticoesFeitas = repeticoesFeitas,
                CargaUsada = te.CargaPlanejada,
                Concluido = true,
                Observacoes = ""
            });
        }
        await _context.SaveChangesAsync();

        return Json(new { sucesso = true });
    }
}