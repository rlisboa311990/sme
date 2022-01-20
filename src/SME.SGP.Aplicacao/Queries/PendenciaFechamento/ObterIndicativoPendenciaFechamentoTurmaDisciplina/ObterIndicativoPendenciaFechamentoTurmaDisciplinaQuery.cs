﻿using FluentValidation;
using MediatR;
using SME.SGP.Dominio;
using System;
using System.Collections.Generic;
using System.Text;

namespace SME.SGP.Aplicacao
{
    public class ObterIndicativoPendenciaFechamentoTurmaDisciplinaQuery : IRequest<bool>
    {
        public ObterIndicativoPendenciaFechamentoTurmaDisciplinaQuery(string turmaId, int bimestre, string disciplinaId)
        {
            TurmaId = turmaId;
            Bimestre = bimestre;
            DisciplinaId = disciplinaId;
        }

        public string TurmaId { get; set; }
        public int Bimestre { get; set; }
        public string DisciplinaId { get; set; }
    }

    public class ObterIndicativoPendenciaFechamentoTurmaDisciplinaQueryValidator : AbstractValidator<ObterIndicativoPendenciaFechamentoTurmaDisciplinaQuery>
    {
        public ObterIndicativoPendenciaFechamentoTurmaDisciplinaQueryValidator()
        {
            RuleFor(c => c.TurmaId)
            .NotEmpty()
            .WithMessage("O id da turma deve ser informado.");

            RuleFor(c => c.TurmaId)
            .NotEmpty()
            .WithMessage("O bimestre deve ser informado.");

            RuleFor(c => c.DisciplinaId)
            .NotEmpty()
            .WithMessage("O id do componente curricular deve ser informada.");
        }
    }
}
