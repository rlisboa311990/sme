import produce from 'immer';

const rotas = new Map();
const inicial = {
  retraido: false,
  rotaAtiva: '/',
  rotas,
};

export default function navegacao(state = inicial, action) {
  return produce(state, draft => {
    switch (action.type) {
      case '@navegacao/retraido':
        draft.retraido = action.payload;
        break;
      case '@navegacao/rotaAtiva':
        draft.rotaAtiva = action.payload;
        break;
      case '@navegacao/rotas':
        draft.rotas.set(action.payload.path, action.payload);
        break;
      default:
        break;
    }
  });
}
