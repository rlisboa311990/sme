﻿using MediatR;
using SME.SGP.Dominio;
using SME.SGP.Dominio.Interfaces;
using SME.SGP.Infra;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SME.SGP.Aplicacao
{
    public class RegistrarRegistroAcaoCommandHandler : IRequestHandler<RegistrarRegistroAcaoCommand, ResultadoRegistroAcaoBuscaAtivaDto>
    {
        private readonly IRepositorioRegistroAcaoBuscaAtiva repositorioRegistroAcao;
        private readonly IMediator mediator;

        public RegistrarRegistroAcaoCommandHandler(IRepositorioRegistroAcaoBuscaAtiva repositorioRegistroAcao, IMediator mediator)
        {
            this.repositorioRegistroAcao = repositorioRegistroAcao ?? throw new ArgumentNullException(nameof(repositorioRegistroAcao));
            this.mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public async Task<ResultadoRegistroAcaoBuscaAtivaDto> Handle(RegistrarRegistroAcaoCommand request, CancellationToken cancellationToken)
        {
            var registroAcao = MapearParaEntidade(request);
            var id = await repositorioRegistroAcao.SalvarAsync(registroAcao);
            var resultado = new ResultadoRegistroAcaoBuscaAtivaDto(id);
            resultado.Auditoria = (AuditoriaDto)registroAcao;
            return resultado;
        }

        private RegistroAcaoBuscaAtiva MapearParaEntidade(RegistrarRegistroAcaoCommand request)
            => new ()
            {
                TurmaId = request.TurmaId,
                AlunoCodigo = request.AlunoCodigo,
                AlunoNome = request.AlunoNome
            };
    }
}
