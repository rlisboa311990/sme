﻿using SME.SGP.Dominio;
using SME.SGP.Infra;
using System.Collections.Generic;

namespace SME.SGP.Aplicacao
{
    public interface IConsultasWorkflowAprovacao
    {
        WorkflowAprovacao ObtemPorId(long id);

        List<WorkflowAprovacaoTimeRespostaDto> ObtemTimelinePorCodigoNotificacao(long notificacaoId);
    }
}