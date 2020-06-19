﻿using MediatR;
using SME.SGP.Aplicacao.Integracoes;
using SME.SGP.Infra;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SME.SGP.Aplicacao
{
    public class ReiniciarSenhaCommandHandler : IRequestHandler<ReiniciarSenhaCommand, UsuarioReinicioSenhaDto>
    {
        public readonly IComandosUsuario comandosUsuario;
        private readonly IServicoEol servicoEOL;

        public ReiniciarSenhaCommandHandler(IComandosUsuario comandosUsuario, IServicoEol servicoEOL)
        {
            this.comandosUsuario = comandosUsuario ?? throw new ArgumentNullException(nameof(comandosUsuario));
            this.servicoEOL = servicoEOL ?? throw new ArgumentNullException(nameof(servicoEOL));
        }

        public async Task<UsuarioReinicioSenhaDto> Handle(ReiniciarSenhaCommand request, CancellationToken cancellationToken)
        {
            var usuario = await servicoEOL.ObterMeusDados(request.CodigoRf);

            var retorno = new UsuarioReinicioSenhaDto();

            if (String.IsNullOrEmpty(usuario.Email))
            {
                retorno.DeveAtualizarEmail = true;
                retorno.Mensagem = $"Usuário {request.CodigoRf} - {usuario.Nome} não possui email cadastrado!";
            }
            else
            {
                await servicoEOL.ReiniciarSenha(request.CodigoRf);
                retorno.Mensagem = $"Senha do usuário {request.CodigoRf} - {usuario.Nome} reiniciada com sucesso. O usuário deverá informar a senha {FormatarSenha(request.CodigoRf)} no seu próximo acesso";
                retorno.DeveAtualizarEmail = false;
            }

            return retorno;
        }

        private string FormatarSenha(string codigoRf) 
        {
            string sufixoSenha = codigoRf.Substring(codigoRf.Length - 4, 4);
            return $"Sgp{sufixoSenha}";
        }
    }
}
