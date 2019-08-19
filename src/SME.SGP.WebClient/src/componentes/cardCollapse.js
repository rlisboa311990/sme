import React from 'react';
import styled from 'styled-components';
import PropTypes from 'prop-types';
import CardHeader from './cardHeader';
import CardBody from './cardBody';
import { Base } from './colors';

const CardCollapse = props => {
  const { titulo, indice, children, show } = props;

  const Card = styled.div`
    border-color: ${Base.CinzaDesabilitado} !important;

    &:last-child {
      margin-bottom: 0;
    }
  `;

  return (
    <Card className="card shadow-sm mb-3">
      <CardHeader indice={indice} border icon>
        {titulo}
      </CardHeader>
      <div className={`collapse fade ${show ? 'show' : null}`} id={`${indice}`}>
        <CardBody>{children}</CardBody>
      </div>
    </Card>
  );
};

CardCollapse.propTypes = {
  titulo: PropTypes.string,
  indice: PropTypes.string,
  children: PropTypes.node,
  show: PropTypes.bool,
};

CardCollapse.defaultProps = {
  show: false,
};

export default CardCollapse;
