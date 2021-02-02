﻿using Dapper.FluentMap;
using Dapper.FluentMap.Dommel;
using SME.SGP.Dominio;

namespace SME.SGP.Dados.Mapeamentos
{
    public static class RegistrarMapeamentos
    {
        public static void Registrar()
        {
            FluentMapper.Initialize(config =>
           {
               config.AddMap(new PlanoCicloMap());
               config.AddMap(new RecuperacaoParalelaObjetivoDesenvolvimentoMap());
               config.AddMap(new RecuperacaoParalelaObjetivoDesenvolvimentoPlanoMap());
               config.AddMap(new MatrizSaberMap());
               config.AddMap(new MatrizSaberPlanoMap());
               config.AddMap(new AuditoriaMap());
               config.AddMap(new CicloMap());
               config.AddMap(new PlanoAnualMap());
               config.AddMap(new PeriodoEscolarMap());
               config.AddMap(new ObjetivoAprendizagemPlanoMap());
               config.AddMap(new DisciplinaPlanoMap());
               config.AddMap(new ComponenteCurricularJuremaMap());
               config.AddMap(new SupervisorEscolaDreMap());
               config.AddMap(new NotificacaoMap());
               config.AddMap(new WorkflowAprovacaoMap());
               config.AddMap(new WorkflowAprovacaoNivelMap());
               config.AddMap(new WorkflowAprovacaoNivelNotificacaoMap());
               config.AddMap(new WorkflowAprovacaoNivelUsuarioMap());
               config.AddMap(new UsuarioMap());
               config.AddMap(new PrioridadePerfilMap());
               config.AddMap(new ConfiguracaoEmailMap());
               config.AddMap(new TipoCalendarioMap());
               config.AddMap(new FeriadoCalendarioMap());
               config.AddMap(new EventoMap());
               config.AddMap(new EventoTipoMap());
               config.AddMap(new AulaMap());
               config.AddMap(new GradeMap());
               config.AddMap(new GradeFiltroMap());
               config.AddMap(new GradeDisciplinaMap());
               config.AddMap(new RegistroFrequenciaMap());
               config.AddMap(new RegistroAusenciaAlunoMap());
               config.AddMap(new PlanoAulaMap());
               config.AddMap(new ObjetivoAprendizagemAulaMap());
               config.AddMap(new AtribuicaoEsporadicaMap());
               config.AddMap(new AtividadeAvaliativaMap());
               config.AddMap(new TipoAvaliacaoMap());
               config.AddMap(new AtribuicaoCJMap());
               config.AddMap(new DreMap());
               config.AddMap(new UeMap());
               config.AddMap(new TurmaMap());
               config.AddMap(new AbrangenciaMap());
               config.AddMap(new FrequenciaAlunoMap());
               config.AddMap(new AtividadeAvaliativaRegenciaMap());
               config.AddMap(new NotificacaoFrequenciaMap());
               config.AddMap(new EventoMatriculaMap());
               config.AddMap(new NotaConceitoMap());
               config.AddMap(new NotaTipoValorMap());
               config.AddMap(new NotaParametroMap());
               config.AddMap(new ConceitoValorMap());
               config.AddMap(new NotaConceitoCicloParametroMap());
               config.AddMap(new AulaPrevistaMap());
               config.AddMap(new NotificacaoAulaPrevistaMap());
               config.AddMap(new AulaPrevistaBimestreMap());
               config.AddMap(new RegistroPoaMap());
               config.AddMap(new AtividadeAvaliativaDisciplinaMap());
               config.AddMap(new FechamentoReaberturaMap());
               config.AddMap(new FechamentoReaberturaBimestreMap());
               config.AddMap(new FechamentoReaberturaNotificacaoMap());
               config.AddMap(new CompensacaoAusenciaMap());
               config.AddMap(new CompensacaoAusenciaAlunoMap());
               config.AddMap(new CompensacaoAusenciaDisciplinaRegenciaMap());
               config.AddMap(new ProcessoExecutandoMap());
               config.AddMap(new PeriodoFechamentoMap());
               config.AddMap(new PeriodoFechamentoBimestreMap());
               config.AddMap(new NotificacaoCompensacaoAusenciaMap());
               config.AddMap(new EventoFechamentoMap());
               config.AddMap(new FechamentoTurmaDisciplinaMap());
               config.AddMap(new FechamentoNotaMap());
               config.AddMap(new RecuperacaoParalelaMap());
               config.AddMap(new RecuperacaoParalelaPeriodoMap());
               config.AddMap(new RecuperacaoParalelaPeriodoObjetivoRespostaMap());
               config.AddMap(new RecuperacaoParalelaEixoMap());
               config.AddMap(new RecuperacaoParalelaObjetivoMap());
               config.AddMap(new RecuperacaoParalelaRespostaMap());
               config.AddMap(new NotificacaoAulaMap());
               config.AddMap(new HistoricoEmailUsuarioMap());
               config.AddMap(new PendenciaMap());
               config.AddMap(new PendenciaFechamentoMap());
               config.AddMap(new SinteseValorMap());
               config.AddMap(new FechamentoAlunoMap());
               config.AddMap(new FechamentoTurmaMap());
               config.AddMap(new ConselhoClasseMap());
               config.AddMap(new ConselhoClasseAlunoMap());
               config.AddMap(new ConselhoClasseNotaMap());
               config.AddMap(new WfAprovacaoNotaFechamentoMap());
               config.AddMap(new GrupoComunicacaoMap());
               config.AddMap(new ComunicadoMap());
               config.AddMap(new ComunicadoAlunoMap());
               config.AddMap(new ComunicadoTurmaMap());
               config.AddMap(new ComunicadoGrupoMap());
               config.AddMap(new ConselhoClasseRecomendacaoMap());
               config.AddMap(new TipoEscolaMap());
               config.AddMap(new CicloEnsinoMap());
               config.AddMap(new RelatorioSemestralTurmaPAPMap());
               config.AddMap(new RelatorioSemestralPAPAlunoMap());
               config.AddMap(new RelatorioSemestralAlunoSecaoMap());
               config.AddMap(new SecaoRelatorioSemestralPAPMap());
               config.AddMap(new ConselhoClasseParecerAnoMap());
               config.AddMap(new ConselhoClasseParecerConclusivoMap());
               config.AddMap(new ObjetivoAprendizagemMap());
               config.AddMap(new PlanoAnualTerritorioSaberMap());
               config.AddMap(new RelatorioCorrelacaoMap());
               config.AddMap(new RelatorioCorrelacaoJasperMap());
               config.AddMap(new HistoricoReinicioSenhaMap());
               config.AddMap(new AnotacaoFrequenciaAlunoMap());
               config.AddMap(new MotivoAusenciaMap());
               config.AddMap(new CartaIntencoesMap());
               config.AddMap(new DiarioBordoMap());
               config.AddMap(new DiarioBordoObservacaoMap());
               config.AddMap(new DevolutivaMap());
               config.AddMap(new CartaIntencoesObservacaoMap());
               config.AddMap(new PendenciaAulaMap());
               config.AddMap(new DiarioBordoObservacaoNotificacaoMap());
               config.AddMap(new NotificacaoCartaIntencoesObservacaoMap());
               config.AddMap(new DevolutivaDiarioBordoNotificacaoMap());
               config.AddMap(new PlanejamentoAnualComponenteMap());
               config.AddMap(new PlanejamentoAnualMap());
               config.AddMap(new PlanejamentoAnualObjetivosAprendizagemMap());
               config.AddMap(new PlanejamentoAnualPeriodoEscolarMap());
               config.AddMap(new ArquivoMap());
               config.AddMap(new HistoricoNotaMap());
               config.AddMap(new HistoricoNotaFechamentoMap());
               config.AddMap(new HistoricoNotaConselhoClasseMap());
               config.AddMap(new ClassificacaoDocumentoMap());
               config.AddMap(new TipoDocumentoMap());
               config.AddMap(new DocumentoMap());
               config.AddMap(new PendenciaUsuarioMap());
               config.AddMap(new PendenciaCalendarioUeMap());
               config.AddMap(new PendenciaParametroEventoMap());
               config.AddMap(new PendenciaProfessorMap());
               config.AddMap(new QuestionarioMap());
               config.AddMap(new QuestaoMap());
               config.AddMap(new OpcaoRespostaMap());
               config.AddMap(new SecaoEncaminhamentoAEEMap());
               config.AddMap(new EncaminhamentoAEEMap());
               config.AddMap(new EncaminhamentoAEESecaoMap());
               config.AddMap(new QuestaoEncaminhamentoAEEMap());
               config.AddMap(new RespostaEncaminhamentoAEEMap());
               config.AddMap(new RegistroIndividualMap());
               config.ForDommel();
           });
        }
    }
}