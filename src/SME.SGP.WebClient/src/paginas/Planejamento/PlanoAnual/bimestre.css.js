import styled from 'styled-components';
import { Base } from '../../../componentes/colors';

export const Badge = styled.button`
  &:last-child {
    margin-right: 10 !important;
  }

  &.selecionada {
    background: ${Base.CinzaBadge} !important;
    border-color: ${Base.CinzaBadge} !important;
  }
`;

export const ListaObjetivos = styled.div`
  max-height: 300px !important;
`;

export const ListItem = styled.li`
  border-color: ${Base.AzulAnakiwa} !important;
`;

export const ListItemButton = styled(ListItem)`
  cursor: pointer;

  &.selecionado {
    background: ${Base.AzulAnakiwa} !important;
  }
`;
