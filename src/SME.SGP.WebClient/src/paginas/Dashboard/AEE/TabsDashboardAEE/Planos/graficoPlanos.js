import PropTypes from 'prop-types';
import React from 'react';
import QuantidadePlanosSituacao from './QuantidadePlanosSituacao/quantidadePlanosSituacao';

const GraficosPlanos = props => {
  const { anoLetivo, dreId, ueId } = props;
  return (
    <>
      <QuantidadePlanosSituacao
        anoLetivo={anoLetivo}
        dreId={dreId}
        ueId={ueId}
      />
    </>
  );
};

GraficosPlanos.propTypes = {
  anoLetivo: PropTypes.oneOfType(PropTypes.any),
  dreId: PropTypes.string,
  ueId: PropTypes.string,
};

GraficosPlanos.defaultProps = {
  anoLetivo: null,
  dreId: '',
  ueId: '',
};

export default GraficosPlanos;
