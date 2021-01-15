import React, { useCallback, useEffect, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';

import { CampoData, Label, Loader } from '~/componentes';
import { Paginacao } from '~/componentes-sgp';

import { setExibirLoaderGeralRegistroAnteriores } from '~/redux/modulos/registroIndividual/actions';

import Item from './item/item';
import MetodosRegistroIndividual from '~/paginas/DiarioClasse/RegistroIndividual/metodosRegistroIndividual';

const RegistrosAnterioresConteudo = () => {
  const [dataInicio, setDataInicio] = useState();
  const [dataFim, setDataFim] = useState(window.moment());
  const [carregandoGeral, setCarregandoGeral] = useState(false);
  const [numeroPagina, setNumeroPagina] = useState(1);
  const [numeroRegistros, setNumeroRegistros] = useState(10);

  const exibirLoaderGeralRegistroAnteriores = useSelector(
    store => store.registroIndividual.exibirLoaderGeralRegistroAnteriores
  );
  const dadosAlunoObjectCard = useSelector(
    store => store.registroIndividual.dadosAlunoObjectCard
  );
  const dadosPrincipaisRegistroIndividual = useSelector(
    store => store.registroIndividual.dadosPrincipaisRegistroIndividual
  );
  const turmaSelecionada = useSelector(state => state.usuario.turmaSelecionada);

  const dispatch = useDispatch();

  const ehMesmaData = useCallback(data => {
    const dataAtualComparativa = window.moment().format('YYYY-MM-DD');
    const dataFimComparativa = data?.format('YYYY-MM-DD');
    const ehMesma = window
      .moment(dataAtualComparativa)
      .isSame(dataFimComparativa);

    return ehMesma;
  }, []);

  const verificarData = useCallback(
    (dataInicial = dataInicio, dataFinal = dataFim) => {
      const dataFormatadaInicio = dataInicial?.format('MM-DD-YYYY');
      const dataFormatadaFim = dataFinal?.format('MM-DD-YYYY');
      const dataAtualUmDia = window
        .moment()
        .subtract(1, 'days')
        .format('MM-DD-YYYY');
      const dataFimEscolhida = ehMesmaData(window.moment())
        ? dataAtualUmDia
        : dataFormatadaFim;
      return [dataFormatadaInicio, dataFimEscolhida];
    },
    [dataInicio, dataFim, ehMesmaData]
  );

  useEffect(() => {
    const temDadosAlunos = Object.keys(dadosAlunoObjectCard).length;
    const temDadosRegistros = Object.keys(dadosPrincipaisRegistroIndividual)
      .length;

    if (
      temDadosAlunos &&
      !temDadosRegistros &&
      !exibirLoaderGeralRegistroAnteriores &&
      dataInicio &&
      dataFim &&
      numeroPagina &&
      numeroRegistros
    ) {
      (async () => {
        dispatch(setExibirLoaderGeralRegistroAnteriores(true));
        const [dataFormatadaInicio, dataFimEscolhida] = verificarData();
        await MetodosRegistroIndividual.obterRegistroIndividualPorData(
          dataFormatadaInicio,
          dataFimEscolhida,
          numeroPagina,
          numeroRegistros
        );
        dispatch(setExibirLoaderGeralRegistroAnteriores(false));
      })();
    }
  }, [
    dadosAlunoObjectCard,
    dadosPrincipaisRegistroIndividual,
    dataFim,
    dataInicio,
    dispatch,
    exibirLoaderGeralRegistroAnteriores,
    numeroPagina,
    numeroRegistros,
    verificarData,
  ]);

  const escolherData = useCallback(() => {
    const anoAtual = dataFim?.format('YYYY');
    const anoLetivo = turmaSelecionada?.anoLetivo;
    const diferencaDias = dataFim?.diff(`${anoAtual}-01-01`, 'days');
    let dataInicioSelecionada = window.moment(`${anoAtual}-01-01`);

    if (Number(diferencaDias) > 60) {
      dataInicioSelecionada = window.moment().subtract(60, 'd');
    }

    if (Number(anoLetivo) !== Number(anoAtual)) {
      dataInicioSelecionada = window.moment(`${anoLetivo}-01-01`);
      setDataFim(window.moment(`${anoLetivo}-12-31`));
    }

    setDataInicio(dataInicioSelecionada);
  }, [dataFim, turmaSelecionada]);

  useEffect(() => {
    if (!dataInicio && dataFim) {
      escolherData();
    }
  }, [dataInicio, dataFim, escolherData]);

  const onChangePaginacao = async pagina => {
    setCarregandoGeral(true);
    setNumeroPagina(pagina);
    const [dataFormatadaInicio, dataFimEscolhida] = verificarData();
    await MetodosRegistroIndividual.obterRegistroIndividualPorData(
      dataFormatadaInicio,
      dataFimEscolhida,
      pagina,
      numeroRegistros
    );
    setCarregandoGeral(false);
  };

  const onChangeNumeroLinhas = async (paginaAtual, numeroLinhas) => {
    setCarregandoGeral(true);
    setNumeroPagina(paginaAtual);
    setNumeroRegistros(numeroLinhas);
    const [dataFormatadaInicio, dataFimEscolhida] = verificarData();
    MetodosRegistroIndividual.obterRegistroIndividualPorData(
      dataFormatadaInicio,
      dataFimEscolhida,
      paginaAtual,
      numeroLinhas
    );
    setCarregandoGeral(false);
  };

  const desabilitarDataFim = dataCorrente => {
    return dataCorrente && dataCorrente > window.moment();
  };

  const mudarDataInicio = async data => {
    if (data && dataFim) {
      setCarregandoGeral(true);

      const [, dataFimEscolhida] = verificarData(data, dataFim);
      const dataFormatada = data?.format('MM-DD-YYYY');
      const dataFormatadaFim = dataFim?.format('MM-DD-YYYY');
      const dataEscolhida = ehMesmaData(dataFim)
        ? dataFimEscolhida
        : dataFormatadaFim;
      await MetodosRegistroIndividual.obterRegistroIndividualPorData(
        dataFormatada,
        dataEscolhida,
        numeroPagina,
        numeroRegistros
      );
      setCarregandoGeral(false);
      return;
    }
    setDataInicio(data);
  };

  const mudarDataFim = async data => {
    if (data && dataInicio) {
      setCarregandoGeral(true);

      const [dataFormatadaInicio, dataFimEscolhida] = verificarData(
        dataInicio,
        data
      );
      const dataFormatada = data?.format('MM-DD-YYYY');
      const dataEscolhida = ehMesmaData(data)
        ? dataFimEscolhida
        : dataFormatada;
      await MetodosRegistroIndividual.obterRegistroIndividualPorData(
        dataFormatadaInicio,
        dataEscolhida,
        numeroPagina,
        numeroRegistros
      );
      setCarregandoGeral(false);
    }
    setDataFim(data);
  };

  return (
    <Loader ignorarTip loading={carregandoGeral} className="w-100">
      <div className="px-3">
        <div className="row">
          <div className="col-12 pl-0">
            <Label text="Visualizar registros no período" />
          </div>
          <div className="col-3 p-0 pb-2 pr-3">
            <CampoData
              formatoData="DD/MM/YYYY"
              name="dataInicio"
              valor={dataInicio}
              onChange={mudarDataInicio}
              placeholder="Data início"
              desabilitarData={desabilitarDataFim}
            />
          </div>
          <div className="col-3 p-0 pb-2 mb-4">
            <CampoData
              formatoData="DD/MM/YYYY"
              name="dataFim"
              valor={dataFim}
              onChange={mudarDataFim}
              placeholder="Data fim"
              desabilitarData={desabilitarDataFim}
            />
          </div>
        </div>
        {dadosPrincipaisRegistroIndividual?.registrosIndividuais?.items.map(
          dados => (
            <Item
              key={`${dados.id}`}
              dados={dados}
              setCarregandoGeral={setCarregandoGeral}
            />
          )
        )}

        {!!dadosPrincipaisRegistroIndividual?.registrosIndividuais?.items
          .length && (
          <div className="row">
            <div className="col-12 d-flex justify-content-center mt-2">
              <Paginacao
                mostrarNumeroLinhas
                numeroRegistros={
                  dadosPrincipaisRegistroIndividual?.registrosIndividuais
                    ?.totalRegistros
                }
                onChangePaginacao={onChangePaginacao}
                onChangeNumeroLinhas={onChangeNumeroLinhas}
                pageSize={numeroRegistros}
                pageSizeOptions={['10', '20', '50', '100']}
                locale={{ items_per_page: '' }}
              />
            </div>
          </div>
        )}
      </div>
    </Loader>
  );
};

export default RegistrosAnterioresConteudo;
