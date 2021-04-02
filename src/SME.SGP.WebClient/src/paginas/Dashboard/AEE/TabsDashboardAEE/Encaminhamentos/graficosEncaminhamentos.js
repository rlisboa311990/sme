import PropTypes from 'prop-types';
import React from 'react';
import QuantidadeEncaminhamentosSituacao from './QuantidadeEncaminhamentosSituacao/quantidadeEncaminhamentosSituacao';

const GraficosEncaminhamentos = props => {
  const { anoLetivo, dreId, ueId } = props;
  return (
    <>
      <QuantidadeEncaminhamentosSituacao
        anoLetivo={anoLetivo}
        dreId={dreId}
        ueId={ueId}
      />
    </>
  );
};

GraficosEncaminhamentos.propTypes = {
  anoLetivo: PropTypes.oneOfType(PropTypes.any),
  dreId: PropTypes.string,
  ueId: PropTypes.string,
};

GraficosEncaminhamentos.defaultProps = {
  anoLetivo: null,
  dreId: '',
  ueId: '',
};

export default GraficosEncaminhamentos;
