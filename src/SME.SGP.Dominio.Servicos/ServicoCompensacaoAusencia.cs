﻿using MediatR;
using SME.SGP.Aplicacao;
using SME.SGP.Aplicacao.Integracoes;
using SME.SGP.Dominio.Interfaces;
using SME.SGP.Infra;
using SME.SGP.Infra.Dtos;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using SME.SGP.Dominio.Constantes.MensagensNegocio;
using SME.SGP.Dominio.Enumerados;
using SME.SGP.Infra.Utilitarios;

namespace SME.SGP.Dominio.Servicos
{
    public class ServicoCompensacaoAusencia : IServicoCompensacaoAusencia
    {
        private readonly IRepositorioCompensacaoAusencia repositorioCompensacaoAusencia;
        private readonly IRepositorioCompensacaoAusenciaAluno repositorioCompensacaoAusenciaAluno;
        private readonly IRepositorioCompensacaoAusenciaDisciplinaRegencia repositorioCompensacaoAusenciaDisciplinaRegencia;
        private readonly IConsultasPeriodoEscolar consultasPeriodoEscolar;
        private readonly IRepositorioTurmaConsulta repositorioTurmaConsulta;
        private readonly IRepositorioTipoCalendarioConsulta repositorioTipoCalendario;
        private readonly IRepositorioNotificacaoCompensacaoAusencia repositorioNotificacaoCompensacaoAusencia;
        private readonly IConsultasDisciplina consultasDisciplina;
        private readonly IUnitOfWork unitOfWork;
        private readonly IRepositorioComponenteCurricularConsulta repositorioComponenteCurricular;
        private readonly IMediator mediator;
        private readonly IOptions<ConfiguracaoArmazenamentoOptions> configuracaoArmazenamentoOptions;

        public ServicoCompensacaoAusencia(IRepositorioCompensacaoAusencia repositorioCompensacaoAusencia,
            IRepositorioCompensacaoAusenciaAluno repositorioCompensacaoAusenciaAluno,
            IRepositorioCompensacaoAusenciaDisciplinaRegencia repositorioCompensacaoAusenciaDisciplinaRegencia,
            IConsultasPeriodoEscolar consultasPeriodoEscolar,
            IRepositorioTipoCalendarioConsulta repositorioTipoCalendario,
            IRepositorioTurmaConsulta repositorioTurmaConsulta,
            IRepositorioComponenteCurricularConsulta repositorioComponenteCurricular,
            IRepositorioNotificacaoCompensacaoAusencia repositorioNotificacaoCompensacaoAusencia,
            IConsultasDisciplina consultasDisciplina,
            IUnitOfWork unitOfWork,
            IMediator mediator,
            IOptions<ConfiguracaoArmazenamentoOptions> configuracaoArmazenamentoOptions)
        {
            this.repositorioCompensacaoAusencia = repositorioCompensacaoAusencia ?? throw new System.ArgumentNullException(nameof(repositorioCompensacaoAusencia));
            this.repositorioCompensacaoAusenciaAluno = repositorioCompensacaoAusenciaAluno ?? throw new System.ArgumentNullException(nameof(repositorioCompensacaoAusenciaAluno));
            this.repositorioCompensacaoAusenciaDisciplinaRegencia = repositorioCompensacaoAusenciaDisciplinaRegencia ?? throw new System.ArgumentNullException(nameof(repositorioCompensacaoAusenciaDisciplinaRegencia));
            this.consultasPeriodoEscolar = consultasPeriodoEscolar ?? throw new System.ArgumentNullException(nameof(consultasPeriodoEscolar));
            this.repositorioTipoCalendario = repositorioTipoCalendario ?? throw new System.ArgumentNullException(nameof(repositorioTipoCalendario));
            this.repositorioTurmaConsulta = repositorioTurmaConsulta ?? throw new System.ArgumentNullException(nameof(repositorioTurmaConsulta));
            this.repositorioNotificacaoCompensacaoAusencia = repositorioNotificacaoCompensacaoAusencia ?? throw new System.ArgumentNullException(nameof(repositorioNotificacaoCompensacaoAusencia));
            this.consultasDisciplina = consultasDisciplina ?? throw new ArgumentNullException(nameof(consultasDisciplina));
            this.unitOfWork = unitOfWork ?? throw new System.ArgumentNullException(nameof(unitOfWork));
            this.mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            this.repositorioComponenteCurricular = repositorioComponenteCurricular ?? throw new System.ArgumentNullException(nameof(repositorioComponenteCurricular));
            this.configuracaoArmazenamentoOptions = configuracaoArmazenamentoOptions ?? throw new ArgumentNullException(nameof(configuracaoArmazenamentoOptions));
        }

