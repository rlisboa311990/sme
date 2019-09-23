import styled from 'styled-components';
import { Base } from '../colors';

export const Container = styled.div`
  .ocultar-coluna-multi-selecao {
    .ant-table-selection-column {
      display: none !important;
    }
  }

  .ant-checkbox-indeterminate .ant-checkbox-inner::after {
    background-color: ${Base.Roxo} !important;
  }

  .ant-checkbox-checked .ant-checkbox-inner {
    background-color: ${Base.Roxo};
    border-color: ${Base.Branco} !important;
  }

  .ant-table-thead tr th {
    background: ${Base.CinzaFundo} !important;
    border-right: solid 1px ${Base.CinzaDesabilitado};
    text-align: center;
  }

  .ant-table-tbody tr td {
    border-right: solid 1px ${Base.CinzaDesabilitado};
    cursor: pointer;
    white-space: nowrap;
  }

  .ant-table-column-title {
    color: #323c47;
    font-size: 14px;
    font-weight: bold;
    letter-spacing: 0.12px;
    text-transform: capitalize;
  }

  .ant-table-pagination.ant-pagination {
    float: none;
    text-align: center !important;
  }

  .ant-pagination-item {
    border-radius: 0;

    a {
      color: ${Base.Branco} !important;
      font-family: Roboto;
      font-style: normal;
      font-stretch: normal;
      font-weight: bold;
      letter-spacing: normal;
    }
  }

  .ant-pagination.mini .ant-pagination-item {
    height: 45px;
    line-height: 45px;
    margin: 0;
    min-width: 45px;
  }

  .ant-pagination-prev {
    border: solid 1px ${Base.CinzaDesabilitado} !important;
  }

  .ant-pagination.mini .ant-pagination-prev {
    border-radius: 4px 0px 0px 4px;
    border-right: none !important;
    height: 45px;
    line-height: 40px;
    margin: 0;
    min-width: 45px;
  }

  .ant-pagination.mini .ant-pagination-next {
    border-radius: 0px 4px 4px 0px;
    border-left: none !important;
    height: 45px;
    line-height: 40px;
    margin: 0;
    min-width: 45px;
  }

  .ant-pagination-next {
    border: solid 1px ${Base.CinzaDesabilitado} !important;
  }

  .ant-pagination-item {
    border: solid 1px ${Base.CinzaDesabilitado} !important;
  }

  .ant-pagination-item-active {
    background: ${Base.Roxo} !important;
    border-color: ${Base.Roxo} !important;
    color: ${Base.Branco} !important;
    font-size: 12px;
  }

  .ant-pagination-item-active:focus,
  .ant-pagination-item-active:hover {
    border-color: ${Base.Roxo} !important;
  }

  .ant-pagination-item-active:focus a,
  .ant-pagination-item-active:hover a {
    color: ${Base.Branco} !important;
  }

  .ant-table-tbody tr td span.cor-vermelho {
    color: ${Base.Vermelho};
  }

  .ant-table-tbody tr:hover td {
    background: ${Base.Roxo} !important;
    color: ${Base.Branco} !important;
    span.cor-vermelho {
      color: ${Base.Branco} !important;
    }
  }

  .ant-table-tbody tr:hover {
    background: ${Base.Roxo} !important;
    color: ${Base.Branco} !important;
  }

  .ant-table-tbody tr.ant-table-row-selected > td {
    background: ${Base.Roxo};
    color: ${Base.Branco} !important;
  }

  .ant-table-tbody > tr {
    -webkit-transition: none;
    transition: none;

    td {
      -webkit-transition: none;
      transition: none;
    }
  }
`;
