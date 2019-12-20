﻿using SME.SGP.Dominio.Enumerados;
using SME.SGP.Dto;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SME.SGP.Dominio.Interfaces
{
    public interface IRepositorioAbrangencia
    {
        void ExcluirAbrangencias(IEnumerable<long> ids);

        void MarcarAbrangenciasNaoVinculadas(IEnumerable<long> ids);

        void InserirAbrangencias(IEnumerable<Abrangencia> enumerable, string login);

        Task<bool> JaExisteAbrangencia(string login, Guid perfil);

        Task<IEnumerable<AbrangenciaFiltroRetorno>> ObterAbrangenciaPorFiltro(string texto, string login, Guid perfil, bool consideraHistorico);

        Task<IEnumerable<AbrangenciaSinteticaDto>> ObterAbrangenciaSintetica(string login, Guid perfil, string turmaId = "");

        Task<AbrangenciaFiltroRetorno> ObterAbrangenciaTurma(string turma, string login, Guid perfil);

        Task<AbrangenciaDreRetorno> ObterDre(string dreCodigo, string ueCodigo, string login, Guid perfil);

        Task<IEnumerable<AbrangenciaDreRetorno>> ObterDres(string login, Guid perfil, Modalidade? modalidade = null, int periodo = 0, bool consideraHistorico = false);

        Task<IEnumerable<int>> ObterModalidades(string login, Guid perfil, int anoLetivo, bool consideraHistorico);

        Task<IEnumerable<int>> ObterSemestres(string login, Guid perfil, Modalidade modalidade, bool consideraHistorico);

        Task<IEnumerable<AbrangenciaTurmaRetorno>> ObterTurmas(string codigoUe, string login, Guid perfil, Modalidade modalidade, int periodo = 0, bool consideraHistorico = false);

        Task<AbrangenciaUeRetorno> ObterUe(string codigo, string login, Guid perfil);

        Task<IEnumerable<AbrangenciaUeRetorno>> ObterUes(string codigoDre, string login, Guid perfil, Modalidade? modalidade = null, int periodo = 0, bool consideraHistorico = false);

        void RemoverAbrangenciasForaEscopo(string login, Guid perfil, TipoAbrangencia porTurma);

        Task<IEnumerable<int>> ObterAnosLetivos(string login, Guid perfil, bool consideraHistorico);
        IEnumerable<Turma> ObterTurmasMarcadasParaDesvinculo(string login, Guid perfil);
        void FinalizarVinculos(string login, Guid perfil, string codigoTurma, DateTime dataFimVinculo);
        void DesfazerMarcacaoAbrangenciasNaoVinculadas(string login, Guid perfil, IEnumerable<Turma> turmasNaoCobertas);
        DateTime? ObterDataUltimoProcessamento();
    }
}