import PropTypes from 'prop-types';
import React, { useCallback, useEffect, useState } from 'react';
import { Loader } from '~/componentes';
import DataUltimaAtualizacao from '~/componentes-sgp/DataUltimaAtualizacao/dataUltimaAtualizacao';
import GraficoBarras from '~/componentes-sgp/Graficos/graficosBarras';
import { erros } from '~/servicos';
import ServicoDashboardFrequencia from '~/servicos/Paginas/Dashboard/ServicoDashboardFrequencia';

const GraficoFrequenciaGlobalPorAno = props => {
  const { anoLetivo, dreId, ueId, modalidade, semestre } = props;

  const [dadosGrafico, setDadosGrafico] = useState([]);
  const [exibirLoader, setExibirLoader] = useState(false);

  const OPCAO_TODOS = '-99';

  const obterDadosGrafico = useCallback(async () => {
    setExibirLoader(true);
    const retorno = await ServicoDashboardFrequencia.obterFrequenciaGlobalPorAno(
      anoLetivo,
      dreId === OPCAO_TODOS ? '' : dreId,
      ueId === OPCAO_TODOS ? '' : ueId,
      modalidade,
      semestre
    )
      .catch(e => erros(e))
      .finally(() => setExibirLoader(false));

    if (retorno?.data?.length) {
      setDadosGrafico(retorno.data);
    } else {
      setDadosGrafico([]);
    }
  }, [anoLetivo, dreId, ueId, modalidade, semestre]);

  useEffect(() => {
    if (anoLetivo && dreId && ueId) {
      obterDadosGrafico();
    } else {
      setDadosGrafico([]);
    }
  }, [anoLetivo, dreId, ueId, obterDadosGrafico]);

  return (
    <Loader
      loading={exibirLoader}
      className={exibirLoader ? 'text-center' : ''}
    >
      <DataUltimaAtualizacao dataFormatada="04/01/2021 04:58" />
      {dadosGrafico?.length ? (
        <GraficoBarras
          data={dadosGrafico}
          xField="turma"
          xAxisVisible
          isGroup
          colors={['#0288D1', '#F57C00']}
        />
      ) : !exibirLoader ? (
        'Sem dados'
      ) : (
        ''
      )}
    </Loader>
  );
};

GraficoFrequenciaGlobalPorAno.propTypes = {
  anoLetivo: PropTypes.oneOfType(PropTypes.any),
  dreId: PropTypes.oneOfType([PropTypes.number, PropTypes.string]),
  ueId: PropTypes.oneOfType([PropTypes.number, PropTypes.string]),
  modalidade: PropTypes.oneOfType([PropTypes.number, PropTypes.string]),
  semestre: PropTypes.oneOfType([PropTypes.number, PropTypes.string]),
};

GraficoFrequenciaGlobalPorAno.defaultProps = {
  anoLetivo: null,
  dreId: null,
  ueId: null,
  modalidade: null,
  semestre: null,
};

export default GraficoFrequenciaGlobalPorAno;
