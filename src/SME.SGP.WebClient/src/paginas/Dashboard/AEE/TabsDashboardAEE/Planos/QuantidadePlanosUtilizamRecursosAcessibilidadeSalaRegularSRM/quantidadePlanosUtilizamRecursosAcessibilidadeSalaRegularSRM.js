import PropTypes from 'prop-types';
import React, { useState } from 'react';
import { Base } from '~/componentes';
import CardCollapse from '~/componentes/cardCollapse';
import ServicoDashboardAEE from '~/servicos/Paginas/Dashboard/ServicoDashboardAEE';
import GraficoBarrasPadraoAEE from '../../Componentes/graficoBarrasPadraoAEE';

const QuantidadePlanosUtilizamRecursosAcessibilidadeSalaRegularSRM = props => {
  const { anoLetivo, dreId, ueId } = props;

  const configCabecalho = {
    altura: '44px',
    corBorda: Base.AzulBordaCollapse,
  };

  const [exibir, setExibir] = useState(false);

  const key = 'quantidade-planos-utilizam-recursos-acessibilidade-regular-srm';

  return (
    <div className="mt-3">
      <CardCollapse
        titulo="Quantidade de planos que utilizam recursos de acessibilidade na Sala Regular e na SRM"
        key={`${key}-collapse-key`}
        indice={`${key}-collapse-indice`}
        alt={`${key}-alt`}
        configCabecalho={configCabecalho}
        show={exibir}
        onClick={() => {
          setExibir(!exibir);
        }}
      >
        {exibir ? (
          <div className="col-md-12">
            <GraficoBarrasPadraoAEE
              anoLetivo={anoLetivo}
              dreId={dreId}
              ueId={ueId}
              chavesGraficoAgrupado={[
                { nomeChave: 'quantidadeSim', legenda: 'legendaSim' },
                { nomeChave: 'quantidadeNao', legenda: 'legendaNao' },
              ]}
              nomeIndiceDesc="descricao"
              ServicoObterValoresGrafico={
                ServicoDashboardAEE.obterPlanosAcessibilidades
              }
            />
          </div>
        ) : (
          ''
        )}
      </CardCollapse>
    </div>
  );
};

QuantidadePlanosUtilizamRecursosAcessibilidadeSalaRegularSRM.propTypes = {
  anoLetivo: PropTypes.oneOfType(PropTypes.any),
  dreId: PropTypes.string,
  ueId: PropTypes.string,
};

QuantidadePlanosUtilizamRecursosAcessibilidadeSalaRegularSRM.defaultProps = {
  anoLetivo: null,
  dreId: '',
  ueId: '',
};

export default QuantidadePlanosUtilizamRecursosAcessibilidadeSalaRegularSRM;
