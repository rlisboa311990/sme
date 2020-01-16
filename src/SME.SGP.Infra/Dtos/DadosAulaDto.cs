﻿using SME.SGP.Dominio;
using System.Collections.Generic;

namespace SME.SGP.Infra
{
    public class DadosAulaDto
    {
        public bool podeCadastrarAvaliacao;

        public List<AtividadeAvaliativa> Atividade { get; set; }
        public int? DisciplinaId { get; set; }
        public string Disciplina { get; set; }
        public int? DisciplinaCompartilhadaId { get; set; }
        public string DisciplinaCompartilhada { get; set; }
        public bool EhRegencia { get; set; }
        public bool EhCompartilhada { get; set; }
        public string Horario { get; set; }
        public string Modalidade { get; set; }
        public string Tipo { get; set; }
        public string Turma { get; set; }
        public string UnidadeEscolar { get; set; }
    }
}