import produce from 'immer';

const INICIAL = {
  bimestres: [],
  bimestresErro: {
    type: '',
    content: [],
    title: '',
    onClose: null,
    visible: false,
  },
};

export default function bimestres(state = INICIAL, action) {
  return produce(state, draft => {
    switch (action.type) {
      case '@bimestres/SalvarBimestre':
        draft.bimestres[action.payload.indice] = action.payload.bimestre;
        draft.bimestresErro = state.bimestresErro;

        break;
      case '@bimestres/PrePostBimestre':
        const paraEnvio = state.bimestres.filter(x => x.ehExpandido);
        paraEnvio.forEach(elem => {
          draft.bimestres[elem.indice].objetivo = state.bimestres[
            elem.indice
          ].setarObjetivo();
          draft.bimestres[elem.indice].paraEnviar = true;
        });
        draft.bimestresErro = state.bimestresErro;

        break;
      case '@bimestres/PrePostBimestre':
        draft.bimestres = state.bimestres.map(bimestre => {
          bimestre.paraEnviar = false;
          return bimestre;
        });
        draft.bimestresErro = state.bimestresErro;

        break;
      case '@bimestres/SalvarMateria':
        draft.bimestres[action.payload.indice].materias =
          action.payload.materias;
        draft.bimestresErro = state.bimestresErro;

        break;

      case '@bimestres/SalvarEhExpandido':
        draft.bimestres[action.payload.indice].ehExpandido =
          action.payload.ehExpandido;
        draft.bimestres[action.payload.indice].ehEdicao =
          action.payload.ehExpandido;
        draft.bimestresErro = state.bimestresErro;

        break;

      case '@bimestres/SelecionarMateria':
        draft.bimestres[action.payload.indice].materias[
          action.payload.indiceMateria
        ].selected = action.payload.selecionarMateria;
        draft.bimestresErro = state.bimestresErro;

        if (state.bimestres[action.payload.indice])
          draft.bimestres[action.payload.indice].objetivo = state.bimestres[
            action.payload.indice
          ].setarObjetivo();

        break;

      case '@bimestres/SalvarObjetivos':
        draft.bimestres[action.payload.indice].objetivosAprendizagem =
          action.payload.objetivos;
        draft.bimestresErro = state.bimestresErro;

        break;

      case '@bimestres/SetarDescricaoFunction':
        draft.bimestres[action.payload.indice].setarObjetivo =
          action.payload.setarObjetivo;
        draft.bimestresErro = state.bimestresErro;

        break;

      case '@bimestres/SelecionarObjetivo':
        draft.bimestres[action.payload.indice].objetivosAprendizagem[
          action.payload.indiceObjetivo
        ].selected = action.payload.selecionarObjetivo;
        draft.bimestresErro = state.bimestresErro;

        if (state.bimestres[action.payload.indice])
          draft.bimestres[action.payload.indice].objetivo = state.bimestres[
            action.payload.indice
          ].setarObjetivo();

        break;

      case '@bimestres/DefinirObjetivoFocado':
        draft.bimestres[action.payload.indice].objetivoIdFocado =
          action.payload.codigoObjetivo;
        draft.bimestresErro = state.bimestresErro;

        break;

      case '@bimestres/SetarDescricao':
        draft.bimestres[action.payload.indice].objetivo =
          action.payload.descricao;
        draft.bimestresErro = state.bimestresErro;

        break;

      case '@bimestres/BimestresErro':
        draft.bimestres = state.bimestres;
        draft.bimestresErro = action.payload;

        break;

      case '@bimestres/LimparBimestresErro':
        draft.bimestres = state.bimestres;
        draft.bimestresErro = action.payload;

        break;

      case '@bimestres/PosPostBimestre':
        draft.bimestres = draft.bimestres.map(x => {
          x.paraEnviar = false;
          return x;
        });
        draft.bimestresErro = state.bimestresErro;

        break;
      case '@bimestres/setEdicaoFalse':
        draft.bimestres = draft.bimestres.map(x => {
          x.ehEdicao = false;
          return x;
        });
        break;
      default:
        break;
    }
  });
}
