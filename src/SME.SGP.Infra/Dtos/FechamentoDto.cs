﻿using System.Collections.Generic;

namespace SME.SGP.Infra
{
    public class FechamentoDto
    {
        public FechamentoDto()
        {
            FechamentosBimestres = new List<FechamentoBimestreDto>();
        }

        public long? DreId { get; set; }
        public IEnumerable<FechamentoBimestreDto> FechamentosBimestres { get; set; }
        public long Id { get; set; }
        public bool Migrado { get; set; }
        public long TipoCalendarioId { get; set; }
        public long? UeId { get; set; }
    }
}