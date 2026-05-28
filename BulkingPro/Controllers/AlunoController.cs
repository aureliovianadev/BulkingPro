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

    // GET: /Aluno/Treinos
    public async Task<IActionResult> Treinos()
    {
        var aluno = await _userManager.GetUserAsync(User);
        if (aluno == null) return Challenge();

        var planoAtivo = await _context.PlanosTreino
            .Include(p => p.Treinos)
                .ThenInclude(t => t.TreinoExercicios)
                    .ThenInclude(te => te.Exercicio)
                        .ThenInclude(e => e.GrupoMuscular)
            .FirstOrDefaultAsync(p => p.AlunoId == aluno.Id && p.Status == 1);

        if (planoAtivo == null)
        {
            ViewBag.Mensagem = "Você ainda não possui um plano de treino ativo. Aguarde seu personal trainer montar um plano para você!";
            return View(new TreinoAlunoViewModel { DiasTreino = new List<DiaTreinoAlunoViewModel>() });
        }

        // Buscar comentários do aluno
        var comentarios = await _context.ComentariosTreino
            .Where(c => c.AlunoId == aluno.Id)
            .ToDictionaryAsync(c => c.TreinoExercicioId, c => c.Comentario);

        // Buscar execuções realizadas
        var execucoesRealizadas = await _context.ExecucoesTreinoExercicios
            .Include(ete => ete.ExecucaoTreino)
            .Where(ete => ete.ExecucaoTreino.AlunoId == aluno.Id && ete.Concluido)
            .Select(ete => new { ete.TreinoExercicioId, ete.ExecucaoTreino.DataExecucao })
            .ToListAsync();

        var hoje = DateTime.Today;
        var diaSemanaHoje = (int)hoje.DayOfWeek;
        
        // Mapeamento: para cada treino, verificar se foi realizado hoje
        var treinosRealizadosHoje = execucoesRealizadas
            .Where(e => e.DataExecucao.Date == hoje)
            .Select(e => e.TreinoExercicioId)
            .Distinct()
            .ToList();

        var diasTreino = new List<DiaTreinoAlunoViewModel>();

        foreach (var treino in planoAtivo.Treinos.OrderBy(t => t.OrdemDia))
        {
            // Associa o dia da semana baseado na ordem (1 = Segunda, 7 = Domingo)
            var diaSemanaTreino = (DayOfWeek)((treino.OrdemDia % 7));
            var isHoje = (int)diaSemanaTreino == diaSemanaHoje;

            var exercicios = new List<ExercicioTreinoAlunoViewModel>();
            foreach (var te in treino.TreinoExercicios.OrderBy(te => te.Ordem))
            {
                // ⭐ CORREÇÃO: Exibe baseado no que foi SALVO, não no tipo do exercício
                string repsOuTempo;
                
                // Se tem tempo salvo (em segundos), exibe como tempo
                if (te.TempoExecucaoSegundos.HasValue && te.TempoExecucaoSegundos.Value > 0)
                {
                    var minutos = te.TempoExecucaoSegundos.Value / 60;
                    var segundos = te.TempoExecucaoSegundos.Value % 60;
                    repsOuTempo = minutos > 0 ? $"{minutos}min {segundos}s" : $"{segundos}s";
                }
                // Senão, exibe as repetições
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
                    MeuComentario = comentarios.GetValueOrDefault(te.Id),
                    JaComentou = comentarios.ContainsKey(te.Id)
                });
            }

            diasTreino.Add(new DiaTreinoAlunoViewModel
            {
                TreinoId = treino.Id,
                Nome = treino.Nome,
                OrdemDia = treino.OrdemDia,
                DiaSemana = diaSemanaTreino,
                Hoje = isHoje,
                Realizado = false,
                Exercicios = exercicios
            });
        }

        var vm = new TreinoAlunoViewModel
        {
            PlanoAtivo = planoAtivo,
            DiasTreino = diasTreino,
            DataAtual = hoje
        };

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
            
            // ⭐ Se tem tempo salvo, registra como tempo
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