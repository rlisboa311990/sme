﻿using System.Collections.Generic;

namespace SME.SGP.Infra
{
    public class NotasConceitosBimestreRetornoDto
    {
        public NotasConceitosBimestreRetornoDto()
        {
            Avaliacoes = new List<NotasConceitosAvaliacaoRetornoDto>();
            Alunos = new List<NotasConceitosAlunoRetornoDto>();
        }

        public List<NotasConceitosAlunoRetornoDto> Alunos { get; set; }
        public List<NotasConceitosAvaliacaoRetornoDto> Avaliacoes { get; set; }
        public string Descricao { get; set; }
        public int Numero { get; set; }
    }
}