        public async Task Salvar(long id, CompensacaoAusenciaDto compensacaoDto)
        {
            try
            {
                // Busca dados da turma
                var turma = await BuscaTurma(compensacaoDto.TurmaId);

                // Consiste periodo
                var periodo = await BuscaPeriodo(turma, compensacaoDto.Bimestre);

                var usuario = await mediator.Send(new ObterUsuarioLogadoQuery());

                if (!usuario.EhGestorEscolar())
                    await ValidaProfessorPodePersistirTurma(compensacaoDto.TurmaId, usuario, periodo.PeriodoFim);

                // Valida mesma compensação no ano
                var compensacaoExistente = await mediator.Send(new ObterCompensacaoAusenciaPorAnoTurmaENomeQuery(turma.AnoLetivo, turma.Id, compensacaoDto.Atividade, id));

                if (compensacaoExistente != null)
                    throw new NegocioException($"Já existe essa compensação cadastrada para turma no ano letivo.");

                var compensacaoBanco = new CompensacaoAusencia();

                if (id > 0)
                    compensacaoBanco = await repositorioCompensacaoAusencia.ObterPorIdAsync(id);
                
                var permiteRegistroFrequencia = await mediator.Send(
                    new ObterComponenteRegistraFrequenciaQuery(long.Parse(compensacaoDto.DisciplinaId)));

                if (!permiteRegistroFrequencia)
                    throw new NegocioException(MensagemNegocioCompensacaoAusencia.COMPONENTE_CURRICULAR_NAO_PERMITE_REGISTRAR_FREQUENCIA);

                // Carrega dasdos da disciplina no EOL
                await ConsisteDisciplina(long.Parse(compensacaoDto.DisciplinaId), compensacaoDto.DisciplinasRegenciaIds, compensacaoBanco.Migrado);

                var descricaoAtual = compensacaoBanco.Descricao;

                // Persiste os dados
                var compensacao = MapearEntidade(compensacaoDto, compensacaoBanco);
                compensacao.TurmaId = turma.Id;
                compensacao.AnoLetivo = turma.AnoLetivo;
                
                unitOfWork.IniciarTransacao();
                
                await repositorioCompensacaoAusencia.SalvarAsync(compensacao);
                await GravarDisciplinasRegencia(id > 0, compensacao.Id, compensacaoDto.DisciplinasRegenciaIds, usuario);
                
                var codigosAlunosCompensacao = await GravarCompensacaoAlunos(id > 0, compensacao.Id, compensacaoDto.TurmaId, compensacaoDto.DisciplinaId, compensacaoDto.Alunos, periodo, usuario);
                
                unitOfWork.PersistirTransacao();
                
                await MoverRemoverExcluidos(compensacaoDto.Descricao, descricaoAtual);

                if (codigosAlunosCompensacao.Any())
                    await mediator.Send(new IncluirFilaCalcularFrequenciaPorTurmaCommand(codigosAlunosCompensacao, periodo.PeriodoFim, compensacaoDto.TurmaId, compensacaoDto.DisciplinaId));

                await mediator.Send(new PublicarFilaSgpCommand(RotasRabbitSgp.NotificarCompensacaoAusencia, new FiltroNotificacaoCompensacaoAusenciaDto(compensacao.Id), Guid.NewGuid(), usuario));
            }
            catch (Exception ex)
            {
                unitOfWork.Rollback();
                await mediator.Send(new SalvarLogViaRabbitCommand("Não foi possível salvar a compensação de ausência", LogNivel.Critico, LogContexto.Geral, ex.Message, rastreamento: ex.StackTrace, excecaoInterna: ex.InnerException?.ToString()));
                throw;
            }
        }

