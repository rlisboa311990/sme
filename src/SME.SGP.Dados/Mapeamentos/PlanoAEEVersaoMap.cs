﻿using SME.SGP.Dominio;
using System;
using System.Collections.Generic;
using System.Text;

namespace SME.SGP.Dados
{
    public class PlanoAEEVersaoMap : BaseMap<PlanoAEEVersao>
    {
        public PlanoAEEVersaoMap()
        {
            ToTable("plano_aee_versao");
            Map(a => a.PlanoAEEId).ToColumn("plano_aee_id");
        }
    }
}
