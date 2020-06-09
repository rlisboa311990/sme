﻿using MediatR;
using Sentry;
using SME.SGP.Aplicacao.Commands.Aulas.InserirAula;
using SME.SGP.Dominio;
using SME.SGP.Infra;
using SME.SGP.Infra.Dtos.Aula;
using System;
using System.Threading.Tasks;

namespace SME.SGP.Aplicacao
{
    public class InserirAulaUseCase : AbstractUseCase, IInserirAulaUseCase
    {
        public InserirAulaUseCase(IMediator mediator): base(mediator)
        {
        }

        public async Task<RetornoBaseDto> Executar(InserirAulaDto inserirAulaDto)
        {
            var usuarioLogado = await mediator.Send(new ObterUsuarioLogadoQuery());

            if (inserirAulaDto.RecorrenciaAula == RecorrenciaAula.AulaUnica)
            {
                return await mediator.Send(new InserirAulaUnicaCommand(usuarioLogado, inserirAulaDto.DataAula, inserirAulaDto.Quantidade, inserirAulaDto.CodigoTurma, inserirAulaDto.CodigoComponenteCurricular, inserirAulaDto.NomeComponenteCurricular, inserirAulaDto.TipoCalendarioId, inserirAulaDto.TipoAula, inserirAulaDto.CodigoUe, inserirAulaDto.EhRegencia));
            }
            else
            {
                try
                {
                    mediator.Enfileirar(new InserirAulaRecorrenteCommand(usuarioLogado, inserirAulaDto.DataAula, inserirAulaDto.Quantidade, inserirAulaDto.CodigoTurma, inserirAulaDto.CodigoComponenteCurricular, inserirAulaDto.NomeComponenteCurricular, inserirAulaDto.TipoCalendarioId, inserirAulaDto.TipoAula, inserirAulaDto.CodigoUe, inserirAulaDto.EhRegencia, inserirAulaDto.RecorrenciaAula));
                    return new RetornoBaseDto("Serão cadastradas aulas recorrentes, em breve você receberá uma notificação com o resultado do processamento.");
                }
                catch (Exception ex)
                {
                    SentrySdk.AddBreadcrumb("Criação de aulas recorrentes", "Hangfire");
                    SentrySdk.CaptureException(ex);
                }
                return new RetornoBaseDto("Ocorreu um erro ao solicitar a criação de aulas recorrentes, por favor tente novamente.");
            }
        }
    }
}
