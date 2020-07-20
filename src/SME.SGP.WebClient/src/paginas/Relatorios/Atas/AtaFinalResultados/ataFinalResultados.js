import React, { useCallback, useEffect, useState } from 'react';
import { useSelector } from 'react-redux';
import { SelectComponent } from '~/componentes';
import { Cabecalho } from '~/componentes-sgp';
import Button from '~/componentes/button';
import Card from '~/componentes/card';
import { Colors } from '~/componentes/colors';
import { URL_HOME } from '~/constantes/url';
import modalidade from '~/dtos/modalidade';
import RotasDto from '~/dtos/rotasDto';
import tipoEscolaDTO from '~/dtos/tipoEscolaDto';
import AbrangenciaServico from '~/servicos/Abrangencia';
import { erros, sucesso } from '~/servicos/alertas';
import api from '~/servicos/api';
import history from '~/servicos/history';
import ServicoConselhoAtaFinal from '~/servicos/Paginas/ConselhoAtaFinal/ServicoConselhoAtaFinal';
import FiltroHelper from '~componentes-sgp/filtro/helper';

const AtaFinalResultados = () => {
  const usuarioStore = useSelector(store => store.usuario);
  const permissoesTela = usuarioStore.permissoes[RotasDto.ATA_FINAL_RESULTADOS];

  const [listaAnosLetivo, setListaAnosLetivo] = useState([]);
  const [listaSemestre, setListaSemestre] = useState([]);
  const [listaDres, setListaDres] = useState([]);
  const [listaUes, setListaUes] = useState([]);
  const [listaModalidades, setListaModalidades] = useState([]);
  const [listaTurmas, setListaTurmas] = useState([]);

  const [anoLetivo, setAnoLetivo] = useState(undefined);
  const [dreId, setDreId] = useState(undefined);
  const [ueId, setUeId] = useState(undefined);
  const [modalidadeId, setModalidadeId] = useState(undefined);
  const [semestre, setSemestre] = useState(undefined);
  const [turmaId, setTurmaId] = useState(undefined);
  const [formato, setFormato] = useState('1');

  const [desabilitarBtnGerar, setDesabilitarBtnGerar] = useState(true);

  const listaFormatos = [
    { valor: '1', desc: 'PDF' },
    { valor: '4', desc: 'EXCEL' },
  ];

  const obterAnosLetivos = useCallback(async () => {
    const anosLetivo = await AbrangenciaServico.buscarTodosAnosLetivos().catch(
      e => erros(e)
    );
    if (anosLetivo && anosLetivo.data) {
      const anos = [];
      anosLetivo.data.forEach(ano => {
        anos.push({ desc: ano, valor: ano });
      });
      setAnoLetivo(anos[0].valor);
      setListaAnosLetivo(anos);
    } else {
      setListaAnosLetivo([]);
    }
  }, []);

  const obterModalidades = async (ue, ano) => {
    if (ue && ano) {
      const { data } = await api.get(`/v1/ues/${ue}/modalidades?ano=${ano}`);
      if (data) {
        const lista = data.map(item => ({
          desc: item.nome,
          valor: String(item.id),
        }));

        if (lista && lista.length && lista.length === 1) {
          setModalidadeId(lista[0].valor);
        }
        setListaModalidades(lista);
      }
    }
  };

  const obterUes = useCallback(async dre => {
    if (dre) {
      const { data } = await AbrangenciaServico.buscarUes(dre);
      if (data) {
        const lista = data.map(item => ({
          desc: item.nome,
          valor: String(item.codigo),
        }));

        if (lista && lista.length && lista.length === 1) {
          setUeId(lista[0].valor);
        }

        setListaUes(lista);
      } else {
        setListaUes([]);
      }
    }
  }, []);

  const onChangeDre = dre => {
    setDreId(dre);

    setListaUes([]);
    setUeId(undefined);

    setListaModalidades([]);
    setModalidadeId(undefined);

    setListaSemestre([]);
    setSemestre(undefined);

    setListaTurmas([]);
    setTurmaId(undefined);
  };

  const obterDres = async () => {
    const { data } = await AbrangenciaServico.buscarDres();
    if (data && data.length) {
      const lista = data
        .map(item => ({
          desc: item.nome,
          valor: String(item.codigo),
          abrev: item.abreviacao,
        }))
        .sort(FiltroHelper.ordenarLista('desc'));
      setListaDres(lista);

      if (lista && lista.length && lista.length === 1) {
        setDreId(lista[0].valor);
      }
    } else {
      setListaDres([]);
    }
  };

  const obterTurmas = useCallback(async (modalidadeSelecionada, ue) => {
    if (ue && modalidadeSelecionada) {
      const { data } = await AbrangenciaServico.buscarTurmas(
        ue,
        modalidadeSelecionada
      );
      if (data) {
        const lista = data.map(item => ({
          desc: item.nome,
          valor: item.codigo,
        }));

        const temAbrangenciaTodasTurmas = await AbrangenciaServico.usuarioTemAbrangenciaTodasTurmas().catch(
          e => erros(e)
        );
        if (temAbrangenciaTodasTurmas && temAbrangenciaTodasTurmas.data) {
          lista.unshift({ desc: 'Todas', valor: '-99' });
        }
        setListaTurmas(lista);

        if (lista && lista.length && lista.length === 1) {
          setTurmaId(lista[0].valor);
        }
      }
    }
  }, []);

  const obterSemestres = async (
    modalidadeSelecionada,
    anoLetivoSelecionado
  ) => {
    const retorno = await api.get(
      `v1/abrangencias/false/semestres?anoLetivo=${anoLetivoSelecionado}&modalidade=${modalidadeSelecionada ||
        0}`
    );
    if (retorno && retorno.data) {
      const lista = retorno.data.map(periodo => {
        return { desc: periodo, valor: periodo };
      });

      if (lista && lista.length && lista.length === 1) {
        setSemestre(lista[0].valor);
      }
      setListaSemestre(lista);
    }
  };

  useEffect(() => {
    if (anoLetivo && ueId) {
      obterModalidades(ueId, anoLetivo);
    } else {
      setModalidadeId(undefined);
      setListaModalidades([]);
    }
  }, [anoLetivo, ueId]);

  useEffect(() => {
    if (dreId) {
      obterUes(dreId);
    } else {
      setUeId(undefined);
      setListaUes([]);
    }
  }, [dreId, obterUes]);

  useEffect(() => {
    if (modalidadeId && ueId) {
      obterTurmas(modalidadeId, ueId);
    } else {
      setTurmaId(undefined);
      setListaTurmas([]);
    }
  }, [modalidadeId, ueId, obterTurmas]);

  useEffect(() => {
    if (modalidadeId && anoLetivo) {
      if (modalidadeId == modalidade.EJA) {
        obterSemestres(modalidadeId, anoLetivo);
      } else {
        setSemestre(undefined);
        setListaSemestre([]);
      }
    } else {
      setSemestre(undefined);
      setListaSemestre([]);
    }
  }, [modalidadeId, anoLetivo, obterTurmas]);

  useEffect(() => {
    const desabilitar =
      !anoLetivo || !dreId || !ueId || !modalidadeId || !turmaId || !formato;

    if (modalidadeId == modalidade.EJA) {
      setDesabilitarBtnGerar(!semestre || desabilitar);
    } else {
      setDesabilitarBtnGerar(desabilitar);
    }
  }, [anoLetivo, dreId, ueId, modalidadeId, turmaId, formato, semestre]);

  useEffect(() => {
    obterAnosLetivos();
    obterDres();
  }, [obterAnosLetivos]);

  const onClickVoltar = () => {
    history.push(URL_HOME);
  };

  const onClickCancelar = () => {
    setAnoLetivo(undefined);
    setDreId(undefined);
    setListaAnosLetivo([]);
    setListaDres([]);

    obterAnosLetivos();
    obterDres();

    setFormato('PDF');
  };

  const onClickGerar = async () => {
    if (permissoesTela.podeConsultar) {
      const params = { turmasCodigos: [] };
      if (turmaId === '-99') {
        params.turmasCodigos = listaTurmas.map(item => String(item.valor));
      } else {
        params.turmasCodigos = [String(turmaId)];
      }
      const retorno = await ServicoConselhoAtaFinal.gerar(params).catch(e =>
        erros(e)
      );
      if (retorno && retorno.status === 200) {
        sucesso(
          'Solicitação de geração do relatório gerada com sucesso. Em breve você receberá uma notificação com o resultado.'
        );
        setDesabilitarBtnGerar(true);
      }
    }
  };

  const onChangeUe = ue => {
    setUeId(ue);

    setListaModalidades([]);
    setModalidadeId(undefined);

    setListaSemestre([]);
    setSemestre(undefined);

    setListaTurmas([]);
    setTurmaId(undefined);
  };

  const onChangeModalidade = novaModalidade => {
    setModalidadeId(novaModalidade);

    setListaSemestre([]);
    setSemestre(undefined);

    setListaTurmas([]);
    setTurmaId(undefined);
  };

  const onChangeAnoLetivo = ano => {
    setAnoLetivo(ano);

    setListaModalidades([]);
    setModalidadeId(undefined);

    setListaSemestre([]);
    setSemestre(undefined);

    setListaTurmas([]);
    setTurmaId(undefined);
  };

  const onChangeSemestre = valor => setSemestre(valor);
  const onChangeTurma = valor => setTurmaId(valor);
  const onChangeFormato = valor => setFormato(valor);

  return (
    <>
      <Cabecalho pagina="Ata de Conselho" />
      <Card>
        <div className="col-md-12">
          <div className="row">
            <div className="col-md-12 d-flex justify-content-end pb-4">
              <Button
                id="btn-voltar-ata-final-resultado"
                label="Voltar"
                icon="arrow-left"
                color={Colors.Azul}
                border
                className="mr-2"
                onClick={onClickVoltar}
              />
              <Button
                id="btn-cancelar-ata-final-resultado"
                label="Cancelar"
                color={Colors.Roxo}
                border
                bold
                className="mr-3"
                onClick={() => onClickCancelar()}
              />
              <Button
                id="btn-gerar-ata-final-resultado"
                icon="print"
                label="Gerar"
                color={Colors.Azul}
                border
                bold
                className="mr-2"
                onClick={() => onClickGerar()}
                disabled={desabilitarBtnGerar || !permissoesTela.podeConsultar}
              />
            </div>
            <div className="col-sm-12 col-md-6 col-lg-2 col-xl-2 mb-2">
              <SelectComponent
                label="Ano Letivo"
                lista={listaAnosLetivo}
                valueOption="valor"
                valueText="desc"
                disabled={
                  !permissoesTela.podeConsultar ||
                  (listaAnosLetivo && listaAnosLetivo.length === 1)
                }
                onChange={onChangeAnoLetivo}
                valueSelect={anoLetivo}
              />
            </div>
            <div className="col-sm-12 col-md-6 col-lg-5 col-xl-5 mb-2">
              <SelectComponent
                label="Diretoria Regional de Educação (DRE)"
                lista={listaDres}
                valueOption="valor"
                valueText="desc"
                disabled={
                  !permissoesTela.podeConsultar ||
                  (listaDres && listaDres.length === 1)
                }
                onChange={onChangeDre}
                valueSelect={dreId}
              />
            </div>
            <div className="col-sm-12 col-md-6 col-lg-5 col-xl-5 mb-2">
              <SelectComponent
                label="Unidade Escolar (UE)"
                lista={listaUes}
                valueOption="valor"
                valueText="desc"
                disabled={
                  !permissoesTela.podeConsultar ||
                  (listaUes && listaUes.length === 1)
                }
                onChange={onChangeUe}
                valueSelect={ueId}
              />
            </div>
            <div className="col-sm-12 col-md-6 col-lg-5 col-xl-3 mb-2">
              <SelectComponent
                label="Modalidade"
                lista={listaModalidades}
                valueOption="valor"
                valueText="desc"
                disabled={
                  !permissoesTela.podeConsultar ||
                  (listaModalidades && listaModalidades.length === 1)
                }
                onChange={onChangeModalidade}
                valueSelect={modalidadeId}
              />
            </div>
            <div className="col-sm-12 col-md-3 col-lg-2 col-xl-2 mb-2">
              <SelectComponent
                lista={listaSemestre}
                valueOption="valor"
                valueText="desc"
                label="Semestre"
                disabled={
                  !permissoesTela.podeConsultar ||
                  !modalidadeId ||
                  modalidadeId != modalidade.EJA ||
                  (listaSemestre && listaSemestre.length === 1)
                }
                valueSelect={semestre}
                onChange={onChangeSemestre}
              />
            </div>
            <div className="col-sm-12 col-md-3 col-lg-3 col-xl-2 mb-2">
              <SelectComponent
                lista={listaTurmas}
                valueOption="valor"
                valueText="desc"
                label="Turma"
                disabled={
                  !permissoesTela.podeConsultar ||
                  (listaTurmas && listaTurmas.length === 1)
                }
                valueSelect={turmaId}
                onChange={onChangeTurma}
              />
            </div>
            <div className="col-sm-12 col-md-3 col-lg-2 col-xl-2 mb-2">
              <SelectComponent
                label="Formato"
                lista={listaFormatos}
                valueOption="valor"
                valueText="desc"
                valueSelect={formato}
                onChange={onChangeFormato}
                disabled
              />
            </div>
          </div>
        </div>
      </Card>
    </>
  );
};

export default AtaFinalResultados;
