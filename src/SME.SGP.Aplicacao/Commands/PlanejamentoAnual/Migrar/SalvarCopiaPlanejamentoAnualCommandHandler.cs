﻿using MediatR;
using SME.SGP.Dominio;
using SME.SGP.Dominio.Interfaces;
using SME.SGP.Infra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SME.SGP.Aplicacao
{
    public class SalvarCopiaPlanejamentoAnualCommandHandler : AbstractUseCase, IRequestHandler<SalvarCopiaPlanejamentoAnualCommand, bool>
    {
        private readonly IRepositorioPlanejamentoAnual repositorioPlanejamentoAnual;
        private readonly IRepositorioPlanejamentoAnualPeriodoEscolar repositorioPlanejamentoAnualPeriodoEscolar;
        private readonly IRepositorioPlanejamentoAnualComponente repositorioPlanejamentoAnualComponente;
        private readonly IRepositorioPlanejamentoAnualObjetivosAprendizagem repositorioPlanejamentoAnualObjetivosAprendizagem;
        private readonly IRepositorioPeriodoEscolar repositorioPeriodoEscolar;

        public SalvarCopiaPlanejamentoAnualCommandHandler(IRepositorioPlanejamentoAnual repositorioPlanejamentoAnual,
                                                          IRepositorioPlanejamentoAnualPeriodoEscolar repositorioPlanejamentoAnualPeriodoEscolar,
                                                          IRepositorioPlanejamentoAnualComponente repositorioPlanejamentoAnualComponente,
                                                          IRepositorioPlanejamentoAnualObjetivosAprendizagem repositorioPlanejamentoAnualObjetivosAprendizagem,
                                                          IRepositorioPeriodoEscolar repositorioPeriodoEscolar,
                                                          IMediator mediator) : base(mediator)
        {
            this.repositorioPlanejamentoAnual = repositorioPlanejamentoAnual ?? throw new ArgumentNullException(nameof(repositorioPlanejamentoAnual));
            this.repositorioPlanejamentoAnualPeriodoEscolar = repositorioPlanejamentoAnualPeriodoEscolar ?? throw new ArgumentNullException(nameof(repositorioPlanejamentoAnualPeriodoEscolar));
            this.repositorioPlanejamentoAnualComponente = repositorioPlanejamentoAnualComponente ?? throw new ArgumentNullException(nameof(repositorioPlanejamentoAnualComponente));
            this.repositorioPlanejamentoAnualObjetivosAprendizagem = repositorioPlanejamentoAnualObjetivosAprendizagem ?? throw new ArgumentNullException(nameof(repositorioPlanejamentoAnualObjetivosAprendizagem));
            this.repositorioPeriodoEscolar = repositorioPeriodoEscolar ?? throw new ArgumentNullException(nameof(repositorioPeriodoEscolar));
        }

        public async Task<bool> Handle(SalvarCopiaPlanejamentoAnualCommand request, CancellationToken cancellationToken)
        {
            var planejamentoAnualId = await repositorioPlanejamentoAnual
                .ObterIdPorTurmaEComponenteCurricular(request.PlanejamentoAnual.TurmaId, request.PlanejamentoAnual.ComponenteCurricularId);

            if (planejamentoAnualId == 0)
                planejamentoAnualId = await repositorioPlanejamentoAnual.SalvarAsync(request.PlanejamentoAnual);

            foreach (var periodo in request.PlanejamentoAnual.PeriodosEscolares)
            {
                var planejamentoAnualPeriodoEscolarExistente = await repositorioPlanejamentoAnualPeriodoEscolar
                    .ObterPorPlanejamentoAnualIdEPeriodoId(planejamentoAnualId, periodo.PeriodoEscolarId, true);

                long? planejamentoAnualPeriodoEscolarId = planejamentoAnualPeriodoEscolarExistente?.Id;

                if (planejamentoAnualPeriodoEscolarExistente == null)
                {
                    var copiaPeriodo = new PlanejamentoAnualPeriodoEscolar(periodo.PeriodoEscolarId, planejamentoAnualId);
                    planejamentoAnualPeriodoEscolarId = await repositorioPlanejamentoAnualPeriodoEscolar.SalvarAsync(copiaPeriodo);
                }

                foreach (var componente in periodo.ComponentesCurriculares)
                {
                    var planejamentoAnualComponente = await repositorioPlanejamentoAnualComponente
                        .ObterPorPlanejamentoAnualPeriodoEscolarId(componente.ComponenteCurricularId, planejamentoAnualPeriodoEscolarId.Value, true);

                    if (planejamentoAnualComponente == null)
                        planejamentoAnualComponente = new PlanejamentoAnualComponente(componente.ComponenteCurricularId, componente.Descricao, planejamentoAnualPeriodoEscolarId.Value);
                    else
                        planejamentoAnualComponente.Descricao = componente.Descricao;

                    var planejamentoAnualComponenteId = await repositorioPlanejamentoAnualComponente.SalvarAsync(planejamentoAnualComponente);

                    var listaPlanejamentoAnualObjetivoAprendizagemExistente = await repositorioPlanejamentoAnualObjetivosAprendizagem
                        .ObterPorPlanejamentoAnualComponenteId(planejamentoAnualComponenteId);

                    foreach (var objetivo in componente.ObjetivosAprendizagem)
                    {
                        var planejamentoAnualObjetivoAprendizagem = listaPlanejamentoAnualObjetivoAprendizagemExistente
                            .SingleOrDefault(oa => oa.ObjetivoAprendizagemId.Equals(objetivo.ObjetivoAprendizagemId));

                        if (planejamentoAnualObjetivoAprendizagem == null)
                        {
                            var copiaObjetivo = new PlanejamentoAnualObjetivoAprendizagem(planejamentoAnualComponenteId, objetivo.ObjetivoAprendizagemId);
                            await repositorioPlanejamentoAnualObjetivosAprendizagem.SalvarAsync(copiaObjetivo);
                        }                        
                    }

                    var listaPlanejamentoAnualObjetivoAprendizagemRemover = listaPlanejamentoAnualObjetivoAprendizagemExistente
                        .Where(loa => !componente.ObjetivosAprendizagem.Select(oa => oa.ObjetivoAprendizagemId).Contains(loa.ObjetivoAprendizagemId))
                        .ToList();

                    listaPlanejamentoAnualObjetivoAprendizagemRemover
                        .ForEach(oa => repositorioPlanejamentoAnualObjetivosAprendizagem.RemoverLogico(oa.Id).Wait());
                }
            }

            return true;
        }
    }
}
