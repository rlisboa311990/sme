﻿namespace SME.SGP.Dominio
{
    public class AtribuicaoCJ : EntidadeBase
    {
        public string DisciplinaId { get; set; }
        public string DreId { get; set; }
        public Modalidade Modalidade { get; set; }
        public string ProfessorRf { get; set; }
        public bool Substituir { get; set; }

        public Turma Turma { get; set; }
        public string TurmaId { get; set; }
        public string UeId { get; set; }
    }
}