﻿using SME.SGP.Dominio;

namespace SME.SGP.Dados.Mapeamentos
{
    public class RecuperacaoParalelaMap : BaseMap<RecuperacaoParalela>
    {
        public RecuperacaoParalelaMap()
        {
            ToTable("recuperacao_paralela");
            Map(c => c.TurmaId).ToColumn("turma_id");
        }
    }
}