﻿using MediatR;
using SME.SGP.Dominio;
using SME.SGP.Dominio.Interfaces;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SME.SGP.Dominio.Constantes.MensagensNegocio;

namespace SME.SGP.Aplicacao
{
    public class SalvarDocumentoCommandHandler : AbstractUseCase, IRequestHandler<SalvarDocumentoCommand, bool>
    {
        private readonly IRepositorioDocumento repositorioDocumento;

        public SalvarDocumentoCommandHandler(IRepositorioDocumento repositorioDocumento, IMediator mediator) : base(mediator)
        {
            this.repositorioDocumento = repositorioDocumento ?? throw new ArgumentNullException(nameof(repositorioDocumento));
        }

        public async Task<bool> Handle(SalvarDocumentoCommand request, CancellationToken cancellationToken)
        {
            var arquivos =
                (await mediator.Send(new ObterArquivosPorCodigosQuery(request.SalvarDocumentoDto.ArquivosCodigos),
                    cancellationToken)).ToList();

            if (arquivos == null || !arquivos.Any())
                throw new NegocioException(MensagemNegocioDocumento.NAO_FORAM_ENCONTRADOS_ARQUIVOS_COM_ESTES_CODIGOS);

            var classificacaoDocumento =
                await mediator.Send(
                    new ObterClassificacaoDocumentoPorIdQuery(request.SalvarDocumentoDto.ClassificacaoId),
                    cancellationToken);

            if (!classificacaoDocumento.EhRegistroMultiplo && arquivos.Count > 1)
            {
                throw new NegocioException(string.Format(
                    MensagemNegocioDocumento.NAO_EH_PERMITIDO_MULTIPLOS_ARQUIVOS_CLASSIFICACAO_DOCUMENTO,
                    classificacaoDocumento.Descricao));
            }

            var existeArquivo = await mediator.Send(
                new VerificaUsuarioPossuiArquivoQuery(request.SalvarDocumentoDto.TipoDocumentoId,
                    request.SalvarDocumentoDto.ClassificacaoId, request.SalvarDocumentoDto.UsuarioId,
                    request.SalvarDocumentoDto.UeId), cancellationToken);
            
            if (existeArquivo)
                throw new NegocioException(MensagemNegocioDocumento.ESTE_USUARIO_JA_POSSUI_ARQUIVO);

            if (request.SalvarDocumentoDto.TipoDocumentoId == 1)
            {
                var usuario = await mediator.Send(new ObterUsuarioPorIdQuery(request.SalvarDocumentoDto.UsuarioId), cancellationToken);
                var tiposDocumentos = await mediator.Send(new ObterTipoDocumentoClassificacaoQuery(), cancellationToken);

                var classificacao = tiposDocumentos
                    .FirstOrDefault(t => t.Id == request.SalvarDocumentoDto.TipoDocumentoId)?.Classificacoes
                    .FirstOrDefault(c => c.Id == request.SalvarDocumentoDto.ClassificacaoId);

                if (!usuario.Perfis.Any(u => classificacao != null && u.NomePerfil == classificacao.Classificacao))
                    throw new NegocioException(MensagemNegocioDocumento.USUARIO_VINCULADO_DOCUMENTO_NAO_POSSUIR_PERFIL_CORRESPONDE_TIPO_PLANO_SELECIONADO);
            }

            if (await mediator.Send(
                    new ValidarTipoDocumentoDaClassificacaoQuery(request.SalvarDocumentoDto.ClassificacaoId,
                        Dominio.Enumerados.TipoDocumento.PlanoTrabalho), cancellationToken)
                && await mediator.Send(new ValidarUeEducacaoInfantilQuery(request.SalvarDocumentoDto.UeId),
                    cancellationToken))
            {
                throw new NegocioException(MensagemNegocioDocumento.ESCOLAR_EDUCACAO_INFANTIL_NAO_PODEM_FAZER_UPLOAD_PLANO_TRABALHO);
            }

            foreach (var documento in arquivos.Select(arquivo => new Documento
                     {
                         ClassificacaoDocumentoId = request.SalvarDocumentoDto.ClassificacaoId,
                         UsuarioId = request.SalvarDocumentoDto.UsuarioId,
                         UeId = request.SalvarDocumentoDto.UeId,
                         ArquivoId = arquivo.Id,
                         AnoLetivo = request.SalvarDocumentoDto.AnoLetivo,
                         TurmaId = request.SalvarDocumentoDto.TurmaId,
                         ComponenteCurricularId = request.SalvarDocumentoDto.ComponenteCurricularId
                     }))
            {
                await repositorioDocumento.SalvarAsync(documento);
            }

            return true;
        }
    }
}
