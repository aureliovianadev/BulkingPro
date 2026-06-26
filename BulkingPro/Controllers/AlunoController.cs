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

        var planoAtivo = await _context.PlanosTreino
            .Include(p => p.Treinos)
                .ThenInclude(t => t.TreinoExercicios)
                    .ThenInclude(te => te.Exercicio)
            .FirstOrDefaultAsync(p => p.AlunoId == aluno.Id && p.Status == 1);

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

        var treinosRealizados = await _context.ExecucoesTreino
            .Where(e => e.AlunoId == aluno.Id && e.Concluido)
            .Select(e => e.DataExecucao.Date)
            .ToListAsync();

        var treinosSemana = new List<TreinoSemana>();
        var hoje = DateTime.Today;
        var inicioSemana = hoje.AddDays(-(int)hoje.DayOfWeek + 1);

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
    public async Task<IActionResult> Treinos(string? planoId)
    {
        var aluno = await _userManager.GetUserAsync(User);
        if (aluno == null) return Challenge();

        var vm = new TreinosAlunoViewModel();
        vm.DataReferencia = DateTime.Today;

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

        PlanoTreino? planoSelecionado = null;

        if (!string.IsNullOrEmpty(planoId) && int.TryParse(planoId, out int id))
        {
            planoSelecionado = todosPlanos.FirstOrDefault(p => p.Id == id);
        }

        if (planoSelecionado == null)
        {
            planoSelecionado = todosPlanos.FirstOrDefault(p => p.Status == 1);
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

        vm.PlanoSelecionadoId = planoSelecionado.Id.ToString();
        vm.PlanoAtual = vm.PlanosDisponiveis.FirstOrDefault(p => p.Id == vm.PlanoSelecionadoId);
        vm.TemPlanoAtivo = planoSelecionado.Status == 1;

        var hoje = DateTime.Today;
        var dataInicioPlano = planoSelecionado.DataInicio;
        var dataFimPlano = planoSelecionado.DataFim ?? DateTime.MaxValue;

        // Buscar execuções do aluno
        var todasExecucoes = await _context.ExecucoesTreinoExercicios
            .Include(e => e.ExecucaoTreino)
            .Where(e => e.ExecucaoTreino.AlunoId == aluno.Id && e.Concluido)
            .ToListAsync();

        // Filtrar execuções dentro do período do plano
        var execucoes = todasExecucoes
            .Where(e => e.ExecucaoTreino.DataExecucao.Date <= hoje
                && e.ExecucaoTreino.DataExecucao.Date >= dataInicioPlano.Date
                && e.ExecucaoTreino.DataExecucao.Date <= dataFimPlano.Date)
            .Select(e => new { e.TreinoExercicioId, e.ExecucaoTreino.DataExecucao })
            .ToList();

        var comentarios = await _context.ComentariosTreino
            .Where(c => c.AlunoId == aluno.Id)
            .ToDictionaryAsync(c => c.TreinoExercicioId, c => c.Comentario);

        var diasTreino = new List<DiaTreinoComDataViewModel>();

        // Para cada dia de treino do plano, calcular a data correta
        foreach (var treino in planoSelecionado.Treinos.OrderBy(t => t.OrdemDia))
        {
            var ordemDia = treino.OrdemDia;
            var diaSemana = ordemDia == 7 ? DayOfWeek.Sunday : (DayOfWeek)ordemDia;

            // Encontrar a PRIMEIRA ocorrência deste dia da semana
            // dentro do período do plano (a partir da data de início)
            var primeiraData = dataInicioPlano;
            
            // Avançar até encontrar o dia da semana correto
            while (primeiraData.DayOfWeek != diaSemana)
            {
                primeiraData = primeiraData.AddDays(1);
            }

            // Se a primeira data encontrada for anterior à data de início,
            // avançar uma semana (isso não deve acontecer, mas garantia)
            if (primeiraData < dataInicioPlano)
            {
                primeiraData = primeiraData.AddDays(7);
            }

            // Agora percorrer TODAS as semanas do plano a partir da primeira data
            var dataReferencia = primeiraData;
            while (dataReferencia <= dataFimPlano)
            {
                var isHoje = dataReferencia.Date == hoje.Date;
                var isDataFutura = dataReferencia.Date > hoje.Date;

                var exercicios = new List<ExercicioTreinoAlunoViewModel>();

                foreach (var te in treino.TreinoExercicios.OrderBy(te => te.Ordem))
                {
                    var foiExecutado = execucoes.Any(e => e.TreinoExercicioId == te.Id 
                        && e.DataExecucao.Date == dataReferencia.Date);

                    comentarios.TryGetValue(te.Id, out string? comentario);

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
                        Concluido = foiExecutado && !isDataFutura
                    });
                }

                var treinoRealizado = exercicios.Any(e => e.Concluido);
                DateTime? dataRealizacao = null;

                if (treinoRealizado)
                {
                    var primeiraExecucao = execucoes
                        .Where(e => treino.TreinoExercicios.Select(te => te.Id).Contains(e.TreinoExercicioId)
                            && e.DataExecucao.Date == dataReferencia.Date)
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

                // Avançar para a próxima semana (mesmo dia da semana)
                dataReferencia = dataReferencia.AddDays(7);
            }
        }

        // Ordenar os dias por data
        vm.DiasTreino = diasTreino.OrderBy(d => d.DataReferencia).ToList();

        return View(vm);
    }

    // ── Desmarcar treino realizado ──
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DesmarcarTreinoRealizado([FromBody] ExecucaoTreinoViewModel vm)
    {
        try
        {
            var aluno = await _userManager.GetUserAsync(User);
            if (aluno == null) return Unauthorized();

            var hoje = DateTime.Today;
            var inicioDoDia = hoje.Date;
            var fimDoDia = hoje.Date.AddDays(1).AddTicks(-1);
            
            var execucao = await _context.ExecucoesTreino
                .Include(e => e.Exercicios)
                .FirstOrDefaultAsync(e => e.TreinoId == vm.TreinoId 
                    && e.AlunoId == aluno.Id 
                    && e.DataExecucao >= inicioDoDia
                    && e.DataExecucao <= fimDoDia);

            if (execucao == null)
            {
                return Json(new { sucesso = false, erro = "Nenhum treino encontrado para desmarcar hoje." });
            }

            _context.ExecucoesTreinoExercicios.RemoveRange(execucao.Exercicios);
            _context.ExecucoesTreino.Remove(execucao);
            
            await _context.SaveChangesAsync();

            return Json(new { sucesso = true, mensagem = "Treino desmarcado com sucesso!" });
        }
        catch (Exception ex)
        {
            return Json(new { sucesso = false, erro = "Erro ao desmarcar treino: " + ex.Message });
        }
    }

    // ── Marcar treino realizado ──
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarcarTreinoRealizado([FromBody] ExecucaoTreinoViewModel vm)
    {
        try
        {
            var aluno = await _userManager.GetUserAsync(User);
            if (aluno == null) return Unauthorized();

            var treino = await _context.Treinos
                .Include(t => t.TreinoExercicios)
                    .ThenInclude(te => te.Exercicio)
                .FirstOrDefaultAsync(t => t.Id == vm.TreinoId);

            if (treino == null)
            {
                return Json(new { sucesso = false, erro = "Treino não encontrado." });
            }

            var hoje = DateTime.Today;
            var inicioDoDia = hoje.Date;
            var fimDoDia = hoje.Date.AddDays(1).AddTicks(-1);

            var jaRegistrou = await _context.ExecucoesTreino
                .AnyAsync(e => e.TreinoId == vm.TreinoId 
                    && e.AlunoId == aluno.Id 
                    && e.DataExecucao >= inicioDoDia
                    && e.DataExecucao <= fimDoDia);

            if (jaRegistrou)
            {
                return Json(new { sucesso = false, erro = "Você já registrou este treino hoje!" });
            }

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

            foreach (var te in treino.TreinoExercicios)
            {
                string repeticoesFeitas;
                
                if (te.TempoExecucaoSegundos.HasValue && te.TempoExecucaoSegundos.Value > 0)
                {
                    var minutos = te.TempoExecucaoSegundos.Value / 60;
                    var segundos = te.TempoExecucaoSegundos.Value % 60;
                    repeticoesFeitas = minutos > 0 ? $"{minutos}min" : $"{segundos}s";
                }
                else
                {
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

            return Json(new { sucesso = true, mensagem = "Treino marcado como realizado!" });
        }
        catch (Exception ex)
        {
            return Json(new { sucesso = false, erro = "Erro ao marcar treino: " + ex.Message });
        }
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
}