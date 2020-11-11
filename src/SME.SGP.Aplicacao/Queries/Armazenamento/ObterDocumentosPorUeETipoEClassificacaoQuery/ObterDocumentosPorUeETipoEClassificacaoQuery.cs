﻿using FluentValidation;
using MediatR;
using SME.SGP.Dominio;
using SME.SGP.Infra;
using System;
using System.Collections.Generic;
using System.Text;

namespace SME.SGP.Aplicacao
{
    public class ObterDocumentosPorUeETipoEClassificacaoQuery : IRequest<IEnumerable<DocumentoDto>>
    {
        public ObterDocumentosPorUeETipoEClassificacaoQuery(long ueId, long tipoDocumentoId, long classificacaoId)
        {
            UeId = ueId;
            TipoDocumentoId = tipoDocumentoId;
            ClassificacaoId = classificacaoId;
        }

        public long UeId { get; set; }
        public long TipoDocumentoId { get; set; }
        public long ClassificacaoId { get; set; }
    }

    public class ObterDocumentosPorUeETipoEClassificacaoQueryValidator : AbstractValidator<ObterDocumentosPorUeETipoEClassificacaoQuery>
    {
        public ObterDocumentosPorUeETipoEClassificacaoQueryValidator()
        {
            RuleFor(c => c.UeId)
            .NotEmpty()
            .WithMessage("O id da ue deve ser informado.");
            RuleFor(c => c.TipoDocumentoId)
            .NotEmpty()
            .WithMessage("O id do tipo de documento deve ser informado.");
            RuleFor(c => c.ClassificacaoId)
            .NotEmpty()
            .WithMessage("O id da classificação deve ser informado.");

        }
    }
}
