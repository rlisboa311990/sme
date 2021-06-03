﻿using SME.SGP.Dominio.Enumerados;
using SME.SGP.Dto;
using SME.SGP.Infra;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SME.SGP.Dominio.Interfaces
{
    public interface IRepositorioAbrangencia
    {
        void AtualizaAbrangenciaHistorica(IEnumerable<long> paraAtualizar);

        void ExcluirAbrangencias(IEnumerable<long> ids);

        void ExcluirAbrangenciasHistoricas(IEnumerable<long> ids);

        void InserirAbrangencias(IEnumerable<Abrangencia> abrangencias, string login);

        Task<bool> JaExisteAbrangencia(string login, Guid perfil);

        Task<IEnumerable<AbrangenciaFiltroRetorno>> ObterAbrangenciaPorFiltro(string texto, string login, Guid perfil, bool consideraHistorico);

        Task<IEnumerable<AbrangenciaSinteticaDto>> ObterAbrangenciaSintetica(string login, Guid perfil, string turmaId = "", bool consideraHistorico = false);
        Task<bool> ObterUsuarioPossuiAbrangenciaAcessoSondagemTiposEscola(string usuarioRF, Guid UsuarioPerfil);        
        Task<IEnumerable<AbrangenciaHistoricaDto>> ObterAbrangenciaHistoricaPorLogin(string login);

        Task<AbrangenciaFiltroRetorno> ObterAbrangenciaTurma(string turma, string login, Guid perfil, bool consideraHistorico = false, bool abrangenciaPermitida = false);

        Task<IEnumerable<int>> ObterAnosLetivos(string login, Guid perfil, bool consideraHistorico, int anoMinimo);

        Task<IEnumerable<string>> ObterAnosTurmasPorCodigoUeModalidade(string login, Guid perfil, string codigoUe, Modalidade modalidade, bool consideraHistorico);

        Task<AbrangenciaDreRetorno> ObterDre(string dreCodigo, string ueCodigo, string login, Guid perfil);

        Task<IEnumerable<AbrangenciaDreRetorno>> ObterDres(string login, Guid perfil, Modalidade? modalidade = null, int periodo = 0, bool consideraHistorico = false, int anoLetivo = 0, string filtro = "", bool filtroEhCodigo = false);

        Task<IEnumerable<int>> ObterModalidades(string login, Guid perfil, int anoLetivo, bool consideraHistorico, IEnumerable<Modalidade> modadlidadesQueSeraoIgnoradas);

        Task<IEnumerable<int>> ObterSemestres(string login, Guid perfil, Modalidade modalidade, bool consideraHistorico, int anoLetivo = 0);

        Task<IEnumerable<AbrangenciaTurmaRetorno>> ObterTurmas(string codigoUe, string login, Guid perfil, Modalidade modalidade, int periodo = 0, bool consideraHistorico = false, int anoLetivo = 0);
        Task<IEnumerable<Modalidade>> ObterModalidadesPorUeAbrangencia(string codigoUe, string login, Guid perfilAtual, IEnumerable<Modalidade> modadlidadesQueSeraoIgnoradas);
        Task<AbrangenciaUeRetorno> ObterUe(string codigo, string login, Guid perfil);

        Task<bool> UsuarioPossuiAbrangenciaAdm(long usuarioId);

        Task<IEnumerable<AbrangenciaUeRetorno>> ObterUes(string codigoDre, string login, Guid perfil, Modalidade? modalidade = null, int periodo = 0, bool consideraHistorico = false, int anoLetivo = 0, int[] ignorarTiposUE = null);

        bool PossuiAbrangenciaTurmaAtivaPorLogin(string login);

        bool PossuiAbrangenciaTurmaInfantilAtivaPorLogin(string login);

        void RemoverAbrangenciasForaEscopo(string login, Guid perfil, TipoAbrangenciaSincronizacao escopo);

        Task<bool> UsuarioPossuiAbrangenciaDeUmDosTipos(Guid perfil, IEnumerable<TipoPerfil> tipos);

        Task<IEnumerable<Modalidade>> ObterModalidadesPorUe(string codigoUe);
        Task<IEnumerable<Modalidade>> ObterModalidadesPorCodigosUe(string[] codigosUe);

        Task<IEnumerable<OpcaoDropdownDto>> ObterDropDownTurmasPorUeAnoLetivoModalidadeSemestre(string codigoUe, int anoLetivo, Modalidade? modalidade, int semestre);

        Task<IEnumerable<OpcaoDropdownDto>> ObterDropDownTurmasPorUeAnoLetivoModalidadeSemestreAnos(string codigoUe, int anoLetivo, Modalidade? modalidade, int semestre, IList<string> anos);
        Task<IEnumerable<AbrangenciaTurmaRetorno>> ObterTurmasPorTipos(string codigoUe, string login, Guid perfil, Modalidade modalidade, int[] tipos, int periodo = 0, bool consideraHistorico = false, int anoLetivo = 0);
    }
}