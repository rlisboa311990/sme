import React, { useState, useEffect } from 'react';
import styled from 'styled-components';
import { useSelector } from 'react-redux';
import Card from '~/componentes/card';
import Grid from '~/componentes/grid';
import Calendario from '~/componentes-sgp/Calendario/Calendario';
import { Base, Colors } from '~/componentes/colors';
import SelectComponent from '~/componentes/select';
import api from '~/servicos/api';
import Button from '~/componentes/button';
import history from '~/servicos/history';

const Div = styled.div``;
const Titulo = styled(Div)`
  color: ${Base.CinzaMako};
  font-size: 24px;
`;
const Icone = styled.i``;

const CalendarioEscolar = () => {
  const [tiposCalendario, setTiposCalendario] = useState([]);
  const [tipoCalendarioSelecionado, setTipoCalendarioSelecionado] = useState();

  const [diasLetivos, setDiasLetivos] = useState({});

  useEffect(() => {
    const tiposCalendarioLista = [];
    api.get('v1/calendarios/tipos').then(resposta => {
      if (resposta.data) {
        resposta.data.forEach(tipo => {
          tiposCalendarioLista.push({ desc: tipo.nome, valor: tipo.id });
        });
        setTiposCalendario(tiposCalendarioLista);
      }
    });
  }, []);

  useEffect(() => {
    if (tipoCalendarioSelecionado)
      api
        .get(
          `https://demo0765509.mockable.io/api/v1/calendarios/${tipoCalendarioSelecionado}/dias-letivos`
        )
        .then(resposta => {
          if (resposta.data) setDiasLetivos(resposta.data);
        })
        .catch(() => {
          setDiasLetivos({});
        });
  }, [tipoCalendarioSelecionado]);

  const aoSelecionarTipoCalendario = tipo => {
    setTipoCalendarioSelecionado(tipo);
  };

  const aoClicarBotaoVoltar = () => {
    history.push('/');
  };

  const [eventoSme, setEventoSme] = useState(true);

  const aoClicarEventoSme = () => {
    setEventoSme(!eventoSme);
  };

  const dresStore = useSelector(state => state.filtro.dres);
  const [dres, setDres] = useState([]);
  const [dreSelecionada, setDreSelecionada] = useState();

  useEffect(() => {
    if (dresStore) setDres(dresStore);
  }, [dresStore]);

  const unidadesEscolaresStore = useSelector(
    state => state.filtro.unidadesEscolares
  );
  const [unidadesEscolares, setUnidadesEscolares] = useState([]);
  const [unidadeEscolarSelecionada, setUnidadeEscolarSelecionada] = useState();

  useEffect(() => {
    if (unidadesEscolaresStore) setUnidadesEscolares(unidadesEscolaresStore);
  }, [unidadesEscolaresStore]);

  const aoSelecionarDre = dre => {
    setDreSelecionada(dre);
  };

  const aoSelecionarUnidadeEscolar = unidade => {
    setUnidadeEscolarSelecionada(unidade);
  };

  const [filtros, setFiltros] = useState({});

  const aoClicarBotaoFiltro = () => {
    setFiltros({
      tipoCalendarioSelecionado,
      eventoSme,
      dreSelecionada,
      unidadeEscolarSelecionada,
    });
  };

  return (
    <Div className="col-12">
      <Grid cols={12} className="mb-1 p-0">
        <Titulo className="font-weight-bold">
          Consulta de calendário escolar
        </Titulo>
      </Grid>
      <Card className="rounded mb-4">
        <Grid cols={12} className="mb-4">
          <Div className="row">
            <Grid cols={4}>
              <SelectComponent
                className="fonte-14"
                onChange={aoSelecionarTipoCalendario}
                lista={tiposCalendario}
                valueOption="valor"
                valueText="desc"
                valueSelect={tipoCalendarioSelecionado}
                placeholder="Tipo de Calendário"
              />
            </Grid>
            <Grid cols={4}>
              {diasLetivos && diasLetivos.DiasLetivos && (
                <Div>
                  <Button
                    label={diasLetivos.DiasLetivos.toString()}
                    color={
                      diasLetivos.EstaAbaixoPermitido
                        ? Colors.Vermelho
                        : Colors.Verde
                    }
                    className="float-left"
                  />
                  <Div className="float-left w-50 ml-2 mt-1">
                    Nº de Dias Letivos no Calendário
                  </Div>
                </Div>
              )}
              {diasLetivos && diasLetivos.EstaAbaixoPermitido && (
                <Div
                  className="clearfix font-weight-bold pt-2"
                  style={{ clear: 'both', color: Base.Vermelho, fontSize: 12 }}
                >
                  <Icone className="fa fa-exclamation-triangle mr-2" />
                  Abaixo do mínimo estabelecido pela legislação.
                </Div>
              )}
            </Grid>
            <Grid cols={4}>
              <Button
                label="Voltar"
                icon="arrow-left"
                color={Colors.Azul}
                onClick={aoClicarBotaoVoltar}
                border
                className="ml-auto"
              />
            </Grid>
          </Div>
        </Grid>
        <Grid cols={12} className="mb-4">
          <Div className="row">
            <Div className="col" style={{ maxWidth: 90 }}>
              <Button
                label="SME"
                color={eventoSme ? Colors.Verde : Colors.Vermelho}
                onClick={aoClicarEventoSme}
              />
            </Div>
            <Div className="col" style={{ maxWidth: 600 }}>
              <SelectComponent
                className="fonte-14"
                onChange={aoSelecionarDre}
                lista={dres}
                valueOption="valor"
                valueText="desc"
                valueSelect={dreSelecionada}
                placeholder="Diretoria Regional de Educação (DRE)"
              />
            </Div>
            <Div className="col" style={{ maxWidth: 300 }}>
              <SelectComponent
                className="fonte-14"
                onChange={aoSelecionarUnidadeEscolar}
                lista={unidadesEscolares}
                valueOption="valor"
                valueText="desc"
                valueSelect={unidadeEscolarSelecionada}
                placeholder="Unidade Escolar (UE)"
              />
            </Div>
            <Div className="col w-100">
              <Button
                label="Aplicar filtro"
                color={Colors.Roxo}
                className="ml-auto"
                onClick={aoClicarBotaoFiltro}
              />
            </Div>
          </Div>
        </Grid>
        <Grid cols={12}>
          <Calendario filtros={filtros} />
        </Grid>
      </Card>
    </Div>
  );
};

export default CalendarioEscolar;
