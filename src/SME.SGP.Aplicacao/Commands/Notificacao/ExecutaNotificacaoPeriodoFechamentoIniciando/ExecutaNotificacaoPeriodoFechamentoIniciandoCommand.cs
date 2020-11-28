﻿using MediatR;
using SME.SGP.Dominio;
using System;
using System.Collections.Generic;
using System.Text;

namespace SME.SGP.Aplicacao
{
    public class ExecutaNotificacaoPeriodoFechamentoIniciandoCommand : IRequest<bool>
    {
        public ExecutaNotificacaoPeriodoFechamentoIniciandoCommand(PeriodoFechamentoBimestre periodoIniciandoBimestre, ModalidadeTipoCalendario modalidadeTipoCalendario)
        {
            PeriodoIniciandoBimestre = periodoIniciandoBimestre;
            ModalidadeTipoCalendario = modalidadeTipoCalendario;
        }

        public PeriodoFechamentoBimestre PeriodoIniciandoBimestre { get; set; }
        public ModalidadeTipoCalendario ModalidadeTipoCalendario { get; set; }
    }
}
