﻿using SME.SGP.Dominio.Entidades;
using System;
using System.Collections.Generic;
using System.Text;

namespace SME.SGP.Dados.Mapeamentos
{
    public class GradeFiltroMap : BaseMap<GradeFiltro>
    {
        public GradeFiltroMap()
        {
            ToTable("grade_filtro");
            Map(a => a.GradeId).ToColumn("grade_id");
            Map(a => a.TipoEscola).ToColumn("tipo_escola");
            Map(a => a.Modalidade).ToColumn("modalidade");
            Map(a => a.DuracaoTurno).ToColumn("duracao_turno");
        }
    }
}
