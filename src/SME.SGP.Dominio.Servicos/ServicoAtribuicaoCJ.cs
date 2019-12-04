﻿using SME.SGP.Aplicacao.Integracoes;
using SME.SGP.Aplicacao.Servicos;
using SME.SGP.Dominio.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SME.SGP.Dominio.Servicos
{
    public class ServicoAtribuicaoCJ : IServicoAtribuicaoCJ
    {
        private static readonly long[] componentesQueNaoPodemSerSubstituidos = { 1033, 1051, 1052, 1053, 1054, 1030 };
        private readonly IRepositorioAbrangencia repositorioAbrangencia;
        private readonly IRepositorioAtribuicaoCJ repositorioAtribuicaoCJ;
        private readonly IRepositorioAula repositorioAula;
        private readonly IRepositorioTurma repositorioTurma;
        private readonly IServicoAbrangencia servicoAbrangencia;
        private readonly IServicoEOL servicoEOL;

        public ServicoAtribuicaoCJ(IRepositorioAtribuicaoCJ repositorioAtribuicaoCJ, IServicoAbrangencia servicoAbrangencia, IRepositorioTurma repositorioTurma,
            IRepositorioAbrangencia repositorioAbrangencia, IServicoEOL servicoEOL, IRepositorioAula repositorioAula)
        {
            this.repositorioAtribuicaoCJ = repositorioAtribuicaoCJ ?? throw new ArgumentNullException(nameof(repositorioAtribuicaoCJ));
            this.servicoAbrangencia = servicoAbrangencia ?? throw new ArgumentNullException(nameof(servicoAbrangencia));
            this.repositorioTurma = repositorioTurma ?? throw new ArgumentNullException(nameof(repositorioTurma));
            this.repositorioAbrangencia = repositorioAbrangencia ?? throw new ArgumentNullException(nameof(repositorioAbrangencia));
            this.servicoEOL = servicoEOL ?? throw new ArgumentNullException(nameof(servicoEOL));
            this.repositorioAula = repositorioAula ?? throw new ArgumentNullException(nameof(repositorioAula));
        }

        public async Task Salvar(AtribuicaoCJ atribuicaoCJ)
        {
            var professorValidoNoEol = servicoEOL.ValidarProfessor(atribuicaoCJ.ProfessorRf);

            if (!professorValidoNoEol)
                throw new NegocioException("Este professor não é válido para ser CJ.");

            ValidaComponentesCurricularesQueNaoPodemSerSubstituidos(atribuicaoCJ);
            ValidaSeTemAulaCriada(atribuicaoCJ);

            await repositorioAtribuicaoCJ.SalvarAsync(atribuicaoCJ);
            TratarAbrangencia(atribuicaoCJ);
        }

        private async void TratarAbrangencia(AtribuicaoCJ atribuicaoCJ)
        {
            if (atribuicaoCJ.Substituir)
            {
                var turma = repositorioTurma.ObterPorId(atribuicaoCJ.TurmaId);
                if (turma == null)
                    throw new NegocioException($"Não foi possível localizar a turma {atribuicaoCJ.TurmaId} da abrangência.");

                var abrangencias = new Abrangencia[] { new Abrangencia() { Perfil = Perfis.PERFIL_CJ, TurmaId = turma.Id } };

                servicoAbrangencia.SalvarAbrangencias(abrangencias, atribuicaoCJ.ProfessorRf);
            }
            else
            {
                var abrangencias = await repositorioAbrangencia.ObterAbrangenciaSintetica(atribuicaoCJ.ProfessorRf, Perfis.PERFIL_CJ, atribuicaoCJ.TurmaId);

                if (abrangencias != null && abrangencias.Any())
                {
                    servicoAbrangencia.RemoverAbrangencias(abrangencias.Select(a => a.Id).ToArray());
                }
            }
        }

        private void ValidaComponentesCurricularesQueNaoPodemSerSubstituidos(AtribuicaoCJ atribuicaoCJ)
        {
            if (componentesQueNaoPodemSerSubstituidos.Any(a => a == atribuicaoCJ.DisciplinaId))
            {
                var nomeComponenteCurricular = servicoEOL.ObterDisciplinasPorIds(new long[] { atribuicaoCJ.DisciplinaId });
                if (nomeComponenteCurricular != null && nomeComponenteCurricular.Any())
                {
                    throw new NegocioException($"O componente curricular {nomeComponenteCurricular.FirstOrDefault().Nome} não pode ser substituido.");
                }
                else throw new NegocioException($"Não foi possível localizar o nome do componente curricular de identificador {atribuicaoCJ.DisciplinaId} no EOL.");
            }
        }

        private async void ValidaSeTemAulaCriada(AtribuicaoCJ atribuicaoCJ)
        {
            if (atribuicaoCJ.Id > 0 && !atribuicaoCJ.Substituir)
            {
                var aulas = await repositorioAula.ObterAulas(atribuicaoCJ.TurmaId, atribuicaoCJ.UeId, atribuicaoCJ.ProfessorRf, null, atribuicaoCJ.DisciplinaId.ToString());
                if (aulas != null && aulas.Any())
                    throw new NegocioException($"Não é possível tirar a substituição da turma {atribuicaoCJ.TurmaId} para o componente curricular {atribuicaoCJ.DisciplinaId}");
            }
        }
    }
}