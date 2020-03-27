﻿using SME.SGP.Aplicacao.Integracoes;
using SME.SGP.Aplicacao.Integracoes.Respostas;
using SME.SGP.Dominio;
using SME.SGP.Dominio.Interfaces;
using SME.SGP.Infra;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SME.SGP.Aplicacao
{
    public class ConsultasFechamentoTurmaDisciplina : IConsultasFechamentoTurmaDisciplina
    {
        private readonly IConsultasAulaPrevista consultasAulaPrevista;
        private readonly IConsultasPeriodoEscolar consultasPeriodoEscolar;
        private readonly IRepositorioConceito repositorioConceito;
        private readonly IRepositorioSintese repositorioSintese;
        private readonly IRepositorioFechamentoTurmaDisciplina repositorioFechamentoTurmaDisciplina;
        private readonly IRepositorioFrequenciaAlunoDisciplinaPeriodo repositorioFrequenciaAlunoDisciplinaPeriodo;
        private readonly IRepositorioParametrosSistema repositorioParametrosSistema;
        private readonly IRepositorioPeriodoEscolar repositorioPeriodoEscolar;
        private readonly IRepositorioTipoCalendario repositorioTipoCalendario;
        private readonly IRepositorioTurma repositorioTurma;
        private readonly IServicoAluno servicoAluno;
        private readonly IServicoEOL servicoEOL;
        private readonly IServicoUsuario servicoUsuario;
        private readonly IConsultasPeriodoFechamento consultasFechamento;

        public ConsultasFechamentoTurmaDisciplina(IRepositorioFechamentoTurmaDisciplina repositorioFechamentoTurmaDisciplina,
            IRepositorioTipoCalendario repositorioTipoCalendario,
            IRepositorioTurma repositorioTurma,
            IRepositorioPeriodoEscolar repositorioPeriodoEscolar,
            IRepositorioFrequenciaAlunoDisciplinaPeriodo repositorioFrequenciaAlunoDisciplinaPeriodo,
            IConsultasAulaPrevista consultasAulaPrevista,
            IConsultasPeriodoEscolar consultasPeriodoEscolar,
            IServicoEOL servicoEOL,
            IServicoUsuario servicoUsuario,
            IServicoAluno servicoAluno,
            IRepositorioConceito repositorioConceito,
            IRepositorioSintese repositorioSintese,
            IRepositorioParametrosSistema repositorioParametrosSistema,
            IConsultasPeriodoFechamento consultasFechamento
            )
        {
            this.repositorioFechamentoTurmaDisciplina = repositorioFechamentoTurmaDisciplina ?? throw new ArgumentNullException(nameof(repositorioFechamentoTurmaDisciplina));
            this.repositorioTipoCalendario = repositorioTipoCalendario ?? throw new ArgumentNullException(nameof(repositorioTipoCalendario));
            this.repositorioTurma = repositorioTurma ?? throw new ArgumentNullException(nameof(repositorioTurma));
            this.repositorioPeriodoEscolar = repositorioPeriodoEscolar ?? throw new ArgumentNullException(nameof(repositorioPeriodoEscolar));
            this.repositorioFrequenciaAlunoDisciplinaPeriodo = repositorioFrequenciaAlunoDisciplinaPeriodo ?? throw new ArgumentNullException(nameof(repositorioFrequenciaAlunoDisciplinaPeriodo));
            this.consultasAulaPrevista = consultasAulaPrevista ?? throw new ArgumentNullException(nameof(consultasAulaPrevista));
            this.consultasPeriodoEscolar = consultasPeriodoEscolar ?? throw new ArgumentNullException(nameof(consultasPeriodoEscolar));
            this.servicoEOL = servicoEOL ?? throw new ArgumentNullException(nameof(servicoEOL));
            this.servicoUsuario = servicoUsuario ?? throw new ArgumentNullException(nameof(servicoUsuario));
            this.servicoAluno = servicoAluno ?? throw new ArgumentNullException(nameof(servicoAluno));
            this.repositorioConceito = repositorioConceito ?? throw new ArgumentNullException(nameof(repositorioConceito));
            this.repositorioSintese = repositorioSintese ?? throw new ArgumentNullException(nameof(repositorioSintese));
            this.repositorioParametrosSistema = repositorioParametrosSistema ?? throw new ArgumentNullException(nameof(repositorioParametrosSistema));
            this.consultasFechamento = consultasFechamento ?? throw new ArgumentNullException(nameof(consultasFechamento));
        }

        public async Task<FechamentoTurmaDisciplina> ObterFechamentoTurmaDisciplina(string turmaId, long disciplinaId, int bimestre)
            => await repositorioFechamentoTurmaDisciplina.ObterFechamentoTurmaDisciplina(turmaId, disciplinaId, bimestre);

        public async Task<IEnumerable<NotaConceitoBimestreDto>> ObterNotasBimestre(string codigoAluno, long fechamentoTurmaId)
            => await repositorioFechamentoTurmaDisciplina.ObterNotasBimestre(codigoAluno, fechamentoTurmaId);

        public async Task<FechamentoTurmaDisciplinaBimestreDto> ObterNotasFechamentoTurmaDisciplina(string turmaId, long disciplinaId, int? bimestre)
        {
            var turma = repositorioTurma.ObterPorCodigo(turmaId);
            var tipoCalendario = repositorioTipoCalendario.BuscarPorAnoLetivoEModalidade(turma.AnoLetivo, ModalidadeParaModalidadeTipoCalendario(turma.ModalidadeCodigo));
            if (tipoCalendario == null)
                throw new NegocioException("Não foi encontrado tipo de calendário escolar, para a modalidade informada.");

            var periodosEscolares = repositorioPeriodoEscolar.ObterPorTipoCalendario(tipoCalendario.Id);
            if (periodosEscolares == null || !periodosEscolares.Any())
                throw new NegocioException("Não foi encontrado período Escolar para a modalidade informada.");

            var bimestreAtual = bimestre;
            if (!bimestreAtual.HasValue || bimestre == 0)
                bimestreAtual = ObterBimestreAtual(periodosEscolares);

            var periodoAtual = periodosEscolares.FirstOrDefault(x => x.Bimestre == bimestreAtual);
            if (periodoAtual == null)
                throw new NegocioException("Não foi encontrado período escolar para o bimestre solicitado.");

            // Carrega alunos
            var alunos = await servicoEOL.ObterAlunosPorTurma(turma.CodigoTurma, turma.AnoLetivo);
            if (alunos == null || !alunos.Any())
                throw new NegocioException("Não foi encontrado alunos para a turma informada");

            // DTO de retorno
            var fechamentoBimestre = new FechamentoTurmaDisciplinaBimestreDto()
            {
                Bimestre = bimestreAtual.Value,
                Periodo = tipoCalendario.Periodo,
                TotalAulasDadas = 0, // Carregar
                TotalAulasPrevistas = 0, // Carregar
                Alunos = new List<NotaConceitoAlunoBimestreDto>()
            };

            var disciplinasId = new long[] { disciplinaId };
            var disciplinaEOL = servicoEOL.ObterDisciplinasPorIds(disciplinasId).FirstOrDefault();
            IEnumerable<DisciplinaResposta> disciplinasRegencia = null;

            if (disciplinaEOL.Regencia)
                disciplinasRegencia = await servicoEOL.ObterDisciplinasParaPlanejamento(long.Parse(turmaId), servicoUsuario.ObterLoginAtual(), servicoUsuario.ObterPerfilAtual());

            if (!disciplinaEOL.LancaNota)
                fechamentoBimestre.EhSintese = true;

            // Carrega fechamento da Turma x Disciplina x Bimestre
            var fechamentoTurma = await ObterFechamentoTurmaDisciplina(turmaId, disciplinaId, bimestreAtual.Value);
            if (fechamentoTurma != null || fechamentoBimestre.EhSintese)
            {
                if (fechamentoTurma != null)
                {
                    fechamentoBimestre.Situacao = fechamentoTurma.Situacao;
                    fechamentoBimestre.FechamentoId = fechamentoTurma.Id;
                }

                fechamentoBimestre.Alunos = new List<NotaConceitoAlunoBimestreDto>();

                var bimestreDoPeriodo = consultasPeriodoEscolar.ObterPeriodoEscolarPorData(tipoCalendario.Id, periodoAtual.PeriodoFim);

                foreach (var aluno in alunos)
                {
                    var alunoDto = new NotaConceitoAlunoBimestreDto();
                    alunoDto.CodigoAluno = aluno.CodigoAluno;
                    alunoDto.NumeroChamada = aluno.NumeroAlunoChamada;
                    alunoDto.Nome = aluno.NomeAluno;
                    alunoDto.Ativo = aluno.CodigoSituacaoMatricula.Equals(SituacaoMatriculaAluno.Ativo);

                    var marcador = servicoAluno.ObterMarcadorAluno(aluno, bimestreDoPeriodo);
                    if (marcador != null)
                    {
                        alunoDto.Informacao = marcador.Descricao;
                    }

                    var frequenciaAluno = repositorioFrequenciaAlunoDisciplinaPeriodo.ObterPorAlunoData(aluno.CodigoAluno, periodoAtual.PeriodoFim, TipoFrequenciaAluno.PorDisciplina, disciplinaId.ToString());
                    if (frequenciaAluno != null)
                    {
                        alunoDto.QuantidadeFaltas = frequenciaAluno.TotalAusencias;
                        alunoDto.QuantidadeCompensacoes = frequenciaAluno.TotalCompensacoes;
                        alunoDto.PercentualFrequencia = frequenciaAluno.PercentualFrequencia;
                    }

                    // Carrega Frequencia do aluno
                    if (aluno.CodigoAluno != null)
                    {
                        if (fechamentoBimestre.EhSintese && fechamentoTurma == null)
                        {
                            var mediaFrequencia = double.Parse(repositorioParametrosSistema.ObterValorPorTipoEAno(TipoParametroSistema.CompensacaoAusenciaPercentualRegenciaClasse));

                            bool frequente = alunoDto.PercentualFrequencia >= mediaFrequencia;

                            alunoDto.SinteseId = frequente ? (int)SinteseEnum.Frequente : (int)SinteseEnum.NaoFrequente;
                            alunoDto.Sintese = frequente ? SinteseEnum.Frequente.GetAttribute<DisplayAttribute>().Name :
                                                           SinteseEnum.NaoFrequente.GetAttribute<DisplayAttribute>().Name;
                        }
                        else
                        {
                            // Carrega notas do bimestre
                            var notasConceitoBimestre = await ObterNotasBimestre(aluno.CodigoAluno, fechamentoTurma.Id);

                            if (notasConceitoBimestre.Count() > 0)
                                alunoDto.Notas = new List<NotaConceitoBimestreRetornoDto>();

                            foreach (var notaConceitoBimestre in notasConceitoBimestre)
                            {
                                ((List<NotaConceitoBimestreRetornoDto>)alunoDto.Notas).Add(new NotaConceitoBimestreRetornoDto()
                                {
                                    DisciplinaId = notaConceitoBimestre.DisciplinaId,
                                    Disciplina = disciplinaEOL.Regencia ?
                                        disciplinasRegencia.FirstOrDefault(a => a.CodigoComponenteCurricular == notaConceitoBimestre.DisciplinaId).Nome :
                                        disciplinaEOL.Nome,
                                    NotaConceito = notaConceitoBimestre.Nota > 0 ? notaConceitoBimestre.Nota.ToString() : 
                                                   notaConceitoBimestre.ConceitoId > 0 ? ObterConceito(notaConceitoBimestre.ConceitoId) : 
                                                   ObterSintese(notaConceitoBimestre.SinteseId)
                                });
                            }
                        }

                        fechamentoBimestre.Alunos.Add(alunoDto);
                    }
                }
            }

            var aulaPrevisa = await consultasAulaPrevista.ObterAulaPrevistaDada(turma.ModalidadeCodigo, turma.CodigoTurma, disciplinaId.ToString());
            var aulaPrevistaBimestreAtual = new AulasPrevistasDadasDto();
            if (aulaPrevisa != null)
            {
                aulaPrevistaBimestreAtual = aulaPrevisa.AulasPrevistasPorBimestre.FirstOrDefault(a => a.Bimestre == bimestreAtual);
            }

            fechamentoBimestre.Bimestre = bimestreAtual.Value;
            fechamentoBimestre.TotalAulasDadas = aulaPrevistaBimestreAtual.Cumpridas;
            fechamentoBimestre.TotalAulasPrevistas = aulaPrevistaBimestreAtual.Previstas.Quantidade;

            fechamentoBimestre.PodeProcessarReprocessar = await consultasFechamento.TurmaEmPeriodoDeFechamento(turma.CodigoTurma, DateTime.Now, bimestreAtual.Value);

            return fechamentoBimestre;
        }

        private ModalidadeTipoCalendario ModalidadeParaModalidadeTipoCalendario(Modalidade modalidade)
        {
            switch (modalidade)
            {
                case Modalidade.EJA:
                    return ModalidadeTipoCalendario.EJA;

                default:
                    return ModalidadeTipoCalendario.FundamentalMedio;
            }
        }

        private int ObterBimestreAtual(IEnumerable<PeriodoEscolar> periodosEscolares)
        {
            var dataPesquisa = DateTime.Now;

            var periodoEscolar = periodosEscolares.FirstOrDefault(x => x.PeriodoInicio.Date <= dataPesquisa.Date && x.PeriodoFim.Date >= dataPesquisa.Date);

            if (periodoEscolar == null)
                return 1;
            else return periodoEscolar.Bimestre;
        }

        private string ObterConceito(long id)
        {
            var conceito = repositorioConceito.ObterPorId(id);
            return conceito != null ? conceito.Valor : "";
        }

        private string ObterSintese(long id)
        {
            var sintese = repositorioSintese.ObterPorId(id);
            return sintese != null ? sintese.Descricao : "";
        }
    }
}