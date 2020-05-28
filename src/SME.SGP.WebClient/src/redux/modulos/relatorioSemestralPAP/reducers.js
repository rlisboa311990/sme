import produce from 'immer';

const inicial = {
  dadosAlunoObjectCard: {},
  alunosRelatorioSemestral: [],
  relatorioSemestralEmEdicao: false,
  dadosRelatorioSemestral: {},
  dadosParaSalvarRelatorioSemestral: [],
  auditoriaRelatorioSemestral: null,
  desabilitarCampos: false,
  dentroPeriodo: true,
  codigoAlunoSelecionado: null,
};

export default function RelatorioSemestralPAP(state = inicial, action) {
  return produce(state, draft => {
    switch (action.type) {
      case '@relatorioSemestralPAP/setDadosAlunoObjectCard': {
        return {
          ...draft,
          dadosAlunoObjectCard: action.payload,
        };
      }
      case '@relatorioSemestralPAP/setAlunosRelatorioSemestral': {
        return {
          ...draft,
          alunosRelatorioSemestral: action.payload,
        };
      }
      case '@relatorioSemestralPAP/limparDadosRelatorioSemestral': {
        return {
          ...draft,
          dadosAlunoObjectCard: {},
          relatorioSemestralEmEdicao: false,
          dadosRelatorioSemestral: {},
          dadosParaSalvarRelatorioSemestral: [],
          auditoriaRelatorioSemestral: null,
          desabilitarCampos: false,
          codigoAlunoSelecionado: null,
        };
      }
      case '@relatorioSemestralPAP/setRelatorioSemestralEmEdicao': {
        return {
          ...draft,
          relatorioSemestralEmEdicao: action.payload,
        };
      }
      case '@relatorioSemestralPAP/setDadosRelatorioSemestral': {
        return {
          ...draft,
          dadosRelatorioSemestral: action.payload,
        };
      }
      case '@relatorioSemestralPAP/setDadosParaSalvarRelatorioSemestral': {
        const dados = state.dadosParaSalvarRelatorioSemestral;
        if (dados.length > 0) {
          const valor = dados.find(item => item.id == action.payload.id);
          if (valor) {
            const indexItem = dados.findIndex(
              item => item.id == action.payload.id
            );
            draft.dadosParaSalvarRelatorioSemestral[indexItem] = action.payload;
          } else {
            draft.dadosParaSalvarRelatorioSemestral.push(action.payload);
          }
        } else {
          draft.dadosParaSalvarRelatorioSemestral.push(action.payload);
        }
        break;
      }
      case '@relatorioSemestralPAP/SetValoresSecaoRelatorioSemestral':
        if (
          draft.dadosParaSalvarRelatorioSemestral == null ||
          draft.dadosParaSalvarRelatorioSemestral.length === 0
        )
          return;

        draft.dadosParaSalvarRelatorioSemestral.forEach(element => {
          let secao = draft.dadosRelatorioSemestral.secoes.find(
            x => x.id === element.id
          );

          if (secao != null) secao.valor = element.valor;
        });

        break;
      case '@relatorioSemestralPAP/limparDadosParaSalvarRelatorioSemestral': {
        return {
          ...draft,
          dadosParaSalvarRelatorioSemestral: [],
        };
      }
      case '@relatorioSemestralPAP/setAuditoriaRelatorioSemestral': {
        return {
          ...draft,
          auditoriaRelatorioSemestral: action.payload,
        };
      }
      case '@relatorioSemestralPAP/setDesabilitarCampos': {
        return {
          ...draft,
          desabilitarCampos: action.payload,
        };
      }
      case '@relatorioSemestralPAP/setDentroPeriodo': {
        return {
          ...draft,
          dentroPeriodo: action.payload,
        };
      }
      case '@relatorioSemestralPAP/setCodigoAlunoSelecionado': {
        return {
          ...draft,
          codigoAlunoSelecionado: action.payload,
        };
      }

      default:
        return draft;
    }
  });
}
