﻿using MediatR;
using Newtonsoft.Json;
using SME.SGP.Dominio;
using SME.SGP.Dominio.Interfaces;
using SME.SGP.Dto;
using SME.SGP.Infra;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SME.SGP.Aplicacao
{
    public class AlterarConselhoClasseCommadHandler : InserirAlterarConselhoClasseAbstrato, IRequestHandler<AlterarConselhoClasseCommad, ConselhoClasseNotaRetornoDto>
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IRepositorioConselhoClasseAlunoConsulta repositorioConselhoClasseAlunoConsulta;
        private readonly IRepositorioConselhoClasseNotaConsulta repositorioConselhoClasseNotaConsulta;
        private readonly IRepositorioConselhoClasseAluno repositorioConselhoClasseAluno;

        public AlterarConselhoClasseCommadHandler(
                            IMediator mediator,
                            IUnitOfWork unitOfWork,
                            IRepositorioConselhoClasseAlunoConsulta repositorioConselhoClasseAlunoConsulta,
                            IRepositorioConselhoClasseNotaConsulta repositorioConselhoClasseNotaConsulta,
                            IRepositorioConselhoClasseAluno repositorioConselhoClasseAluno,
                            IRepositorioConselhoClasseNota repositorioConselhoClasseNota) : base(mediator, repositorioConselhoClasseNota)
        {
            this.unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            this.repositorioConselhoClasseAlunoConsulta = repositorioConselhoClasseAlunoConsulta ?? throw new ArgumentNullException(nameof(repositorioConselhoClasseAlunoConsulta));
            this.repositorioConselhoClasseNotaConsulta = repositorioConselhoClasseNotaConsulta ?? throw new ArgumentNullException(nameof(repositorioConselhoClasseNotaConsulta));
            this.repositorioConselhoClasseAluno = repositorioConselhoClasseAluno ?? throw new ArgumentNullException(nameof(repositorioConselhoClasseAluno));
        }

        public async Task<ConselhoClasseNotaRetornoDto> Handle(AlterarConselhoClasseCommad request, CancellationToken cancellationToken)
        {
            AuditoriaDto auditoria = null;
            long conselhoClasseAlunoId = 0;
            bool enviarAprovacao = false;

            var conselhoClasseAluno = await repositorioConselhoClasseAlunoConsulta
                .ObterPorConselhoClasseAlunoCodigoAsync(request.ConselhoClasseId, request.CodigoAluno);

            unitOfWork.IniciarTransacao();
            try
            {
                conselhoClasseAlunoId = conselhoClasseAluno != null ? conselhoClasseAluno.Id : await SalvarConselhoClasseAlunoResumido(request.ConselhoClasseId, request.CodigoAluno);

                await mediator.Send(new InserirTurmasComplementaresCommand(request.Turma.Id, conselhoClasseAlunoId, request.CodigoAluno));

                var conselhoClasseNota = await repositorioConselhoClasseNotaConsulta
                    .ObterPorConselhoClasseAlunoComponenteCurricularAsync(conselhoClasseAlunoId, request.ConselhoClasseNotaDto.CodigoComponenteCurricular);

                double? notaAnterior = null;
                long? conceitoIdAnterior = null;

                if (conselhoClasseNota == null)
                    conselhoClasseNota = ObterConselhoClasseNota(request.ConselhoClasseNotaDto, conselhoClasseAlunoId);
                else
                {
                    notaAnterior = conselhoClasseNota.Nota;
                    conceitoIdAnterior = conselhoClasseNota.ConceitoId;

                    conselhoClasseNota.Justificativa = request.ConselhoClasseNotaDto.Justificativa;
                    if (request.ConselhoClasseNotaDto.Nota.HasValue)
                    {
                        // Gera histórico de alteração
                        if (conselhoClasseNota.Nota != null && conselhoClasseNota.Nota != request.ConselhoClasseNotaDto.Nota.Value)
                            await mediator.Send(new SalvarHistoricoNotaConselhoClasseCommand(conselhoClasseNota.Id, conselhoClasseNota.Nota.Value, request.ConselhoClasseNotaDto.Nota.Value));

                        conselhoClasseNota.Nota = request.ConselhoClasseNotaDto.Nota.Value;
                    }
                    else conselhoClasseNota.Nota = null;

                    if (request.ConselhoClasseNotaDto.Conceito.HasValue)
                    {
                        // Gera histórico de alteração
                        if (conselhoClasseNota.ConceitoId != request.ConselhoClasseNotaDto.Conceito.Value)
                            await mediator.Send(new SalvarHistoricoConceitoConselhoClasseCommand(conselhoClasseNota.Id, conselhoClasseNota.ConceitoId, request.ConselhoClasseNotaDto.Conceito.Value));

                        conselhoClasseNota.ConceitoId = request.ConselhoClasseNotaDto.Conceito.Value;
                    }
                }

                if (request.Turma.AnoLetivo == 2020)
                    ValidarNotasFechamentoConselhoClasse2020(conselhoClasseNota);

                if (conselhoClasseNota.Id > 0 || conselhoClasseAluno.AlteradoEm.HasValue)
                    await repositorioConselhoClasseAluno.SalvarAsync(conselhoClasseAluno);

                enviarAprovacao = await EnviarParaAprovacao(request.Turma, request.UsuarioLogado);
                if (enviarAprovacao)
                    await GerarWFAprovacao(conselhoClasseNota, request.Turma, request.Bimestre, request.UsuarioLogado, request.CodigoAluno, notaAnterior, conceitoIdAnterior);
                else
                    await repositorioConselhoClasseNota.SalvarAsync(conselhoClasseNota);

                auditoria = (AuditoriaDto)conselhoClasseAluno;

                unitOfWork.PersistirTransacao();
            }
            catch
            {
                unitOfWork.Rollback();
            }

            var alunos = await mediator
                .Send(new ObterAlunosPorTurmaQuery(request.Turma.CodigoTurma, consideraInativos: true));

            if (alunos == null || !alunos.Any())
                throw new NegocioException($"Não foram encontrados alunos para a turma {request.Turma.CodigoTurma} no Eol");

            var alunoFiltrado = alunos.FirstOrDefault(a => a.CodigoAluno == request.CodigoAluno);

            if (alunoFiltrado != null)
                await mediator.Send(new ConsolidarTurmaConselhoClasseAlunoCommand(request.CodigoAluno, request.Turma.Id, request.Bimestre.Value, alunoFiltrado.Inativo));

            var consolidacaoTurma = new ConsolidacaoTurmaDto(request.Turma.Id, request.Bimestre ?? 0);
            var mensagemParaPublicar = JsonConvert.SerializeObject(consolidacaoTurma);
            await mediator.Send(new PublicarFilaSgpCommand(RotasRabbitSgpFechamento.ConsolidarTurmaConselhoClasseSync, mensagemParaPublicar, Guid.NewGuid(), null));

            //Tratar após o fechamento da transação - ano letivo e turmaId
            if (!enviarAprovacao)
            {
                var consolidacaoNotaAlunoDto = new ConsolidacaoNotaAlunoDto()
                {
                    AlunoCodigo = request.CodigoAluno,
                    TurmaId = request.Turma.Id,
                    Bimestre = ObterBimestre(request.Bimestre),
                    AnoLetivo = request.Turma.AnoLetivo,
                    Nota = request.ConselhoClasseNotaDto.Nota,
                    ConceitoId = request.ConselhoClasseNotaDto.Conceito,
                    ComponenteCurricularId = request.ConselhoClasseNotaDto.CodigoComponenteCurricular
                };
                await mediator.Send(new ConsolidacaoNotaAlunoCommand(consolidacaoNotaAlunoDto));
            }

            var conselhoClasseNotaRetorno = new ConselhoClasseNotaRetornoDto()
            {
                ConselhoClasseId = request.ConselhoClasseId,
                FechamentoTurmaId = request.FechamentoTurmaId,
                Auditoria = auditoria,
                ConselhoClasseAlunoId = conselhoClasseAlunoId,
                EmAprovacao = enviarAprovacao
            };

            return conselhoClasseNotaRetorno;
        }

        private async Task<long> SalvarConselhoClasseAlunoResumido(long conselhoClasseId, string alunoCodigo)
        {
            var conselhoClasseAluno = new ConselhoClasseAluno()
            {
                AlunoCodigo = alunoCodigo,
                ConselhoClasseId = conselhoClasseId
            };

            return await repositorioConselhoClasseAluno.SalvarAsync(conselhoClasseAluno);
        }
    }
}
