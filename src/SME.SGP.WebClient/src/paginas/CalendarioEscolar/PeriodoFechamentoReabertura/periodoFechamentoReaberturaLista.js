import { Form, Formik } from 'formik';
import * as moment from 'moment';
import React, { useCallback, useEffect, useState } from 'react';
import { useSelector } from 'react-redux';
import { ListaPaginada, Loader } from '~/componentes';
import { Cabecalho, DreDropDown, UeDropDown } from '~/componentes-sgp';
import Button from '~/componentes/button';
import Card from '~/componentes/card';
import { Colors } from '~/componentes/colors';
import SelectComponent from '~/componentes/select';
import SelectAutocomplete from '~/componentes/select-autocomplete';
import { URL_HOME } from '~/constantes/url';
import modalidadeTipoCalendario from '~/dtos/modalidadeTipoCalendario';
import RotasDto from '~/dtos/rotasDto';
import { confirmar, erros, sucesso } from '~/servicos/alertas';
import history from '~/servicos/history';
import ServicoCalendarios from '~/servicos/Paginas/Calendario/ServicoCalendarios';
import ServicoFechamentoReabertura from '~/servicos/Paginas/Calendario/ServicoFechamentoReabertura';
import { verificaSomenteConsulta } from '~/servicos/servico-navegacao';

import { CampoBimestre } from './periodoFechamentoReaberuraLista.css';
import modalidade from '~/dtos/modalidade';

