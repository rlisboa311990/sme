import PropTypes from 'prop-types';
import React from 'react';
import CheckboxGroup from '~/componentes/checkboxGroup';

const CampoDinamicoCheckbox = props => {
  const { questaoAtual, form, label, desabilitado } = props;

  const options = questaoAtual?.opcaoResposta.map(item => {
    return { label: item.nome, value: item.id };
  });

  return (
    <div className="col-md-12 mb-3">
      {label}
      <CheckboxGroup
        options={options}
        disabled={desabilitado}
        id={String(questaoAtual?.id)}
        name={String(questaoAtual?.id)}
        form={form}
      />
    </div>
  );
};

CampoDinamicoCheckbox.propTypes = {
  questaoAtual: PropTypes.oneOfType([PropTypes.any]),
  form: PropTypes.oneOfType([PropTypes.any]),
  label: PropTypes.oneOfType([PropTypes.any]),
  desabilitado: PropTypes.bool,
};

CampoDinamicoCheckbox.defaultProps = {
  questaoAtual: null,
  form: null,
  label: '',
  desabilitado: false,
};

export default CampoDinamicoCheckbox;
