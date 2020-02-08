﻿namespace SME.SGP.Dominio
{
    public enum TipoParametroSistema
    {
        //Calendário

        EjaDiasLetivos = 1,
        FundamentalMedioDiasLetivos = 2,

        //Frequência

        PercentualFrequenciaAlerta = 3,
        PercentualFrequenciaCritico = 4,
        QuantidadeAulasNotificarProfessor = 5,
        QuantidadeAulasNotificarGestorUE = 6,
        QuantidadeAulasNotificarSupervisorUE = 7,
        QuantidadeDiasNotificarAlteracaoChamadaEfetivada = 8,

        //Aula Prevista
        QuantidadeDiasNotificarProfessor = 9,

        //Compensação Ausência

        CompensacaoAusenciaPercentualRegenciaClasse = 10,
        CompensacaoAusenciaPercentualFund2 = 11,
        QuantidadeMaximaCompensacaoAusencia = 12,

        // Média
        MediaBimestre = 14,

        //Sistema
        HabilitarServicosEmBackground = 100
    }
}