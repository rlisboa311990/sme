import { combineReducers } from 'redux';

import navegacao from './navegacao/reducers';
import alertas from './alertas/reducers';
import usuario from './usuario/reducers';
import notificacoes from './notificacoes/reducers';
import perfil from './perfil/reducers';
import calendarioEscolar from './calendarioEscolar/reducers';
import calendarioProfessor from './calendarioProfessor/reducers';
import bimestres from './planoAnual/reducers';
import filtro from './filtro/reducers';
import atribuicaoEsporadica from './atribuicaoEsporadica/reducers';
import loader from './loader/reducer';
import notasConceitos from './notasConceitos/reducer';

export default combineReducers({
  navegacao,
  alertas,
  usuario,
  perfil,
  calendarioEscolar,
  notificacoes,
  bimestres,
  filtro,
  calendarioProfessor,
  atribuicaoEsporadica,
  loader,
  notasConceitos,
});
