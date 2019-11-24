import React, { useEffect, useState } from 'react';
import { useSelector } from 'react-redux';
import { CampoData } from '~/componentes';
import Cabecalho from '~/componentes-sgp/cabecalho';
import ListaFrequencia from '~/componentes-sgp/ListaFrequencia/listaFrequencia';
import Ordenacao from '~/componentes-sgp/Ordenacao/ordenacao';
import Button from '~/componentes/button';
import Card from '~/componentes/card';
import CardCollapse from '~/componentes/cardCollapse';
import { Colors } from '~/componentes/colors';
import SelectComponent from '~/componentes/select';
import { URL_HOME } from '~/constantes/url';
import { confirmar, erros, sucesso } from '~/servicos/alertas';
import api from '~/servicos/api';
import history from '~/servicos/history';
import Alert from '~/componentes/alert';

const FrequenciaPlanoAula = () => {
  const usuario = useSelector(store => store.usuario);
  const { turmaSelecionada } = usuario;
  const turmaId = turmaSelecionada ? turmaSelecionada.turma : 0;
  const anoLetivo = turmaSelecionada ? turmaSelecionada.anoLetivo : 0;

  const [listaDisciplinas, setListaDisciplinas] = useState([]);
  const [disciplinaSelecionada, setDisciplinaSelecionada] = useState(undefined);
  const [listaDatasAulas, setListaDatasAulas] = useState([]);
  const [dataSelecionada, setDataSelecionada] = useState('');

  const [frequencia, setFrequencia] = useState([]);
  const [aulaId, setAulaId] = useState(0);
  const [exibirCardFrequencia, setExibirCardFrequencia] = useState(false);
  const [modoEdicaoFrequencia, setModoEdicaoFrequencia] = useState(false);
  const [desabilitarDisciplina, setDesabilitarDisciplina] = useState(false);
  const [diasParaHabilitar, setDiasParaHabilitar] = useState([]);

  useEffect(() => {
    const obterDisciplinas = async () => {
      const disciplinas = await api.get(
        `v1/professores/${usuario.rf}/turmas/${turmaId}/disciplinas`
      );
      setListaDisciplinas(disciplinas.data);
      if (disciplinas.data && disciplinas.data.length == 1) {
        setDisciplinaSelecionada(String(disciplinas.data[0].codigoComponenteCurricular));
        setDesabilitarDisciplina(true);
        obterDatasDeAulasDisponiveis(disciplinas.data[0].codigoComponenteCurricular);
      }
    };

    if (turmaId) {
      obterDatasDeAulasDisponiveis();
      obterDisciplinas();
    } else {
      resetarTela();
      setAulaId(0);
      setListaDisciplinas([]);
      setListaDatasAulas([]);
      setDesabilitarDisciplina(false);
      setDiasParaHabilitar([]);
    }
  }, [turmaSelecionada.turma]);

  const obterListaFrequencia = async aulaId => {
    const frequenciaAlunos = await api
      .get(`v1/calendarios/frequencias`, { params: { aulaId } })
      .catch(e => erros(e));
    if (frequenciaAlunos && frequenciaAlunos.data) {
      setFrequencia(frequenciaAlunos.data.listaFrequencia);
      setAulaId(frequenciaAlunos.data.aulaId)
    }
  };

  const onClickVoltar = async () => {
    if (modoEdicaoFrequencia) {
      const confirmado = await pergutarParaSalvar();
      if (confirmado) {
        await onClickSalvar();
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
  }

  const irParaHome = () => {
    history.push(URL_HOME);
  }

  const onClickCancelar = async () => {
    if (modoEdicaoFrequencia) {
      const confirmou = await confirmar(
        'Atenção',
        'Você não salvou as informações preenchidas.',
        'Deseja realmente cancelar as alterações?'
      );
      if (confirmou) {
        obterListaFrequencia(aulaId);
        setModoEdicaoFrequencia(false);
      }
    }
  };

  const onClickSalvar = () => {
    return new Promise((resolve, reject) => {
      const valorParaSalvar = {
        aulaId,
        listaFrequencia: frequencia
      };
      return api
        .post(`v1/calendarios/frequencias`, valorParaSalvar).then(salvouFrequencia => {
          if (salvouFrequencia && salvouFrequencia.status == 200) {
            sucesso('Frequência realizada com sucesso.');
            resolve(true);
          } else {
            resolve(false);
          }
        })
        .catch(e => {
          erros(e)
          reject(e);
        });
    });
  };

  const onClickFrequencia = () => {
    if (!exibirCardFrequencia) {
      setModoEdicaoFrequencia(true);
    }
    setExibirCardFrequencia(!exibirCardFrequencia);
  };

  const obterDatasDeAulasDisponiveis = async disciplinaId => {
    const datasDeAulas = await api
      .get(`v1/calendarios/frequencias/aulas/datas/${anoLetivo}/turmas/${turmaId}/disciplinas/${disciplinaId}`)
      .catch(e => erros(e));

      if (datasDeAulas && datasDeAulas.data) {
        setListaDatasAulas(datasDeAulas.data);
        const habilitar = datasDeAulas.data.map(item => window.moment(item.data).format('YYYY-MM-DD'));
        setDiasParaHabilitar(habilitar);
      } else {
        setListaDatasAulas([]);
        setDiasParaHabilitar([]);
      }
  };

  const onChangeDisciplinas = async disciplinaId => {
    if (modoEdicaoFrequencia) {
      const confirmar = await pergutarParaSalvar();
      if (confirmar) {
        await onClickSalvar();
        setarDisciplina(disciplinaId);
      } else {
        setarDisciplina(disciplinaId);
      }
    } else {
      setarDisciplina(disciplinaId);
    }
  };

  const setarDisciplina =  disciplinaId => {
    resetarTela(true);
    setDisciplinaSelecionada(disciplinaId);
    if (disciplinaId) {
      obterDatasDeAulasDisponiveis(disciplinaId);
    } else {
      setListaDatasAulas([]);
      setDiasParaHabilitar([]);
    }
  }

  const onChangeData = async data => {
    if (modoEdicaoFrequencia) {
      const confirmar = await pergutarParaSalvar();
      if (confirmar) {
        await onClickSalvar();
        validaSeTemIdAula(data);
      } else {
        validaSeTemIdAula(data);
      }
    } else {
      validaSeTemIdAula(data);
    }
  };

  const validaSeTemIdAula = data => {
    setDataSelecionada(data);
    resetarTela(true, true);
    const aulaDataSelecionada = listaDatasAulas.find(item => window.moment(item.data).isSame(data, 'date'));
    if (aulaDataSelecionada && aulaDataSelecionada.idAula) {
      obterListaFrequencia(aulaDataSelecionada.idAula);
    }
  }

  const onChangeFrequencia = () => {
    setModoEdicaoFrequencia(true);
  }

  const resetarTela = (naoDisciplina, naoData) => {
    if (!naoDisciplina) {
      setDisciplinaSelecionada(undefined);
    }
    if (!naoData) {
      setDataSelecionada('')
    }
    setFrequencia([]);
    setExibirCardFrequencia(false);
    setModoEdicaoFrequencia(false)
  }

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
                disabled={!modoEdicaoFrequencia}
              />
              <Button
                label="Salvar"
                color={Colors.Roxo}
                border
                bold
                className="mr-2"
                onClick={onClickSalvar}
                disabled={!modoEdicaoFrequencia}
              />
            </div>
          </div>
          <div className="row">
            <div className="col-sm-12 col-md-4 col-lg-4 col-xl-4 mb-2">
              <SelectComponent
                id="disciplina"
                name="disciplinaId"
                lista={listaDisciplinas}
                valueOption="codigoComponenteCurricular"
                valueText="nome"
                valueSelect={disciplinaSelecionada}
                onChange={onChangeDisciplinas}
                placeholder="Disciplina"
                disabled={desabilitarDisciplina}
              />
            </div>
            <div className="col-sm-12 col-md-4 col-lg-3 col-xl-2 mb-2">
              <CampoData
                valor={dataSelecionada}
                onChange={onChangeData}
                placeholder="DD/MM/AAAA"
                formatoData="DD/MM/YYYY"
                desabilitado={!disciplinaSelecionada}
                diasParaHabilitar={diasParaHabilitar}
              />
            </div>
          </div>
          {
            frequencia && frequencia.length ?
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
                    <Ordenacao
                      conteudoParaOrdenar={frequencia}
                      ordenarColunaNumero="numeroAlunoChamada"
                      ordenarColunaTexto="nomeAluno"
                      retornoOrdenado={retorno => setFrequencia(retorno)}
                    ></Ordenacao>
                    <ListaFrequencia dados={frequencia} onChangeFrequencia={onChangeFrequencia}></ListaFrequencia>
                  </CardCollapse>
                </div>
              </div>
              : ''
          }
        </div>
      </Card>
    </>
  );
};

export default FrequenciaPlanoAula;
