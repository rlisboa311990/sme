import React, { useEffect, useState } from 'react';
import { useSelector, useDispatch } from 'react-redux';
import { Modal, Row } from 'antd';
import styled from 'styled-components';
import { alertaFechar } from '../redux/modulos/alertas/actions';
import Rotas from '../rotas/rotas';
import Button from '../componentes/button';
import { Colors } from '../componentes/colors';
import BreadcrumbSgp from '../componentes-sgp/breadcrumb-sgp';
import Alert from '~/componentes/alert';
import Grid from '~/componentes/grid';
import shortid from 'shortid';

const ContainerModal = styled.div`
  .ant-modal-footer {
    border-top: 0px !important;
  }
`;

const ContainerBotoes = styled.div`
  display: flex;
  justify-content: flex-end;
`;

const Conteudo = () => {
  const NavegacaoStore = useSelector(store => store.navegacao);
  const [retraido, setRetraido] = useState(false);
  const dispatch = useDispatch();
  useEffect(() => {
    setRetraido(NavegacaoStore.retraido);
  }, [NavegacaoStore.retraido]);

  const confirmacao = useSelector(state => state.notificacoes.confirmacao);

  useEffect(() => {
    setRetraido(NavegacaoStore.retraido);
  }, [NavegacaoStore.retraido]);

  const fecharConfirmacao = resultado => {
    confirmacao.resolve(resultado);
    dispatch(alertaFechar());
  };
  const notificacoes = useSelector(state => state.notificacoes);
  return (
    <div style={{ marginLeft: retraido ? '115px' : '250px' }}>
      <BreadcrumbSgp />
      <div className="row h-100">
        <main role="main" className="col-md-12 col-lg-12 col-sm-12 col-xl-12">
          <ContainerModal>
            <Modal
              title={confirmacao.titulo}
              visible={confirmacao.visivel}
              onOk={() => fecharConfirmacao(true)}
              onCancel={() => fecharConfirmacao(false)}
              footer={[
                <ContainerBotoes key={shortid.generate()}>
                  <Button
                    key={shortid.generate()}
                    onClick={() => fecharConfirmacao(true)}
                    label={confirmacao.textoOk}
                    color={Colors.Azul}
                    border
                  />
                  <Button
                    key={shortid.generate()}
                    onClick={() => fecharConfirmacao(false)}
                    label={confirmacao.textoCancelar}
                    type="primary"
                    color={Colors.Azul}
                  />
                </ContainerBotoes>,
              ]}
            >
              {confirmacao.texto}
              {confirmacao.texto ? <br /> : ''}
              <b>{confirmacao.textoNegrito}</b>
            </Modal>
          </ContainerModal>
          <div className="card-body m-r-0 m-l-0 p-l-0 p-r-0 m-t-0">
            {notificacoes.alertas.map(alerta => (
              <Row>
                <Grid cols={12}>
                  <Alert alerta={alerta} key={alerta.id} closable />
                </Grid>
              </Row>
            ))}
            <Rotas />
          </div>
        </main>
      </div>
    </div>
  );
};

export default Conteudo;
