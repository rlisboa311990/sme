﻿using MediatR;
using SME.SGP.Dominio.Interfaces;
using SME.SGP.Infra;
using SME.SGP.Infra.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SME.SGP.Dominio;
using SME.SGP.Dto;

namespace SME.SGP.Aplicacao
{
    public class ObterEncaminhamentosNAAPAQueryHandler : ConsultasBase, IRequestHandler<ObterEncaminhamentosNAAPAQuery, PaginacaoResultadoDto<EncaminhamentoNAAPAResumoDto>>
    {
        public IMediator mediator { get; }
        public IRepositorioEncaminhamentoNAAPA repositorioEncaminhamentoNAAPA { get; }


        public ObterEncaminhamentosNAAPAQueryHandler(IContextoAplicacao contextoAplicacao, IMediator mediator, IRepositorioEncaminhamentoNAAPA repositorioEncaminhamentoNAAPA) : base(contextoAplicacao)
        {
            this.mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            this.repositorioEncaminhamentoNAAPA = repositorioEncaminhamentoNAAPA ?? throw new ArgumentNullException(nameof(repositorioEncaminhamentoNAAPA));
        }

        public async Task<PaginacaoResultadoDto<EncaminhamentoNAAPAResumoDto>> Handle(ObterEncaminhamentosNAAPAQuery request, CancellationToken cancellationToken)
        {
            try
            {
                // var turmas = await mediator.Send(new ObterAbrangenciaTurmasPorUeModalidadePeriodoHistoricoAnoLetivoTiposQuery(request.CodigoUe, 
                //     0, 0, request.ExibirHistorico, DateTimeExtension.HorarioBrasilia().Year, null, true));

                var turmas = new List<AbrangenciaTurmaRetorno>();
                var turmasCodigos = turmas != null || turmas.Any() ? turmas.Select(s => s.Codigo) : null;

                return await MapearParaDto(await repositorioEncaminhamentoNAAPA.ListarPaginado(request.ExibirHistorico, request.AnoLetivo, request.DreId, 
                    request.CodigoUe,request.NomeAluno, request.DataAberturaQueixaInicio, request.DataAberturaQueixaFim, request.Situacao, 
                    request.Prioridade, turmasCodigos.ToArray(), Paginacao),request.AnoLetivo);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private async Task<PaginacaoResultadoDto<EncaminhamentoNAAPAResumoDto>> MapearParaDto(PaginacaoResultadoDto<EncaminhamentoNAAPAResumoDto> resultadoDto,int anoLetivo)
        {
            return new PaginacaoResultadoDto<EncaminhamentoNAAPAResumoDto>()
            {
                TotalPaginas = resultadoDto.TotalPaginas,
                TotalRegistros = resultadoDto.TotalRegistros,
                Items = await MapearParaDto(resultadoDto.Items, anoLetivo)
            };
        }

        private async Task<IEnumerable<EncaminhamentoNAAPAResumoDto>> MapearParaDto(IEnumerable<EncaminhamentoNAAPAResumoDto> encaminhamentos, int anoLetivo)
        {
            var listaEncaminhamentos = new List<EncaminhamentoNAAPAResumoDto>();

            foreach (var encaminhamento in encaminhamentos)
            {
                var retorno = await mediator.Send(new ObterTurmasAlunoPorFiltroQuery(encaminhamento.CodigoAluno, anoLetivo, false));
                var aluno = retorno.OrderByDescending(a => a.DataSituacao)?.FirstOrDefault();                

                var ehAtendidoAEE = await mediator.Send(new VerificaEstudantePossuiPlanoAEEPorCodigoEAnoQuery(aluno.CodigoAluno, anoLetivo));
                listaEncaminhamentos.Add(new EncaminhamentoNAAPAResumoDto()
                {
                    Id = encaminhamento.Id,
                    Ue = $"{encaminhamento.TipoEscola.ShortName()} {encaminhamento.UeNome}",
                    UeNome = encaminhamento.UeNome,
                    TipoEscola = encaminhamento.TipoEscola,
                    CodigoAluno = encaminhamento.CodigoAluno,
                    NomeAluno = encaminhamento.NomeAluno,
                    Prioridade = encaminhamento.Prioridade,
                    Situacao = encaminhamento.Situacao,
                    DataAberturaQueixaInicio = encaminhamento.DataAberturaQueixaInicio
                });
            }

            return listaEncaminhamentos;
        }
    }
}