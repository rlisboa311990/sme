import PropTypes from 'prop-types';
import React from 'react';
import QuantidadeRegistrosPAAI from './QuantidadeRegistrosPAAI/quantidadeRegistrosPAAI';
import QuantidadeRegistrosPorObjetivo from './QuantidadeRegistrosPorObjetivo/quantidadeRegistrosPorObjetivo';

const GraficosRegistroItinerancia = props => {
  const { anoLetivo, dreId, ueId, mesSelecionado } = props;
  return (
    <>
      <QuantidadeRegistrosPAAI
        anoLetivo={anoLetivo}
        dreId={dreId}
        ueId={ueId}
        mesSelecionado={mesSelecionado}
      />
      <QuantidadeRegistrosPorObjetivo
        anoLetivo={anoLetivo}
        dreId={dreId}
        ueId={ueId}
        mesSelecionado={mesSelecionado}
      />
    </>
  );
};

GraficosRegistroItinerancia.propTypes = {
  anoLetivo: PropTypes.oneOfType(PropTypes.any),
  dreId: PropTypes.string,
  ueId: PropTypes.string,
  mesSelecionado: PropTypes.string,
};

GraficosRegistroItinerancia.defaultProps = {
  anoLetivo: null,
  dreId: '',
  ueId: '',
  mesSelecionado: '',
};

export default GraficosRegistroItinerancia;
