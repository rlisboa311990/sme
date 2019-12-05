import React, { useState, useEffect, useRef, useCallback } from 'react';
import { useSelector, useDispatch } from 'react-redux';
import shortid from 'shortid';
import { store } from '~/redux';
import {
  selecionarTurma,
  turmasUsuario,
  removerTurma,
} from '~/redux/modulos/usuario/actions';
import Grid from '~/componentes/grid';
import Button from '~/componentes/button';
import { Colors } from '~/componentes/colors';
import SelectComponent from '~/componentes/select';
import api from '~/servicos/api';
import {
  Container,
  Campo,
  Busca,
  Fechar,
  SetaFunction,
  ItemLista,
} from './index.css';
import {
  salvarAnosLetivos,
  salvarModalidades,
  salvarPeriodos,
  salvarDres,
  salvarUnidadesEscolares,
  salvarTurmas,
} from '~/redux/modulos/filtro/actions';
import FiltroHelper from './helper';
import { erro } from '~/servicos/alertas';
import modalidade from '~/dtos/modalidade';
import ServicoFiltro from '~/servicos/Componentes/ServicoFiltro';

const Filtro = () => {
  const dispatch = useDispatch();
  const [alternarFocoCampo, setAlternarFocoCampo] = useState(false);
  const [alternarFocoBusca, setAlternarFocoBusca] = useState(false);

  const Seta = SetaFunction(alternarFocoBusca);

  const divBuscaRef = useRef();
  const campoBuscaRef = useRef();

  const usuarioStore = useSelector(state => state.usuario);
  const turmaUsuarioSelecionada = usuarioStore.turmaSelecionada;
  const [campoAnoLetivoDesabilitado, setCampoAnoLetivoDesabilitado] = useState(
    true
  );
  const [
    campoModalidadeDesabilitado,
    setCampoModalidadeDesabilitado,
  ] = useState(true);
  const [campoPeriodoDesabilitado, setCampoPeriodoDesabilitado] = useState(
    true
  );
  const [campoDreDesabilitado, setCampoDreDesabilitado] = useState(true);
  const [
    campoUnidadeEscolarDesabilitado,
    setCampoUnidadeEscolarDesabilitado,
  ] = useState(true);
  const [campoTurmaDesabilitado, setCampoTurmaDesabilitado] = useState(true);

  const anosLetivoStore = useSelector(state => state.filtro.anosLetivos);
  const [anosLetivos, setAnosLetivos] = useState([...anosLetivoStore]);
  const [anoLetivoSelecionado, setAnoLetivoSelecionado] = useState(
    turmaUsuarioSelecionada ? turmaUsuarioSelecionada.anoLetivo : ''
  );

  const modalidadesStore = useSelector(state => state.filtro.modalidades);
  const [modalidades, setModalidades] = useState(modalidadesStore);
  const [modalidadeSelecionada, setModalidadeSelecionada] = useState(
    turmaUsuarioSelecionada ? turmaUsuarioSelecionada.modalidade : ''
  );

  const periodosStore = useSelector(state => state.filtro.periodos);
  const [periodos, setPeriodos] = useState(periodosStore);
  const [periodoSelecionado, setPeriodoSelecionado] = useState(
    turmaUsuarioSelecionada ? turmaUsuarioSelecionada.periodo : ''
  );

  const dresStore = useSelector(state => state.filtro.dres);
  const [dres, setDres] = useState(dresStore);
  const [dreSelecionada, setDreSelecionada] = useState(
    turmaUsuarioSelecionada ? turmaUsuarioSelecionada.dre : ''
  );

  const unidadesEscolaresStore = useSelector(
    state => state.filtro.unidadesEscolares
  );
  const [unidadesEscolares, setUnidadesEscolares] = useState(
    unidadesEscolaresStore
  );
  const [unidadeEscolarSelecionada, setUnidadeEscolarSelecionada] = useState(
    turmaUsuarioSelecionada ? turmaUsuarioSelecionada.unidadeEscolar : ''
  );

  const turmasStore = useSelector(state => state.filtro.turmas);
  const [turmas, setTurmas] = useState(turmasStore);
  const [turmaSelecionada, setTurmaSelecionada] = useState(
    turmaUsuarioSelecionada ? turmaUsuarioSelecionada.turma : ''
  );

  const [textoAutocomplete, setTextoAutocomplete] = useState(
    turmaUsuarioSelecionada ? turmaUsuarioSelecionada.desc : ''
  );
  const [resultadosFiltro, setResultadosFiltro] = useState([]);

  const ObtenhaDres = useCallback(
    async (estado, periodo) => {
      if (!modalidadeSelecionada) return;

      const listaDres = await FiltroHelper.ObtenhaDres(
        modalidadeSelecionada,
        periodo
      );

      if (estado) {
        dispatch(salvarDres(listaDres));
        setDres(listaDres);
        setCampoDreDesabilitado(listaDres.length === 1);
      }
    },
    [dispatch, modalidadeSelecionada]
  );

  const aplicarFiltro = useCallback(() => {
    if (
      anoLetivoSelecionado &&
      modalidadeSelecionada &&
      dreSelecionada &&
      unidadeEscolarSelecionada &&
      turmaSelecionada
    ) {
      const modalidadeDesc = modalidades.find(
        modalidade => modalidade.valor.toString() === `${modalidadeSelecionada}`
      );

      const turmaDesc = turmas.find(turma => turma.valor === turmaSelecionada);

      const unidadeEscolarDesc = unidadesEscolares.find(
        unidade => unidade.valor === unidadeEscolarSelecionada
      );

      setTextoAutocomplete(
        `${modalidadeDesc.desc} - ${turmaDesc.desc} - ${unidadeEscolarDesc.desc}`
      );

      setAlternarFocoBusca(false);

      const turmaSelecionadaCompleta = turmas.find(
        t => t.valor == turmaSelecionada
      );

      const turma = {
        anoLetivo: anoLetivoSelecionado,
        modalidade: modalidadeSelecionada,
        dre: dreSelecionada,
        unidadeEscolar: unidadeEscolarSelecionada,
        turma: turmaSelecionada,
        ano: turmaSelecionadaCompleta.ano,
        desc: `${modalidadeDesc.desc} - ${turmaDesc.desc} - ${unidadeEscolarDesc.desc}`,
        periodo: periodoSelecionado || 0,
      };

      dispatch(turmasUsuario(turmas));
      dispatch(selecionarTurma(turma));

      setTextoAutocomplete(turma.desc);
    }
  }, [
    anoLetivoSelecionado,
    dispatch,
    dreSelecionada,
    modalidadeSelecionada,
    modalidades,
    periodoSelecionado,
    turmaSelecionada,
    turmas,
    unidadeEscolarSelecionada,
    unidadesEscolares,
  ]);

  const reabilitarCampos = () => {
    setCampoDreDesabilitado(false);
    setCampoAnoLetivoDesabilitado(false);
    setCampoModalidadeDesabilitado(false);
    setCampoPeriodoDesabilitado(false);
    setCampoTurmaDesabilitado(false);
    setCampoUnidadeEscolarDesabilitado(false);
  };

  useEffect(() => {
    let estado = true;
    const ObterAnosLetivos = async deveSalvarAnosLetivos => {
      const anosLetivo = await ServicoFiltro.listarAnosLetivos()
        .then(resposta => {
          const anos = [];
          if (resposta.data) {
            resposta.data.forEach(ano => {
              anos.push({ desc: ano, valor: ano });
            });
          }

          return anos;
        })
        .catch(() => anosLetivos);

      if (deveSalvarAnosLetivos) {
        dispatch(salvarAnosLetivos(anosLetivo));
        setAnosLetivos(anosLetivo);
        setCampoAnoLetivoDesabilitado(anosLetivo.length === 1);
      }
    };
    ObterAnosLetivos(estado);
    return () => {
      estado = false;
      return estado;
    };
  }, [dispatch]);

  useEffect(() => {
    let estado = true;

    setAnoLetivoSelecionado(turmaUsuarioSelecionada.anoLetivo);
    setModalidadeSelecionada(turmaUsuarioSelecionada.modalidade);
    setPeriodoSelecionado(turmaUsuarioSelecionada.periodo);
    setDreSelecionada(turmaUsuarioSelecionada.dre);
    setUnidadeEscolarSelecionada(turmaUsuarioSelecionada.unidadeEscolar);
    setTurmaSelecionada(turmaUsuarioSelecionada.turma);
    setTextoAutocomplete(turmaUsuarioSelecionada.desc);

    return () => {
      estado = false;
      return estado;
    };
  }, [turmaUsuarioSelecionada]);

  useEffect(() => {
    if (anosLetivos && anosLetivos.length === 1) {
      setAnoLetivoSelecionado(anosLetivos[0].valor);
    }
  }, [anosLetivos]);

  useEffect(() => {
    let estado = true;

    if (!anoLetivoSelecionado) {
      setModalidadeSelecionada();
      setCampoModalidadeDesabilitado(true);
      return;
    }
    const ObtenhaModalidades = async deveSalvarModalidade => {
      const modalidadesLista = await FiltroHelper.ObtenhaModalidades();

      if (deveSalvarModalidade) {
        setModalidades([...modalidadesLista]);
        dispatch(salvarModalidades(modalidadesLista));
        setCampoModalidadeDesabilitado(modalidadesLista.length === 1);
      }
    };
    ObtenhaModalidades(estado);

    return () => (estado = false);
  }, [anoLetivoSelecionado, dispatch]);

  useEffect(() => {
    if (modalidades && modalidades.length === 1)
      setModalidadeSelecionada(modalidades[0].valor);
  }, [modalidades]);

  // lista dres
  useEffect(() => {
    let estado = true;
    const ObtenhaPeriodos = async deveSalvarPeriodos => {
      const periodo = await FiltroHelper.ObtenhaPeriodos(modalidadeSelecionada);

      if (!modalidade) return;

      if (deveSalvarPeriodos) {
        dispatch(salvarPeriodos(periodo));
        setPeriodos(periodo);
        setCampoPeriodoDesabilitado(periodo.length === 1);
      }
    };
    if (!modalidadeSelecionada) {
      setPeriodoSelecionado();
      setDreSelecionada();
      setCampoPeriodoDesabilitado(true);
      setCampoDreDesabilitado(true);
      return;
    }

    if (modalidadeSelecionada === modalidade.EJA.toString()) {
      ObtenhaPeriodos(estado);
      setCampoDreDesabilitado(true);
    } else {
      ObtenhaDres(estado);
    }

    return () => (estado = false);
  }, [ObtenhaDres, dispatch, modalidadeSelecionada]);

  useEffect(() => {
    if (periodos && periodos.length === 1)
      setPeriodoSelecionado(periodos[0].valor);
  }, [periodos]);

  // lista dres 2
  useEffect(() => {
    const estado = true;

    if (modalidadeSelecionada !== modalidade.EJA.toString()) return;

    if (!periodoSelecionado) {
      setDreSelecionada();
      setCampoDreDesabilitado(true);
      return;
    }

    ObtenhaDres(estado, periodoSelecionado);
  }, [ObtenhaDres, modalidadeSelecionada, periodoSelecionado]);

  useEffect(() => {
    if (dres && dres.length === 1) setDreSelecionada(dres[0].valor);
  }, [dres]);

  useEffect(() => {
    let estado = true;
    const ObtenhaUnidadesEscolares = async (deveSalvarUes, periodo) => {
      if (!modalidadeSelecionada) return;

      const ues = await FiltroHelper.ObtenhaUnidadesEscolares(
        modalidadeSelecionada,
        dreSelecionada,
        periodo
      );

      if (!ues) {
        setDreSelecionada();
        setCampoDreDesabilitado(true);
        erro('Esta DRE não possui unidades escolares da modalidade escolhida');
        return;
      }

      if (deveSalvarUes) {
        dispatch(salvarUnidadesEscolares(ues));
        setUnidadesEscolares(ues);
        setCampoUnidadeEscolarDesabilitado(ues.length === 1);
      }
    };
    if (!dreSelecionada) {
      setUnidadeEscolarSelecionada();
      setCampoUnidadeEscolarDesabilitado(true);
      return;
    }

    const periodo =
      modalidadeSelecionada === modalidade.EJA.toString()
        ? periodoSelecionado
        : null;

    ObtenhaUnidadesEscolares(estado, periodo);

    return () => (estado = false);
  }, [dispatch, dreSelecionada, modalidadeSelecionada, periodoSelecionado]);

  useEffect(() => {
    if (unidadesEscolares && unidadesEscolares.length === 1)
      setUnidadeEscolarSelecionada(unidadesEscolares[0].valor);
  }, [unidadesEscolares]);

  // Hook listagem de turmas
  useEffect(() => {
    const ObtenhaTurmas = async deveSalvarTurmas => {
      const periodo =
        modalidadeSelecionada === modalidade.EJA.toString()
          ? periodoSelecionado
          : null;

      if (!modalidadeSelecionada) return;

      const listaTurmas = await FiltroHelper.ObtenhaTurmas(
        modalidadeSelecionada,
        unidadeEscolarSelecionada,
        periodo
      );

      if (!listaTurmas || listaTurmas.length === 0) {
        setUnidadeEscolarSelecionada();
        setCampoUnidadeEscolarDesabilitado(true);
        erro('Esta unidade escolar não possui turmas da modalidade escolhida');
        return;
      }

      if (deveSalvarTurmas) {
        dispatch(salvarTurmas(listaTurmas));
        setTurmas(listaTurmas);
        setCampoTurmaDesabilitado(listaTurmas.length === 1);
      }
    };
    let estado = true;

    if (!unidadeEscolarSelecionada) {
      setTurmaSelecionada();
      setCampoTurmaDesabilitado(true);
      return;
    }

    ObtenhaTurmas(estado);

    return () => {
      estado = false;
      return estado;
    };
  }, [
    dispatch,
    modalidadeSelecionada,
    periodoSelecionado,
    unidadeEscolarSelecionada,
  ]);

  useEffect(() => {
    if (turmas && turmas.length === 1) {
      setTurmaSelecionada(turmas[0].valor);
      aplicarFiltro();
    }
  }, [aplicarFiltro, turmas]);

  const mostrarEsconderBusca = () => {
    setAlternarFocoBusca(!alternarFocoBusca);
    setAlternarFocoCampo(false);
  };

  useEffect(() => {
    const controlaClickFora = evento => {
      if (
        !evento.target.nodeName === 'svg' &&
        !evento.target.nodeName === 'path' &&
        !evento.target.classList.contains('fa-caret-down') &&
        !evento.target.classList.contains('ant-select-dropdown-menu-item') &&
        !evento.target.classList.contains(
          'ant-select-dropdown-menu-item-active'
        ) &&
        !evento.target.classList.contains(
          'ant-select-selection__placeholder'
        ) &&
        !evento.target.classList.contains(
          'ant-select-selection-selected-value'
        ) &&
        !evento.target.classList.contains(
          'ant-select-dropdown-menu-item-selected'
        ) &&
        divBuscaRef.current &&
        !divBuscaRef.current.contains(evento.target)
      )
        // mostrarEsconderBusca
        setAlternarFocoBusca(!alternarFocoBusca);
      setAlternarFocoCampo(false);
    };

    if (!turmaUsuarioSelecionada && !alternarFocoBusca && alternarFocoCampo)
      campoBuscaRef.current.focus();
    if (alternarFocoBusca)
      document.addEventListener('click', controlaClickFora);
    return () => document.removeEventListener('click', controlaClickFora);
  }, [alternarFocoBusca, alternarFocoCampo, turmaUsuarioSelecionada]);

  useEffect(() => {
    if (!turmaUsuarioSelecionada) campoBuscaRef.current.focus();
    if (!textoAutocomplete) setResultadosFiltro([]);
  }, [textoAutocomplete, turmaUsuarioSelecionada]);

  useEffect(() => {
    if (!turmaUsuarioSelecionada) campoBuscaRef.current.focus();
  }, [resultadosFiltro, turmaUsuarioSelecionada]);

  const onChangeAutocomplete = () => {
    const texto = campoBuscaRef.current.value;
    setTextoAutocomplete(texto);

    if (texto.length >= 2) {
      api.get(`v1/abrangencias/${texto}`).then(resposta => {
        if (resposta.data) {
          setResultadosFiltro(resposta.data);
        }
      });
    }
  };

  const selecionaTurmaAutocomplete = resultado => {
    setTextoAutocomplete(resultado.descricaoFiltro);

    const turma = {
      anoLetivo: resultado.anoLetivo,
      modalidade: resultado.codigoModalidade,
      dre: resultado.codigoDre,
      unidadeEscolar: resultado.codigoUe,
      turma: resultado.codigoTurma,
      desc: resultado.descricaoFiltro,
      periodo: resultado.semestre,
    };

    dispatch(selecionarTurma(turma));
    dispatch(turmasUsuario(turmas));

    setResultadosFiltro([]);
  };

  let selecionado = -1;

  const aoPressionarTeclaBaixoAutocomplete = evento => {
    if (resultadosFiltro && resultadosFiltro.length > 0) {
      const resultados = document.querySelectorAll('.list-group-item');
      if (resultados && resultados.length > 0) {
        if (evento.key === 'ArrowUp') {
          if (selecionado > 0) selecionado -= 1;
        } else if (evento.key === 'ArrowDown') {
          if (selecionado < resultados.length - 1) selecionado += 1;
        }
        resultados.forEach(resultado =>
          resultado.classList.remove('selecionado')
        );
        if (resultados[selecionado]) {
          resultados[selecionado].classList.add('selecionado');
          campoBuscaRef.current.focus();
        }
      }
    }
  };

  const aoSubmeterAutocomplete = evento => {
    evento.preventDefault();
    if (resultadosFiltro) {
      if (resultadosFiltro.length === 1) {
        setModalidadeSelecionada(
          resultadosFiltro[0].codigoModalidade.toString()
        );
        setDreSelecionada(resultadosFiltro[0].codigoDre);
        setUnidadeEscolarSelecionada(resultadosFiltro[0].codigoUe);
        setTimeout(() => {
          setTurmaSelecionada(resultadosFiltro[0].codigoTurma);
        }, 1000);
        selecionaTurmaAutocomplete(resultadosFiltro[0]);
      } else {
        const itemSelecionado = document.querySelector(
          '.list-group-item.selecionado'
        );
        if (itemSelecionado) {
          const indice = itemSelecionado.getAttribute('tabindex');
          if (indice) {
            const resultado = resultadosFiltro[indice];
            if (resultado) {
              setModalidadeSelecionada(resultado.codigoModalidade.toString());
              setDreSelecionada(resultado.codigoDre);
              setUnidadeEscolarSelecionada(resultado.codigoUe);
              setTimeout(() => {
                setTurmaSelecionada(resultado.codigoTurma);
              }, 1000);
              selecionaTurmaAutocomplete(resultado);
            }
          }
        }
      }
    }
  };

  const aoFocarBusca = () => {
    if (alternarFocoBusca) {
      setAlternarFocoBusca(false);
      setAlternarFocoCampo(true);
    }
  };

  const aoTrocarAnoLetivo = ano => {
    if (ano !== anoLetivoSelecionado) setModalidadeSelecionada();

    setAnoLetivoSelecionado(ano);
  };

  const aoTrocarModalidade = modalidade => {
    if (modalidade !== modalidadeSelecionada) {
      setDreSelecionada();
      setPeriodoSelecionado();
    }

    setModalidadeSelecionada(modalidade);
  };

  const aoTrocarPeriodo = periodo => {
    if (periodo !== periodoSelecionado) setDreSelecionada();

    setPeriodoSelecionado(periodo);
  };

  const aoTrocarDre = dre => {
    if (dre !== dreSelecionada) setUnidadeEscolarSelecionada();

    setDreSelecionada(dre);
  };

  const aoTrocarUnidadeEscolar = unidade => {
    if (unidade !== unidadeEscolarSelecionada) setTurmaSelecionada();

    setUnidadeEscolarSelecionada(unidade);
  };

  const aoTrocarTurma = turma => {
    setTurmaSelecionada(turma);
  };

  const removerTurmaSelecionada = () => {
    dispatch(removerTurma());
    setModalidadeSelecionada();
    setPeriodoSelecionado();
    setDreSelecionada();
    setUnidadeEscolarSelecionada();
    setTurmaSelecionada();
    setTextoAutocomplete('');
    setAnoLetivoSelecionado();

    reabilitarCampos();
  };

  return (
    <Container className="position-relative w-100" id="containerFiltro">
      <form className="w-100" onSubmit={aoSubmeterAutocomplete}>
        <div className="form-group mb-0 w-100 position-relative">
          <Busca className="fa fa-search fa-lg bg-transparent position-absolute text-center" />
          <Campo
            type="text"
            className="form-control form-control-lg rounded d-flex px-5 border-0 fonte-14"
            placeholder="Pesquisar Turma"
            ref={campoBuscaRef}
            onFocus={aoFocarBusca}
            onChange={onChangeAutocomplete}
            onKeyDown={aoPressionarTeclaBaixoAutocomplete}
            readOnly={!!turmaUsuarioSelecionada.turma}
            value={textoAutocomplete}
          />
          {!!turmaUsuarioSelecionada.turma && (
            <Fechar
              className="fa fa-times position-absolute"
              onClick={removerTurmaSelecionada}
            />
          )}
          <Seta
            className="fa fa-caret-down rounded-circle position-absolute text-center"
            onClick={mostrarEsconderBusca}
          />
        </div>
        {resultadosFiltro.length > 0 && (
          <div className="container position-absolute bg-white shadow rounded mt-1 p-0">
            <div className="list-group">
              {resultadosFiltro.map((resultado, indice) => {
                return (
                  <ItemLista
                    key={shortid.generate()}
                    className="list-group-item list-group-item-action border-0 rounded-0"
                    onClick={() => selecionaTurmaAutocomplete(resultado)}
                    tabIndex={indice}
                  >
                    {resultado.descricaoFiltro}
                  </ItemLista>
                );
              })}
            </div>
          </div>
        )}
        {alternarFocoBusca && (
          <div
            ref={divBuscaRef}
            className="container position-absolute bg-white shadow rounded mt-1 px-3 pt-5 pb-1"
          >
            <div className="form-row">
              <Grid cols={3} className="form-group">
                <SelectComponent
                  className="fonte-14"
                  onChange={aoTrocarAnoLetivo}
                  lista={anosLetivos}
                  containerVinculoId="containerFiltro"
                  valueOption="valor"
                  valueText="desc"
                  valueSelect={
                    anoLetivoSelecionado && `${anoLetivoSelecionado}`
                  }
                  placeholder="Ano"
                  disabled={campoAnoLetivoDesabilitado}
                />
              </Grid>
              <Grid
                cols={
                  modalidadeSelecionada === modalidade.EJA.toString() ? 5 : 9
                }
                className="form-group"
              >
                <SelectComponent
                  className="fonte-14"
                  onChange={aoTrocarModalidade}
                  lista={modalidades}
                  valueOption="valor"
                  containerVinculoId="containerFiltro"
                  valueText="desc"
                  valueSelect={
                    modalidadeSelecionada && `${modalidadeSelecionada}`
                  }
                  placeholder="Modalidade"
                  disabled={campoModalidadeDesabilitado}
                />
              </Grid>
              {modalidadeSelecionada === modalidade.EJA.toString() && (
                <Grid cols={4} className="form-group">
                  <SelectComponent
                    className="fonte-14"
                    onChange={aoTrocarPeriodo}
                    lista={periodos}
                    valueOption="valor"
                    containerVinculoId="containerFiltro"
                    valueText="desc"
                    valueSelect={periodoSelecionado && `${periodoSelecionado}`}
                    placeholder="Período"
                    disabled={campoPeriodoDesabilitado}
                  />
                </Grid>
              )}
            </div>
            <div className="form-group">
              <SelectComponent
                className="fonte-14"
                onChange={aoTrocarDre}
                lista={dres}
                valueOption="valor"
                containerVinculoId="containerFiltro"
                valueText="desc"
                valueSelect={dreSelecionada && `${dreSelecionada}`}
                placeholder="Diretoria Regional De Educação (DRE)"
                disabled={campoDreDesabilitado}
              />
            </div>
            <div className="form-group">
              <SelectComponent
                className="fonte-14"
                onChange={aoTrocarUnidadeEscolar}
                lista={unidadesEscolares}
                valueOption="valor"
                containerVinculoId="containerFiltro"
                valueText="desc"
                valueSelect={
                  unidadeEscolarSelecionada && `${unidadeEscolarSelecionada}`
                }
                placeholder="Unidade Escolar (UE)"
                disabled={campoUnidadeEscolarDesabilitado}
              />
            </div>
            <div className="form-row d-flex justify-content-between">
              <Grid cols={3} className="form-group">
                <SelectComponent
                  className="fonte-14"
                  onChange={aoTrocarTurma}
                  lista={turmas}
                  valueOption="valor"
                  valueText="desc"
                  containerVinculoId="containerFiltro"
                  valueSelect={turmaSelecionada && `${turmaSelecionada}`}
                  placeholder="Turma"
                  disabled={campoTurmaDesabilitado}
                />
              </Grid>
              <Grid cols={3} className="form-group text-right">
                <Button
                  label="Aplicar filtro"
                  color={Colors.Roxo}
                  bold
                  onClick={aplicarFiltro}
                />
              </Grid>
            </div>
          </div>
        )}
      </form>
    </Container>
  );
};

export default Filtro;
