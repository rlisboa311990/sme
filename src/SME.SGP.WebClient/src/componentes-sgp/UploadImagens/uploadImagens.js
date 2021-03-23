import { PlusOutlined } from '@ant-design/icons';
import { Modal, Upload } from 'antd';
import PropTypes from 'prop-types';
import React, { useEffect, useState } from 'react';
import styled from 'styled-components';
import { Base } from '~/componentes/colors';
import Loader from '~/componentes/loader';
import { confirmar, erro, erros } from '~/servicos';
import ServicoArmazenamento from '~/servicos/Componentes/ServicoArmazenamento';
import { permiteInserirFormato } from '~/utils/funcoes/gerais';

function getBase64DataURL(file, type) {
  return new Promise((resolve, reject) => {
    const reader = new FileReader();
    const fileBlob = new Blob([file], { type });
    reader.readAsDataURL(fileBlob);
    reader.onload = () => resolve(reader.result);
    reader.onerror = error => reject(error);
  });
}

export const ContainerUpload = styled(Upload)`
  .ant-upload-select-picture-card {
    opacity: ${props => (props.desabilitarUpload ? 0.8 : 1)} !important;
    cursor: ${props =>
      props.desabilitarUpload ? 'not-allowed' : 'pointer'} !important;

    .ant-upload {
      pointer-events: ${props =>
        props.desabilitarUpload ? 'none' : 'auto'} !important;
    }
  }

  .ant-upload-list-picture .ant-upload-list-item-thumbnail,
  .ant-upload-list-picture-card .ant-upload-list-item-thumbnail {
    opacity: 1;
  }

  .tamanho-icone-plus {
    font-size: 20px;
  }
`;

const UploadImagens = props => {
  const {
    servicoCustomRequest,
    afterSuccessUpload,
    parametrosCustomRequest,
    removerImagem,
    listaInicialImagens,
    desabilitar,
    quantidadeMaxima,
    tiposArquivosPermitidos,
  } = props;

  const [listaImagens, setListaImagens] = useState([]);

  const [exibirLoader, setExibirLoader] = useState(false);

  const TAMANHO_MAXIMO_UPLOAD = 5;

  const CONFIG_PADRAO_MODAL = {
    previewVisible: false,
    previewImage: '',
    previewTitle: '',
  };

  const [configModal, setConfigModal] = useState(CONFIG_PADRAO_MODAL);

  const handleCancel = () => setConfigModal(CONFIG_PADRAO_MODAL);

  const handlePreview = async dados => {
    if (dados.uid) {
      setExibirLoader(true);
      const resposta = await ServicoArmazenamento.obterArquivoParaDownload(
        dados.uid
      )
        .catch(e => erros(e))
        .finally(() => setExibirLoader(false));

      if (resposta?.data) {
        const type = resposta.headers['content-type'];
        const urlImagem = await getBase64DataURL(resposta?.data, type || '');

        setConfigModal({
          previewImage: urlImagem,
          previewVisible: true,
          previewTitle: dados.name,
        });
      }
    }
  };

  const montarListaImagensParaExibir = imagens => {
    return imagens.map(item => {
      if (!item.url && item.type && item.fileBase64) {
        item.url = `data:${item.type};base64,${item.fileBase64}`;
      }

      return { ...item, status: 'done' };
    });
  };

  useEffect(() => {
    if (listaInicialImagens?.length) {
      setListaImagens(montarListaImagensParaExibir(listaInicialImagens));
    } else {
      setListaImagens([]);
    }
  }, [listaInicialImagens]);

  const customRequest = options => {
    const { onSuccess, onError, file, onProgress } = options;

    const quantdadeAtualImagens = listaImagens?.length;
    if (quantdadeAtualImagens < quantidadeMaxima && servicoCustomRequest) {
      const fmData = new FormData();
      fmData.append('file', file);

      if (parametrosCustomRequest?.length) {
        parametrosCustomRequest.forEach(item => {
          fmData.append(item.nome, item.valor);
        });
      }

      const config = {
        headers: { 'content-type': 'multipart/form-data' },
        onUploadProgress: event => {
          onProgress({ percent: (event.loaded / event.total) * 100 }, file);
        },
      };

      servicoCustomRequest(fmData, config)
        .then(resposta => {
          onSuccess(file, resposta.data);
          if (afterSuccessUpload) {
            afterSuccessUpload(resposta.data);
          }
        })
        .catch(e => {
          onError({ event: e });
          erros(e);
        });
    } else {
      // TODO
    }
  };

  const onRemove = async dados => {
    if (removerImagem) {
      const confirmado = await confirmar(
        'Excluir',
        '',
        'Deseja realmente excluir esta imagem?'
      );
      if (confirmado) {
        removerImagem(dados?.uid);
      }
    } else {
      // TODO
    }
  };

  const excedeuLimiteMaximo = arquivo => {
    const tamanhoArquivo = arquivo.size / 1024 / 1024;
    return tamanhoArquivo > TAMANHO_MAXIMO_UPLOAD;
  };

  const beforeUpload = arquivo => {
    if (!permiteInserirFormato(arquivo, tiposArquivosPermitidos)) {
      erro('Formato não permitido');
      return false;
    }

    if (excedeuLimiteMaximo(arquivo)) {
      erro('Tamanho máximo 5 MB');
      return false;
    }

    return true;
  };

  return (
    <Loader loading={exibirLoader}>
      <ContainerUpload
        listType="picture-card"
        fileList={listaImagens}
        onPreview={handlePreview}
        customRequest={customRequest}
        onRemove={onRemove}
        disabled={desabilitar}
        accept={tiposArquivosPermitidos}
        desabilitarUpload={listaImagens?.length >= quantidadeMaxima}
        beforeUpload={beforeUpload}
      >
        <div>
          <PlusOutlined
            style={{ color: Base.Roxo }}
            className="tamanho-icone-plus"
          />
          <div style={{ marginTop: 8 }}>Carregar</div>
        </div>
      </ContainerUpload>
      <Modal
        visible={configModal?.previewVisible}
        title={configModal?.previewTitle}
        onCancel={handleCancel}
        footer={null}
      >
        <img
          alt={configModal?.previewTitle}
          style={{ width: '100%' }}
          src={configModal?.previewImage}
        />
      </Modal>
    </Loader>
  );
};

UploadImagens.propTypes = {
  servicoCustomRequest: PropTypes.func,
  parametrosCustomRequest: PropTypes.oneOfType([PropTypes.array]),
  afterSuccessUpload: PropTypes.func,
  removerImagem: PropTypes.func,
  listaInicialImagens: PropTypes.oneOfType([PropTypes.array]),
  desabilitar: PropTypes.bool,
  quantidadeMaxima: PropTypes.number,
  tiposArquivosPermitidos: PropTypes.string,
};

UploadImagens.defaultProps = {
  servicoCustomRequest: null,
  parametrosCustomRequest: [],
  afterSuccessUpload: null,
  removerImagem: null,
  listaInicialImagens: [],
  desabilitar: false,
  quantidadeMaxima: 3,
  tiposArquivosPermitidos: '.jpg, .jpeg, .png',
};

export default UploadImagens;
