﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SME.SGP.Dominio
{
    public class Usuario : EntidadeBase
    {
        private const string MENSAGEM_ERRO_USUARIO_SEM_ACESSO = "Usuário sem perfis de acesso.";
        private readonly Guid PERFIL_AD = Guid.Parse("45E1E074-37D6-E911-ABD6-F81654FE895D");
        private readonly Guid PERFIL_CJ = Guid.Parse("41e1e074-37d6-e911-abd6-f81654fe895d");
        private readonly Guid PERFIL_CP = Guid.Parse("44E1E074-37D6-E911-ABD6-F81654FE895D");
        private readonly Guid PERFIL_DIRETOR = Guid.Parse("46E1E074-37D6-E911-ABD6-F81654FE895D");
        private readonly Guid PERFIL_PROFESSOR = Guid.Parse("40E1E074-37D6-E911-ABD6-F81654FE895D");
        private readonly Guid PERFIL_SECRETARIO = Guid.Parse("43E1E074-37D6-E911-ABD6-F81654FE895D");
        private readonly Guid PERFIL_SUPERVISOR = Guid.Parse("4EE1E074-37D6-E911-ABD6-F81654FE895D");
        public string CodigoRf { get; set; }
        public string Email { get; set; }
        public DateTime? ExpiracaoRecuperacaoSenha { get; set; }
        public string Login { get; set; }
        public string Nome { get; set; }
        public IEnumerable<Notificacao> Notificacoes { get { return notificacoes; } }
        public Guid PerfilAtual { get; private set; }
        public IEnumerable<PrioridadePerfil> Perfis { get; private set; }
        public Guid? TokenRecuperacaoSenha { get; set; }
        public DateTime UltimoLogin { get; set; }
        private IList<Notificacao> notificacoes { get; set; }

        public void Adicionar(Notificacao notificacao)
        {
            if (notificacao != null)
                notificacoes.Add(notificacao);
        }

        public void AtualizaUltimoLogin()
        {
            this.UltimoLogin = DateTime.Now;
        }

        public void DefinirEmail(string novoEmail)
        {
            if (Perfis == null || !Perfis.Any())
            {
                throw new NegocioException(MENSAGEM_ERRO_USUARIO_SEM_ACESSO);
            }
            if ((PossuiPerfilDre() ||
                 PossuiPerfilSme()) &&
                !novoEmail.Contains("@sme.prefeitura.sp.gov.br"))
            {
                throw new NegocioException("Usuários da SME ou DRE devem utilizar e-mail profissional. Ex: usuario@sme.prefeitura.sp.gov.br");
            }
            Email = novoEmail;
        }

        public void DefinirPerfilAtual(Guid perfilAtual)
        {
            this.PerfilAtual = perfilAtual;
        }

        public void DefinirPerfis(IEnumerable<PrioridadePerfil> perfisUsuario)
        {
            Perfis = perfisUsuario;
        }

        public bool EhProfessor()
        {
            return PerfilAtual == PERFIL_PROFESSOR;
        }

        public bool EhProfessorCj()
        {
            return PerfilAtual == PERFIL_CJ;
        }

        public void FinalizarRecuperacaoSenha()
        {
            TokenRecuperacaoSenha = null;
            ExpiracaoRecuperacaoSenha = null;
        }

        public void IniciarRecuperacaoDeSenha()
        {
            if (string.IsNullOrWhiteSpace(Email))
            {
                throw new NegocioException("Você não tem um e-mail cadastrado para recuperar sua senha. Para restabelecer o seu acesso, procure o Diretor da sua UE ou Administrador do SGP da sua unidade.");
            }

            TokenRecuperacaoSenha = Guid.NewGuid();
            ExpiracaoRecuperacaoSenha = DateTime.Now.AddHours(6);
        }

        public Guid ObterPerfilPrioritario()
        {
            if (Perfis == null || !Perfis.Any())
            {
                throw new NegocioException(MENSAGEM_ERRO_USUARIO_SEM_ACESSO);
            }
            var possuiPerfilPrioritario = Perfis.Any(c => c.CodigoPerfil == PERFIL_PROFESSOR);
            if (possuiPerfilPrioritario)
            {
                return PERFIL_PROFESSOR;
            }
            return Perfis.FirstOrDefault().CodigoPerfil;
        }

        public TipoPerfil? ObterTipoPerfilAtual()
        {
            return Perfis.FirstOrDefault(a => a.CodigoPerfil == PerfilAtual).Tipo;
        }

        public void PodeAlterarEvento(Evento evento)
        {
            if (evento.TipoEvento.LocalOcorrencia == EventoLocalOcorrencia.DRE)
            {
                if (PerfilAtual != PERFIL_DIRETOR && PerfilAtual != PERFIL_AD && PerfilAtual != PERFIL_CP)
                    throw new NegocioException("Você não tem permissão para alterar este evento.");
            }
        }

        public void PodeCriarEvento(Evento evento)
        {
            if (!PossuiPerfilSme() && string.IsNullOrWhiteSpace(evento.DreId))
            {
                throw new NegocioException("É necessário informar a DRE.");
            }

            if (!PossuiPerfilSmeOuDre() && string.IsNullOrWhiteSpace(evento.UeId))
            {
                throw new NegocioException("É necessário informar a UE.");
            }

            if (evento.TipoEvento.LocalOcorrencia == EventoLocalOcorrencia.SME && !PossuiPerfilSme())
            {
                throw new NegocioException("Somente usuários da SME podem criar este tipo de evento.");
            }

            if (evento.TipoEvento.LocalOcorrencia == EventoLocalOcorrencia.DRE && (!PossuiPerfilDre() && !PossuiPerfilSme()))
            {
                throw new NegocioException("Somente usuários da SME ou da DRE podem criar este tipo de evento.");
            }
        }

        public void PodeCriarEventoComDataPassada(Evento evento)
        {
            if (evento.DataInicio < DateTime.Today)
            {
                if (ObterTipoPerfilAtual() != TipoPerfil.SME)
                {
                    if (ObterTipoPerfilAtual() != TipoPerfil.DRE || evento.TipoEvento.LocalOcorrencia != EventoLocalOcorrencia.DRE)
                        throw new NegocioException("Não é possível criar evento com datas passadas.");
                }
            }
        }

        public bool PodeRegistrarFrequencia(Aula aula)
        {
            return aula.ProfessorRf == CodigoRf;
        }

        public bool PodeReiniciarSenha()
        {
            return !string.IsNullOrEmpty(Email);
        }

        public bool PodeVisualizarEventosOcorrenciaDre()
        {
            var perfilAtual = Perfis.FirstOrDefault(a => a.CodigoPerfil == PerfilAtual);
            if (perfilAtual.Tipo == TipoPerfil.UE)
                return (PerfilAtual == PERFIL_DIRETOR || PerfilAtual == PERFIL_AD || PerfilAtual == PERFIL_CP || PerfilAtual == PERFIL_SECRETARIO);
            else return true;
        }

        public bool PossuiPerfilCJ()
            => Perfis != null &&
                Perfis.Any(c => c.CodigoPerfil == PERFIL_CJ);

        public bool PossuiPerfilDre()
        {
            return Perfis != null && Perfis.Any(c => c.Tipo == TipoPerfil.DRE);
        }

        public bool PossuiPerfilDreOuUe()
        {
            if (Perfis == null || !Perfis.Any())
            {
                throw new NegocioException(MENSAGEM_ERRO_USUARIO_SEM_ACESSO);
            }
            return PossuiPerfilDre() || PossuiPerfilUe();
        }

        public bool PossuiPerfilSme()
        {
            return Perfis != null && Perfis.Any(c => c.Tipo == TipoPerfil.SME);
        }

        public bool PossuiPerfilSmeOuDre()
        {
            if (Perfis == null || !Perfis.Any())
            {
                throw new NegocioException(MENSAGEM_ERRO_USUARIO_SEM_ACESSO);
            }
            return PossuiPerfilSme() || PossuiPerfilDre();
        }

        public bool PossuiPerfilUe()
        {
            return Perfis != null && Perfis.Any(c => c.Tipo == TipoPerfil.UE);
        }

        public bool TemPerfilSupervisorOuDiretor()
        {
            return (PerfilAtual == PERFIL_DIRETOR || PerfilAtual == PERFIL_SUPERVISOR);
        }

        public bool TokenRecuperacaoSenhaEstaValido()
        {
            return ExpiracaoRecuperacaoSenha > DateTime.Now;
        }

        public void ValidarSenha(string novaSenha)
        {
            if (novaSenha.Length < 8)
                throw new NegocioException("A senha deve conter no minimo 8 caracteres.");

            if (novaSenha.Length > 12)
                throw new NegocioException("A senha deve conter no máximo 12 caracteres.");

            if (novaSenha.Contains(" "))
                throw new NegocioException("A senhão não pode conter espaço em branco.");

            var regexSenha = new Regex(@"(?=.*?[A-Z])(?=.*?[a-z])(?=((?=.*[!@#$\-%&/\\\[\]|*()_=+])|(?=.*?[0-9]+)))");
            if (!regexSenha.IsMatch(novaSenha))
                throw new NegocioException("A senha deve conter pelo menos 1 letra maiúscula, 1 minúscula, 1 número e/ou 1 caractere especial.");
        }
    }
}