        private async Task MoverRemoverExcluidos(string novo, string atual)
        {
            if (!string.IsNullOrEmpty(novo))
                await mediator.Send(new MoverArquivosTemporariosCommand(TipoArquivo.CompensacaoAusencia, atual, novo));

            if (!string.IsNullOrEmpty(atual))
                await mediator.Send(new RemoverArquivosExcluidosCommand(atual, novo, TipoArquivo.CompensacaoAusencia.Name()));
        }

        private async Task ConsisteDisciplina(long disciplinaId, IEnumerable<string> disciplinasRegenciaIds, bool registroMigrado)
        {
            var disciplinasEOL = await repositorioComponenteCurricular.ObterDisciplinasPorIds(new long[] {disciplinaId});

            if (!disciplinasEOL.Any())
                throw new NegocioException("Componente curricular não encontrado no EOL.");

            var disciplina = disciplinasEOL.FirstOrDefault();

            if (!registroMigrado && disciplina.Regencia && ((disciplinasRegenciaIds == null) || !disciplinasRegenciaIds.Any()))
                throw new NegocioException("Regência de classe deve informar o(s) componente(s) curricular(es) relacionados a esta atividade.");
        }

        private async Task<PeriodoEscolarDto> BuscaPeriodo(Turma turma, int bimestre)
        {
            var tipoCalendario = await repositorioTipoCalendario.BuscarPorAnoLetivoEModalidade(turma.AnoLetivo, turma.ModalidadeCodigo == Modalidade.EJA ? ModalidadeTipoCalendario.EJA : ModalidadeTipoCalendario.FundamentalMedio, turma.Semestre);

            var parametroSistema = await mediator.Send(new ObterParametroSistemaPorTipoEAnoQuery(TipoParametroSistema.PermiteCompensacaoForaPeriodo, turma.AnoLetivo));

            PeriodoEscolarDto periodo = null;
            // Eja possui 2 calendarios por ano
            if (turma.ModalidadeCodigo == Modalidade.EJA)
            {
                if (turma.Semestre == 1)
                    periodo = (await consultasPeriodoEscolar.ObterPorTipoCalendario(tipoCalendario.Id)).Periodos.FirstOrDefault(p => p.Bimestre == bimestre && p.PeriodoInicio < new DateTime(turma.AnoLetivo, 6, 1));
                else
                    periodo = (await consultasPeriodoEscolar.ObterPorTipoCalendario(tipoCalendario.Id)).Periodos.FirstOrDefault(p => p.Bimestre == bimestre && p.PeriodoFim > new DateTime(turma.AnoLetivo, 6, 1));
            }
            else
                periodo = (await consultasPeriodoEscolar.ObterPorTipoCalendario(tipoCalendario.Id)).Periodos.FirstOrDefault(p => p.Bimestre == bimestre);

            if (parametroSistema is not { Ativo: true })
            {
                var turmaEmPeriodoAberto =
                    await mediator.Send(new TurmaEmPeriodoAbertoQuery(turma, DateTime.Today, bimestre, false,
                        tipoCalendario.Id));
                    
                if (!turmaEmPeriodoAberto)
                    throw new NegocioException($"Período do {bimestre}º Bimestre não está aberto.");
            }

            return periodo;
        }

        private async Task<Turma> BuscaTurma(string turmaId)
        {
            var turma = await repositorioTurmaConsulta.ObterTurmaComUeEDrePorCodigo(turmaId);
            if (turma == null)
                throw new NegocioException("Turma não localizada!");

            return turma;
        }

