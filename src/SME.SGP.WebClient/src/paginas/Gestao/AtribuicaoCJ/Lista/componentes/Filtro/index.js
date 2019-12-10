import React, { useState } from 'react';
import PropTypes from 'prop-types';

// Formulario
import { Form, Formik } from 'formik';
import * as Yup from 'yup';

// Componentes
import { Grid, Localizador } from '~/componentes';

// Componentes SGP
import DreDropDown from '~/componentes-sgp/DreDropDown/';
import UeDropDown from '~/componentes-sgp/UeDropDown/';

// Styles
import { Row } from './styles';

function Filtro({ onFiltrar }) {
  const [refForm, setRefForm] = useState({});
  const [anoLetivo] = useState('2019');
  const [valoresIniciais] = useState({
    anoLetivo: '',
    dreId: '',
    ueId: '',
    professorRf: '',
  });

  const validacoes = () => {
    return Yup.object({});
  };

  const validarFiltro = valores => {
    const formContext = refForm && refForm.getFormikContext();
    if (formContext.isValid && Object.keys(formContext.errors).length === 0) {
      onFiltrar(valores);
    }
  };

  return (
    <Formik
      enableReinitialize
      initialValues={valoresIniciais}
      validationSchema={validacoes()}
      onSubmit={valores => onFiltrar(valores)}
      ref={refFormik => setRefForm(refFormik)}
      validate={valores => validarFiltro(valores)}
      validateOnChange
      validateOnBlur
    >
      {form => (
        <Form className="col-md-12 mb-4">
          <Row className="row mb-2">
            <Grid cols={6}>
              <DreDropDown form={form} onChange={() => null} />
            </Grid>
            <Grid cols={6}>
              <UeDropDown
                dreId={form.values.dreId}
                form={form}
                onChange={() => null}
              />
            </Grid>
          </Row>
          <Row className="row">
            <Localizador
              dreId={form.values.dreId}
              anoLetivo={anoLetivo}
              form={form}
              onChange={() => null}
            />
          </Row>
        </Form>
      )}
    </Formik>
  );
}

Filtro.propTypes = {
  onFiltrar: PropTypes.func,
};

Filtro.defaultProps = {
  onFiltrar: () => null,
};

export default Filtro;
