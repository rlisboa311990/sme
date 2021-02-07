﻿using SME.SGP.Dominio;
using System;
using System.Collections.Generic;
using System.Text;

namespace SME.SGP.Dados.Mapeamentos
{
    public class PlanoAEEQuestaoMap : BaseMap<PlanoAEEQuestao>
    {
        public PlanoAEEQuestaoMap()
        {
            ToTable("plano_aee_questao");
            Map(a => a.PlanoAEEVersaoId).ToColumn("plano_aee_versao_id");
            Map(a => a.QuestaoId).ToColumn("questao_id");
        }
    }
}