        private async Task GravarDisciplinasRegencia(bool alteracao, long compensacaoId, IEnumerable<string> disciplinasRegenciaIds, Usuario usuarioLogado)
        {
            if (disciplinasRegenciaIds == null)
                return;

            var listaPersistencia = new List<CompensacaoAusenciaDisciplinaRegencia>();
            IEnumerable<CompensacaoAusenciaDisciplinaRegencia> disciplinas = new List<CompensacaoAusenciaDisciplinaRegencia>();
            if (alteracao)
                disciplinas = await repositorioCompensacaoAusenciaDisciplinaRegencia.ObterPorCompensacao(compensacaoId);

            // Remove as disciplinas não existentes mais
            foreach (var disciplinaExcluida in disciplinas.Where(x => !disciplinasRegenciaIds.Any(d => d == x.DisciplinaId)))
            {
                disciplinaExcluida.Excluir();
                listaPersistencia.Add(disciplinaExcluida);
            }

            // Inclui as disciplinas novas
            foreach (var disciplinaId in disciplinasRegenciaIds)
            {
                listaPersistencia.Add(new CompensacaoAusenciaDisciplinaRegencia()
                {
                    CompensacaoAusenciaId = compensacaoId,
                    DisciplinaId = disciplinaId,
                    Excluido = false
                });
            }

            await SalvarDisciplinasRegencia(alteracao, usuarioLogado, listaPersistencia);
        }

        private async Task SalvarDisciplinasRegencia(bool alteracao, Usuario usuarioLogado, List<CompensacaoAusenciaDisciplinaRegencia> listaPersistencia)
        {
            //Inserir Lista de novos registros quando alteração é false
            if (!alteracao && listaPersistencia.Any())
                await repositorioCompensacaoAusenciaDisciplinaRegencia.InserirVarios(listaPersistencia, usuarioLogado);
            //Atualizar individualmente quando alteração é true
            else if (alteracao && listaPersistencia.Any())
                listaPersistencia.ForEach(disciplina => repositorioCompensacaoAusenciaDisciplinaRegencia.Salvar(disciplina));
        }

