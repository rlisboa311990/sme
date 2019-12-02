﻿using SME.SGP.Aplicacao.Integracoes.Respostas;
using SME.SGP.Dominio;
using SME.SGP.Dto;
using SME.SGP.Infra;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SME.SGP.Aplicacao.Integracoes
{
    public interface IServicoEOL
    {
        Task<AlterarSenhaRespostaDto> AlterarSenha(string login, string novaSenha);

        Task AtribuirCJSeNecessario(Guid usuarioId);

        Task<UsuarioEolAutenticacaoRetornoDto> Autenticar(string login, string senha);

        Task<AbrangenciaRetornoEolDto> ObterAbrangencia(string login, Guid perfil);

        Task<AbrangenciaRetornoEolDto> ObterAbrangenciaParaSupervisor(string[] uesIds);

        Task<IEnumerable<AlunoPorTurmaResposta>> ObterAlunosPorTurma(string turmaId);

        Task<IEnumerable<DisciplinaResposta>> ObterDisciplinasParaPlanejamento(long codigoTurma, string login, Guid perfil);

        Task<IEnumerable<DisciplinaResposta>> ObterDisciplinasPorCodigoTurmaLoginEPerfil(string codigoTurma, string login, Guid perfil);

        IEnumerable<DisciplinaDto> ObterDisciplinasPorIds(int[] ids);

        IEnumerable<DreRespostaEolDto> ObterDres();

        IEnumerable<EscolasRetornoDto> ObterEscolasPorCodigo(string[] codigoUes);

        IEnumerable<EscolasRetornoDto> ObterEscolasPorDre(string dreId);

        IEnumerable<UsuarioEolRetornoDto> ObterFuncionariosPorCargoUe(string UeId, long cargoId);

        Task<IEnumerable<UsuarioEolRetornoDto>> ObterFuncionariosPorUe(BuscaFuncionariosFiltroDto buscaFuncionariosFiltroDto);

        Task<IEnumerable<ProfessorResumoDto>> ObterListaNomePorListaRF(IEnumerable<string> codigosRF);

        Task<IEnumerable<ProfessorResumoDto>> ObterListaResumosPorListaRF(IEnumerable<string> codigosRF, int anoLetivo);

        IEnumerable<ProfessorTurmaReposta> ObterListaTurmasPorProfessor(string codigoRf);

        Task<MeusDadosDto> ObterMeusDados(string login);

        Task<UsuarioEolAutenticacaoRetornoDto> ObterPerfisPorLogin(string login);

        Task<int[]> ObterPermissoesPorPerfil(Guid perfilGuid);

        Task<IEnumerable<ProfessorResumoDto>> ObterProfessoresAutoComplete(int anoLetivo, string dreId, string nomeProfessor);

        Task<IEnumerable<ProfessorTitularDisciplinaEol>> ObterProfessoresTitularesDisciplinas(string turmaId, Modalidade modalidadeId, string ueId);

        Task<UsuarioResumoCoreDto> ObterResumoCore(string login);

        Task<ProfessorResumoDto> ObterResumoProfessorPorRFAnoLetivo(string codigoRF, int anoLetivo);

        IEnumerable<SupervisoresRetornoDto> ObterSupervisoresPorCodigo(string[] codigoSupervisores);

        IEnumerable<SupervisoresRetornoDto> ObterSupervisoresPorDre(string dreId);

        Task<IEnumerable<TurmaDto>> ObterTurmasAtribuidasAoProfessorPorEscolaEAnoLetivo(string rfProfessor, string codigoEscola, int anoLetivo);

        Task<IEnumerable<TurmaPorUEResposta>> ObterTurmasPorUE(string ueId, string anoLetivo);

        Task ReiniciarSenha(string login);

        Task RemoverCJSeNecessario(Guid usuarioId);
    }
}