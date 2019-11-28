import React, { useState, useEffect } from 'react';
import PropTypes from 'prop-types';

// Componentes
import InputRF from './componentes/InputRF';
import InputNome from './componentes/InputNome';
import { Grid, Label } from '~/componentes';

// Services
import service from './services/LocalizadorService';

// Funções
import { valorNuloOuVazio } from '~/utils/funcoes/gerais';

function Localizador({ onChange, showLabel, form, dreId, anoLetivo }) {
  const [dataSource, setDataSource] = useState([]);
  const [pessoaSelecionada, setPessoaSelecionada] = useState({});

  const onChangeInput = async valor => {
    if (valor.length < 2) return;
    const { data: dados } = await service.buscarAutocomplete({
      nome: valor,
      dreId,
      anoLetivo,
    });

    if (dados && dados.length > 0) {
      setDataSource(
        dados.map(x => ({ professorRf: x.codigoRF, professorNome: x.nome }))
      );
    }
  };

  const onBuscarPorRF = async ({ rf }) => {
    const { data: dados } = await service.buscarPorRf({ rf, anoLetivo });
    if (!dados) return;
    setPessoaSelecionada({
      professorRf: dados.codigoRF,
      professorNome: dados.nome,
    });
  };

  const onSelectPessoa = objeto => {
    setPessoaSelecionada({
      professorRf: parseInt(objeto.key, 10),
      professorNome: objeto.props.value,
    });
  };

  useEffect(() => {
    onChange(pessoaSelecionada);
    form.setValues({
      ...form.values,
      ...pessoaSelecionada,
    });
  }, [pessoaSelecionada]);

  useEffect(() => {
    if (form.initialValues) {
      setPessoaSelecionada(form.initialValues);
    }
  }, [form.initialValues]);

  return (
    <>
      <Grid cols={4}>
        {showLabel && (
          <Label text="Registro Funcional (RF)" control="professorRf" />
        )}
        <InputRF
          pessoaSelecionada={pessoaSelecionada}
          onSelect={onBuscarPorRF}
          name="professorRf"
          form={form}
        />
      </Grid>
      <Grid className="pr-0" cols={8}>
        {showLabel && <Label text="Nome" control="professorNome" />}
        <InputNome
          dataSource={dataSource}
          onSelect={onSelectPessoa}
          onChange={onChangeInput}
          pessoaSelecionada={pessoaSelecionada}
          form={form}
          name="professorNome"
        />
      </Grid>
    </>
  );
}

Localizador.propTypes = {
  onChange: () => {},
  form: PropTypes.oneOfType([
    PropTypes.objectOf(PropTypes.object),
    PropTypes.any,
  ]),
  showLabel: PropTypes.bool,
  dreId: PropTypes.string,
  anoLetivo: PropTypes.oneOfType([PropTypes.number, PropTypes.string]),
};

Localizador.defaultProps = {
  onChange: PropTypes.func,
  form: {},
  showLabel: false,
  dreId: null,
  anoLetivo: null,
};

export default Localizador;
