import React, { useState, useEffect } from 'react';
import PropTypes from 'prop-types';

// Componentes
import InputRF from './componentes/InputRF';
import InputNome from './componentes/InputNome';
import { Grid, Label } from '~/componentes';

// Services
import service from './services/LocalizadorService';

function Localizador({ onChange, showLabel, form, dreId, anoLetivo }) {
  const [dataSource, setDataSource] = useState([]);
  const [pessoaSelecionada, setPessoaSelecionada] = useState({});

  const onChangeInput = async valor => {
    if (valor.length < 2) return;
    const { dados } = await service.buscarAutocomplete({
      nome: valor,
      dreId,
      anoLetivo,
    });
    setDataSource(dados);
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
      <Grid cols={8}>
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

Localizador.defaultProps = {
  onChange: () => {},
  form: PropTypes.objectOf(PropTypes.object),
};

Localizador.propTypes = {
  onChange: PropTypes.func,
  form: {},
};

export default Localizador;
