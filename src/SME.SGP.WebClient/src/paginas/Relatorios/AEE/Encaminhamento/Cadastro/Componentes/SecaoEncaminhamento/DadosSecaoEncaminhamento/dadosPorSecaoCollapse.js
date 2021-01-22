import PropTypes from 'prop-types';
import React from 'react';
import { Base } from '~/componentes';
import CardCollapse from '~/componentes/cardCollapse';
import MontarDadosPorSecao from './montarDadosPorSecao';

const DadosPorSecaoCollapse = props => {
  const { dados, index, match, codigoAluno, codigoTurma } = props;
  const { nome } = dados;

  const configCabecalho = {
    altura: '50px',
    corBorda: Base.AzulBordaCard,
  };

  const onClickCardCollapse = () => {};

  return (
    <div className="mt-2 mb-2">
      <CardCollapse
        key={`secao-${index}-collapse-key`}
        titulo={nome}
        indice={`secao-${index}-collapse-indice`}
        alt={`secao-${index}-alt`}
        onClick={onClickCardCollapse}
        configCabecalho={configCabecalho}
        show
      >
        <MontarDadosPorSecao
          dados={dados}
          match={match}
          codigoAluno={codigoAluno}
          codigoTurma={codigoTurma}
        />
      </CardCollapse>
    </div>
  );
};

DadosPorSecaoCollapse.propTypes = {
  dados: PropTypes.oneOfType([PropTypes.object]),
  index: PropTypes.oneOfType([PropTypes.any]),
  match: PropTypes.oneOfType([PropTypes.object]),
  codigoAluno: PropTypes.string,
  codigoTurma: PropTypes.string,
};

DadosPorSecaoCollapse.defaultProps = {
  dados: '',
  index: null,
  match: {},
  codigoAluno: '',
  codigoTurma: '',
};

export default DadosPorSecaoCollapse;
