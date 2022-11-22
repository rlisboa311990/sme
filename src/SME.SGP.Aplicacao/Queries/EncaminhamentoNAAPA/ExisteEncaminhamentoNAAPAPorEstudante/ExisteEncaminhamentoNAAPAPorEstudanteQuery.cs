﻿using FluentValidation;
using MediatR;

namespace SME.SGP.Aplicacao
{
    public class ExisteEncaminhamentoNAAPAPorEstudanteQuery : IRequest<bool>
    {
        public ExisteEncaminhamentoNAAPAPorEstudanteQuery(string codigoEstudante, long ueId)
        {
            CodigoEstudante = codigoEstudante;
            UeId = ueId;
        }

        public string CodigoEstudante { get; }
        public long UeId { get; }
    }

    public class ExisteEncaminhamentoNAAPAPorEstudanteQueryValidator : AbstractValidator<ExisteEncaminhamentoNAAPAPorEstudanteQuery>
    {
        public ExisteEncaminhamentoNAAPAPorEstudanteQueryValidator()
        {
            RuleFor(a => a.CodigoEstudante)
                .NotEmpty()
                .WithMessage("O código do estudante/criança deve ser informado para consulta de seu Encaminhamento NAAPA");
            RuleFor(a => a.CodigoEstudante)
                .NotEmpty()
                .WithMessage("O id da ue deve ser informado para consulta de seu Encaminhamento NAAPA");
        }
    }
}