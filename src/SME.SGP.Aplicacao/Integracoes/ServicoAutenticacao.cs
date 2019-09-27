﻿using SME.SGP.Dominio;
using SME.SGP.Dominio.Interfaces;
using SME.SGP.Dto;
using System.Threading.Tasks;

namespace SME.SGP.Aplicacao.Integracoes
{
    public class ServicoAutenticacao : IServicoAutenticacao
    {
        private readonly IServicoEOL servicoEOL;
        private readonly IServicoUsuario servicoUsuario;

        public ServicoAutenticacao(IServicoEOL servicoEOL, IServicoUsuario servicoUsuario)
        {
            this.servicoEOL = servicoEOL ?? throw new System.ArgumentNullException(nameof(servicoEOL));
            this.servicoUsuario = servicoUsuario ?? throw new System.ArgumentNullException(nameof(servicoUsuario));
        }

        public async Task<AlterarSenhaRespostaDto> AlterarSenhaPrimeiroAcesso(PrimeiroAcessoDto primeiroAcessoDto)
        {
            var usuario = new Usuario();

            if (primeiroAcessoDto.UsuarioExterno)
                usuario.CPF = primeiroAcessoDto.RFCPF;
            else
                usuario.CodigoRf = primeiroAcessoDto.RFCPF;

            usuario.Login = primeiroAcessoDto.Usuario;
            usuario.Senha = primeiroAcessoDto.NovaSenha;

            usuario.validarSenha();

            //return await servicoEOL.AlterarSenha(usuario.Login, usuario.Senha);

            //Irei descomentar assim que a api do EOL for mergeada para a master
            return new AlterarSenhaRespostaDto { SenhaAlterada = true };
        }

        public async Task<UsuarioAutenticacaoRetornoDto> AutenticarNoEol(string login, string senha)
        {
            var retornoServicoEol = await servicoEOL.Autenticar(login, senha);

            var retornoDto = new UsuarioAutenticacaoRetornoDto();
            if (retornoServicoEol != null)
            {
                retornoDto.Autenticado = retornoServicoEol.Status == AutenticacaoStatusEol.Ok;
                retornoDto.ModificarSenha = retornoServicoEol.Status == AutenticacaoStatusEol.SenhaPadrao;

                var usuario = servicoUsuario.ObterUsuarioPorCodigoRfOuAdiciona(retornoServicoEol.CodigoRf);

                retornoDto.Token = GeraTokenSeguranca(usuario);
            }

            return retornoDto;
        }

        private string GeraTokenSeguranca(Usuario usuario)
        {
            return string.Empty;
        }
    }
}