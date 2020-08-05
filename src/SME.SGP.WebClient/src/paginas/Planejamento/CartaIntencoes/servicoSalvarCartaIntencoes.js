import { store } from '~/redux';
import {
  limparDadosParaSalvarCartaIntencoes,
  setCartaIntencoesEmEdicao,
  setErrosCartaIntencoes,
  setExibirModalErrosCartaIntencoes,
  limparDadosCartaIntencoes,
} from '~/redux/modulos/cartaIntencoes/actions';
import { confirmar, erros, sucesso } from '~/servicos/alertas';
import ServicoCartaIntencoes from '~/servicos/Paginas/CartaIntencoes/ServicoCartaIntencoes';

class ServicoSalvarCartaIntencoes {
  perguntaDescartarRegistros = async () => {
    const { dispatch } = store;
    const state = store.getState();

    const { cartaIntencoes } = state;

    const { cartaIntencoesEmEdicao } = cartaIntencoes;

    let continuar = true;
    if (cartaIntencoesEmEdicao) {
      const descartar = await confirmar(
        'Atenção',
        '',
        'Os registros deste aluno ainda não foram salvos, deseja descartar os registros?'
      );
      if (descartar) {
        dispatch(limparDadosCartaIntencoes());
        continuar = true;
      } else {
        continuar = false;
      }
    }

    return continuar;
  };

  campoInvalido = valorEditado => {
    const state = store.getState();
    const { cartaIntencoes } = state;
    const { cartaIntencoesEmEdicao } = cartaIntencoes;

    let ehInvalido = false;
    if (cartaIntencoesEmEdicao) {
      if (!valorEditado) {
        ehInvalido = true;
      }
    }

    return ehInvalido;
  };

  validarSalvarCartaIntencoes = async () => {
    const { dispatch } = store;
    const state = store.getState();

    const { cartaIntencoes } = state;

    const {
      cartaIntencoesEmEdicao,
      dadosParaSalvarCartaIntencoes,
      desabilitarCampos,
    } = cartaIntencoes;

    const todosCamposValidos = () => {
      const camposInvalidos = [];

      dadosParaSalvarCartaIntencoes.forEach(bimestreAlterado => {
        if (bimestreAlterado && !bimestreAlterado.descricao) {
          const msg = `A descrição da carta de intensões do ${bimestreAlterado.bimestre}º bimestre deve ser informada`;
          camposInvalidos.push(msg);
        }
      });

      if (camposInvalidos.length) {
        dispatch(setErrosCartaIntencoes(camposInvalidos));
        dispatch(setExibirModalErrosCartaIntencoes(true));
        return false;
      }
      return true;
    };

    const salvar = async () => {
      const retorno = await ServicoCartaIntencoes.salvarCartaIntencoes(
        dadosParaSalvarCartaIntencoes
      ).catch(e => erros(e));

      if (retorno && retorno.status === 200) {
        dispatch(setCartaIntencoesEmEdicao(false));
        dispatch(limparDadosParaSalvarCartaIntencoes());
        sucesso('Suas informações foram salvas com sucesso.');

        return true;
      }
      return false;
    };

    if (desabilitarCampos) {
      return true;
    }

    if (cartaIntencoesEmEdicao) {
      // Voltar para a tela e não executa a ação!
      const temRegistrosInvalidos = !todosCamposValidos();
      if (temRegistrosInvalidos) {
        return false;
      }

      // Tenta salvar os registros se estão válidos e continuar para executação a ação!
      return salvar();
    }
    return true;
  };
}

export default new ServicoSalvarCartaIntencoes();
