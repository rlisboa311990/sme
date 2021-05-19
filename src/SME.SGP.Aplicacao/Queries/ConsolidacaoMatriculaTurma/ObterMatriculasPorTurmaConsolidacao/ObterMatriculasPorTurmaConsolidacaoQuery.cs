﻿using FluentValidation;
using MediatR;
using SME.SGP.Infra;
using System.Collections.Generic;

namespace SME.SGP.Aplicacao
{
    public class ObterMatriculasPorTurmaConsolidacaoQuery : IRequest<IEnumerable<ConsolidacaoMatriculaTurmaDto>>
    {
        public ObterMatriculasPorTurmaConsolidacaoQuery(int anoLetivo, string ueCodigo)
        {
            AnoLetivo = anoLetivo;
            UeCodigo = ueCodigo;
        }

        public int AnoLetivo { get; set; }
        public string UeCodigo { get; set; }
    }

    public class ObterMatriculasPorTurmaConsolidacaoQueryValidator : AbstractValidator<ObterMatriculasPorTurmaConsolidacaoQuery>
    {
        public ObterMatriculasPorTurmaConsolidacaoQueryValidator()
        {
            RuleFor(a => a.AnoLetivo)
                .NotEmpty()
                .WithMessage("O ano letivo deve ser informado para obter os dados de consolidação de matrículas das turmas");
            RuleFor(a => a.UeCodigo)
                .NotEmpty()
                .WithMessage("O código da UE deve ser informado para obter os dados de consolidação de matrículas das turmas");
        }
    }
}
