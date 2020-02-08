import React from 'react';
import t from 'prop-types';

// Componentes
import { SelectComponent } from '~/componentes';

function PeriodosDropDown({ onChangePeriodo, valor, desabilitado }) {
  const opcoes = [
    {
      valor: '1',
      descricao: 'Encaminhamento',
    },
    {
      valor: '2',
      descricao: 'Acompanhamento 1º Semestre',
    },
    {
      valor: '3',
      descricao: 'Acompanhamento 2º Semestre',
    },
  ];

  return (
    <SelectComponent
      onChange={onChangePeriodo}
      valueOption="valor"
      valueText="descricao"
      lista={opcoes}
      placeholder="Selecione o período"
      valueSelect={valor}
      disabled={desabilitado}
    />
  );
}

PeriodosDropDown.propTypes = {
  onChangePeriodo: t.func,
  valor: t.string,
  desabilitado: t.bool,
};

PeriodosDropDown.defaultProps = {
  onChangePeriodo: () => {},
  valor: undefined,
  desabilitado: false,
};

export default PeriodosDropDown;