        private async Task<List<string>> GravarCompensacaoAlunos(bool alteracao, long compensacaoId, string turmaId, string disciplinaId, IEnumerable<CompensacaoAusenciaAlunoDto> alunosDto, PeriodoEscolarDto periodo, Usuario usuarioLogado)
        {
            var mensagensExcessao = new StringBuilder();

            var listaPersistencia = new List<CompensacaoAusenciaAluno>();
            IEnumerable<CompensacaoAusenciaAluno> alunos = new List<CompensacaoAusenciaAluno>();

            if (alteracao)
                alunos = await mediator.Send(new ObterCompensacaoAusenciaAlunoPorCompensacaoQuery(compensacaoId));

            // excluir os removidos da lista
            foreach (var alunoRemovido in alunos.Where(a => !alunosDto.Any(d => d.Id == a.CodigoAluno)))
            {
                alunoRemovido.Excluir();
                listaPersistencia.Add(alunoRemovido);
            }
            
            var obterFrequenciaPorListaDeAlunosDisciplinaData = await mediator.Send(new ObterFrequenciaPorListaDeAlunosDisciplinaDataQuery(alunosDto?.Select(x => x.Id).ToArray(), disciplinaId, periodo.PeriodoFim, turmaId));

            // altera as faltas compensadas
            var alunosAlterarFaltasCompensada = alunos.Where(a => !a.Excluido);

            if (alunosAlterarFaltasCompensada.Any())
            {
                var alunosCodigos = alunosAlterarFaltasCompensada?.Select(x => x.CodigoAluno);
                var consultaAlunosAlterarFaltasCompensada = obterFrequenciaPorListaDeAlunosDisciplinaData.Where(o => alunosCodigos.Contains(o.CodigoAluno) && o.DisciplinaId == disciplinaId && o.PeriodoFim == periodo.PeriodoFim && o.TurmaId == turmaId);
                foreach (var aluno in alunosAlterarFaltasCompensada)
                {
                    var frequenciaAluno = consultaAlunosAlterarFaltasCompensada.FirstOrDefault(x => x.CodigoAluno == aluno.CodigoAluno && x.DisciplinaId == disciplinaId && x.PeriodoFim == periodo.PeriodoFim && x.TurmaId == turmaId);
                    if (frequenciaAluno == null)
                    {
                        mensagensExcessao.Append($"O aluno(a) [{aluno.CodigoAluno}] não possui ausência para compensar. ");
                        continue;
                    }
                    
                    var faltasNaoCompensadas = frequenciaAluno.NumeroFaltasNaoCompensadas > 0
                        ? frequenciaAluno.NumeroFaltasNaoCompensadas + aluno.QuantidadeFaltasCompensadas
                        : aluno.QuantidadeFaltasCompensadas;

                    var alunoDto = alunosDto.FirstOrDefault(a => a.Id == aluno.CodigoAluno);
                    if (alunoDto.QtdFaltasCompensadas > faltasNaoCompensadas)
                    {
                        mensagensExcessao.Append(
                            $"O aluno(a) [{alunoDto.Id}] possui apenas {frequenciaAluno.NumeroFaltasNaoCompensadas} faltas não compensadas. ");
                        continue;
                    }

                    aluno.QuantidadeFaltasCompensadas = alunoDto.QtdFaltasCompensadas;
                    listaPersistencia.Add(aluno);
                }
            }

            // adiciona os alunos novos
            var listaAlunosDto = alunosDto?.Where(d => !alunos.Any(a => a.CodigoAluno == d.Id));
            if (listaAlunosDto.Any())
            {
                var listaIdsAluno = listaAlunosDto.Select(x => x.Id);
                var consultaAlunosFrequencia = obterFrequenciaPorListaDeAlunosDisciplinaData.Where(c => listaIdsAluno.Contains(c.CodigoAluno) && c.DisciplinaId == disciplinaId && c.PeriodoFim == periodo.PeriodoFim && c.TurmaId == turmaId);
                foreach (var alunoDto in listaAlunosDto)
                {
                    var frequenciaAluno = consultaAlunosFrequencia?.FirstOrDefault(x => x.CodigoAluno == alunoDto.Id && x.DisciplinaId == disciplinaId && x.TurmaId == turmaId);
                    if (frequenciaAluno == null)
                    {
                        mensagensExcessao.Append($"O aluno(a) [{alunoDto.Id}] não possui ausência para compensar. ");
                        continue;
                    }

                    if (alunoDto.QtdFaltasCompensadas > frequenciaAluno.NumeroFaltasNaoCompensadas && frequenciaAluno.NumeroFaltasNaoCompensadas > 0)
                    {
                        mensagensExcessao.Append(
                            $"O aluno(a) [{alunoDto.Id}] possui apenas {frequenciaAluno.NumeroFaltasNaoCompensadas} faltas não compensadas. ");

                        continue;
                    }

                    listaPersistencia.Add(MapearCompensacaoAlunoEntidade(compensacaoId, alunoDto));
                }
            }

            if (!string.IsNullOrEmpty(mensagensExcessao.ToString()))
                throw new NegocioException(mensagensExcessao.ToString());

            await SalvarCompensacaoAlunos(alteracao, usuarioLogado, listaPersistencia);

            // Recalcula Frequencia dos alunos envolvidos na Persistencia
            return listaPersistencia.Select(a => a.CodigoAluno).ToList();
        }

        private async Task SalvarCompensacaoAlunos(bool alteracao, Usuario usuarioLogado, List<CompensacaoAusenciaAluno> listaPersistencia)
        {
            //Inserir Lista de novos registros quando alteração é false
            if (!alteracao && listaPersistencia.Any())
                await repositorioCompensacaoAusenciaAluno.InserirVarios(listaPersistencia, usuarioLogado);
            //Atualizar individualmente quando alteração é true
            else if (alteracao && listaPersistencia.Any())
                listaPersistencia.ForEach(aluno => repositorioCompensacaoAusenciaAluno.Salvar(aluno));
        }

        private CompensacaoAusenciaAluno MapearCompensacaoAlunoEntidade(long compensacaoId, CompensacaoAusenciaAlunoDto alunoDto) => new CompensacaoAusenciaAluno()
        {
            CompensacaoAusenciaId = compensacaoId,
            CodigoAluno = alunoDto.Id,
            QuantidadeFaltasCompensadas = alunoDto.QtdFaltasCompensadas,
            Notificado = false,
            Excluido = false
        };