const PeriodoFechamentoReaberturaLista = () => {
  const usuario = useSelector(store => store.usuario);

  const permissoesTela =
    usuario.permissoes[RotasDto.PERIODO_FECHAMENTO_REABERTURA];
  let { anoLetivo } = usuario.turmaSelecionada;

  if (!anoLetivo) {
    anoLetivo = new Date().getFullYear();
  }

  const [somenteConsulta, setSomenteConsulta] = useState(false);

  const [listaTipoCalendarioEscolar, setListaTipoCalendarioEscolar] = useState(
    []
  );
  const [tipoCalendarioSelecionado, setTipoCalendarioSelecionado] = useState(
    undefined
  );
  const [desabilitarTipoCalendario, setDesabilitarTipoCalendario] = useState(
    false
  );
  const [carregandoTipos, setCarregandoTipos] = useState(false);
  const [idsReaberturasSelecionadas, setIdsReaberturasSelecionadas] = useState(
    []
  );

  const [ueSelecionada, setUeSelecionada] = useState(undefined);
  const [dreSelecionada, setDreSelecionada] = useState('');
  const [filtroValido, setFiltroValido] = useState(false);
  const [filtro, setFiltro] = useState({});
  const [modalidadeTurma, setModalidadeTurma] = useState('');
  const [listaTipoCalendario, setListaTipoCalendario] = useState([]);
  const [filtroTipoCalendario, setFiltroTipoCalendario] = useState('');

  const criarCampoBimestre = (index, data) => {
    const bimestre = data[index];
    return bimestre ? (
      <CampoBimestre>
        <i className="fas fa-check" />
      </CampoBimestre>
    ) : (
      <></>
    );
  };

  const getColunasBimestreAnual = () => {
    return [
      {
        title: '1',
        dataIndex: 'bimestres',
        key: '1',
        render: data => criarCampoBimestre(0, data),
      },
      {
        title: '2',
        dataIndex: 'bimestres',
        key: '2',
        render: data => criarCampoBimestre(1, data),
      },
      {
        title: '3',
        dataIndex: 'bimestres',
        key: '3',
        render: data => criarCampoBimestre(2, data),
      },
      {
        title: '4',
        dataIndex: 'bimestres',
        key: '4',
        render: data => criarCampoBimestre(3, data),
      },
    ];
  };

  const getColunasBimestreSemestral = () => {
    return [
      {
        title: '1',
        dataIndex: 'bimestres',
        key: '1',
        render: data => criarCampoBimestre(0, data),
      },
      {
        title: '2',
        dataIndex: 'bimestres',
        key: '2',
        render: data => criarCampoBimestre(1, data),
      },
    ];
  };

  const [colunasBimestre, setColunasBimestre] = useState(
    getColunasBimestreAnual()
  );

  useEffect(() => {
    setSomenteConsulta(verificaSomenteConsulta(permissoesTela));
  }, [permissoesTela]);

  const onFiltrar = useCallback(() => {
    if (tipoCalendarioSelecionado) {
      const novoFiltro = {
        tipoCalendarioId: tipoCalendarioSelecionado,
      };

      if (dreSelecionada) {
        novoFiltro.dreCodigo = dreSelecionada;
        if (ueSelecionada) {
          novoFiltro.ueCodigo = ueSelecionada;
        }
      } else {
        novoFiltro.ueCodigo = '';
        setUeSelecionada('');
      }
      setFiltroValido(true);
      setFiltro({ ...novoFiltro });
    } else {
      setFiltroValido(false);
    }
  }, [dreSelecionada, tipoCalendarioSelecionado, ueSelecionada]);

  useEffect(() => {
    onFiltrar();
  }, [tipoCalendarioSelecionado, ueSelecionada, dreSelecionada, onFiltrar]);

  const obterListaTiposCalAnoLetivo = useCallback(
    lista => {
      if (usuario.turmaSelecionada && usuario.turmaSelecionada.modalidade) {
        const ehEja = usuario.turmaSelecionada.modalidade == modalidade.EJA;
        const listaPorAnoLetivoModalidade = lista.filter(item => {
          if (ehEja) {
            return item.modalidade == modalidadeTipoCalendario.EJA;
          }
          return item.modalidade == modalidadeTipoCalendario.FUNDAMENTAL_MEDIO;
        });
        return listaPorAnoLetivoModalidade;
      }

      return lista.filter(item => item.anoLetivo == anoLetivo);
    },
    [usuario.turmaSelecionada]
  );

  // useEffect(() => {
  //   async function consultaTipos() {
  //     setCarregandoTipos(true);

  //     const listaTipo = await ServicoCalendarios.obterTiposCalendario(
  //       anoLetivo
  //     );
  //     if (listaTipo && listaTipo.data && listaTipo.data.length) {
  //       listaTipo.data.map(item => {
  //         item.id = String(item.id);
  //         item.descricaoTipoCalendario = `${item.anoLetivo} - ${item.nome} - ${item.descricaoPeriodo}`;
  //       });
  //       setListaTipoCalendarioEscolar(listaTipo.data);
  //       if (listaTipo.data.length === 1) {
  //         setTipoCalendarioSelecionado(String(listaTipo.data[0].id));
  //         setDesabilitarTipoCalendario(true);
  //       } else {
  //         setDesabilitarTipoCalendario(false);
  //         setTipoCalendarioSelecionado(undefined);
  //       }
  //     } else {
  //       setListaTipoCalendarioEscolar([]);
  //     }
  //     setCarregandoTipos(false);
  //   }
  //   consultaTipos();
  // }, [usuario.turmaSelecionada.anoLetivo, obterListaTiposCalAnoLetivo]);

  useEffect(() => {
    async function consultaTipoCalendario() {
      const {
        data,
      } = await ServicoCalendarios.obterTiposCalendarioAutoComplete(
        tipoCalendarioSelecionado
      );
      setListaTipoCalendario(data);
    }
    consultaTipoCalendario();

    return () => {
      setListaTipoCalendario([]);
    };
  }, [tipoCalendarioSelecionado]);

  const onClickVoltar = () => {
    history.push(URL_HOME);
  };

  const onClickExcluir = async () => {
    const confirmado = await confirmar(
      'Excluir Fechamento(s)',
      '',
      'Você tem certeza que deseja excluir este(s) registros(s)?',
      'Excluir',
      'Cancelar'
    );
    if (confirmado) {
      const idsDeletar = idsReaberturasSelecionadas.map(tipo => tipo.id);
      const excluir = await ServicoFechamentoReabertura.deletar(
        idsDeletar
      ).catch(e => erros(e));

      if (excluir && excluir.status == 200) {
        setIdsReaberturasSelecionadas([]);
        sucesso(excluir.data);
        onFiltrar();
      }
    }
  };

  const onClickNovo = () => {
    history.push(`/calendario-escolar/periodo-fechamento-reabertura/novo`);
  };
  const onClickEditar = item => {
    history.push(
      `/calendario-escolar/periodo-fechamento-reabertura/editar/${item.id}`
    );
  };

  const onSelecionarItems = ids => {
    setIdsReaberturasSelecionadas(ids);
  };

  const formatarCampoDataGrid = data => {
    let dataFormatada = '';
    if (data) {
      dataFormatada = moment(data).format('DD/MM/YYYY');
    }
    return <span> {dataFormatada}</span>;
  };

  const colunas = [
    {
      title: 'Descrição',
      dataIndex: 'descricao',
      width: '65%',
    },
    {
      title: 'Início',
      dataIndex: 'dataInicio',
      width: '15%',
      render: data => formatarCampoDataGrid(data),
    },
    {
      title: 'Fim',
      dataIndex: 'dataFim',
      width: '15%',
      render: data => formatarCampoDataGrid(data),
    },
    {
      title: 'Bimestres',
      children: colunasBimestre,
    },
  ];

  const onChangeTipoCalendario = id => {
    if (id) {
      const tipo = listaTipoCalendarioEscolar.find(t => t.id === id);
      if (tipo.modalidade === modalidadeTipoCalendario.FUNDAMENTAL_MEDIO) {
        setColunasBimestre(getColunasBimestreAnual);
      } else {
        setColunasBimestre(getColunasBimestreSemestral);
      }
      setTipoCalendarioSelecionado(id);
    } else {
      setTipoCalendarioSelecionado(undefined);
    }
  };

  const onChangeDre = dreId => {
    setUeSelecionada('');
    setDreSelecionada(dreId);
  };

  const selecionaTipoCalendario = descricao => {
    const tipo = listaTipoCalendario?.find(t => t.descricao === descricao);
    if (Number(tipo?.id) || !tipo?.id) {
      const value =
        tipo?.modalidade === modalidadeTipoCalendario.FUNDAMENTAL_MEDIO
          ? getColunasBimestreAnual
          : getColunasBimestreSemestral;
      setColunasBimestre(value);
      setFiltroTipoCalendario(descricao);
      setTipoCalendarioSelecionado(tipo?.id);
      return;
    }
    setTipoCalendarioSelecionado(undefined);
  };

  const filtraTipoCalendario = (item, texto) => {
    return (
      item.descricao.toLowerCase().indexOf(texto) > -1 || item.id === texto
    );
  };

  return (
    <>
      <Cabecalho pagina="Período de Fechamento (Reabertura)" />
      <Card>
        <Formik
          enableReinitialize
          initialValues={{
            ueId: undefined,
            dreId: undefined,
            tipoCalendarioId: undefined,
          }}
          validateOnChange
          validateOnBlur
        >
          {form => (
            <Form className="col-md-12">
              <div className="row mb-4">
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
                    label="Excluir"
                    color={Colors.Vermelho}
                    border
                    className="mr-2"
                    onClick={onClickExcluir}
                    disabled={
                      !permissoesTela.podeExcluir ||
                      (idsReaberturasSelecionadas &&
                        idsReaberturasSelecionadas.length < 1)
                    }
                  />
                  <Button
                    label="Novo"
                    color={Colors.Roxo}
                    border
                    bold
                    className="mr-2"
                    onClick={onClickNovo}
                    disabled={somenteConsulta || !permissoesTela.podeIncluir}
                  />
                </div>
                <div className="col-md-12 mb-2">
                  <Loader loading={carregandoTipos} tip="">
                    <div style={{ maxWidth: '300px' }}>
                      {/* <SelectComponent
                        name="tipoCalendarioId"
                        id="tipoCalendarioId"
                        lista={listaTipoCalendarioEscolar}
                        valueOption="id"
                        valueText="descricaoTipoCalendario"
                        onChange={id => onChangeTipoCalendario(id)}
                        valueSelect={tipoCalendarioSelecionado}
                        disabled={desabilitarTipoCalendario}
                        placeholder="Selecione um tipo de calendário"
                      /> */}
                      <SelectAutocomplete
                        hideLabel
                        showList
                        placeholder="Selecione um tipo de calendário"
                        className="col-md-12"
                        name="tipoCalendarioId"
                        id="tipoCalendarioId"
                        lista={listaTipoCalendario}
                        // disabled={!permissoesTela.podeConsultar}
                        valueField="id"
                        textField="descricao"
                        onSelect={selecionaTipoCalendario}
                        onChange={selecionaTipoCalendario}
                        value={filtroTipoCalendario}
                        filtro={filtraTipoCalendario}
                      />
                    </div>
                  </Loader>
                </div>
                <div className="col-md-6 mb-2">
                  {tipoCalendarioSelecionado && (
                    <DreDropDown
                      label="Diretoria Regional de Educação (DRE)"
                      form={form}
                      onChange={dreId => {
                        setDreSelecionada(dreId);
                        const tipoSelecionado = listaTipoCalendarioEscolar.find(
                          item => item.id == tipoCalendarioSelecionado
                        );
                        if (tipoSelecionado && tipoSelecionado.modalidade) {
                          const modalidadeT = ServicoCalendarios.converterModalidade(
                            tipoSelecionado.modalidade
                          );
                          setModalidadeTurma(modalidadeT);
                        } else {
                          setModalidadeTurma('');
                        }
                      }}
                      desabilitado={false}
                    />
                  )}
                </div>
                <div className="col-md-6 pb-2">
                  {tipoCalendarioSelecionado && (
                    <UeDropDown
                      dreId={form.values.dreId}
                      label="Unidade Escolar (UE)"
                      form={form}
                      url=""
                      onChange={ueId => setUeSelecionada(ueId)}
                      desabilitado={false}
                      modalidade={modalidadeTurma}
                    />
                  )}
                </div>
                <div className="col-md-12 pt-2">
                  {tipoCalendarioSelecionado ? (
                    <ListaPaginada
                      url="v1/fechamentos/reaberturas"
                      id="lista-fechamento-reaberturas"
                      colunaChave="id"
                      colunas={colunas}
                      filtro={filtro}
                      onClick={onClickEditar}
                      multiSelecao
                      selecionarItems={onSelecionarItems}
                      filtroEhValido={filtroValido}
                    />
                  ) : (
                    ''
                  )}
                </div>
              </div>
            </Form>
          )}
        </Formik>
      </Card>
    </>
  );
};

export default PeriodoFechamentoReaberturaLista;
