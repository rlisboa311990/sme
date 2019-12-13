import React, { useEffect, useState, useCallback } from 'react';
import { useSelector, useDispatch } from 'react-redux';
import styled from 'styled-components';
import { CampoData, Auditoria, Loader } from '~/componentes';
import Cabecalho from '~/componentes-sgp/cabecalho';
import ListaFrequencia from '~/componentes-sgp/ListaFrequencia/listaFrequencia';
import Ordenacao from '~/componentes-sgp/Ordenacao/ordenacao';
import Button from '~/componentes/button';
import Card from '~/componentes/card';
import CardCollapse from '~/componentes/cardCollapse';
import { Colors } from '~/componentes/colors';
import PlanoAula from '../PlanoAula/plano-aula';
import SelectComponent from '~/componentes/select';
import { URL_HOME } from '~/constantes/url';
import { confirmar, erros, sucesso } from '~/servicos/alertas';
import api from '~/servicos/api';
import history from '~/servicos/history';
import Alert from '~/componentes/alert';
import { verificaSomenteConsulta } from '~/servicos/servico-navegacao';
import RotasDto from '~/dtos/rotasDto';
import { SelecionarDisciplina } from '~/redux/modulos/planoAula/actions';
import { stringNulaOuEmBranco } from '~/utils/funcoes/gerais';
import ModalMultiLinhas from '~/componentes/modalMultiLinhas';
import modalidade from '~/dtos/modalidade';
import ServicoDisciplina from '~/servicos/Paginas/ServicoDisciplina';
import Grid from '~/componentes/grid';

