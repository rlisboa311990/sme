import React from 'react';
import PropTypes from 'prop-types';
import styled from 'styled-components';
import Select from 'antd/es/select';
import Icon from 'antd/es/icon';
import shortid from 'shortid';
import { Base } from './colors';
import Label from './label';

const Container = styled.div`
  .ant-select {
    width: 100%;
  }

  .ant-select-arrow {
    color: ${Base.CinzaMako};
  }
  .ant-select-selection--single {
    align-items: center;
    display: flex;
    height: 38px;
  }
  .ant-select-selection__rendered {
    width: 98%;
  }
  .ant-select-selection__placeholder {
    display: block;
  }
  .ant-select .ant-select-search__field {
    display: block;
    max-width: 100% !important;
  }
  .ant-select-selection-selected-value {
    font-weight: bold;
  }
  .ant-select-selection--multiple {
    min-height: 38px;

    .ant-select-selection__placeholder {
      line-height: 25px;
    }

    .ant-select-selection__rendered {
      margin-top: 3px;
    }
  }
`;

const SelectComponent = props => {
  const {
    name,
    id,
    className,
    classNameContainer,
    onChange,
    label,
    valueText,
    valueOption,
    valueSelect,
    lista,
    placeholder,
    alt,
    multiple,
    disabled,
  } = props;

  const { Option } = Select;

  console.log(valueSelect);
  console.log(valueOption);
  console.log(valueText);

  return (
    <Container className={classNameContainer && classNameContainer}>
      {label ? <Label text={label} control={name} /> : ''}
      <Select
        mode={multiple && 'multiple'}
        suffixIcon={<Icon type="caret-down" />}
        className={`overflow-hidden ${className && className}`}
        name={name}
        id={id}
        onChange={onChange}
        value={valueSelect && valueSelect}
        placeholder={placeholder}
        notFoundContent="Sem dados"
        alt={alt}
        optionFilterProp="children"
        allowClear
        disabled={disabled}
      >
        {lista && lista.length &&
          lista.map(item => {
            return (
              <Option key={shortid.generate()} value={`${item[valueOption]}`}>
                {`${item[valueText]}`}
              </Option>
            );
          })}
      </Select>
    </Container>
  );
};

SelectComponent.propTypes = {
  name: PropTypes.string,
  id: PropTypes.string,
  className: PropTypes.string,
  onChange: PropTypes.func,
  label: PropTypes.string,
  valueText: PropTypes.string,
  valueOption: PropTypes.string.isRequired,
  valueSelect: PropTypes.any,
  lista: PropTypes.array,
  placeholder: PropTypes.string,
  disabled: PropTypes.bool,
};

export default SelectComponent;
