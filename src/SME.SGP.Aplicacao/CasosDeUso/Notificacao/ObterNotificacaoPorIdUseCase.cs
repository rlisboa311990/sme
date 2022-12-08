﻿using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MediatR;
using SME.SGP.Dominio;
using SME.SGP.Infra;

namespace SME.SGP.Aplicacao;

public class ObterNotificacaoPorIdUseCase : IObterNotificacaoPorIdUseCase
{
    private readonly IMediator mediator;

    public ObterNotificacaoPorIdUseCase(IMediator mediator)
    {
        this.mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    public async Task<NotificacaoDetalheDto> Executar(long param)
    {
        var notificacao = await mediator.Send(new ObterNotificacaoPorIdQuery(param));

        if (notificacao == null)
            throw new NegocioException($"Notificação de Id: '{param}' não localizada.");

        if (notificacao.Status != NotificacaoStatus.Lida && notificacao.MarcarComoLidaAoObterDetalhe())
        {
            await mediator.Send(new SalvarNotificacaoCommand(notificacao));
            await mediator.Send(new NotificarLeituraNotificacaoCommand(notificacao));
        }

        var retorno = await MapearEntidadeParaDetalheDto(notificacao);

        return retorno;        
    }
    
    private async Task<NotificacaoDetalheDto> MapearEntidadeParaDetalheDto(Notificacao retorno)
    {
        var codigoRelatorio = string.Empty;
        var tipoRelatorio = 0;
        var relatorioExiste = true;

        if (NotificacaoTipo.Relatorio == retorno.Tipo)
            codigoRelatorio = ObterCodigoArquivo(retorno.Mensagem);

        if (codigoRelatorio.Any())
            tipoRelatorio = await mediator.Send(new ObterTipoRelatorioPorCodigoQuery(codigoRelatorio));

        if (!string.IsNullOrEmpty(codigoRelatorio) && (tipoRelatorio != (int)TipoRelatorio.Itinerancias))
            relatorioExiste = await VerificarSeArquivoExiste(codigoRelatorio);

        return new NotificacaoDetalheDto
        {
            AlteradoEm = retorno.AlteradoEm.ToString(),
            AlteradoPor = retorno.AlteradoPor,
            CriadoEm = retorno.CriadoEm.ToString("dd/MM/yyyy"),
            CriadoPor = retorno.CriadoPor,
            Id = retorno.Id,
            Mensagem = relatorioExiste ? retorno.Mensagem : "O arquivo não está mais disponível, solicite a geração do relatório novamente.",
            Situacao = retorno.Status.ToString(),
            Tipo = retorno.Tipo.GetAttribute<DisplayAttribute>().Name,
            Titulo = retorno.Titulo,
            MostrarBotaoRemover = retorno.PodeRemover,
            MostrarBotoesDeAprovacao = retorno.DeveAprovar,
            MostrarBotaoMarcarComoLido = retorno.DeveMarcarComoLido,
            CategoriaId = (int)retorno.Categoria,
            TipoId = (int)retorno.Tipo,
            StatusId = (int)retorno.Status,
            Codigo = retorno.Codigo,
            Observacao = retorno.WorkflowAprovacaoNivel == null ? string.Empty : retorno.WorkflowAprovacaoNivel.Observacao
        };
    }  
    
    private static string ObterCodigoArquivo(string mensagem)
    {
        const string pattern = "[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}";
        Regex rg = new(pattern);
        var codigo = rg.Match(mensagem);
        return codigo.ToString();
    } 
    
    private async Task<bool> VerificarSeArquivoExiste(string codigoArquivo)
    {
        var guidRelatorio = new Guid(codigoArquivo);
        return await mediator.Send(new VerificarExistenciaRelatorioPorCodigoQuery(guidRelatorio));
    }        
}