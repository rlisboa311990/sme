using System;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using SME.SGP.Dominio;
using SME.SGP.Dominio.Constantes.MensagensNegocio;
using SME.SGP.Dominio.Enumerados;
using SME.SGP.Infra;

namespace SME.SGP.Aplicacao
{
    public class SalvarConselhoClasseAlunoNotaUseCase : ISalvarConselhoClasseAlunoNotaUseCase
    {
        private readonly IMediator mediator;
        private const int BIMESTRE_2 = 2;
        private const int BIMESTRE_4 = 4;

        public SalvarConselhoClasseAlunoNotaUseCase(IMediator mediator)
        {
            this.mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public async Task<ConselhoClasseNotaRetornoDto> Executar(SalvarConselhoClasseAlunoNotaDto dto)
        {
            var turma = await mediator.Send(new ObterTurmaPorCodigoQuery(dto.CodigoTurma));

            if (turma == null)
                throw new NegocioException("Turma não encontrada");

            var ehAnoAnterior = turma.AnoLetivo != DateTime.Now.Year;

            var fechamentoTurma = await mediator.Send(new ObterFechamentoTurmaPorIdAlunoCodigoQuery(dto.FechamentoTurmaId,
                dto.CodigoAluno, ehAnoAnterior));

            FechamentoTurmaDisciplina fechamentoTurmaDisciplina;
            var periodoEscolar = new PeriodoEscolar();

            if (fechamentoTurma == null)
            {
                if (!ehAnoAnterior)
                    throw new NegocioException("Não existe fechamento de turma para o conselho de classe");

                periodoEscolar = await mediator.Send(new ObterPeriodoEscolarPorTurmaBimestreQuery(turma, dto.Bimestre));

                if (periodoEscolar == null && dto.Bimestre > 0)
                    throw new NegocioException("Período escolar não encontrado");

                fechamentoTurma = new FechamentoTurma()
                {
                    TurmaId = turma.Id,
                    Migrado = false,
                    PeriodoEscolarId = periodoEscolar?.Id,
                    Turma = turma,
                    PeriodoEscolar = periodoEscolar
                };

                fechamentoTurmaDisciplina = new FechamentoTurmaDisciplina()
                {
                    DisciplinaId = dto.ConselhoClasseNotaDto.CodigoComponenteCurricular
                };
            }
            else
            {
                if (fechamentoTurma.PeriodoEscolarId != null)
                {
                    periodoEscolar =
                        await mediator.Send(new ObterPeriodoEscolarePorIdQuery(fechamentoTurma.PeriodoEscolarId.Value));
                }

                fechamentoTurmaDisciplina = new FechamentoTurmaDisciplina()
                {
                    DisciplinaId = dto.ConselhoClasseNotaDto.CodigoComponenteCurricular
                };
            }

            var usuario = await mediator.Send(new ObterUsuarioLogadoQuery());

            periodoEscolar = await ObtenhaPeriodoEscolar(periodoEscolar, turma, dto.Bimestre);

            await ValidaProfessorPodePersistirTurma(turma, periodoEscolar, usuario);

            await VerificaSePodeEditarNota(periodoEscolar, turma, dto.CodigoAluno, dto.Bimestre);

            await ValidarConceitoOuNota(dto, fechamentoTurma, alunoConselho, periodoEscolar);

            await mediator.Send(new GravarFechamentoTurmaConselhoClasseCommand(
                fechamentoTurma, fechamentoTurmaDisciplina, periodoEscolar?.Bimestre));

            return await mediator.Send(new GravarConselhoClasseCommad(fechamentoTurma, dto.ConselhoClasseId, dto.CodigoAluno,
                dto.ConselhoClasseNotaDto, periodoEscolar?.Bimestre, usuario));
        }

        private async Task<PeriodoEscolar> ObtenhaPeriodoEscolar(PeriodoEscolar periodo, Turma turma, int bimestre)
        {
            if (periodo.PeriodoFim == DateTime.MinValue)
            {
                bimestre = bimestre == 0 ? turma.ModalidadeTipoCalendario == ModalidadeTipoCalendario.EJA ? BIMESTRE_2 : BIMESTRE_4 : bimestre;
                
                return await mediator.Send(new ObterPeriodoEscolarPorTurmaBimestreQuery(turma, bimestre));
            }

            return periodo;
        }

        private async Task VerificaSePodeEditarNota(PeriodoEscolar periodoEscolar, Turma turma, string codigoAluno, int bimestre)
        {
            var alunos = await mediator.Send(new ObterAlunosPorTurmaEAnoLetivoQuery(turma.CodigoTurma));
            var alunoConselho = alunos.FirstOrDefault(x => x.CodigoAluno == codigoAluno);
            var visualizaNotas = (periodoEscolar is null && !alunoConselho.Inativo) ||
                     (!alunoConselho.Inativo && alunoConselho.DataMatricula.Date <= periodoEscolar.PeriodoFim) ||
                     (alunoConselho.Inativo && alunoConselho.DataSituacao.Date > periodoEscolar.PeriodoInicio);

            if (!visualizaNotas || !await this.mediator.Send(new VerificaSePodeEditarNotaQuery(codigoAluno, turma, periodoEscolar)))
                throw new NegocioException(MensagemNegocioFechamentoNota.NOTA_ALUNO_NAO_PODE_SER_INSERIDA_OU_ALTERADA_NO_PERIODO);
        }

        private async Task<bool> PossuiPermissaoTurma(Turma turma, PeriodoEscolar periodo, Usuario usuarioLogado)
        {
            if (usuarioLogado.EhProfessorCj())
                return await mediator.Send(new PossuiAtribuicaoCJPorDreUeETurmaQuery(turma.Ue?.Dre?.CodigoDre, turma.Ue?.CodigoUe, turma.Id.ToString(), usuarioLogado.CodigoRf));

            return await mediator.Send(new ProfessorPodePersistirTurmaQuery(usuarioLogado.CodigoRf, turma.CodigoTurma, periodo.PeriodoFim));
        }

        private async Task ValidaProfessorPodePersistirTurma(Turma turma, PeriodoEscolar periodo, Usuario usuarioLogado)
        {
            if (!usuarioLogado.EhGestorEscolar() && !await PossuiPermissaoTurma(turma, periodo, usuarioLogado))
                throw new NegocioException(MensagensNegocioFrequencia.Nao_pode_fazer_alteracoes_anotacao_nesta_turma_componente_e_data);
        }

        private async Task ValidarConceitoOuNota(SalvarConselhoClasseAlunoNotaDto dto, FechamentoTurma fechamentoTurma,
            AlunoPorTurmaResposta alunoConselho, PeriodoEscolar periodoEscolar)
        {
            var turmasCodigos = new[] { dto.CodigoTurma };
                
            var notasFechamentoAluno = (fechamentoTurma is { PeriodoEscolarId: { } } ?
                await mediator.Send(new ObterNotasFechamentosPorTurmasCodigosBimestreQuery(turmasCodigos, dto.CodigoAluno,
                    dto.Bimestre, alunoConselho.DataMatricula, !alunoConselho.EstaInativo(periodoEscolar.PeriodoFim) 
                        ? periodoEscolar.PeriodoFim : alunoConselho.DataSituacao, fechamentoTurma.Turma.AnoLetivo)) :
                await mediator.Send(new ObterNotasFinaisBimestresAlunoQuery(turmasCodigos, dto.CodigoAluno, 
                    alunoConselho.DataMatricula, !alunoConselho.EstaInativo(periodoEscolar.PeriodoFim) 
                        ? periodoEscolar.PeriodoFim : alunoConselho.DataSituacao, dto.Bimestre))).ToList();

            if (notasFechamentoAluno.FirstOrDefault(c => c.ComponenteCurricularCodigo == dto.ConselhoClasseNotaDto.CodigoComponenteCurricular) == null)
                return;
            
            var notaTipoValor = await mediator.Send(new ObterTipoNotaPorTurmaIdQuery(fechamentoTurma.TurmaId, fechamentoTurma.Turma.TipoTurma));

            if (notaTipoValor == null)
                return;
            
            switch (notaTipoValor.TipoNota)
            {
                case TipoNota.Conceito when dto.ConselhoClasseNotaDto.Conceito == null:
                    throw new NegocioException("O conceito pós-conselho deve ser informado no conselho de classe do aluno.");
                case TipoNota.Nota when dto.ConselhoClasseNotaDto.Nota == null:
                    throw new NegocioException("A nota pós-conselho deve ser informada no conselho de classe do aluno.");
            }
        }
    }
}