﻿using MediatR;
using SME.SGP.Dominio;
using SME.SGP.Infra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SME.SGP.Aplicacao
{
    public abstract class NotificacaoNotaFechamentoCommandBase<TRequest, TResponse> : IRequestHandler<TRequest, TResponse> 
        where TRequest : IRequest<TResponse>
    {
        protected const string MENSAGEM_DINAMICA_TABELA_POR_ALUNO = "<mensagemDinamicaTabelaPorAluno>";
        
        protected readonly IMediator mediator;
        protected List<TurmasDoAlunoDto> Alunos;
        protected IEnumerable<WfAprovacaoNotaFechamentoTurmaDto> WFAprovacoes;
        
        public NotificacaoNotaFechamentoCommandBase(IMediator mediator)
        {
            this.mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        protected async Task CarregarInformacoesParaNotificacao(IEnumerable<WfAprovacaoNotaFechamentoTurmaDto> wfAprovacoes)
        {
            WFAprovacoes = wfAprovacoes;
            await CarregarTodosAlunos();
        }

        public abstract Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken);

        protected virtual string ObterTabelaNotas(List<WfAprovacaoNotaFechamentoTurmaDto> aprovacoesPorTurma)
        {
            return aprovacoesPorTurma.FirstOrDefault().ConceitoAnteriorId.HasValue || aprovacoesPorTurma.FirstOrDefault().WfAprovacao.ConceitoId.HasValue
                ? MontarTabelaNotasRegencia(aprovacoesPorTurma,aprovacoesPorTurma.FirstOrDefault().LancaNota)
                : MontarTabelaNotas(aprovacoesPorTurma);
        }
        private async Task CarregarTodosAlunos()
        {
            var codigos = WFAprovacoes.Select(wf => long.Parse(wf.CodigoAluno)).ToArray();
            var wfAprovacaoParecerConclusivo = WFAprovacoes.FirstOrDefault();

            if (wfAprovacaoParecerConclusivo == null)
                return;
            
            var anoLetivo = wfAprovacaoParecerConclusivo.AnoLetivo;
            Alunos = (await ObterAlunos(codigos, anoLetivo)).OrderBy(c => c.NomeAluno).Distinct().ToList();
        }

        private async Task<IEnumerable<TurmasDoAlunoDto>> ObterAlunos(long[] codigos, int anoLetivo)
            => await mediator.Send(new ObterAlunosEolPorCodigosEAnoQuery(codigos, anoLetivo));
        
        private string MontarTabelaNotasRegencia(IEnumerable<WfAprovacaoNotaFechamentoTurmaDto> notasAprovacao, bool lancaNota)
		{
			var mensagem = new StringBuilder();
			mensagem.AppendLine("<table style='margin-left: auto; margin-right: auto;' border='2' cellpadding='5'>");
			mensagem.AppendLine("<tr>");
			mensagem.AppendLine("<td style='padding: 20px; text-align:left;'><b>Componente Curricular</b></td>");
			mensagem.AppendLine("<td style='padding: 20px; text-align:left;'><b>Estudante</b></td>");
			mensagem.AppendLine("<td style='padding: 5px; text-align:left;'><b>Valor anterior</b></td>");
			mensagem.AppendLine("<td style='padding: 5px; text-align:left;'><b>Novo valor</b></td>");
			mensagem.AppendLine("<td style='padding: 10px; text-align:left;'><b>Usuário que alterou</b></td>");
			mensagem.AppendLine("<td style='padding: 10px; text-align:left;'><b>Data da alteração</b></td>");
			mensagem.AppendLine("</tr>");

			notasAprovacao = notasAprovacao
				.OrderBy(n => n.WfAprovacao.AlteradoEm)
				.ThenBy(n => n.WfAprovacao.CriadoEm);

			var agrupamentoAlunoNotasAprovacoes = (from nota in notasAprovacao
				join aluno in Alunos on nota.CodigoAluno equals aluno.CodigoAluno.ToString()
				select new AlunoNotaFechamentoDto
				{
					CodigoAluno = aluno.CodigoAluno.ToString(),
					NumeroAlunoChamada = aluno?.NumeroAlunoChamada, 
					NomeAluno = aluno.NomeAluno,
					AlteradoPor = nota.WfAprovacao.AlteradoPor,
					AlteradoRf = nota.WfAprovacao.AlteradoRF,
					AlteradoEm = nota.WfAprovacao.AlteradoEm,
					CriadoEm = nota.WfAprovacao.CriadoEm,
					CriadoRf = nota.WfAprovacao.CriadoRF,
					CriadoPor = nota.WfAprovacao.CriadoPor,
					ConceitoId = nota.WfAprovacao.ConceitoId,
					ConceitoAnteriorId = nota.ConceitoAnteriorId,
					ComponenteCurricularDescricao = nota.ComponenteCurricularDescricao,
					NotaAnterior = nota.NotaAnterior,
					Nota = nota.WfAprovacao.Nota,
				}).ToList();
				
			foreach (var alunoNotaAprovacao in agrupamentoAlunoNotasAprovacoes)
			{
				string nomeUsuarioAlterou = alunoNotaAprovacao.AlteradoPor ?? alunoNotaAprovacao.CriadoPor;
				string rfUsuarioAlterou = alunoNotaAprovacao.AlteradoRf ?? alunoNotaAprovacao.CriadoRf;

				var (dataNotificacao, horaNotificacao) = RetornarDataHoraNotificacao(alunoNotaAprovacao.AlteradoEm, alunoNotaAprovacao.CriadoEm);

				mensagem.AppendLine("<tr>");

				if (alunoNotaAprovacao.Nota.HasValue && lancaNota)
				{
					mensagem.Append($"<td style='padding: 20px; text-align:left;'>{alunoNotaAprovacao.ComponenteCurricularDescricao}</td>");
					mensagem.Append($"<td style='padding: 20px; text-align:left;'>{alunoNotaAprovacao.NumeroAlunoChamada} - {alunoNotaAprovacao.NomeAluno} ({alunoNotaAprovacao.CodigoAluno})</td>");
					mensagem.Append($"<td style='padding: 5px; text-align:right;'>{ObterNota(alunoNotaAprovacao.NotaAnterior.Value)}</td>");
					mensagem.Append($"<td style='padding: 5px; text-align:right;'>{ObterNota(alunoNotaAprovacao.Nota.Value)}</td>");
					mensagem.Append($"<td style='padding: 10px; text-align:right;'>{nomeUsuarioAlterou} ({rfUsuarioAlterou}) </td>");
					mensagem.Append($"<td style='padding: 10px; text-align:right;'>{dataNotificacao} ({horaNotificacao}) </td>");
				}
				else
				{
					mensagem.Append($"<td style='padding: 20px; text-align:left;'>{alunoNotaAprovacao.ComponenteCurricularDescricao}</td>");
					mensagem.Append($"<td style='padding: 20px; text-align:left;'>{alunoNotaAprovacao.NumeroAlunoChamada} - {alunoNotaAprovacao.NomeAluno} ({alunoNotaAprovacao.CodigoAluno})</td>");
					mensagem.Append($"<td style='padding: 5px; text-align:right;'>{ObterConceito(alunoNotaAprovacao.ConceitoAnteriorId)}</td>");
					mensagem.Append($"<td style='padding: 5px; text-align:right;'>{ObterConceito(alunoNotaAprovacao.ConceitoId)}</td>");
					mensagem.Append($"<td style='padding: 10px; text-align:right;'>{nomeUsuarioAlterou} ({rfUsuarioAlterou}) </td>");
					mensagem.Append($"<td style='padding: 10px; text-align:right;'>{dataNotificacao} ({horaNotificacao}) </td>");
				}

				mensagem.AppendLine("</tr>");
			}
			mensagem.AppendLine("</table>");
			mensagem.AppendLine("<p>Você precisa aceitar esta notificação para que a alteração seja considerada válida.</p>");

			return mensagem.ToString();
		}
	    private string MontarTabelaNotas(IEnumerable<WfAprovacaoNotaFechamentoTurmaDto> notasAprovacao)
		{
			var mensagem = new StringBuilder();

			mensagem.AppendLine("<table style='margin-left: auto; margin-right: auto;' border='2' cellpadding='5'>");
			mensagem.AppendLine("<tr>");
			mensagem.AppendLine("<td style='padding: 20px; text-align:left;'><b>Componente Curricular</b></td>");
			mensagem.AppendLine("<td style='padding: 20px; text-align:left;'><b>Estudante</b></td>");
			mensagem.AppendLine("<td style='padding: 5px; text-align:left;'><b>Valor anterior</b></td>");
			mensagem.AppendLine("<td style='padding: 5px; text-align:left;'><b>Novo valor</b></td>");
			mensagem.AppendLine("<td style='padding: 10px; text-align:left;'><b>Usuário que alterou</b></td>");
			mensagem.AppendLine("<td style='padding: 10px; text-align:left;'><b>Data da alteração</b></td>");
			mensagem.AppendLine("</tr>");

			notasAprovacao = notasAprovacao
				.OrderBy(n => n.WfAprovacao.AlteradoEm)
				.ThenBy(n => n.WfAprovacao.CriadoEm);

			var agrupamentoAlunoNotasAprovacoes = (from nota in notasAprovacao
				join aluno in Alunos on nota.CodigoAluno equals aluno.CodigoAluno.ToString()
				select new AlunoNotaFechamentoDto
				{
					CodigoAluno = aluno.CodigoAluno.ToString(),
					NumeroAlunoChamada = aluno?.NumeroAlunoChamada, 
					NomeAluno = aluno.NomeAluno,
					AlteradoPor = nota.WfAprovacao.AlteradoPor,
					AlteradoRf = nota.WfAprovacao.AlteradoRF,
					AlteradoEm = nota.WfAprovacao.AlteradoEm,
					CriadoEm = nota.WfAprovacao.CriadoEm,
					CriadoRf = nota.WfAprovacao.CriadoRF,
					CriadoPor = nota.WfAprovacao.CriadoPor,
					ConceitoId = nota.WfAprovacao.ConceitoId,
					ComponenteCurricularDescricao = nota.ComponenteCurricularDescricao,
					NotaAnterior = nota.NotaAnterior,
					Nota = nota.WfAprovacao.Nota,
				}).ToList();
				
			foreach (var alunoNotaAprovacao in agrupamentoAlunoNotasAprovacoes)
			{
				string nomeUsuarioAlterou = alunoNotaAprovacao.AlteradoPor ?? alunoNotaAprovacao.CriadoPor;
				string rfUsuarioAlterou = alunoNotaAprovacao.AlteradoRf ?? alunoNotaAprovacao.CriadoRf;
				var (dataNotificacao, horaNotificacao) = RetornarDataHoraNotificacao(alunoNotaAprovacao.AlteradoEm, alunoNotaAprovacao.CriadoEm);

				mensagem.AppendLine("<tr>");

				if (alunoNotaAprovacao.Nota.HasValue)
				{
					mensagem.Append($"<td style='padding: 20px; text-align:left;'>{alunoNotaAprovacao.ComponenteCurricularDescricao}</td>");
					mensagem.Append($"<td style='padding: 20px; text-align:left;'>{alunoNotaAprovacao.NumeroAlunoChamada} - {alunoNotaAprovacao.NomeAluno} ({alunoNotaAprovacao.CodigoAluno})</td>");
					mensagem.Append($"<td style='padding: 5px; text-align:right;'>{ObterNota(alunoNotaAprovacao.NotaAnterior)}</td>");
					mensagem.Append($"<td style='padding: 5px; text-align:right;'>{ObterNota(alunoNotaAprovacao.Nota)}</td>");
					mensagem.Append($"<td style='padding: 10px; text-align:right;'> {nomeUsuarioAlterou} ({rfUsuarioAlterou}) </td>");
					mensagem.Append($"<td style='padding: 10px; text-align:right;'>{dataNotificacao} ({horaNotificacao}) </td>");
				}
				else
				{
					mensagem.Append($"<td style='padding: 20px; text-align:left;'>{alunoNotaAprovacao.ComponenteCurricularDescricao}</td>");
					mensagem.Append($"<td style='padding: 20px; text-align:left;'>{alunoNotaAprovacao.NumeroAlunoChamada} - {alunoNotaAprovacao.NomeAluno} ({alunoNotaAprovacao.CodigoAluno})</td>");
					mensagem.Append($"<td style='padding: 5px; text-align:right;'>{ObterConceito(alunoNotaAprovacao.ConceitoAnteriorId)}</td>");
					mensagem.Append($"<td style='padding: 5px; text-align:right;'>{ObterConceito(alunoNotaAprovacao.ConceitoId)}</td>");
					mensagem.Append($"<td style='padding: 10px; text-align:right;'> {nomeUsuarioAlterou} ({rfUsuarioAlterou}) </td>");
					mensagem.Append($"<td style='padding: 10px; text-align:right;'>{dataNotificacao} ({horaNotificacao}) </td>");
				}

				mensagem.AppendLine("</tr>");
			}
			mensagem.AppendLine("</table>");
			mensagem.AppendLine("<p>Você precisa aceitar esta notificação para que a alteração seja considerada válida.</p>");

			return mensagem.ToString();
		}
	    
	    private static (string dataNotificacao, string horaNotificacao) RetornarDataHoraNotificacao(DateTime? alteradoEm, DateTime criadoEm)
	    {
		    var dataFormatada = criadoEm.ToString("dd/MM/yyyy");
		    var horaFormatada = criadoEm.ToString("HH:mm:ss");

		    if (alteradoEm.HasValue)
		    {
			    dataFormatada = alteradoEm.Value.ToString("dd/MM/yyyy");
			    horaFormatada = alteradoEm.Value.ToString("HH:mm:ss");
		    }

		    return (dataFormatada, horaFormatada);
	    }
	    
	    private static string ObterNota(double? nota)
	    {
		    if (!nota.HasValue)
			    return string.Empty;

		    return nota.ToString();
	    }

	    private static string ObterConceito(long? conceitoId)
	    {
		    if (!conceitoId.HasValue)
			    return string.Empty;

		    if (conceitoId == (int)ConceitoValores.P)
			    return ConceitoValores.P.ToString();
		    else if (conceitoId == (int)ConceitoValores.S)
			    return ConceitoValores.S.ToString();
		    else
			    return ConceitoValores.NS.ToString();
	    }
    }
 }
