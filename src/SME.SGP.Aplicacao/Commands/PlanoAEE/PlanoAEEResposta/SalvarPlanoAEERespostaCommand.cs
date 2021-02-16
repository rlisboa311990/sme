﻿using MediatR;
using SME.SGP.Dominio;

namespace SME.SGP.Aplicacao
{
    public class SalvarPlanoAEERespostaCommand : IRequest<long>
    {
        public long PlanoAEEQuestaoId { get; set; }
        public string Resposta { get; set; }
        public TipoQuestao TipoQuestao { get; set; }

        public SalvarPlanoAEERespostaCommand(long planoAEEQuestaoId, string resposta, TipoQuestao tipoQuestao)
        {
            PlanoAEEQuestaoId = planoAEEQuestaoId;
            Resposta = resposta;
            TipoQuestao = tipoQuestao;
        }
    }
}
