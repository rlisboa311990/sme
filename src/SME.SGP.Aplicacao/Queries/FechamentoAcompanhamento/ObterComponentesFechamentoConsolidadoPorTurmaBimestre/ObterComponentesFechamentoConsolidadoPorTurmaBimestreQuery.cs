﻿using FluentValidation;
using MediatR;
using SME.SGP.Infra;
using System.Collections.Generic;

namespace SME.SGP.Aplicacao
{
    public class ObterComponentesFechamentoConsolidadoPorTurmaBimestreQuery : IRequest<IEnumerable<ConsolidacaoTurmaComponenteCurricularDto>>
    {
        public ObterComponentesFechamentoConsolidadoPorTurmaBimestreQuery(long turmaId, int bimestre)
        {
            TurmaId = turmaId;
            Bimestre = bimestre;
        }

        public long TurmaId { get; set; }
        public int Bimestre { get; set; }
    }
    public class ObterComponentesFechamentoConsolidadoPorTurmaBimestreQueryValidator : AbstractValidator<ObterComponentesFechamentoConsolidadoPorTurmaBimestreQuery>
    {
        public ObterComponentesFechamentoConsolidadoPorTurmaBimestreQueryValidator()
        {
            RuleFor(a => a.TurmaId)
                .NotEmpty()
                .WithMessage("O id da turma deve ser informado.");            
            RuleFor(a => a.Bimestre)
                .NotNull()
                .WithMessage("O bimestre deve ser informado.");           
        }
    }
}
