import { Input } from 'antd';
import { Field } from 'formik';
import PropTypes from 'prop-types';
import React from 'react';
import styled from 'styled-components';
import { Base } from './colors';
import Label from './label';

const Campo = styled.div`
  span {
    color: ${Base.Vermelho};
  }
  .campo {
    margin-bottom: 5px;
  }
  .ant-input {
    height: 38px;
  }
  label {
    font-weight: bold;
  }
  .ant-input-affix-wrapper .ant-input:not(:first-child) {
    padding-left: 40px;
  }
  .form-control {
    // border: 0;
    // height: auto !important;
  }
`;

const CampoTexto = React.forwardRef((props, ref) => {
  const {
    name,
    id,
    form,
    className,
    classNameCampo,
    type,
    maskType,
    placeholder,
    onChange,
    onKeyDown,
    value,
    desabilitado,
    maxlength,
    label,
    semMensagem,
    style,
    iconeBusca,
    allowClear,
  } = props;

  const possuiErro = () => {
    return form && form.errors[name] && form.touched[name];
  };

  const executaOnBlur = event => {
    const { relatedTarget } = event;
    if (relatedTarget && relatedTarget.getAttribute('type') === 'button') {
      event.preventDefault();
    }
  };

  const onChangeCampo = e => {
    form.setFieldValue(name, e.target.value);
    form.setFieldTouched(name, true, true);
    onChange(e);
  };

  return (
    <Campo className={classNameCampo}>
      {label ? <Label text={label} control={name || ''} /> : ''}
      {form ? (
        <>
          {' '}
          <Field
            name={name}
            id={id || name}
            className={`form-control campo ${
              possuiErro() ? 'is-invalid' : ''
            } ${className || ''} ${desabilitado ? 'desabilitado' : ''}`}
            component={type || Input}
            type={maskType}
            readOnly={desabilitado}
            disabled={desabilitado}
            onBlur={executaOnBlur}
            maxLength={maxlength || ''}
            innerRef={ref}
            onKeyDown={onKeyDown}
            placeholder={placeholder}
            onChange={onChangeCampo}
            style={style}
            prefix={iconeBusca && <i className="fa fa-search fa-lg" />}
            value={value || form.values[name]}
            allowClear={allowClear}
          />
          {!semMensagem && form && form.touched[name] ? (
            <span>{form.errors[name]}</span>
          ) : (
            ''
          )}
        </>
      ) : (
        <Input
          ref={ref}
          placeholder={placeholder}
          onChange={onChange}
          disabled={desabilitado}
          onKeyDown={onKeyDown}
          value={value}
          prefix={iconeBusca && <i className="fa fa-search fa-lg" />}
          allowClear
        />
      )}
    </Campo>
  );
});

CampoTexto.propTypes = {
  onChange: PropTypes.func,
  semMensagem: PropTypes.bool,
  form: PropTypes.oneOfType([PropTypes.any]),
};

CampoTexto.defaultProps = {
  onChange: () => {},
  semMensagem: false,
  form: null,
};

export default CampoTexto;
