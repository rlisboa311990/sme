import { Steps } from 'antd';
import React, { useCallback, useEffect } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import shortid from 'shortid';
import { setDadosSecoesPorEtapaDeEncaminhamentoAEE } from '~/redux/modulos/encaminhamentoAEE/actions';
import { erros } from '~/servicos';
import ServicoEncaminhamentoAEE from '~/servicos/Paginas/Relatorios/AEE/ServicoEncaminhamentoAEE';
import { ContainerStepsEncaminhamento } from '../../../encaminhamentoAEECadastro.css';
import DadosPorSecaoCollapse from './dadosPorSecaoCollapse';

const { Step } = Steps;

const DadosSecaoEncaminhamento = () => {
  const dispatch = useDispatch();

  const dadosSecaoLocalizarEstudante = useSelector(
    store => store.encaminhamentoAEE.dadosSecaoLocalizarEstudante
  );

  const dadosSecoesPorEtapaDeEncaminhamentoAEE = useSelector(
    store => store.encaminhamentoAEE.dadosSecoesPorEtapaDeEncaminhamentoAEE
  );

  const obterSecoesPorEtapaDeEncaminhamentoAEE = useCallback(async () => {
    const resposta = await ServicoEncaminhamentoAEE.obterSecoesPorEtapaDeEncaminhamentoAEE(
      1
    ).catch(e => erros(e));

    if (resposta?.data) {
      dispatch(setDadosSecoesPorEtapaDeEncaminhamentoAEE(resposta.data));
    } else {
      dispatch(setDadosSecoesPorEtapaDeEncaminhamentoAEE([]));
    }
  }, [dispatch]);

  useEffect(() => {
    if (
      dadosSecaoLocalizarEstudante?.codigoAluno &&
      dadosSecaoLocalizarEstudante?.anoLetivo
    ) {
      obterSecoesPorEtapaDeEncaminhamentoAEE();
    } else {
      dispatch(setDadosSecoesPorEtapaDeEncaminhamentoAEE([]));
    }
  }, [
    dispatch,
    dadosSecaoLocalizarEstudante,
    obterSecoesPorEtapaDeEncaminhamentoAEE,
  ]);

  return dadosSecoesPorEtapaDeEncaminhamentoAEE?.length ? (
    <ContainerStepsEncaminhamento direction="vertical" current={1}>
      {dadosSecoesPorEtapaDeEncaminhamentoAEE.map(item => {
        return (
          <Step
            status="process"
            title={
              <DadosPorSecaoCollapse dados={item} index={shortid.generate()} />
            }
          />
        );
      })}
    </ContainerStepsEncaminhamento>
  ) : (
    ''
  );
};

export default DadosSecaoEncaminhamento;
