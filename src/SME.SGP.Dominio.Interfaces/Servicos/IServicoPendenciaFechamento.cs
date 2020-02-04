﻿using System;

namespace SME.SGP.Dominio.Interfaces
{
    public interface IServicoPendenciaFechamento
    {
        int ValidarAulasReposicaoPendente(long fechamentoId, Turma turma, string disciplinaId, DateTime inicioPeriodo, DateTime fimPeriodo);

        int ValidarAulasSemFrequenciaRegistrada(long fechamentoId, Turma turma, string disciplinaId, DateTime inicioPeriodo, DateTime fimPeriodo);

        int ValidarAulasSemPlanoAulaNaDataDoFechamento(long fechamentoId, Turma turma, string disciplinaId, DateTime inicioPeriodo, DateTime fimPeriodo);

        int ValidarAvaliacoesSemNotasParaNenhumAluno(long fechamentoId, string codigoTurma, string disciplinaId, DateTime inicioPeriodo, DateTime fimPeriodo);
    }
}