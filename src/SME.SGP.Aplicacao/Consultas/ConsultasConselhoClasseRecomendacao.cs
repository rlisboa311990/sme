﻿using SME.SGP.Dominio;
using SME.SGP.Dominio.Interfaces;
using SME.SGP.Infra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SME.SGP.Aplicacao
{
    public class ConsultasConselhoClasseRecomendacao : IConsultasConselhoClasseRecomendacao
    {
        private readonly IConsultasFechamentoAluno consultasFechamentoAluno;
        private readonly IConsultasFechamentoTurma consultasFechamentoTurma;
        private readonly IConsultasPeriodoEscolar consultasPeriodoEscolar;
        private readonly IRepositorioConselhoClasseAluno repositorioConselhoClasseAluno;
        private readonly IRepositorioConselhoClasseRecomendacao repositorioConselhoClasseRecomendacao;
        private readonly IRepositorioTurma repositorioTurma;

        public ConsultasConselhoClasseRecomendacao(IRepositorioConselhoClasseRecomendacao repositorioConselhoClasseRecomendacao,
            IRepositorioConselhoClasseAluno repositorioConselhoClasseAluno, IConsultasPeriodoEscolar consultasPeriodoEscolar, IRepositorioTurma repositorioTurma,
            IConsultasFechamentoAluno consultasFechamentoAluno, IConsultasFechamentoTurma consultasFechamentoTurma)
        {
            this.repositorioConselhoClasseAluno = repositorioConselhoClasseAluno ?? throw new ArgumentNullException(nameof(repositorioConselhoClasseAluno));
            this.consultasPeriodoEscolar = consultasPeriodoEscolar ?? throw new ArgumentNullException(nameof(consultasPeriodoEscolar));
            this.repositorioTurma = repositorioTurma ?? throw new ArgumentNullException(nameof(repositorioTurma));
            this.consultasFechamentoAluno = consultasFechamentoAluno ?? throw new ArgumentNullException(nameof(consultasFechamentoAluno));
            this.repositorioConselhoClasseRecomendacao = repositorioConselhoClasseRecomendacao ?? throw new ArgumentNullException(nameof(repositorioConselhoClasseRecomendacao));
            this.consultasFechamentoTurma = consultasFechamentoTurma ?? throw new ArgumentNullException(nameof(consultasFechamentoTurma));
        }

        public string MontaTextUlLis(IEnumerable<string> textos)
        {
            var str = new StringBuilder("<ul>");

            foreach (var item in textos)
            {
                str.AppendFormat("<li>{0}</li>", item);
            }
            str.AppendLine("</ul>");

            return str.ToString().Trim();
        }

        public async Task<ConsultasConselhoClasseRecomendacaoConsultaDto> ObterRecomendacoesAlunoFamilia(string turmaCodigo, string alunoCodigo, int bimestre, Modalidade turmaModalidade, bool EhFinal = false)
        {
            if (bimestre == 0 && !EhFinal)
                bimestre = ObterBimestreAtual(turmaModalidade);

            var conselhoClasseAluno = await repositorioConselhoClasseAluno.ObterPorFiltrosAsync(turmaCodigo, alunoCodigo, bimestre, EhFinal);

            var anotacoesDoAluno = await consultasFechamentoAluno.ObterAnotacaoAlunoParaConselhoAsync(alunoCodigo, turmaCodigo, bimestre, EhFinal);
            if (anotacoesDoAluno == null)
                anotacoesDoAluno = new List<FechamentoAlunoAnotacaoConselhoDto>();

            if (conselhoClasseAluno == null)
            {
                return await ObterRecomendacoesIniciais(anotacoesDoAluno, bimestre);
            }
            else return TransformaEntidadeEmConsultaDto(conselhoClasseAluno, anotacoesDoAluno, bimestre);
        }

        private int ObterBimestreAtual(Modalidade turmaModalidade)
        {
            return consultasPeriodoEscolar.ObterBimestre(DateTime.Today, turmaModalidade);
        }

        private async Task<ConsultasConselhoClasseRecomendacaoConsultaDto> ObterRecomendacoesIniciais(IEnumerable<FechamentoAlunoAnotacaoConselhoDto> anotacoesAluno, int bimestre)
        {
            var recomendacoes = await repositorioConselhoClasseRecomendacao.ObterTodosAsync();

            if (!recomendacoes.Any())
                throw new NegocioException("Não foi possível localizar as recomendações da família e aluno.");

            return new ConsultasConselhoClasseRecomendacaoConsultaDto()
            {
                FechamentoTurmaId = anotacoesAluno.First().FechamentoTurmaId,
                RecomendacaoAluno = MontaTextUlLis(recomendacoes.Where(a => a.Tipo == ConselhoClasseRecomendacaoTipo.Aluno).Select(b => b.Recomendacao)),
                RecomendacaoFamilia = MontaTextUlLis(recomendacoes.Where(a => a.Tipo == ConselhoClasseRecomendacaoTipo.Familia).Select(b => b.Recomendacao)),
                AnotacoesAluno = anotacoesAluno,
                Bimestre = bimestre
            };
        }

        private ConsultasConselhoClasseRecomendacaoConsultaDto TransformaEntidadeEmConsultaDto(ConselhoClasseAluno conselhoClasseAluno,
            IEnumerable<FechamentoAlunoAnotacaoConselhoDto> anotacoesAluno, int bimestre)
        {
            return new ConsultasConselhoClasseRecomendacaoConsultaDto()
            {
                FechamentoTurmaId = conselhoClasseAluno.ConselhoClasse.FechamentoTurmaId,
                ConselhoClasseId = conselhoClasseAluno.ConselhoClasseId,
                RecomendacaoAluno = conselhoClasseAluno.RecomendacoesAluno,
                RecomendacaoFamilia = conselhoClasseAluno.RecomendacoesFamilia,
                AnotacoesAluno = anotacoesAluno,
                AnotacoesPedagogicas = conselhoClasseAluno.AnotacoesPedagogicas,
                Bimestre = bimestre,
                Auditoria = (AuditoriaDto)conselhoClasseAluno
            };
        }
    }
}