        private CompensacaoAusencia MapearEntidade(CompensacaoAusenciaDto compensacaoDto,
            CompensacaoAusencia compensacao)
        {
            compensacao.DisciplinaId = compensacaoDto.DisciplinaId;
            compensacao.Bimestre = compensacaoDto.Bimestre;
            compensacao.Nome = compensacaoDto.Atividade;
            compensacao.Descricao = compensacaoDto.Descricao.Replace(configuracaoArmazenamentoOptions.Value.BucketTemp, configuracaoArmazenamentoOptions.Value.BucketArquivos);

            return compensacao;
        }

        public async Task Excluir(long[] compensacoesIds)
        {
            var compensacoesExcluir = new List<CompensacaoAusencia>();
            var compensacoesAlunosExcluir = new List<CompensacaoAusenciaAluno>();
            var compensacoesDisciplinasExcluir = new List<CompensacaoAusenciaDisciplinaRegencia>();
            var listaCompensacaoDescricao = new List<string>();

            List<long> idsComErroAoExcluir = new List<long>();

            // Carrega lista de objetos a excluir marcando-los para exclusão
            foreach (var compensacaoId in compensacoesIds)
            {
                var compensacao = repositorioCompensacaoAusencia.ObterPorId(compensacaoId);
                listaCompensacaoDescricao.Add(compensacao.Descricao);
                compensacao.Excluir();
                compensacoesExcluir.Add(compensacao);

                var compensacoesAlunos = await mediator.Send(new ObterCompensacaoAusenciaAlunoPorCompensacaoQuery(compensacaoId));

                foreach (var compensacaoAluno in compensacoesAlunos)
                {
                    compensacaoAluno.Excluir();
                    compensacoesAlunosExcluir.Add(compensacaoAluno);
                }

                var compensacoesDisciplinas = await repositorioCompensacaoAusenciaDisciplinaRegencia.ObterPorCompensacao(compensacaoId);
                foreach (var compensacaoDisciplina in compensacoesDisciplinas)
                {
                    compensacaoDisciplina.Excluir();
                    compensacoesDisciplinasExcluir.Add(compensacaoDisciplina);
                }
            }

            // Excluir lista carregada
            foreach (var compensacaoExcluir in compensacoesExcluir)
            {
                var turma = await repositorioTurmaConsulta.ObterTurmaComUeEDrePorId(compensacaoExcluir.TurmaId);
                var periodo = await BuscaPeriodo(turma, compensacaoExcluir.Bimestre);

                unitOfWork.IniciarTransacao();
                try
                {
                    // Exclui dependencias
                    var alunosDaCompensacao = compensacoesAlunosExcluir
                        .Where(c => c.CompensacaoAusenciaId == compensacaoExcluir.Id).ToList();
                    alunosDaCompensacao.ForEach(c => repositorioCompensacaoAusenciaAluno.Salvar(c));

                    compensacoesDisciplinasExcluir.Where(c => c.CompensacaoAusenciaId == compensacaoExcluir.Id).ToList()
                        .ForEach(c => repositorioCompensacaoAusenciaDisciplinaRegencia.Salvar(c));

                    // Exclui compensação
                    await repositorioCompensacaoAusencia.SalvarAsync(compensacaoExcluir);
                    // Excluir notificações
                    repositorioNotificacaoCompensacaoAusencia.Excluir(compensacaoExcluir.Id);

                    unitOfWork.PersistirTransacao();

                    var listaAlunos = alunosDaCompensacao.Select(a => a.CodigoAluno).ToList();

                    if (alunosDaCompensacao.Any())
                    {
                        await mediator.Send(new IncluirFilaCalcularFrequenciaPorTurmaCommand(listaAlunos,
                            periodo.PeriodoFim, turma.CodigoTurma, compensacaoExcluir.DisciplinaId));
                    }
                }
                catch (Exception)
                {
                    idsComErroAoExcluir.Add(compensacaoExcluir.Id);
                    unitOfWork.Rollback();
                }
            }

            if (listaCompensacaoDescricao != null && listaCompensacaoDescricao.Any())
            {
                foreach (var item in listaCompensacaoDescricao)
                {
                    await mediator.Send(
                        new DeletarArquivoDeRegistroExcluidoCommand(item, TipoArquivo.CompensacaoAusencia.Name()));
                }
            }

            if (idsComErroAoExcluir.Any())
                throw new NegocioException($"Não foi possível excluir as compensações de ids {string.Join(",", idsComErroAoExcluir)}");
        }