const FrequenciaPlanoAula = () => {
  const usuario = useSelector(store => store.usuario);
  const dispatch = useDispatch();

  const [somenteConsulta, setSomenteConsulta] = useState(false);
  const [permiteRegistroFrequencia, setPermiteRegistroFrequencia] = useState(
    true
  );
  const permissoesTela = usuario.permissoes[RotasDto.FREQUENCIA_PLANO_AULA];

  const { turmaSelecionada, ehProfessor, ehProfessorCj } = usuario;
  const ehEja = !!(
    turmaSelecionada &&
    String(turmaSelecionada.modalidade) === String(modalidade.EJA)
  );
  const ehMedio = !!(
    turmaSelecionada &&
    String(turmaSelecionada.modalidade) === String(modalidade.ENSINO_MEDIO)
  );
  const turmaId = turmaSelecionada ? turmaSelecionada.turma : 0;
  const anoLetivo = turmaSelecionada ? turmaSelecionada.anoLetivo : 0;

  const [carregandoFrequencia, setCarregandoFrequencia] = useState(true);
  const [carregandoDisciplinas, setCarregandoDisciplinas] = useState(true);

  const [listaDisciplinas, setListaDisciplinas] = useState([]);
  const [disciplinaSelecionada, setDisciplinaSelecionada] = useState(undefined);
  const [disciplinaIdSelecionada, setDisciplinaIdSelecionada] = useState(
    undefined
  );
  const [listaDatasAulas, setListaDatasAulas] = useState([]);
  const [dataSelecionada, setDataSelecionada] = useState('');

  const [frequencia, setFrequencia] = useState([]);
  const [aulaId, setAulaId] = useState(0);
  const [frequenciaId, setFrequenciaId] = useState(0);
  const [
    carregandoDiasParaHabilitar,
    setCarregandoDiasParaHabilitar,
  ] = useState(false);
  const [exibirCardFrequencia, setExibirCardFrequencia] = useState(false);
  const [modoEdicaoFrequencia, setModoEdicaoFrequencia] = useState(false);
  const [desabilitarDisciplina, setDesabilitarDisciplina] = useState(false);
  const [diasParaHabilitar, setDiasParaHabilitar] = useState([]);
  const [auditoria, setAuditoria] = useState([]);
  const [exibirAuditoria, setExibirAuditoria] = useState(false);
  const [desabilitarCampos, setDesabilitarCampos] = useState(false);
  const [modoEdicaoPlanoAula, setModoEdicaoPlanoAula] = useState(false);
  const [ehRegencia, setEhRegencia] = useState(false);
  const [aula, setAula] = useState(undefined);
  const [auditoriaPlano, setAuditoriaPlano] = useState([]);
  const [planoAula, setPlanoAula] = useState({
    aulaId: 0,
    id: 0,
    qtdAulas: 0,
    temObjetivos: ehProfessor && !ehEja,
    descricao: null,
    desenvolvimentoAula: null,
    recuperacaoAula: null,
    licaoCasa: null,
    objetivosAprendizagemAula: [],
    migrado: false,
  });
  const [temObjetivos, setTemObjetivos] = useState(false);
  const [errosValidacaoPlano, setErrosValidacaoPlano] = useState([]);
  const [materias, setMaterias] = useState([]);
  const [mostrarErros, setMostarErros] = useState(false);

  const [carregandoSalvar, setCarregandoSalvar] = useState(false);

  const obterDatasDeAulasDisponiveis = useCallback(
    async disciplinaId => {
      setCarregandoDiasParaHabilitar(true);
      const datasDeAulas = await api
        .get(
          `v1/calendarios/frequencias/aulas/datas/${anoLetivo}/turmas/${turmaId}/disciplinas/${disciplinaId}`
        )
        .catch(e => {
          setCarregandoDiasParaHabilitar(false);
          erros(e);
        });

      setCarregandoDiasParaHabilitar(false);
      if (datasDeAulas && datasDeAulas.data) {
        setListaDatasAulas(datasDeAulas.data);
        const habilitar = datasDeAulas.data.map(item =>
          window.moment(item.data).format('YYYY-MM-DD')
        );
        setDiasParaHabilitar(habilitar);
      } else {
        setListaDatasAulas([]);
        setDiasParaHabilitar([]);
      }
    },
    [anoLetivo, turmaId]
  );

  const setarDisciplina = disciplinaId => {
    resetarTelaFrequencia(true);
    const disciplina = listaDisciplinas.find(
      disc => String(disc.codigoComponenteCurricular) === disciplinaId
    );
    setDisciplinaSelecionada(disciplina);
    setDisciplinaIdSelecionada(disciplinaId);
    if (disciplinaId) {
      obterDatasDeAulasDisponiveis(disciplinaId);
    } else {
      setListaDatasAulas([]);
      setDiasParaHabilitar([]);
    }
  };

  useEffect(() => {
    const obterDisciplinas = async () => {
      const disciplinas = await ServicoDisciplina.obterDisciplinasPorTurma(
        turmaId
      );
      if (disciplinas.data) {
        setListaDisciplinas(disciplinas.data);
      } else {
        setListaDisciplinas([]);
      }
      if (disciplinas.data && disciplinas.data.length === 1) {
        const disciplina = disciplinas.data[0];
        setDisciplinaSelecionada(disciplina);
        setDisciplinaIdSelecionada(
          String(disciplina.codigoComponenteCurricular)
        );
        setDesabilitarDisciplina(true);
        obterDatasDeAulasDisponiveis(disciplina.codigoComponenteCurricular);
        dispatch(SelecionarDisciplina(disciplinas.data[0]));
      }
      setCarregandoDisciplinas(false);
    };

    if (turmaId) {
      setDisciplinaSelecionada(undefined);
      setDisciplinaIdSelecionada(undefined);
      obterDisciplinas(turmaId);
    } else {
      resetarTelaFrequencia();
      setAulaId(0);
      setListaDisciplinas([]);
      setListaDatasAulas([]);
      setDesabilitarDisciplina(false);
      setDiasParaHabilitar([]);
    }

    const somenteConsultarFrequencia = verificaSomenteConsulta(permissoesTela);
    setSomenteConsulta(somenteConsultarFrequencia);
  }, [
    dispatch,
    obterDatasDeAulasDisponiveis,
    permissoesTela,
    turmaId,
    turmaSelecionada.turma,
  ]);

  useEffect(() => {
    const desabilitar =
      frequenciaId > 0
        ? somenteConsulta || !permissoesTela.podeAlterar
        : somenteConsulta || !permissoesTela.podeIncluir;
    setDesabilitarCampos(desabilitar);
  }, [
    frequenciaId,
    permissoesTela.podeAlterar,
    permissoesTela.podeIncluir,
    somenteConsulta,
  ]);

  const obterListaFrequencia = async aulaId => {
    setAulaId(aulaId);
    const frequenciaAlunos = await api
      .get(`v1/calendarios/frequencias`, { params: { aulaId } })
      .catch(e => erros(e));
    if (frequenciaAlunos && frequenciaAlunos.data) {
      setFrequenciaId(frequenciaAlunos.data.id);
      setAuditoria({
        criadoPor: frequenciaAlunos.data.criadoPor,
        criadoRf: frequenciaAlunos.data.criadoRf,
        criadoEm: frequenciaAlunos.data.criadoEm,
        alteradoPor: frequenciaAlunos.data.alteradoPor,
        alteradoRf: frequenciaAlunos.data.alteradoRf,
        alteradoEm: frequenciaAlunos.data.alteradoEm,
      });
      setExibirAuditoria(true);
      setFrequencia(frequenciaAlunos.data.listaFrequencia);
      setPermiteRegistroFrequencia(!frequenciaAlunos.data.desabilitado);
    }
    setCarregandoFrequencia(false);
  };

  const [carregandoMaterias, setCarregandoMaterias] = useState(true);

  const obterPlanoAula = async aula => {
    setEhRegencia(disciplinaSelecionada.regencia);
    const plano = await api.get(`v1/planos/aulas/${aula.idAula}`);
    const dadosPlano = plano.data;
    if (dadosPlano) {
      planoAula.qtdAulas = dadosPlano.qtdAulas;
      if (dadosPlano.id > 0) {
        dadosPlano.objetivosAprendizagemAula.forEach(objetivo => {
          objetivo.selected = true;
        });
        dadosPlano.objetivosAprendizagemAula = [
          ...dadosPlano.objetivosAprendizagemAula,
        ];
        dadosPlano.temObjetivos =
          (disciplinaSelecionada.regencia || ehProfessor) && !ehEja;
        setPlanoAula(dadosPlano);
        const audPlano = {
          criadoEm: dadosPlano.criadoEm,
          criadoPor: dadosPlano.criadoPor,
          alteradoEm: dadosPlano.alteradoEm,
          alteradoPor: dadosPlano.alteradoPor,
        };
        setAuditoriaPlano(audPlano);
      } else {
        setModoEdicaoPlanoAula(false);
      }
    }

    if (disciplinaSelecionada.regencia || ehProfessor || ehProfessorCj) {
      let disciplinas = {};
      if (disciplinaSelecionada.regencia) {
        setTemObjetivos(true);
        disciplinas = await api.get(
          `v1/professores/turmas/${turmaId}/disciplinas/planejamento?codigoDisciplina=${disciplinaSelecionada.codigoComponenteCurricular}&regencia=true`
        );
        if (disciplinas.data && disciplinas.data.length > 0) {
          const disciplinasRegencia = [];
          disciplinas.data.forEach(disciplina => {
            disciplinasRegencia.push({
              id: disciplina.codigoComponenteCurricular,
              descricao: disciplina.nome,
            });
          });
          setMaterias([...disciplinasRegencia]);
        }
      } else {
        disciplinas = await api.get(
          `v1/objetivos-aprendizagem/disciplinas/turmas/${turmaId}/componentes/${disciplinaSelecionada.codigoComponenteCurricular}?dataAula=${aula.data}`
        );
        const dadosDisciplinas = disciplinas.data;
        if (dadosDisciplinas) {
          setTemObjetivos(true);
          setMaterias([...dadosDisciplinas]);
        } else {
          disciplinas = await api.get(
            `v1/professores/turmas/${turmaId}/disciplinas/planejamento?codigoDisciplina=${disciplinaSelecionada.codigoComponenteCurricular}&regencia=false`
          );
          if (disciplinas.data && disciplinas.data.length > 0) {
            const dados = disciplinas.data[0];
            setTemObjetivos(dados.possuiObjetivos);
            if (dados.possuiObjetivos) {
              const materia = {
                id: dados.codigoComponenteCurricular,
                descricao: dados.nome,
              };
              materias.push(materia);
              setMaterias([...materias]);
            }
          }
        }
      }
      setCarregandoMaterias(false);
    } else {
      setCarregandoMaterias(false);
    }
  };

  const onClickVoltar = async () => {
    if (!desabilitarCampos && (modoEdicaoFrequencia || modoEdicaoPlanoAula)) {
      const confirmado = await pergutarParaSalvar();
      if (confirmado) {
        if (modoEdicaoFrequencia) {
          await onSalvarFrequencia();
        }
        if (modoEdicaoPlanoAula) {
          await onSalvarPlanoAula();
        }
        irParaHome();
      } else {
        irParaHome();
      }
    } else {
      irParaHome();
    }
  };

  const pergutarParaSalvar = () => {
    return confirmar(
      'Atenção',
      '',
      'Suas alterações não foram salvas, deseja salvar agora?'
    );
  };

  const irParaHome = () => {
    history.push(URL_HOME);
  };

  const onClickCancelar = async () => {
    if (!desabilitarCampos && (modoEdicaoFrequencia || modoEdicaoPlanoAula)) {
      const confirmou = await confirmar(
        'Atenção',
        'Você não salvou as informações preenchidas.',
        'Deseja realmente cancelar as alterações?'
      );
      if (confirmou) {
        obterListaFrequencia(aulaId);
        setModoEdicaoFrequencia(false);
        obterPlanoAula(obterAulaSelecionada(dataSelecionada));
        setModoEdicaoPlanoAula(false);
        resetarPlanoAula();
      }
    }
  };

  const obterAulaSelecionada = data => {
    const aulaDataSelecionada = listaDatasAulas.find(item =>
      window.moment(item.data).isSame(data, 'date')
    );
    return aulaDataSelecionada;
  };

  const onClickSalvar = click => {
    if (modoEdicaoFrequencia && permiteRegistroFrequencia) {
      onSalvarFrequencia(click);
    }
    if (modoEdicaoPlanoAula) {
      onSalvarPlanoAula();
    }
  };

  const aposSalvarFrequencia = () => {
    setExibirCardFrequencia(false);
    setModoEdicaoFrequencia(false);
    obterListaFrequencia(aulaId);
  };

  const onSalvarFrequencia = click => {
    setCarregandoSalvar(true);
    return new Promise((resolve, reject) => {
      const valorParaSalvar = {
        aulaId,
        listaFrequencia: frequencia,
      };
      return api
        .post(`v1/calendarios/frequencias`, valorParaSalvar)
        .then(salvouFrequencia => {
          if (salvouFrequencia && salvouFrequencia.status === 200) {
            sucesso('Frequência realizada com sucesso.');
            if (click) {
              aposSalvarFrequencia();
            }
            setCarregandoSalvar(false);
            resolve(true);
            return true;
          }
          resolve(false);
          return false;
        })
        .catch(e => {
          setCarregandoSalvar(false);
          erros(e);
          reject(e);
        });
    });
  };

  const onSalvarPlanoAula = async () => {
    const objetivosId = [];
    planoAula.objetivosAprendizagemAula.forEach(obj => {
      if (obj.selected) {
        objetivosId.push(obj.id);
      }
    });
    const plano = {
      descricao: planoAula.descricao,
      desenvolvimentoAula: planoAula.desenvolvimentoAula,
      recuperacaoAula: planoAula.recuperacaoAula,
      licaoCasa: planoAula.licaoCasa,
      aulaId,
      objetivosAprendizagemJurema: temObjetivos ? objetivosId : [],
    };
    planoAula.objetivosAprendizagemJurema = [...objetivosId];

    await validaPlanoAula();
    if (errosValidacaoPlano.length === 0) {
      await api
        .post('v1/planos/aulas', plano)
        .then(salvouPlano => {
          if (salvouPlano && salvouPlano.status == 200) {
            sucesso('Plano de aula salvo com sucesso.');
            setModoEdicaoPlanoAula(false);
            obterPlanoAula(obterAulaSelecionada(dataSelecionada));
          }
        })
        .catch(e => {
          erros(e);
        });
    } else {
      setMostarErros(true);
    }
  };

  const validaPlanoAula = () => {
    if (
      !temObjetivos &&
      ehProfessorCj &&
      stringNulaOuEmBranco(planoAula.descricao)
    ) {
      errosValidacaoPlano.push(
        'Meus objetivos - O campo meus objetivos específicos é obrigatório'
      );
    }
    if (stringNulaOuEmBranco(planoAula.desenvolvimentoAula)) {
      errosValidacaoPlano.push(
        'Desenvolvimento da aula - A sessão de desenvolvimento da aula deve ser preenchida'
      );
    }
    if (
      !ehProfessorCj &&
      temObjetivos &&
      !ehEja &&
      !ehMedio &&
      !planoAula.migrado &&
      planoAula.objetivosAprendizagemJurema.length === 0
    ) {
      errosValidacaoPlano.push(
        'Objetivos de aprendizagem - É obrigatório selecionar ao menos um objetivo de aprendizagem'
      );
    }
  };

  const onCloseErros = () => {
    setErrosValidacaoPlano([]);
    setMostarErros(false);
  };

  const onClickFrequencia = () => {
    if (
      !desabilitarCampos &&
      !exibirCardFrequencia &&
      permiteRegistroFrequencia
    ) {
      setModoEdicaoFrequencia(true);
    }
    setExibirCardFrequencia(!exibirCardFrequencia);
  };

  const onChangeDisciplinas = async disciplinaId => {
    if (modoEdicaoFrequencia || modoEdicaoPlanoAula) {
      const confirmar = await pergutarParaSalvar();
      if (confirmar) {
        if (modoEdicaoFrequencia) {
          await onSalvarFrequencia();
          setarDisciplina(disciplinaId);
        }
        if (modoEdicaoPlanoAula) {
          await onSalvarPlanoAula();
          setarDisciplina(disciplinaId);
        }
      } else {
        setarDisciplina(disciplinaId);
        resetarPlanoAula();
      }
    } else {
      setarDisciplina(disciplinaId);
    }
  };

  const onChangeData = async data => {
    if (modoEdicaoFrequencia || modoEdicaoPlanoAula) {
      const confirmar = await pergutarParaSalvar();
      if (confirmar) {
        if (modoEdicaoFrequencia) {
          await onSalvarFrequencia();
        }
        if (modoEdicaoPlanoAula) {
          await onSalvarPlanoAula();
        }
        validaSeTemIdAula(data);
      } else {
        validaSeTemIdAula(data);
      }
    } else {
      validaSeTemIdAula(data);
    }
  };

  const [temAvaliacao, setTemAvaliacao] = useState(undefined);
  const [dataVigente, setDataVigente] = useState(false);

  const obterAvaliacao = async (idAula, data) => {
    const avaliacao = api.get(`v1/planos/aulas/${idAula}`);
    avaliacao.then(resposta => {
      if (resposta && resposta.data) {
        if (resposta.data.idAtividadeAvaliativa) {
          setDataVigente(
            window.moment(data).isSameOrAfter(window.moment(), 'day')
          );
          setTemAvaliacao(resposta.data.idAtividadeAvaliativa);
        } else {
          setTemAvaliacao(undefined);
        }
      }
    });
  };

  const validaSeTemIdAula = data => {
    setDataSelecionada(data);
    resetarTelaFrequencia(true, true);
    resetarPlanoAula();
    const aulaDataSelecionada = obterAulaSelecionada(data);
    setAula(aulaDataSelecionada);
    if (aulaDataSelecionada && aulaDataSelecionada.idAula) {
      obterListaFrequencia(aulaDataSelecionada.idAula);
      obterPlanoAula(aulaDataSelecionada);
      obterAvaliacao(aulaDataSelecionada.idAula, data);
    }
  };

  const onChangeFrequencia = () => {
    setModoEdicaoFrequencia(true);
  };

  const resetarPlanoAula = () => {
    setEhRegencia(false);
    planoAula.descricao = null;
    setTemObjetivos(false);
    planoAula.qtdAulas = 0;
    planoAula.desenvolvimentoAula = null;
    planoAula.licaoCasa = null;
    planoAula.recuperacaoAula = null;
    planoAula.objetivosAprendizagemAula = [];
    planoAula.objetivosAprendizagemAula = [
      ...planoAula.objetivosAprendizagemAula,
    ];
    const materiasVazia = [];
    setModoEdicaoPlanoAula(false);
    setMaterias([...materiasVazia]);
    setPlanoAula(planoAula);
  };

  const resetarTelaFrequencia = (naoDisciplina, naoData) => {
    if (!naoDisciplina) {
      setDisciplinaSelecionada(undefined);
      setDisciplinaIdSelecionada(undefined);
    }
    if (!naoData) {
      setDataSelecionada('');
    }
    setFrequencia([]);
    setExibirCardFrequencia(false);
    setModoEdicaoFrequencia(false);
    setExibirAuditoria(true);
  };

  const LinkAcao = styled.span`
    cursor: pointer;
    font-weight: bold;
  `;

  const acessarEditarAvaliacao = () => {
    history.push(`${RotasDto.CADASTRO_DE_AVALIACAO}/editar/${temAvaliacao}`);
  };

  const acessarNotasConceitos = () => {
    history.push(RotasDto.NOTAS);
  };

  return (
    <>
      {usuario && turmaSelecionada.turma ? (
        ''
      ) : (
        <Alert
          alerta={{
            tipo: 'warning',
            id: 'frequencia-selecione-turma',
            mensagem: 'Você precisa escolher uma turma.',
            estiloTitulo: { fontSize: '18px' },
          }}
          className="mb-2"
        />
      )}
      {temAvaliacao ? (
        <div className="row">
          <Grid cols={12} className="px-4">
            <div
              className="alert alert-info alert-dismissible fade show text-center"
              role="alert"
            >
              Atenção, existe uma avaliação neste dia:{' '}
              <LinkAcao onClick={acessarEditarAvaliacao}>
                Editar Avaliação
              </LinkAcao>{' '}
              {dataVigente && (
                <>
                  ou{' '}
                  <LinkAcao onClick={acessarNotasConceitos}>
                    Acessar Notas e Conceitos
                  </LinkAcao>
                </>
              )}
            </div>
          </Grid>
        </div>
      ) : null}
      <Cabecalho pagina="Frequência/Plano de aula" />
      <Card>
        <div className="col-md-12">
          <div className="row">
            <div className="col-md-12 d-flex justify-content-end pb-4">
              <Button
                label="Voltar"
                icon="arrow-left"
                color={Colors.Azul}
                border
                className="mr-2"
                onClick={onClickVoltar}
              />
              <Button
                label="Cancelar"
                color={Colors.Roxo}
                border
                className="mr-2"
                onClick={onClickCancelar}
                disabled={!modoEdicaoFrequencia && !modoEdicaoPlanoAula}
              />
              <Loader loading={carregandoSalvar} tip="">
                <Button
                  label="Salvar"
                  color={Colors.Roxo}
                  border
                  bold
                  className="mr-2"
                  onClick={() => onClickSalvar(true)}
                  disabled={
                    desabilitarCampos ||
                    (!modoEdicaoFrequencia && !modoEdicaoPlanoAula)
                  }
                />
              </Loader>
            </div>
          </div>
          <div className="row">
            <div className="col-sm-12 col-md-4 col-lg-4 col-xl-4 mb-2">
              <Loader loading={carregandoDisciplinas} tip="">
                <SelectComponent
                  id="disciplina"
                  name="disciplinaId"
                  lista={listaDisciplinas}
                  valueOption="codigoComponenteCurricular"
                  valueText="nome"
                  valueSelect={disciplinaIdSelecionada}
                  onChange={onChangeDisciplinas}
                  placeholder="Disciplina"
                  disabled={desabilitarDisciplina}
                />
              </Loader>
            </div>
            <div className="col-sm-12 col-md-4 col-lg-3 col-xl-3 mb-3">
              <CampoData
                valor={dataSelecionada}
                onChange={onChangeData}
                placeholder="DD/MM/AAAA"
                formatoData="DD/MM/YYYY"
                desabilitado={
                  !disciplinaIdSelecionada || carregandoDiasParaHabilitar
                }
                carregando={carregandoDiasParaHabilitar}
                diasParaHabilitar={diasParaHabilitar}
              />
            </div>
          </div>
          {dataSelecionada ? (
            <div className="row">
              <div className="col-sm-12 col-md-12 col-lg-12 col-xl-12 mb-2">
                <CardCollapse
                  key="frequencia-collapse"
                  onClick={onClickFrequencia}
                  titulo="Frequência"
                  indice="frequencia-collapse"
                  show={exibirCardFrequencia}
                  alt="card-collapse-frequencia"
                >
                  <Loader
                    loading={carregandoFrequencia}
                    className="w-100 text-center"
                  >
                    {frequencia && frequencia.length > 0 ? (
                      <>
                        <div className="col-sm-12 col-md-12 col-lg-12 col-xl-12 mb-2">
                          <Ordenacao
                            conteudoParaOrdenar={frequencia}
                            ordenarColunaNumero="numeroAlunoChamada"
                            ordenarColunaTexto="nomeAluno"
                            retornoOrdenado={retorno => setFrequencia(retorno)}
                          />
                          <ListaFrequencia
                            dados={frequencia}
                            frequenciaId={frequenciaId}
                            onChangeFrequencia={onChangeFrequencia}
                            permissoesTela={permissoesTela}
                          />
                        </div>
                        {exibirAuditoria ? (
                          <Auditoria
                            className="mt-2"
                            criadoEm={auditoria.criadoEm}
                            criadoPor={auditoria.criadoPor}
                            alteradoPor={auditoria.alteradoPor}
                            alteradoEm={auditoria.alteradoEm}
                          />
                        ) : (
                          ''
                        )}
                      </>
                    ) : (
                      ''
                    )}
                  </Loader>
                </CardCollapse>
              </div>
              <div className="col-sm-12 col-md-12 col-lg-12">
                <PlanoAula
                  disciplinaIdSelecionada={disciplinaIdSelecionada}
                  dataSelecionada={dataSelecionada}
                  planoAula={planoAula}
                  ehProfessorCj={ehProfessorCj}
                  carregandoMaterias={carregandoMaterias}
                  listaMaterias={materias}
                  dataAula={aula && aula.data ? aula.data : null}
                  ehEja={ehEja}
                  ehMedio={ehMedio}
                  setModoEdicao={e => setModoEdicaoPlanoAula(e)}
                  setTemObjetivos={e => setTemObjetivos(e)}
                  permissoesTela={permissoesTela}
                  somenteConsulta={somenteConsulta}
                  temObjetivos={temObjetivos}
                  temAvaliacao={temAvaliacao}
                  auditoria={auditoriaPlano}
                />
              </div>
            </div>
          ) : (
            ''
          )}
        </div>
        <ModalMultiLinhas
          key="errosBimestre"
          visivel={mostrarErros}
          onClose={onCloseErros}
          type="error"
          conteudo={errosValidacaoPlano}
          titulo="Erros plano de aula"
        />
      </Card>
    </>
  );
};

export default FrequenciaPlanoAula;
