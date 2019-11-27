import React, { useEffect } from 'react';
import PropTypes from 'prop-types';

// Redux
import { useSelector } from 'react-redux';

// Componentes
import { SelectComponent } from '~/componentes';

function AnoLetivoDropDown({ form, name, onChange, label }) {
  const anosLetivos = useSelector(store => store.filtro.anosLetivos);

  useEffect(() => {
    if (anosLetivos.length === 1) {
      form.setFieldValue(name, String(anosLetivos[0].valor), false);
      onChange(String(anosLetivos[0].valor));
    }
  }, [anosLetivos]);

  return (
    <SelectComponent
      label={!label ? null : label}
      valueOption="valor"
      valueText="desc"
      form={form}
      name={name}
      lista={anosLetivos}
      placeholder="Ano letivo"
      onChange={onChange}
      disabled={anosLetivos && anosLetivos.length === 1}
    />
  );
}

AnoLetivoDropDown.propTypes = {
  form: PropTypes.oneOfType([
    PropTypes.objectOf(PropTypes.object),
    PropTypes.any,
  ]),
  name: PropTypes.string,
  onChange: PropTypes.func,
  label: PropTypes.string,
};

AnoLetivoDropDown.defaultProps = {
  form: {},
  name: '',
  onChange: () => null,
  label: null,
};

export default AnoLetivoDropDown;
