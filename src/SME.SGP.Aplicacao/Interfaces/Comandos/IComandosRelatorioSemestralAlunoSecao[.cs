﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SME.SGP.Dominio;

namespace SME.SGP.Aplicacao
{
    public interface IComandosRelatorioSemestralAlunoSecao
    {
        Task SalvarAsync(RelatorioSemestralAlunoSecao secaoRelatorioAluno);
    }
}
