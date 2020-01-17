﻿using SME.Background.Core;
using SME.SGP.Aplicacao;
using SME.SGP.Aplicacao.Integracoes;
using SME.SGP.Dominio.Interfaces;
using SME.SGP.Infra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SME.SGP.Dominio.Servicos
{
    public class ServicoCompensacaoAusencia: IServicoCompensacaoAusencia
    {
        private readonly IRepositorioCompensacaoAusencia repositorioCompensacaoAusencia;
        private readonly IRepositorioCompensacaoAusenciaAluno repositorioCompensacaoAusenciaAluno;
        private readonly IRepositorioCompensacaoAusenciaDisciplinaRegencia repositorioCompensacaoAusenciaDisciplinaRegencia;
        private readonly IRepositorioFrequenciaAlunoDisciplinaPeriodo repositorioFrequencia;
        private readonly IConsultasPeriodoEscolar consultasPeriodoEscolar;
        private readonly IRepositorioTipoCalendario repositorioTipoCalendario;
        private readonly IRepositorioTurma repositorioTurma;
        private readonly IServicoEOL servicoEOL;
        private readonly IUnitOfWork unitOfWork;

        public ServicoCompensacaoAusencia(IRepositorioCompensacaoAusencia repositorioCompensacaoAusencia,
                                          IRepositorioCompensacaoAusenciaAluno repositorioCompensacaoAusenciaAluno,
                                          IRepositorioCompensacaoAusenciaDisciplinaRegencia repositorioCompensacaoAusenciaDisciplinaRegencia,
                                          IRepositorioFrequenciaAlunoDisciplinaPeriodo repositorioFrequencia,
                                          IConsultasPeriodoEscolar consultasPeriodoEscolar,
                                          IRepositorioTipoCalendario repositorioTipoCalendario,
                                          IServicoEOL servicoEOL,
                                          IRepositorioTurma repositorioTurma,
                                          IUnitOfWork unitOfWork)
        {
            this.repositorioCompensacaoAusencia = repositorioCompensacaoAusencia ?? throw new System.ArgumentNullException(nameof(repositorioCompensacaoAusencia));
            this.repositorioCompensacaoAusenciaAluno = repositorioCompensacaoAusenciaAluno ?? throw new System.ArgumentNullException(nameof(repositorioCompensacaoAusenciaAluno));
            this.repositorioCompensacaoAusenciaDisciplinaRegencia = repositorioCompensacaoAusenciaDisciplinaRegencia ?? throw new System.ArgumentNullException(nameof(repositorioCompensacaoAusenciaDisciplinaRegencia));
            this.repositorioFrequencia = repositorioFrequencia ?? throw new System.ArgumentNullException(nameof(repositorioFrequencia));
            this.consultasPeriodoEscolar = consultasPeriodoEscolar ?? throw new System.ArgumentNullException(nameof(consultasPeriodoEscolar));
            this.repositorioTipoCalendario = repositorioTipoCalendario ?? throw new System.ArgumentNullException(nameof(repositorioTipoCalendario));
            this.repositorioTurma = repositorioTurma ?? throw new System.ArgumentNullException(nameof(repositorioTurma));
            this.servicoEOL = servicoEOL ?? throw new System.ArgumentNullException(nameof(servicoEOL));
            this.unitOfWork = unitOfWork ?? throw new System.ArgumentNullException(nameof(unitOfWork));
        }

        public async Task Salvar(long id, CompensacaoAusenciaDto compensacaoDto)
        {
            // Busca dados da turma
            var turma = BuscaTurma(compensacaoDto.TurmaId);

            // Valida mesma compensação no ano
            var compensacaoExistente = await repositorioCompensacaoAusencia.ObterPorAnoTurmaENome(turma.AnoLetivo, turma.Id, compensacaoDto.Atividade, id);
            if (compensacaoExistente != null)
                throw new NegocioException("Já existe essa compensação cadastrada para turma no ano letivo.");

            // Consiste periodo
            var periodo = BuscaPeriodo(turma.AnoLetivo, turma.ModalidadeCodigo, compensacaoDto.Bimestre, turma.Semestre);

            // Carrega dasdos da disciplina no EOL
            ConsisteDisciplina(long.Parse(compensacaoDto.DisciplinaId), compensacaoDto.DisciplinasRegenciaIds);

            // Persiste os dados
            var compensacao = MapearEntidade(id, compensacaoDto);
            compensacao.TurmaId = turma.Id;
            compensacao.AnoLetivo = turma.AnoLetivo;

            unitOfWork.IniciarTransacao();
            try
            {
                await repositorioCompensacaoAusencia.SalvarAsync(compensacao);
                await GravarCompensacaoAlunos(id > 0, compensacao.Id, compensacaoDto.TurmaId, compensacaoDto.DisciplinaId, compensacaoDto.Alunos, periodo);
                await GravarDisciplinasRegencia(id > 0, compensacao.Id, compensacaoDto.DisciplinasRegenciaIds);
                unitOfWork.PersistirTransacao();
            }
            catch (Exception)
            {
                unitOfWork.Rollback();
                throw;
            }
        }

        private void ConsisteDisciplina(long disciplinaId, IEnumerable<string> disciplinasRegenciaIds)
        {
            var disciplinasEOL = servicoEOL.ObterDisciplinasPorIds(new long[] { disciplinaId });

            if (!disciplinasEOL.Any())
                throw new NegocioException("Disciplina não encontrada no EOL.");

            var disciplina = disciplinasEOL.FirstOrDefault();

            if (disciplina.Regencia && ((disciplinasRegenciaIds == null) || !disciplinasRegenciaIds.Any()))
                throw new NegocioException("Regência de classe deve informar a(s) disciplina(s) relacionadas a esta atividade.");

        }

        private PeriodoEscolarDto BuscaPeriodo(int anoLetivo, Modalidade modalidadeCodigo, int bimestre, int semestre)
        {
            var tipoCalendario = repositorioTipoCalendario.BuscarPorAnoLetivoEModalidade(anoLetivo, modalidadeCodigo == Modalidade.EJA ? ModalidadeTipoCalendario.EJA : ModalidadeTipoCalendario.FundamentalMedio);

            PeriodoEscolarDto periodo = null;
            // Eja possui 2 calendarios por ano
            if (modalidadeCodigo == Modalidade.EJA)
            {
                if (semestre == 1)
                    periodo = consultasPeriodoEscolar.ObterPorTipoCalendario(tipoCalendario.Id).Periodos
                        .FirstOrDefault(p => p.Bimestre == bimestre && p.PeriodoInicio < new DateTime(anoLetivo, 6, 1));
                else
                    periodo = consultasPeriodoEscolar.ObterPorTipoCalendario(tipoCalendario.Id).Periodos
                        .FirstOrDefault(p => p.Bimestre == bimestre && p.PeriodoFim > new DateTime(anoLetivo, 6, 1));
            }
            else
                periodo = consultasPeriodoEscolar.ObterPorTipoCalendario(tipoCalendario.Id).Periodos
                    .FirstOrDefault(p => p.Bimestre == bimestre);

            // TODO alterar verificação para checagem de periodo de fechamento e reabertura do fechamento depois de implementado
            if (DateTime.Now < periodo.PeriodoInicio || DateTime.Now > periodo.PeriodoFim)
                throw new NegocioException($"Período do {bimestre}º Bimestre não esta aberto");

            return periodo;
        }

        private Turma BuscaTurma(string turmaId)
        {
            var turma = repositorioTurma.ObterPorId(turmaId);
            if (turma == null)
                throw new NegocioException("Turma não localizada!");

            return turma;
        }

        private async Task GravarDisciplinasRegencia(bool alteracao, long compensacaoId, IEnumerable<string> disciplinasRegenciaIds)
        {
            var listaPersistencia = new List<CompensacaoAusenciaDisciplinaRegencia>();
            IEnumerable<CompensacaoAusenciaDisciplinaRegencia> disciplinas = new List<CompensacaoAusenciaDisciplinaRegencia>();
            if (alteracao)
                disciplinas = await repositorioCompensacaoAusenciaDisciplinaRegencia.ObterPorCompensacao(compensacaoId);

            // Remove as disciplinas não existentes mais
            foreach(var disciplinaExcluida in disciplinas.Where(x => !disciplinasRegenciaIds.Any(d => d == x.DisciplinaId)))
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

            listaPersistencia.ForEach(disciplina => repositorioCompensacaoAusenciaDisciplinaRegencia.Salvar(disciplina));
        }

        private async Task GravarCompensacaoAlunos(bool alteracao, long compensacaoId, string turmaId, string disciplinaId, IEnumerable<CompensacaoAusenciaAlunoDto> alunosDto, PeriodoEscolarDto periodo)
        {
            var mensagensExcessao = new StringBuilder();

            List<CompensacaoAusenciaAluno> listaPersistencia = new List<CompensacaoAusenciaAluno>();
            IEnumerable<CompensacaoAusenciaAluno> alunos = new List<CompensacaoAusenciaAluno>();

            if (alteracao)
                alunos = await repositorioCompensacaoAusenciaAluno.ObterPorCompensacao(compensacaoId);

            // excluir os removidos da lista
            foreach(var alunoRemovido in alunos.Where(a => !alunosDto.Any(d => d.Id == a.CodigoAluno)))
            {
                alunoRemovido.Excluir();
                listaPersistencia.Add(alunoRemovido);
            }

            // altera as faltas compensadas
            foreach (var aluno in alunos.Where(a => !a.Excluido))
            {
                var frequenciaAluno = repositorioFrequencia.ObterPorAlunoDisciplinaData(aluno.CodigoAluno, disciplinaId, periodo.PeriodoFim);
                if (frequenciaAluno == null)
                {
                    mensagensExcessao.Append($"O aluno(a) [{aluno.CodigoAluno}] não possui ausência para compensar. ");
                    continue;
                }

                var faltasNaoCompensadas = frequenciaAluno.NumeroFaltasNaoCompensadas + aluno.QuantidadeFaltasCompensadas;

                var alunoDto = alunosDto.FirstOrDefault(a => a.Id == aluno.CodigoAluno);
                if (alunoDto.QtdFaltasCompensadas > faltasNaoCompensadas)
                {
                    mensagensExcessao.Append($"O aluno(a) [{alunoDto.Id}] possui apenas {frequenciaAluno.NumeroFaltasNaoCompensadas} faltas não compensadas. ");
                    continue;
                }

                aluno.QuantidadeFaltasCompensadas = alunoDto.QtdFaltasCompensadas;
                listaPersistencia.Add(aluno);
            }

            // adiciona os alunos novos
            foreach (var alunoDto in alunosDto.Where(d => !alunos.Any(a => a.CodigoAluno == d.Id)))
            {
                var frequenciaAluno = repositorioFrequencia.ObterPorAlunoDisciplinaData(alunoDto.Id, disciplinaId, periodo.PeriodoFim);
                if (frequenciaAluno == null)
                {
                    mensagensExcessao.Append($"O aluno(a) [{alunoDto.Id}] não possui ausência para compensar. ");
                    continue;
                }

                if (alunoDto.QtdFaltasCompensadas > frequenciaAluno.NumeroFaltasNaoCompensadas)
                {
                    mensagensExcessao.Append($"O aluno(a) [{alunoDto.Id}] possui apenas {frequenciaAluno.NumeroFaltasNaoCompensadas} faltas não compensadas. ");
                    continue;
                }

                listaPersistencia.Add(MapearCompensacaoAlunoEntidade(compensacaoId, alunoDto));
            }

            if (!string.IsNullOrEmpty(mensagensExcessao.ToString()))
                throw new NegocioException(mensagensExcessao.ToString());

            listaPersistencia.ForEach(aluno => repositorioCompensacaoAusenciaAluno.Salvar(aluno));

            // Recalcula Frequencia dos alunos envolvidos na Persistencia
            var codigosAlunos = listaPersistencia.Select(a => a.CodigoAluno).ToList();
            Cliente.Executar<IServicoCalculoFrequencia>(c => c.CalcularFrequenciaPorTurma(codigosAlunos, periodo.PeriodoFim, turmaId, disciplinaId));
        }

        private CompensacaoAusenciaAluno MapearCompensacaoAlunoEntidade(long compensacaoId, CompensacaoAusenciaAlunoDto alunoDto)
            => new CompensacaoAusenciaAluno()
            { 
                CompensacaoAusenciaId = compensacaoId,
                CodigoAluno = alunoDto.Id,
                QuantidadeFaltasCompensadas = alunoDto.QtdFaltasCompensadas,
                Notificado = false,
                Excluido = false
            };


        private CompensacaoAusencia MapearEntidade(long id, CompensacaoAusenciaDto compensacaoDto)
        {
            CompensacaoAusencia compensacao = new CompensacaoAusencia();
            if (id > 0)
                compensacao = repositorioCompensacaoAusencia.ObterPorId(id);

            compensacao.DisciplinaId = compensacaoDto.DisciplinaId;
            compensacao.Bimestre = compensacaoDto.Bimestre;
            compensacao.Nome = compensacaoDto.Atividade;
            compensacao.Descricao = compensacaoDto.Descricao;

            return compensacao;
        }

        public async Task Excluir(long[] compensacoesIds)
        {
            var compensacoesExcluir = new List<CompensacaoAusencia>();
            var compensacoesAlunosExcluir = new List<CompensacaoAusenciaAluno>();
            var compensacoesDisciplinasExcluir = new List<CompensacaoAusenciaDisciplinaRegencia>();

            List<long> idsComErroAoExcluir = new List<long>();

            // Carrega lista de objetos a excluir marcando-los para exclusão
            foreach (var compensacaoId in compensacoesIds)
            {
                var compensacao = repositorioCompensacaoAusencia.ObterPorId(compensacaoId);
                compensacao.Excluir();
                compensacoesExcluir.Add(compensacao);

                var compensacoesAlunos = await repositorioCompensacaoAusenciaAluno.ObterPorCompensacao(compensacaoId);
                foreach(var compensacaoAluno in compensacoesAlunos)
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
            foreach(var compensacaoExcluir in compensacoesExcluir)
            {
                unitOfWork.IniciarTransacao();
                try
                {
                    // Exclui dependencias
                    compensacoesAlunosExcluir.Where(c => c.CompensacaoAusenciaId == compensacaoExcluir.Id).ToList()
                        .ForEach(c => repositorioCompensacaoAusenciaAluno.Salvar(c));
                    compensacoesDisciplinasExcluir.Where(c => c.CompensacaoAusenciaId == compensacaoExcluir.Id).ToList()
                        .ForEach(c => repositorioCompensacaoAusenciaDisciplinaRegencia.Salvar(c));

                    // Exclui compensação
                    await repositorioCompensacaoAusencia.SalvarAsync(compensacaoExcluir);

                    unitOfWork.PersistirTransacao();
                }
                catch (Exception)
                {
                    idsComErroAoExcluir.Add(compensacaoExcluir.Id);
                    unitOfWork.Rollback();
                }
            }

            if (idsComErroAoExcluir.Any())
                throw new NegocioException($"Não foi possível excluir as compensações de ids {string.Join(",", idsComErroAoExcluir)}");
        }
    }
}
