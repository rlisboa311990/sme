import PropTypes from 'prop-types';
import React, { useState } from 'react';
import { Base } from '~/componentes';
import CardCollapse from '~/componentes/cardCollapse';
import ServicoDashboardAEE from '~/servicos/Paginas/Dashboard/ServicoDashboardAEE';
import GraficoBarrasPadraoAEE from '../../Componentes/graficoBarrasPadraoAEE';

const QuantidadeEstudantesMatriculados = props => {
  const { anoLetivo, dreId, ueId } = props;

  const configCabecalho = {
    altura: '44px',
    corBorda: Base.AzulBordaCollapse,
  };

  const [exibir, setExibir] = useState(false);

  const key = 'quantidade-estudantes-matriculados';

  return (
    <div className="mt-3">
      <CardCollapse
        titulo="Quantidade de estudantes matriculados em SRM ou PAEE colaborativo"
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
                { nomeChave: 'quantidadeSRM', legenda: 'legendaSRM' },
                { nomeChave: 'quantidadePAEE', legenda: 'legendaPAEE' },
              ]}
              nomeIndiceDesc="descricao"
              ServicoObterValoresGrafico={
                ServicoDashboardAEE.obterQuantidadeEstudantesMatriculados
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

QuantidadeEstudantesMatriculados.propTypes = {
  anoLetivo: PropTypes.oneOfType(PropTypes.any),
  dreId: PropTypes.string,
  ueId: PropTypes.string,
};

QuantidadeEstudantesMatriculados.defaultProps = {
  anoLetivo: null,
  dreId: '',
  ueId: '',
};

export default QuantidadeEstudantesMatriculados;
