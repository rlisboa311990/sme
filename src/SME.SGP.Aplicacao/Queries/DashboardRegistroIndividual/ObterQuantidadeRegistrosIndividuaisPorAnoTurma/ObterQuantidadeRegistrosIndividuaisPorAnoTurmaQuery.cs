﻿using FluentValidation;
using MediatR;
using SME.SGP.Dominio;
using SME.SGP.Infra;
using System.Collections.Generic;

namespace SME.SGP.Aplicacao
{
    public class ObterQuantidadeRegistrosIndividuaisPorAnoTurmaQuery : IRequest<IEnumerable<GraficoBaseDto>>
    {
        public ObterQuantidadeRegistrosIndividuaisPorAnoTurmaQuery(int anoLetivo, long dreId, long ueId, Modalidade modalidade)
        {
            AnoLetivo = anoLetivo;
            DreId = dreId;
            UeId = ueId;
            Modalidade = modalidade;
        }

        public int AnoLetivo { get; set; }
        public long DreId { get; set; }
        public long UeId { get; set; }
        public Modalidade Modalidade { get; set; }
    }

    public class ObterQuantidadeRegistrosIndividuaisPorAnoTurmaQueryValidator : AbstractValidator<ObterQuantidadeRegistrosIndividuaisPorAnoTurmaQuery>
    {
        public ObterQuantidadeRegistrosIndividuaisPorAnoTurmaQueryValidator()
        {

            RuleFor(c => c.AnoLetivo)
                .NotNull()
                .NotEmpty()
                .WithMessage("O ano letivo deve ser informado.");
            RuleFor(c => c.Modalidade)
                .NotNull()
                .NotEmpty()
                .WithMessage("A modalidade deve ser informada.");
        }
    }
}
