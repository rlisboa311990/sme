﻿using MediatR;
using SME.SGP.Dominio.Interfaces;
using SME.SGP.Infra;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SME.SGP.Aplicacao
{
    public class ObterDadosDashboardFrequenciaPorAnoQueryHandler : IRequestHandler<ObterDadosDashboardFrequenciaPorAnoQuery, IEnumerable<GraficoFrequenciaGlobalPorAnoDto>>
    {
        private readonly IRepositorioFrequencia repositorioFrequencia;

        public ObterDadosDashboardFrequenciaPorAnoQueryHandler(IRepositorioFrequencia repositorioFrequencia)
        {
            this.repositorioFrequencia = repositorioFrequencia ?? throw new ArgumentNullException(nameof(repositorioFrequencia));
        }

        public async Task<IEnumerable<GraficoFrequenciaGlobalPorAnoDto>> Handle(ObterDadosDashboardFrequenciaPorAnoQuery request, CancellationToken cancellationToken)
        {
            var listaFrequencia = await repositorioFrequencia.ObterFrequenciaGlobalPorAnoAsync(request.AnoLetivo, request.DreId, request.UeId, request.Modalidade);
            return MontarDto(listaFrequencia);
        }

        private IEnumerable<GraficoFrequenciaGlobalPorAnoDto> MontarDto(IEnumerable<FrequenciaGlobalPorAnoDto> listaFrequencia)
        {
            var dto =  new List<GraficoFrequenciaGlobalPorAnoDto>();
            foreach(var frequencia in listaFrequencia)
            {
                dto.Add(new GraficoFrequenciaGlobalPorAnoDto()
                {
                    Descricao = frequencia.Descricao,
                    Quantidade = frequencia.Quantidade,
                    Turma = frequencia.Modalidade.ShortName() + " - " + frequencia.Ano
                });
            }
            return dto;
        }
    }
}