        private async Task ValidaProfessorPodePersistirTurma(string turmaId, Usuario usuario, DateTime dataAula)
        {
            if (!await PossuiPermissaoTurma(turmaId, usuario, dataAula))
                throw new NegocioException("Você não pode fazer alterações ou inclusões nesta turma e data.");
        }

        private async Task<bool> PossuiPermissaoTurma(string turmaId, Usuario usuario, DateTime dataAula)
        {
            if (usuario.EhProfessorCj())
                return await PossuiAtribuicaoCJ(turmaId, usuario.CodigoRf);

            return await mediator.Send(
                new ProfessorPodePersistirTurmaQuery(usuario.CodigoRf, turmaId, dataAula.Local()));
        }

        private async Task<bool> PossuiAtribuicaoCJ(string turmaId, string codigoRf)
        {
            var componentes = await consultasDisciplina.ObterDisciplinasPerfilCJ(turmaId, codigoRf);
            return componentes != null && componentes.Any();
        }

        public async Task<string> Copiar(CompensacaoAusenciaCopiaDto compensacaoCopia)
        {
            var compensacaoOrigem = repositorioCompensacaoAusencia.ObterPorId(compensacaoCopia.CompensacaoOrigemId);
            if (compensacaoOrigem == null)
                throw new NegocioException("Compensação de origem não localizada com o identificador informado.");

            var turmasCopiadas = new StringBuilder("");
            var turmasComErro = new StringBuilder("");
            foreach (var turmaId in compensacaoCopia.TurmasIds)
            {
                var turma = await mediator.Send(new ObterTurmaPorCodigoQuery(turmaId));
                CompensacaoAusenciaDto compensacaoDto = new CompensacaoAusenciaDto()
                {
                    TurmaId = turmaId,
                    Bimestre = compensacaoCopia.Bimestre,
                    DisciplinaId = compensacaoOrigem.DisciplinaId,
                    Atividade = compensacaoOrigem.Nome,
                    Descricao = compensacaoOrigem.Descricao,
                    DisciplinasRegenciaIds = new List<string>(),
                    Alunos = new List<CompensacaoAusenciaAlunoDto>()
                };

                var disciplinasRegencia =
                    await repositorioCompensacaoAusenciaDisciplinaRegencia.ObterPorCompensacao(compensacaoOrigem.Id);
                if (disciplinasRegencia != null && disciplinasRegencia.Any())
                    compensacaoDto.DisciplinasRegenciaIds = disciplinasRegencia.Select(s => s.DisciplinaId);

                try
                {
                    await Salvar(0, compensacaoDto);
                    turmasCopiadas.Append(turmasCopiadas.ToString().Length > 0 ? ", " + turma.Nome : turma.Nome);
                }
                catch (Exception e)
                {
                    turmasComErro.AppendLine($"A cópia para a turma {turma.Nome} não foi realizada: {e.Message}\n");
                }
            }

            var respTurmasCopiadas = turmasCopiadas.ToString();
            var textoTurmas = respTurmasCopiadas.Contains(",") ? "as turmas" : "a turma";
            var respostaSucesso = respTurmasCopiadas.Length > 0
                ? $"A cópia para {textoTurmas} {respTurmasCopiadas} foi realizada com sucesso"
                : "";
            var respTurmasComErro = turmasComErro.ToString();
            if (respTurmasComErro.Length > 0)
                throw new NegocioException($"{respTurmasComErro} {respostaSucesso}");

            return respostaSucesso;
        }
    }
}