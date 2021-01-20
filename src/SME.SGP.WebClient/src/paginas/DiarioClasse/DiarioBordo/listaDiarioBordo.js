import React, { useCallback, useEffect, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import shortid from 'shortid';
import {
  Base,
  Button,
  CampoData,
  Card,
  Colors,
  JoditEditor,
  Loader,
  PainelCollapse,
  SelectComponent,
} from '~/componentes';
import { Cabecalho, Paginacao } from '~/componentes-sgp';
import ObservacoesUsuario from '~/componentes-sgp/ObservacoesUsuario/observacoesUsuario';
import ServicoObservacoesUsuario from '~/componentes-sgp/ObservacoesUsuario/ServicoObservacoesUsuario';
import { RotasDto } from '~/dtos';
import { setDadosObservacoesUsuario } from '~/redux/modulos/observacoesUsuario/actions';
import {
  confirmar,
  ehTurmaInfantil,
  erros,
  history,
  ServicoDisciplina,
  sucesso,
  verificaSomenteConsulta,
} from '~/servicos';
import ServicoDiarioBordo from '~/servicos/Paginas/DiarioClasse/ServicoDiarioBordo';
import { Mensagens } from './componentes';

const ListaDiarioBordo = () => {
  const [carregandoGeral, setCarregandoGeral] = useState(false);
  const [turmaInfantil, setTurmaInfantil] = useState(false);
  const [
    listaComponenteCurriculares,
    setListaComponenteCurriculares,
  ] = useState();
  const [
    componenteCurricularSelecionado,
    setComponenteCurricularSelecionado,
  ] = useState();
  const [dataFinal, setDataFinal] = useState();
  const [dataInicial, setDataInicial] = useState();
  const [diarioBordoAtual, setDiarioBordoAtual] = useState();
  const [listaTitulos, setListaTitulos] = useState();
  const [numeroPagina, setNumeroPagina] = useState(1);
  const usuario = useSelector(state => state.usuario);
  const { turmaSelecionada } = usuario;
  const permissoesTela = usuario.permissoes[RotasDto.DIARIO_BORDO];
  const turmaId = turmaSelecionada?.id || 0;
  const turma = turmaSelecionada?.turma || 0;
  const modalidadesFiltroPrincipal = useSelector(
    store => store.filtro.modalidades
  );
  const listaUsuarios = useSelector(
    store => store.observacoesUsuario.listaUsuariosNotificacao
  );

  const dispatch = useDispatch();

  const obterComponentesCurriculares = useCallback(async () => {
    setCarregandoGeral(true);
    const componentes = await ServicoDisciplina.obterDisciplinasPorTurma(
      turma
    ).catch(e => erros(e));

    if (componentes?.data?.length) {
      setListaComponenteCurriculares(componentes.data);

      if (componentes.data.length === 1) {
        const componente = componentes.data[0];
        setComponenteCurricularSelecionado(
          String(componente.codigoComponenteCurricular)
        );
      }
    }

    setCarregandoGeral(false);
  }, [turma]);

  useEffect(() => {
    const naoSetarSomenteConsultaNoStore = !ehTurmaInfantil(
      modalidadesFiltroPrincipal,
      turmaSelecionada
    );
    verificaSomenteConsulta(permissoesTela, naoSetarSomenteConsultaNoStore);
  }, [permissoesTela, turmaSelecionada]);

  const numeroRegistros = 10;
  const numeroTotalRegistros = listaTitulos?.totalRegistros;
  const mostrarPaginacao = numeroTotalRegistros > numeroRegistros;

  const resetarTela = useCallback(() => {}, []);

  useEffect(() => {
    if (turma && turmaInfantil) {
      obterComponentesCurriculares();
      return;
    }
    setListaComponenteCurriculares([]);
    setComponenteCurricularSelecionado(undefined);
    resetarTela();
  }, [turma, obterComponentesCurriculares, resetarTela, turmaInfantil]);

  useEffect(() => {
    const infantil = ehTurmaInfantil(
      modalidadesFiltroPrincipal,
      turmaSelecionada
    );
    setTurmaInfantil(infantil);

    if (!turmaInfantil) {
      resetarTela();
    }
  }, [
    turmaSelecionada,
    modalidadesFiltroPrincipal,
    resetarTela,
    turmaInfantil,
  ]);

  const onChangeComponenteCurricular = valor => {
    setComponenteCurricularSelecionado(valor);
  };

  const onClickConsultarDiario = () => {
    history.push(
      `${RotasDto.DIARIO_BORDO}/detalhes/${diarioBordoAtual?.aulaId}`
    );
  };

  const obterTitulos = useCallback(
    async (dataInicio, dataFim) => {
      setCarregandoGeral(true);
      const retorno = await ServicoDiarioBordo.obterTitulosDiarioBordo({
        turmaId,
        componenteCurricularId: componenteCurricularSelecionado,
        dataInicio,
        dataFim,
        numeroPagina,
        numeroRegistros,
      })
        .catch(e => erros(e))
        .finally(() => setCarregandoGeral(false));

      if (retorno?.status === 200) {
        setListaTitulos(retorno.data);
      }
    },
    [componenteCurricularSelecionado, turmaId, numeroPagina]
  );

  useEffect(() => {
    if (
      ((dataInicial && dataFinal) || (!dataInicial && !dataFinal)) &&
      componenteCurricularSelecionado &&
      numeroPagina
    ) {
      const dataIncialFormatada = dataInicial?.format('MM-DD-YYYY');
      const dataFinalFormatada = dataFinal?.format('MM-DD-YYYY');
      obterTitulos(dataIncialFormatada, dataFinalFormatada);
    }
  }, [
    dataInicial,
    dataFinal,
    componenteCurricularSelecionado,
    obterTitulos,
    numeroPagina,
  ]);

  const onChangePaginacao = pagina => {
    setNumeroPagina(pagina);
  };

  const onColapse = async id => {
    if (id) {
      const dados = await ServicoDiarioBordo.obterDiarioBordoDetalhes(id);
      if (dados?.data) {
        setDiarioBordoAtual(dados.data);
        if (dados.data.observacoes.length) {
          dispatch(setDadosObservacoesUsuario(dados.data.observacoes));
        }
      }
    }
  };

  const salvarEditarObservacao = async valor => {
    const params = {
      observacao: valor.observacao,
      usuariosIdNotificacao: [],
      id: valor.id,
    };
    if (listaUsuarios?.length) {
      params.usuariosIdNotificacao = listaUsuarios.map(u => {
        return u.usuarioId;
      });
    }
    setCarregandoGeral(true);
    const resultado = await ServicoDiarioBordo.salvarEditarObservacao(
      diarioBordoAtual?.id,
      params
    ).catch(e => {
      erros(e);
      setCarregandoGeral(false);
    });
    if (resultado?.status === 200) {
      sucesso(`Observacao ${valor.id ? 'alterada' : 'inserida'} com sucesso`);
      ServicoObservacoesUsuario.atualizarSalvarEditarDadosObservacao(
        valor,
        resultado.data
      );
      setCarregandoGeral(false);
      return resultado;
    }
    setCarregandoGeral(false);
  };

  const excluirObservacao = async obs => {
    const confirmado = await confirmar(
      'Excluir',
      '',
      'Você tem certeza que deseja excluir este registro?'
    );

    if (confirmado) {
      setCarregandoGeral(true);
      const resultado = await ServicoDiarioBordo.excluirObservacao(obs).catch(
        e => {
          erros(e);
          setCarregandoGeral(false);
        }
      );
      if (resultado && resultado.status === 200) {
        sucesso('Registro excluído com sucesso');
        ServicoDiarioBordo.atualizarExcluirDadosObservacao(obs, resultado.data);
      }
      setCarregandoGeral(false);
    }
  };

  const desabilitarData = current => {
    const dataInicioAnoAtual = window.moment(
      new Date(`${turmaSelecionada.anoLetivo}-01-01 00:00:00`)
    );
    const dataFimAnoAtual = window.moment(
      new Date(`${turmaSelecionada.anoLetivo}-12-31 00:00:00`)
    );
    if (current) {
      return current < dataInicioAnoAtual || current >= dataFimAnoAtual;
    }
    return false;
  };

  const onClickVoltar = () => {
    history.push('/');
  };
  const onClickNovo = () => {
    history.push(`${RotasDto.DIARIO_BORDO}/novo`);
  };

  return (
    <Loader loading={carregandoGeral} className="w-100">
      <Mensagens />
      <Cabecalho pagina="Diário de bordo" />
      <Card>
        <div className="col-md-12 p-0">
          <div className="row">
            <div className="col-sm-12 col-md-4 col-lg-4 col-xl-4 mb-4">
              <SelectComponent
                id="disciplina"
                name="disciplinaId"
                lista={listaComponenteCurriculares || []}
                valueOption="codigoComponenteCurricular"
                valueText="nome"
                valueSelect={componenteCurricularSelecionado}
                onChange={onChangeComponenteCurricular}
                placeholder="Selecione um componente curricular"
                disabled={
                  !turmaId ||
                  !turmaInfantil ||
                  (listaComponenteCurriculares &&
                    listaComponenteCurriculares.length === 1)
                }
              />
            </div>
            <div className="col-sm-12 col-lg-8 col-md-8 d-flex justify-content-end pb-4">
              <Button
                label="Voltar"
                icon="arrow-left"
                color={Colors.Azul}
                border
                className="mr-2"
                onClick={onClickVoltar}
              />
              <Button
                label="Novo"
                color={Colors.Roxo}
                bold
                className="mr-2"
                onClick={onClickNovo}
                disabled={
                  !permissoesTela.podeIncluir ||
                  !turmaInfantil ||
                  !listaComponenteCurriculares ||
                  !componenteCurricularSelecionado
                }
              />
            </div>
          </div>
          <div className="row">
            <div className="col-sm-12 col-md-4 col-lg-3 col-xl-3 mb-4 pr-0">
              <CampoData
                valor={dataInicial}
                onChange={data => setDataInicial(data)}
                name="dataInicial"
                placeholder="DD/MM/AAAA"
                formatoData="DD/MM/YYYY"
                desabilitado={
                  !turmaInfantil ||
                  !listaComponenteCurriculares ||
                  !componenteCurricularSelecionado
                }
                desabilitarData={desabilitarData}
              />
            </div>
            <div className="col-sm-12 col-md-4 col-lg-3 col-xl-3 mb-4">
              <CampoData
                valor={dataFinal}
                onChange={data => setDataFinal(data)}
                name="dataFinal"
                placeholder="DD/MM/AAAA"
                formatoData="DD/MM/YYYY"
                desabilitado={
                  !turmaInfantil ||
                  !listaComponenteCurriculares ||
                  !componenteCurricularSelecionado
                }
                desabilitarData={desabilitarData}
              />
            </div>
          </div>
          <div className="row">
            <div className="col-sm-12 mb-3">
              <PainelCollapse accordion onChange={onColapse}>
                {listaTitulos?.items?.map(({ id, titulo }) => (
                  <PainelCollapse.Painel
                    key={id}
                    accordion
                    espacoPadrao
                    corBorda={Base.AzulBordaCollapse}
                    temBorda
                    header={titulo}
                    // altura={44}
                  >
                    <div className="row ">
                      <div className="col-sm-12 mb-3">
                        <JoditEditor
                          id={`${id}-editor-planejamento`}
                          name="planejamento"
                          value={diarioBordoAtual?.planejamento}
                          desabilitar
                        />
                      </div>
                      <div className="col-sm-12 d-flex justify-content-end mb-4">
                        <Button
                          id={shortid.generate()}
                          label="Consultar diário completo"
                          icon="book"
                          color={Colors.Azul}
                          border
                          onClick={onClickConsultarDiario}
                        />
                      </div>
                      <div className="col-sm-12 p-0 position-relative">
                        <ObservacoesUsuario
                          esconderLabel
                          salvarObservacao={obs => salvarEditarObservacao(obs)}
                          editarObservacao={obs => salvarEditarObservacao(obs)}
                          excluirObservacao={obs => excluirObservacao(obs)}
                          permissoes={permissoesTela}
                        />
                      </div>
                    </div>
                  </PainelCollapse.Painel>
                ))}
              </PainelCollapse>
            </div>
          </div>
          {mostrarPaginacao && (
            <div className="row">
              <div className="col-12 d-flex justify-content-center mt-4">
                <Paginacao
                  numeroRegistros={numeroTotalRegistros}
                  pageSize={10}
                  onChangePaginacao={onChangePaginacao}
                />
              </div>
            </div>
          )}
        </div>
      </Card>
    </Loader>
  );
};

export default ListaDiarioBordo;
