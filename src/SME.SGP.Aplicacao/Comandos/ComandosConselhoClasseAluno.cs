﻿using SME.SGP.Dominio;
using SME.SGP.Infra;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using MediatR;
using SME.SGP.Dominio.Constantes.MensagensNegocio;
using System.Linq;
using SME.SGP.Aplicacao.Queries;

namespace SME.SGP.Aplicacao
{
    public class ComandosConselhoClasseAluno : IComandosConselhoClasseAluno
    {
        private readonly IConsultasConselhoClasseAluno consultasConselhoClasseAluno;
        private readonly IConsultasConselhoClasse consultasConselhoClasse;
        private readonly IMediator mediator;
        private const int BIMESTRE_FINAL_FUNDAMENTAL_MEDIO = 4;
        private const int BIMESTRE_FINAL_EJA = 2;

        public ComandosConselhoClasseAluno(IConsultasConselhoClasseAluno consultasConselhoClasseAluno,
                                           IConsultasConselhoClasse consultasConselhoClasse,
                                           IMediator mediator)
        {
            this.consultasConselhoClasseAluno = consultasConselhoClasseAluno ?? throw new ArgumentNullException(nameof(consultasConselhoClasseAluno));
            this.consultasConselhoClasse = consultasConselhoClasse ?? throw new ArgumentNullException(nameof(consultasConselhoClasse));
            this.mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public async Task<ConselhoClasseAluno> SalvarAsync(ConselhoClasseAlunoAnotacoesDto conselhoClasseAlunoDto)
        {
            var dataAtual = DateTimeExtension.HorarioBrasilia();
            var fechamentoTurma = await mediator.Send(new ObterFechamentoTurmaCompletoPorIdQuery(conselhoClasseAlunoDto.FechamentoTurmaId));

            var bimestre = fechamentoTurma.PeriodoEscolarId.HasValue ? fechamentoTurma.PeriodoEscolar.Bimestre : 
                fechamentoTurma.Turma.EhEJA() ? BIMESTRE_FINAL_EJA : BIMESTRE_FINAL_FUNDAMENTAL_MEDIO;

            var periodoAberto = await mediator.Send(new TurmaEmPeriodoAbertoQuery(fechamentoTurma.Turma, dataAtual.Date, bimestre,
                fechamentoTurma.Turma.AnoLetivo == dataAtual.Year));

            if (!periodoAberto)
                throw new NegocioException(MensagemNegocioComuns.APENAS_EH_POSSIVEL_CONSULTAR_ESTE_REGISTRO_POIS_O_PERIODO_NAO_ESTA_EM_ABERTO);
            
            var periodoEscolar = await mediator.Send(new ObterPeriodoEscolarPorTurmaBimestreQuery(fechamentoTurma.Turma, bimestre));

            if (periodoEscolar == null)
                throw new NegocioException("Período escolar não encontrado");

            var alunos = await mediator.Send(new ObterAlunosPorTurmaEAnoLetivoQuery(fechamentoTurma.Turma.CodigoTurma));
            var alunoConselho = alunos.FirstOrDefault(x => x.CodigoAluno == conselhoClasseAlunoDto.AlunoCodigo);

            if (alunoConselho == null)
                throw new NegocioException("Aluno não encontrado para salvar o conselho de classe.");
            
            
            if (fechamentoTurma.Turma.AnoLetivo == dataAtual.Year && alunoConselho.EstaAtivo(periodoEscolar.PeriodoFim))
            {
                var periodoReaberturaCorrespondente = await mediator.Send(new ObterFechamentoReaberturaPorDataTurmaQuery
                {
                    DataParaVerificar = dataAtual, 
                    TipoCalendarioId = periodoEscolar.TipoCalendarioId, 
                    UeId = fechamentoTurma.Turma.UeId
                });
                
                var permiteEdicao = (alunoConselho.PossuiSituacaoAtiva() && alunoConselho.DataMatricula.Date <= periodoEscolar.PeriodoFim) ||
                                    (alunoConselho.Inativo && alunoConselho.DataSituacao.Date > periodoEscolar.PeriodoFim);
                
                if (permiteEdicao)
                    if (alunoConselho.DataSituacao < periodoReaberturaCorrespondente.Inicio || alunoConselho.DataSituacao > periodoReaberturaCorrespondente.Fim)
                        throw new NegocioException(MensagemNegocioFechamentoNota.ALUNO_INATIVO_ANTES_PERIODO_ESCOLAR);
            }
            
            var existeConselhoClasseBimestre = await mediator.Send(new VerificaNotasTodosComponentesCurricularesQuery(alunoConselho.CodigoAluno,
                fechamentoTurma.Turma, periodoEscolar.Id));

            if (!existeConselhoClasseBimestre)
                throw new NegocioException(MensagemNegocioFechamentoNota.EXISTE_COMPONENTES_SEM_NOTA_INFORMADA);

            var conselhoClasseAluno = await MapearParaEntidade(conselhoClasseAlunoDto);

            conselhoClasseAluno.Id = await mediator.Send(new SalvarConselhoClasseAlunoCommand(conselhoClasseAluno));

            await SalvarRecomendacoesAlunoFamilia(conselhoClasseAlunoDto.RecomendacaoAlunoIds, conselhoClasseAlunoDto.RecomendacaoFamiliaIds,
                conselhoClasseAluno.Id);

            return conselhoClasseAluno;
        }

        private async Task<ConselhoClasseAluno> MapearParaEntidade(ConselhoClasseAlunoAnotacoesDto conselhoClasseAlunoDto)
        {
            var conselhoClasseAluno = await consultasConselhoClasseAluno.ObterPorConselhoClasseAsync(conselhoClasseAlunoDto.ConselhoClasseId, conselhoClasseAlunoDto.AlunoCodigo);
            if (conselhoClasseAluno == null)
            {
                ConselhoClasse conselhoClasse = conselhoClasseAlunoDto.ConselhoClasseId == 0 ?
                    new ConselhoClasse() { FechamentoTurmaId = conselhoClasseAlunoDto.FechamentoTurmaId } :
                    consultasConselhoClasse.ObterPorId(conselhoClasseAlunoDto.ConselhoClasseId);

                conselhoClasseAluno = new ConselhoClasseAluno()
                {
                    ConselhoClasse = conselhoClasse,
                    ConselhoClasseId = conselhoClasse.Id,
                    AlunoCodigo = conselhoClasseAlunoDto.AlunoCodigo
                };
            }

            MoverAnotacoesPedagogicas(conselhoClasseAlunoDto, conselhoClasseAluno);
            MoverRecomendacoesAluno(conselhoClasseAlunoDto, conselhoClasseAluno);
            MoverRecomendacoesFamilia(conselhoClasseAlunoDto, conselhoClasseAluno);
            conselhoClasseAluno.AnotacoesPedagogicas = conselhoClasseAlunoDto.AnotacoesPedagogicas;
            conselhoClasseAluno.RecomendacoesAluno = conselhoClasseAlunoDto.RecomendacaoAluno;
            conselhoClasseAluno.RecomendacoesFamilia = conselhoClasseAlunoDto.RecomendacaoFamilia;

            return conselhoClasseAluno;
        }

        private async Task SalvarRecomendacoesAlunoFamilia(IEnumerable<long> recomendacoesAlunoId, IEnumerable<long> recomendacoesFamiliaId, long conselhoClasseAlunoId)
            => await mediator.Send(new SalvarConselhoClasseAlunoRecomendacaoCommand(recomendacoesAlunoId, recomendacoesFamiliaId, conselhoClasseAlunoId));

        private void MoverAnotacoesPedagogicas(ConselhoClasseAlunoAnotacoesDto conselhoClasseAlunoDto, ConselhoClasseAluno conselhoClasseAluno)
        {
            if (!string.IsNullOrEmpty(conselhoClasseAlunoDto.AnotacoesPedagogicas))
            {
                var moverArquivo = mediator.Send(new MoverArquivosTemporariosCommand(TipoArquivo.ConselhoClasse, conselhoClasseAluno.AnotacoesPedagogicas, conselhoClasseAlunoDto.AnotacoesPedagogicas));
                conselhoClasseAlunoDto.AnotacoesPedagogicas = moverArquivo.Result;
            }
            if (!string.IsNullOrEmpty(conselhoClasseAluno.AnotacoesPedagogicas))
            {
                var deletarArquivosNaoUtilziados = mediator.Send(new RemoverArquivosExcluidosCommand(conselhoClasseAluno.AnotacoesPedagogicas, conselhoClasseAlunoDto.AnotacoesPedagogicas, TipoArquivo.ConselhoClasse.Name()));
            }
        }
        private void MoverRecomendacoesAluno(ConselhoClasseAlunoAnotacoesDto conselhoClasseAlunoDto, ConselhoClasseAluno conselhoClasseAluno)
        {
            if (!string.IsNullOrEmpty(conselhoClasseAlunoDto.RecomendacaoAluno))
            {
                var moverArquivo = mediator.Send(new MoverArquivosTemporariosCommand(TipoArquivo.ConselhoClasse, conselhoClasseAluno.RecomendacoesAluno, conselhoClasseAlunoDto.RecomendacaoAluno));
                conselhoClasseAlunoDto.RecomendacaoAluno = moverArquivo.Result;
            }
            if (!string.IsNullOrEmpty(conselhoClasseAluno.RecomendacoesAluno))
            {
                var deletarArquivosNaoUtilziados = mediator.Send(new RemoverArquivosExcluidosCommand(conselhoClasseAluno.RecomendacoesAluno, conselhoClasseAlunoDto.RecomendacaoAluno, TipoArquivo.ConselhoClasse.Name()));
            }
        }
        private void MoverRecomendacoesFamilia(ConselhoClasseAlunoAnotacoesDto conselhoClasseAlunoDto, ConselhoClasseAluno conselhoClasseAluno)
        {
            if (!string.IsNullOrEmpty(conselhoClasseAlunoDto.RecomendacaoFamilia))
            {
                var moverArquivo = mediator.Send(new MoverArquivosTemporariosCommand(TipoArquivo.ConselhoClasse, conselhoClasseAluno.RecomendacoesFamilia, conselhoClasseAlunoDto.RecomendacaoFamilia));
                conselhoClasseAlunoDto.RecomendacaoFamilia = moverArquivo.Result;
            }
            if (!string.IsNullOrEmpty(conselhoClasseAluno.RecomendacoesFamilia))
            {
                var deletarArquivosNaoUtilziados = mediator.Send(new RemoverArquivosExcluidosCommand(conselhoClasseAluno.RecomendacoesFamilia, conselhoClasseAlunoDto.RecomendacaoFamilia, TipoArquivo.ConselhoClasse.Name()));
            }
        }
    }
}