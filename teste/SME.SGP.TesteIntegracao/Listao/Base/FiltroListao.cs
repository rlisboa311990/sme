using System;
using SME.SGP.Dominio;
using SME.SGP.Dominio.Enumerados;

namespace SME.SGP.TesteIntegracao.Listao
{
    public class FiltroListao
    {
        public string Perfil { get; set; }
        public Modalidade Modalidade { get; set; }
        public long ComponenteCurricularId { get; set; }
        public ModalidadeTipoCalendario TipoCalendario { get; set; }
        public DateTime DataAula { get; set; }
        public string AnoTurma { get; set; }
        public bool TurmaHistorica { get; set; }
        public TipoTurma TipoTurma { get; set; }
    }
}