﻿using SME.SGP.Dominio;
using SME.SGP.Dominio.Interfaces;
using SME.SGP.Infra;

namespace SME.SGP.Dados.Repositorios
{
    public class RepositorioAcompanhamentoTurma : RepositorioBase<AcompanhamentoTurma>, IRepositorioAcompanhamentoTurma
    {
        public RepositorioAcompanhamentoTurma(ISgpContext conexao) : base(conexao)
        {
        }
    }
}
