import React, { useEffect } from 'react';
import { useSelector, useDispatch } from 'react-redux';
import {
  Salvar,
  PrePost,
  Post,
} from '../../../redux/modulos/planoAnual/action';
import Grid from '../../../componentes/grid';
import Button from '../../../componentes/button';
import { Colors, Base } from '../../../componentes/colors';
import _ from 'lodash';
import Card from '../../../componentes/card';
import Bimestre from './bimestre';
import { confirmacao } from '../../../servicos/alertas';
import Service from '../../../servicos/Paginas/PlanoAnualServices';
import Alert from '../../../componentes/alert';
import ModalMultiLinhas from '../../../componentes/modalMultiLinhas';

export default function PlanoAnual() {
  const qtdBimestres = 4;
  const anoLetivo = 2019;
  const escolaId = 1;
  const anoEscolar = 1;
  const RF = 6082840;
  const turmaId = 1982186;

  const bimestres = useSelector(store => store.bimestres.bimestres);
  const notificacoes = useSelector(state => state.notificacoes);
  const bimestresErro = useSelector(store => store.bimestres.bimestresErro);
  const ehEdicao = bimestres.filter(x => x.ehEdicao).length > 0;
  const dispatch = useDispatch();

  useEffect(() => {
    if (!bimestres || bimestres.length === 0)
      Service.getMateriasProfessor(RF, turmaId).then(res => {
        ObtenhaBimestres(_.cloneDeep(res));
      });
  }, []);

  useEffect(() => {
    VerificarEnvio();
  }, [bimestres]);

  const ehEja = false;

  const VerificarEnvio = () => {
    const paraEnviar = bimestres.map(x => x.paraEnviar).filter(x => x);

    if (paraEnviar && paraEnviar.length > 0) dispatch(Post(bimestres));
  };

  const ObtenhaNomebimestre = index =>
    `${index}º ${ehEja ? 'Semestre' : 'Bimestre'}`;

  const confirmarCancelamento = () => {};

  const onClickSalvar = () => {
    dispatch(PrePost());
  };

  const ObtenhaBimestres = materias => {
    for (let i = 1; i <= qtdBimestres; i++) {
      const Nome = ObtenhaNomebimestre(i);

      const objetivo = '';

      const bimestre = {
        anoLetivo,
        anoEscolar,
        escolaId,
        turmaId,
        ehExpandido: false,
        indice: i,
        nome: Nome,
        materias: _.cloneDeep(materias),
        objetivo: objetivo,
        paraEnviar: false,
      };

      dispatch(Salvar(i, bimestre));
    }
  };

  const cancelarAlteracoes = () => {
    confirmacao(
      'Atenção',
      `Você não salvou as informações
    preenchidas. Deseja realmente cancelar as alterações?`,
      confirmarCancelamento,
      () => true
    );
  };
  return (
    <>
      <div className="col-md-12">
        {notificacoes.alertas.map(alerta => (
          <Alert alerta={alerta} key={alerta.id} />
        ))}
      </div>
      <ModalMultiLinhas
        key="errosBimestre"
        visivel={bimestresErro.visible}
        onClose={bimestresErro.onClose}
        type={bimestresErro.type}
        conteudo={bimestresErro.content}
        titulo={bimestresErro.title}
      />
      <Card>
        <Grid cols={12}>
          <h1>Plano Anual</h1>
        </Grid>
        <Grid cols={6} className="d-flex justify-content-start mb-3">
          <Button
            label="Migrar Conteúdo"
            icon="share-square"
            color={Colors.Azul}
            border
            disabled
          />
        </Grid>
        <Grid cols={6} className="d-flex justify-content-end mb-3">
          <Button
            label="Voltar"
            icon="arrow-left"
            color={Colors.Azul}
            border
            className="mr-3"
          />
          <Button
            label="Cancelar"
            color={Colors.Roxo}
            border
            bold
            className="mr-3"
            onClick={cancelarAlteracoes}
          />
          <Button
            label="Salvar"
            color={Colors.Roxo}
            onClick={onClickSalvar}
            disabled={!ehEdicao}
            border
            bold
          />
        </Grid>
        <Grid cols={12}>
          {bimestres
            ? bimestres.map(bim => {
                return <Bimestre key={bim.indice} indice={bim.indice} />;
              })
            : null}
        </Grid>
      </Card>
    </>
  );
}
