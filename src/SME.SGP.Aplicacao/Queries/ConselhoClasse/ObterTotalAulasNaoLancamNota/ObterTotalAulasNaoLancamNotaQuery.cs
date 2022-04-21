﻿using FluentValidation;
using MediatR;
using SME.SGP.Infra.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SME.SGP.Aplicacao.Queries.ConselhoClasse.ObterTotalAulasNaoLancamNota
{
    public class ObterTotalAulasNaoLancamNotaQuery : IRequest<IEnumerable<TotalAulasNaoLancamNotaDto>>
    {
        public ObterTotalAulasNaoLancamNotaQuery(string codigoTurma, int bimestre)
        {
            CodigoTurma = codigoTurma;
            Bimestre = bimestre;
        }
        public string CodigoTurma { get; set; }
        public int Bimestre { get; set; }
    }

    public class ObterTotalAulasNaoLancamNotaQueryValidator : AbstractValidator<ObterTotalAulasNaoLancamNotaQuery>
    {
        public ObterTotalAulasNaoLancamNotaQueryValidator()
        {
            RuleFor(x => x.CodigoTurma).NotEmpty().WithMessage("O código da turma deve ser informado para obter o total de aulas que não lancam nota.");
            RuleFor(x => x.Bimestre).NotEmpty().WithMessage("O bimestre deve ser informado para obter o total de aulas que não lancam nota no bimestre.");
        }
    }
}
