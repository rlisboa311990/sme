import React, { useState, useEffect } from 'react';
import PropTypes from 'prop-types';

// Componentes
import { SelectComponent } from '~/componentes';

// Servicos
import AtribuicaoEsporadicaServico from '~/servicos/Paginas/AtribuicaoEsporadica';

// Funções
import { valorNuloOuVazio } from '~/utils/funcoes/gerais';

function DreDropDown({ form, onChange, label, desabilitado }) {
  const [listaDres, setListaDres] = useState([]);

  useEffect(() => {
    async function buscarDres() {
      const { data } = await AtribuicaoEsporadicaServico.buscarDres();
      if (data) {
        setListaDres(
          data.map(item => ({
            desc: item.nome,
            valor: item.codigo,
            abrev: item.abreviacao,
          }))
        );
      }
    }
    buscarDres();
  }, []);

  useEffect(() => {
    if (listaDres.length === 1) {
      form.setFieldValue('dreId', listaDres[0].valor);
      onChange(listaDres[0].valor);
    }
  }, [listaDres]);

  useEffect(() => {
    onChange();
    if (!valorNuloOuVazio(form.values.dreId)) {
      onChange(form.values.dreId);
    }
  }, [form.values.dreId]);

  return (
    <SelectComponent
      label={!label ? null : label}
      form={form}
      name="dreId"
      className="fonte-14"
      onChange={onChange}
      lista={listaDres}
      valueOption="valor"
      containerVinculoId="containerFiltro"
      valueText="desc"
      placeholder="Diretoria Regional De Educação (DRE)"
      disabled={
        listaDres.length === 1 ||
        (typeof desabilitado === 'boolean' && desabilitado === true)
      }
    />
  );
}

DreDropDown.propTypes = {
  form: PropTypes.oneOfType([
    PropTypes.objectOf(PropTypes.object),
    PropTypes.any,
  ]),
  onChange: PropTypes.func,
  label: PropTypes.string,
  desabilitado: PropTypes.bool,
};

DreDropDown.defaultProps = {
  form: {},
  onChange: () => {},
  label: null,
  desabilitado: null,
};

export default DreDropDown;
