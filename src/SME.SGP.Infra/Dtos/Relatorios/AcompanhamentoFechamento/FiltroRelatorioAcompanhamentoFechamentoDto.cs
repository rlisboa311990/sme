﻿using SME.SGP.Dominio;
using System.Collections.Generic;

namespace SME.SGP.Infra
{
    public class FiltroRelatorioAcompanhamentoFechamentoDto
    {
        public FiltroRelatorioAcompanhamentoFechamentoDto()
        {
            TurmasCodigo = new List<long>();
            Bimestres = new List<int>();
        }
        public int AnoLetivo { get; set; }
        public string DreCodigo { get; set; }
        public string UeCodigo { get; set; }
        public Modalidade Modalidade { get; set; }
        public int Semestre { get; set; }
        public List<long> TurmasCodigo { get; set; }
        public List<int> Bimestres { get; set; }
        public SituacaoFechamento? SituacaoFechamento { get; set; }
        public SituacaoConselhoClasse? SituacaoConselhoClasse { get; set; }
        public bool ListarPendencias { get; set; }
        public string UsuarioNome{ get; set; }
        public string UsuarioRF { get; set; }
    }
}
