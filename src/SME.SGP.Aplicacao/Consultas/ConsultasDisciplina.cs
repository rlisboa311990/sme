﻿using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using SME.SGP.Aplicacao.Integracoes;
using SME.SGP.Aplicacao.Integracoes.Respostas;
using SME.SGP.Dominio.Interfaces;
using SME.SGP.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SME.SGP.Aplicacao
{
    public class ConsultasDisciplina : IConsultasDisciplina
    {
        private readonly IConfiguration configuration;
        private readonly IRepositorioCache repositorioCache;
        private readonly IServicoEOL servicoEOL;

        public ConsultasDisciplina(IServicoEOL servicoEOL,
                                   IRepositorioCache repositorioCache,
                                   IConfiguration configuration)
        {
            this.servicoEOL = servicoEOL ?? throw new System.ArgumentNullException(nameof(servicoEOL));
            this.repositorioCache = repositorioCache ?? throw new System.ArgumentNullException(nameof(repositorioCache));
            this.configuration = configuration ?? throw new System.ArgumentNullException(nameof(configuration));
        }

        public async Task<IEnumerable<DisciplinaDto>> ObterDisciplinasPorProfessorETurma(long codigoTurma, string rfProfessor)
        {
            var disciplinasCacheString = repositorioCache.Obter($"Disciplinas-{codigoTurma}-{rfProfessor}");

            if (string.IsNullOrEmpty(disciplinasCacheString))
            {
                var disciplinas = await servicoEOL.ObterDisciplinasPorProfessorETurma(codigoTurma, rfProfessor);
                var disciplinasDto = MapearParaDto(disciplinas);

                await repositorioCache.SalvarAsync($"Disciplinas-{codigoTurma}-{rfProfessor}", JsonConvert.SerializeObject(disciplinasDto));
            }
            return JsonConvert.DeserializeObject<List<DisciplinaDto>>(disciplinasCacheString);
        }

        private IEnumerable<DisciplinaDto> MapearParaDto(IEnumerable<DisciplinaResposta> disciplinas)
        {
            if (disciplinas != null)
            {
                foreach (var disciplina in disciplinas)
                {
                    yield return new DisciplinaDto()
                    {
                        CodigoComponenteCurricular = disciplina.CodigoComponenteCurricular,
                        Nome = disciplina.Nome
                    };
                }
            }
        }
    }
}