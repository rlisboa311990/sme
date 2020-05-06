import React, { useEffect, useState, useCallback, useReducer } from 'react';

// Redux
import { useSelector } from 'react-redux';

// Componentes
import { Loader, ButtonGroup, Card, Grid } from '~/componentes';

// Componentes Internos
import DropDownTipoCalendario from './componentes/DropDownTipoCalendario';

// Estilos
import { Linha } from '~/componentes/EstilosGlobais';

// Componentes SGP
import {
  Cabecalho,
  AlertaSelecionarTurma,
  Calendario,
} from '~/componentes-sgp';

// Serviços
import CalendarioProfessorServico from '~/servicos/Paginas/CalendarioProfessor';
import { erro } from '~/servicos/alertas';

// Reducer
import Reducer, { estadoInicial } from './reducer';
import {
  setarEventosMes,
  setarEventosDia,
  setarCarregandoCalendario,
  setarCarregandoMes,
  setarCarregandoDia,
} from './reducer/actions';

// DTOs
import RotasDTO from '~/dtos/rotasDto';

function CalendarioProfessor() {
  const { turmaSelecionada, permissoes } = useSelector(
    estado => estado.usuario
  );

  const permissaoTela = permissoes[RotasDTO.CALENDARIO_PROFESSOR];

  const [estado, disparar] = useReducer(Reducer, estadoInicial);
  const [tipoCalendarioId, setTipoCalendarioId] = useState(undefined);

  const onClickMesHandler = useCallback(
    mes => {
      async function buscarEventosMes() {
        try {
          disparar(setarCarregandoMes(true));
          const {
            data,
            status,
          } = await CalendarioProfessorServico.buscarEventosAulasMes({
            numeroMes: mes.numeroMes,
            dre: turmaSelecionada.dre,
            anoLetivo: turmaSelecionada.anoLetivo,
            tipoCalendarioId,
            turma: turmaSelecionada.turma,
            ue: turmaSelecionada.unidadeEscolar,
          });

          if (data && status === 200) {
            disparar(setarCarregandoMes(false));
            disparar(
              setarEventosMes({
                ...mes,
                dias: data,
              })
            );
          } else if (status === 204) {
            disparar(setarCarregandoMes(false));
          }
        } catch (error) {
          disparar(setarCarregandoMes(false));
          erro('Não foi possível buscar dados do mês.');
        }
      }
      buscarEventosMes();
    },
    [
      tipoCalendarioId,
      turmaSelecionada.anoLetivo,
      turmaSelecionada.dre,
      turmaSelecionada.turma,
      turmaSelecionada.unidadeEscolar,
    ]
  );

  const onClickDiaHandler = useCallback(
    dia => {
      async function buscarEventosDias() {
        try {
          disparar(setarCarregandoDia(true));
          const {
            data,
            status,
          } = await CalendarioProfessorServico.buscarEventosAulasDia({
            dia: dia.getDate(),
            numeroMes: dia.getMonth() + 1,
            tipoCalendarioId,
            dre: turmaSelecionada.dre,
            ue: turmaSelecionada.unidadeEscolar,
            turma: turmaSelecionada.turma,
            anoLetivo: turmaSelecionada.anoLetivo,
          });

          if (data && status === 200) {
            disparar(setarCarregandoDia(false));
            disparar(
              setarEventosDia({
                diaSelecionado: dia,
                dados: data,
              })
            );
          } else if (status === 204) {
            disparar(setarCarregandoDia(false));
          }
        } catch (error) {
          disparar(setarCarregandoDia(false));
          erro('Não foi possível buscar dados do dia.');
        }
      }
      buscarEventosDias();
    },
    [
      tipoCalendarioId,
      turmaSelecionada.anoLetivo,
      turmaSelecionada.dre,
      turmaSelecionada.turma,
      turmaSelecionada.unidadeEscolar,
    ]
  );

  const onChangeTipoCalendarioIdHandler = useCallback(valor => {
    setTipoCalendarioId(valor);
  }, []);

  useEffect(() => {
    console.log(estado);
  }, [estado]);

  return (
    <>
      <AlertaSelecionarTurma />
      <Cabecalho pagina="Calendário do professor" />
      <Loader loading={false}>
        <Card>
          <ButtonGroup />
          <Grid cols={4} className="p-0 m-0">
            <DropDownTipoCalendario
              turmaSelecionada={turmaSelecionada.turma}
              onChange={valor => onChangeTipoCalendarioIdHandler(valor)}
              valor={tipoCalendarioId}
            />
          </Grid>
          <Grid cols={12}>
            <Linha className="row pt-2">
              <Grid cols={12}>
                <Calendario
                  eventos={estado.eventos}
                  onClickMes={mes => onClickMesHandler(mes)}
                  onClickDia={dia => onClickDiaHandler(dia)}
                  carregandoCalendario={estado.carregandoCalendario}
                  carregandoMes={estado.carregandoMes}
                  carregandoDia={estado.carregandoDia}
                  permissaoTela={permissaoTela}
                  tipoCalendarioId={tipoCalendarioId}
                />
              </Grid>
            </Linha>
          </Grid>
        </Card>
      </Loader>
    </>
  );
}

CalendarioProfessor.propTypes = {};

CalendarioProfessor.defaultProps = {};

export default CalendarioProfessor;
