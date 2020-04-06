﻿using System.Collections.Generic;

namespace SME.SGP.Dominio
{
    public class FechamentoTurmaDisciplina : EntidadeBase
    {
        public FechamentoTurmaDisciplina() 
        {
            FechamentoAlunos = new List<FechamentoAluno>();
        }

        public long FechamentoTurmaId { get; set; }
        public FechamentoTurma FechamentoTurma { get; set; }
        public long DisciplinaId { get; set; }
        public SituacaoFechamento Situacao { get; set; }
        public string Justificativa { get; set; }

        public bool Migrado { get; set; }
        public bool Excluido { get; set; }
        public List<FechamentoAluno> FechamentoAlunos { get; set; }

        public void AtualizarSituacao(SituacaoFechamento situacao)
        {
            Situacao = situacao;
        }
    }
}