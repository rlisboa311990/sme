﻿using SME.SGP.Dominio;
using SME.SGP.Dominio.Interfaces;
using SME.SGP.Infra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SME.SGP.Aplicacao
{
    public class ConsultasAulaPrevista : IConsultasAulaPrevista
    {
        private readonly IRepositorioAulaPrevista repositorio;
        private readonly IRepositorioAulaPrevistaBimestre repositorioBimestre;

        public ConsultasAulaPrevista(IRepositorioAulaPrevista repositorio,
                                     IRepositorioAulaPrevistaBimestre repositorioBimestre)
        {
            this.repositorio = repositorio ?? throw new ArgumentNullException(nameof(repositorio));
            this.repositorioBimestre = repositorioBimestre ?? throw new ArgumentNullException(nameof(repositorioBimestre));
        }

        public async Task<AulasPrevistasDadasAuditoriaDto> BuscarPorId(long id)
        {
            var aulaPrevista = repositorio.ObterPorId(id);
            var aulasPrevistasBimestre = await repositorioBimestre.ObterBimestresAulasPrevistasPorId(id);

            AulasPrevistasDadasAuditoriaDto aulaPrevistaDto = MapearParaDto(aulaPrevista, aulasPrevistasBimestre) ?? new AulasPrevistasDadasAuditoriaDto();

            foreach (var aula in aulaPrevistaDto.AulasPrevistasPorBimestre)
            {
                List<string> mensagens = new List<string>();

                if (aula.Previstas.Quantidade != (aula.Criadas.QuantidadeCJ + aula.Criadas.QuantidadeTitular))
                    mensagens.Add("Quantidade de aulas previstas diferente da quantidade de aulas criadas.");

                if (aula.Previstas.Quantidade != (aula.Cumpridas + aula.Reposicoes))
                    mensagens.Add("Quantidade de aulas previstas diferente do somatório de aulas dadas + aulas repostas, após o final do bimestre.");

                if (mensagens.Any())
                    aula.Previstas.Mensagens = mensagens.ToArray();
            }

            return aulaPrevistaDto;
        }

        private AulasPrevistasDadasAuditoriaDto MapearParaDto(AulaPrevista aulaPrevista, IEnumerable<AulaPrevistaBimestreQuantidade> bimestres = null)
        {
            return aulaPrevista == null ? null : new AulasPrevistasDadasAuditoriaDto
            {
                Id = aulaPrevista.Id,
                AlteradoEm = aulaPrevista.AlteradoEm.HasValue ? aulaPrevista.AlteradoEm.Value : DateTime.MinValue,
                AlteradoPor = aulaPrevista.AlteradoPor,
                AlteradoRF = aulaPrevista.AlteradoRF,
                CriadoEm = aulaPrevista.CriadoEm,
                CriadoPor = aulaPrevista.CriadoPor,
                CriadoRF = aulaPrevista.CriadoRF,
                AulasPrevistasPorBimestre = bimestres?.Select(x => new AulasPrevistasDadasDto
                {
                    Bimestre = x.Bimestre,
                    Criadas = new AulasQuantidadePorProfessorDto()
                    {
                        QuantidadeCJ = x.CriadasCJ,
                        QuantidadeTitular = x.CriadasTitular
                    },
                    Cumpridas = x.Cumpridas,
                    Inicio = x.Inicio,
                    Fim = x.Fim,
                    Previstas = new AulasPrevistasDto() { Quantidade = x.Previstas },
                    Reposicoes = x.Reposicoes
                }).ToList()
            };
        }
    }
}
