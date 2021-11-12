﻿using MediatR;
using SME.SGP.Dominio.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SME.SGP.Aplicacao
{
    public class SalvarAtividadeInfantilGsaNoMuralCommandHandler : AsyncRequestHandler<SalvarAtividadeInfantilGsaNoMuralCommand>
    {
        //private readonly IRepositorioAtividadeInfantil repositorioAtividadeInfantil;

        //public SalvarAtividadeInfantilGsaNoMuralCommandHandler(IRepositorioInfantil repositorioAtividadeInfantil)
        //{
        //    this.repositorioAtividadeInfantil = repositorioAtividadeInfantil ?? throw new ArgumentNullException(nameof(repositorioAtividadeInfantil));
        //}

        protected override async Task Handle(SalvarAtividadeInfantilGsaNoMuralCommand request, CancellationToken cancellationToken)
        {
            //var atividadeInfantil = await repositorioAtividadeInfantil.ObterPorClassroomId(request.AvisoClassroomId);

            //atividadeInfantil = MapearParaEntidade(request, atividadeInfantil);

            //await repositorioAtividadeInfantil.SalvarAsync(atividadeInfantil);
        }

        private Dominio.AtividadeInfantil MapearParaEntidade(SalvarAtividadeInfantilGsaNoMuralCommand request, Dominio.AtividadeInfantil atividadeInfantil)
        {
            if (atividadeInfantil == null)
                atividadeInfantil = new Dominio.AtividadeInfantil();

            atividadeInfantil.AulaId = request.AulaId;
            atividadeInfantil.Mensagem = request.Mensagem;
            atividadeInfantil.AvisoClassroomId = request.AvisoClassroomId;
            atividadeInfantil.CriadoRF = request.UsuarioRf;
            atividadeInfantil.CriadoEm = request.DataCriacao;
            atividadeInfantil.AlteradoRF = request.DataAlteracao.HasValue ? request.UsuarioRf : String.Empty;
            atividadeInfantil.AlteradoEm = request.DataAlteracao;
            atividadeInfantil.Email = request.Email;

            return atividadeInfantil;
        }
    }
}
