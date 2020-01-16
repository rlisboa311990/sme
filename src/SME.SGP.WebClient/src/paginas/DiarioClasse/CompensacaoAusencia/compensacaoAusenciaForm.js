import { Form, Formik } from 'formik';
import React, { useEffect, useState } from 'react';
import { useSelector } from 'react-redux';
import * as Yup from 'yup';
import { CampoTexto, Colors, Label, Loader } from '~/componentes';
import Cabecalho from '~/componentes-sgp/cabecalho';
import Auditoria from '~/componentes/auditoria';
import Button from '~/componentes/button';
import Card from '~/componentes/card';
import Editor from '~/componentes/editor/editor';
import SelectComponent from '~/componentes/select';
import modalidade from '~/dtos/modalidade';
import { confirmar, erros, sucesso } from '~/servicos/alertas';
import { setBreadcrumbManual } from '~/servicos/breadcrumb-services';
import history from '~/servicos/history';
import ServicoCompensacaoAusencia from '~/servicos/Paginas/DiarioClasse/ServicoCompensacaoAusencia';
import ServicoDisciplina from '~/servicos/Paginas/ServicoDisciplina';

import { Badge } from './styles';
import CompensacaoAusenciaListaAlunos from './listasAlunos/compensacaoAusenciaListaAlunos';

const CompensacaoAusenciaForm = ({ match }) => {
  const usuario = useSelector(store => store.usuario);

  const { turmaSelecionada } = usuario;

  const [listaDisciplinas, setListaDisciplinas] = useState([]);
  const [carregandoDisciplinas, setCarregandoDisciplinas] = useState(false);
  const [desabilitarDisciplina, setDesabilitarDisciplina] = useState(false);
  const [novoRegistro, setNovoRegistro] = useState(true);
  const [modoEdicao, setModoEdicao] = useState(false);
  const [idCompensacaoAusencia, setIdCompensacaoAusencia] = useState(0);
  const [listaBimestres, setListaBimestres] = useState([]);
  const [auditoria, setAuditoria] = useState([]);
  const [exibirAuditoria, setExibirAuditoria] = useState(false);
  const [listaDisciplinasRegencia, setListaDisciplinasRegencia] = useState([]);
  const [temRegencia, setTemRegencia] = useState(false);
  const [refForm, setRefForm] = useState({});

  const [valoresIniciais, setValoresIniciais] = useState({
    disciplina: undefined,
    bimestre: '',
    atividade: '',
    detalhes: '',
  });

  const [validacoes] = useState(
    Yup.object({
      disciplina: Yup.string().required('Disciplina obrigatória'),
      bimestre: Yup.string().required('Bimestre obrigatório'),
      atividade: Yup.string()
        .required('Atividade obrigatória')
        .max(250, 'Máximo 250 caracteres'),
      detalhes: Yup.string().required('Detalhe obrigatório'),
    })
  );

  const resetarForm = () => {
    if (refForm && refForm.resetForm) {
      refForm.resetForm();
    }
    setListaDisciplinasRegencia([]);
    setTemRegencia(false);
  };

  const onChangeCampos = () => {
    if (!modoEdicao) {
      setModoEdicao(true);
    }
  };

  const selecionarDisciplina = indice => {
    const disciplinas = [...listaDisciplinasRegencia];
    disciplinas[indice].selecionada = !disciplinas[indice].selecionada;
    setListaDisciplinasRegencia(disciplinas);
    onChangeCampos();
  };

  const obterDisciplinasRegencia = async codigoDisciplinaSelecionada => {
    const disciplina = listaDisciplinas.find(
      c => c.codigoComponenteCurricular == codigoDisciplinaSelecionada
    );
    // TODO REMOVER
    if (disciplina) {
      disciplina.regencia = true;
    }
    // TODO REMOVER
    if (disciplina && disciplina.regencia) {
      const disciplinasRegencia = await ServicoDisciplina.obterDisciplinasPlanejamento(
        codigoDisciplinaSelecionada,
        turmaSelecionada.turma,
        false,
        disciplina.regencia
      ).catch(e => erros(e));

      if (
        disciplinasRegencia &&
        disciplinasRegencia.data &&
        disciplinasRegencia.data.length
      ) {
        setListaDisciplinasRegencia(disciplinasRegencia.data);
        setTemRegencia(true);
      }
    } else {
      setListaDisciplinasRegencia([]);
      setTemRegencia(false);
    }
  };

  const onChangeDisciplina = codigoDisciplina => {
    obterDisciplinasRegencia(codigoDisciplina);
    onChangeCampos();
  };

  useEffect(() => {
    const obterDisciplinas = async () => {
      setCarregandoDisciplinas(true);
      const disciplinas = await ServicoDisciplina.obterDisciplinasPorTurma(
        turmaSelecionada.turma
      );
      if (disciplinas.data && disciplinas.data.length) {
        setListaDisciplinas(disciplinas.data);
      } else {
        setListaDisciplinas([]);
      }

      if (disciplinas.data && disciplinas.data.length === 1) {
        setDesabilitarDisciplina(true);

        if (!(match && match.params && match.params.id)) {
          const disciplina = disciplinas.data[0];
          const valoresIniciaisForm = {
            disciplina: String(disciplina.codigoComponenteCurricular),
            bimestre: '',
            atividade: '',
            detalhes: '',
          };
          setValoresIniciais(valoresIniciaisForm);
        }
      }
      setCarregandoDisciplinas(false);
    };

    if (turmaSelecionada.turma) {
      resetarForm();
      obterDisciplinas(turmaSelecionada.turma);
    } else {
      resetarForm();
    }

    let listaBi = [];
    if (turmaSelecionada.modalidade == modalidade.EJA) {
      listaBi = [
        { valor: 1, descricao: '1°' },
        { valor: 2, descricao: '2°' },
      ];
    } else {
      listaBi = [
        { valor: 1, descricao: '1°' },
        { valor: 2, descricao: '2°' },
        { valor: 3, descricao: '3°' },
        { valor: 4, descricao: '4°' },
      ];
    }
    setListaBimestres(listaBi);
  }, [match, turmaSelecionada.modalidade, turmaSelecionada.turma]);

  useEffect(() => {
    const consultaPorId = async () => {
      setBreadcrumbManual(
        match.url,
        'Alterar Compensação de Ausência',
        '/diario-classe/compensacao-ausencia'
      );
      setIdCompensacaoAusencia(match.params.id);

      const dadosEdicao = await ServicoCompensacaoAusencia.obterPorId(
        match.params.id
      ).catch(e => erros(e));

      if (dadosEdicao && dadosEdicao.data) {
        setValoresIniciais({
          disciplina: String(dadosEdicao.data.disciplina),
          bimestre: dadosEdicao.data.bimestre,
          atividade: dadosEdicao.data.atividade,
        });
        setAuditoria({
          criadoPor: dadosEdicao.data.criadoPor,
          criadoRf: dadosEdicao.data.criadoRf,
          criadoEm: dadosEdicao.data.criadoEm,
          alteradoPor: dadosEdicao.data.alteradoPor,
          alteradoRf: dadosEdicao.data.alteradoRf,
          alteradoEm: dadosEdicao.data.alteradoEm,
        });
        setExibirAuditoria(true);
      }
      setNovoRegistro(false);
    };

    if (turmaSelecionada.turma && match && match.params && match.params.id) {
      consultaPorId(match.params.id);
    }
  }, [match, turmaSelecionada.turma]);

  const validaAntesDoSubmit = form => {
    const arrayCampos = Object.keys(valoresIniciais);
    arrayCampos.forEach(campo => {
      form.setFieldTouched(campo, true, true);
    });
    form.validateForm().then(() => {
      if (form.isValid || Object.keys(form.errors).length == 0) {
        form.handleSubmit(e => e);
      }
    });
  };

  const onClickExcluir = async () => {
    if (!novoRegistro) {
      const confirmado = await confirmar(
        'Excluir compensação',
        '',
        'Você tem certeza que deseja excluir este registro',
        'Excluir',
        'Cancelar'
      );
      if (confirmado) {
        const excluir = await ServicoCompensacaoAusencia.deletar([
          idCompensacaoAusencia,
        ]).catch(e => erros(e));

        if (excluir && excluir.status == 200) {
          sucesso('Compensação excluída com sucesso.');
          history.push('/diario-classe/compensacao-ausencia');
        }
      }
    }
  };

  const resetarTela = form => {
    form.resetForm();
    setModoEdicao(false);
  };

  const onClickCancelar = async form => {
    if (modoEdicao) {
      const confirmou = await confirmar(
        'Atenção',
        'Você não salvou as informações preenchidas.',
        'Deseja realmente cancelar as alterações?'
      );
      if (confirmou) {
        resetarTela(form);
      }
    }
  };

  const perguntaAoSalvar = async () => {
    return confirmar(
      'Atenção',
      '',
      'Suas alterações não foram salvas, deseja salvar agora?'
    );
  };

  const onClickVoltar = async form => {
    if (modoEdicao) {
      const confirmado = await perguntaAoSalvar();
      if (confirmado) {
        validaAntesDoSubmit(form);
      }
    } else {
      history.push('/diario-classe/compensacao-ausencia');
    }
  };

  const onClickCadastrar = async valoresForm => {
    const paramas = valoresForm;
    paramas.id = idCompensacaoAusencia;

    if (temRegencia) {
      paramas.listaDisciplinasRegencia = listaDisciplinasRegencia.filter(
        item => item.selecionada
      );
    }
    console.log(paramas);

    const cadastrado = await ServicoCompensacaoAusencia.salvar(
      paramas
    ).catch(e => erros(e));

    if (cadastrado && cadastrado.status == 200) {
      if (idCompensacaoAusencia) {
        sucesso('Tipo de feriado alterado com sucesso.');
      } else {
        sucesso('Novo tipo de feriado criado com sucesso.');
      }
      history.push('/diario-classe/compensacao-ausencia');
    }
  };

  const alunosCompensacao = [
    {
      alunoCodigo: '2',
      nome: 'jorge',
      qtdFaltasCompensadas: 2,
    },
    {
      alunoCodigo: '3',
      nome: 'camila',
      qtdFaltasCompensadas: 2,
    },
    {
      alunoCodigo: '4',
      nome: 'joao',
      qtdFaltasCompensadas: 2,
    },
    {
      alunoCodigo: '5',
      nome: 'ana',
      qtdFaltasCompensadas: 2,
    },
    {
      alunoCodigo: '6',
      nome: 'asdasdasd',
      qtdFaltasCompensadas: 2,
    },
    {
      alunoCodigo: '7',
      nome: 'aaaaaaa',
      qtdFaltasCompensadas: 2,
    },
    {
      alunoCodigo: '8',
      nome: 'jovem',
      qtdFaltasCompensadas: 2,
    },
    {
      alunoCodigo: '9',
      nome: 'gui',
      qtdFaltasCompensadas: 2,
    },
    {
      alunoCodigo: '10',
      nome: 'amanda',
      qtdFaltasCompensadas: 2,
    },
    {
      alunoCodigo: '11',
      nome: 'jana',
      qtdFaltasCompensadas: 2,
    },
    {
      alunoCodigo: '12',
      nome: 'menina',
      qtdFaltasCompensadas: 2,
    },
  ];

  const alunos = [
    {
      alunoCodigo: '1',
      nome: 'teste',
      frequencia: '70%',
      faltas: 23,
    },
  ];

  return (
    <>
      <Cabecalho pagina="Cadastrar Compensação de Ausência" />
      <Card>
        <Formik
          enableReinitialize
          ref={refF => setRefForm(refF)}
          initialValues={valoresIniciais}
          validationSchema={validacoes}
          onSubmit={onClickCadastrar}
          validateOnChange
          validateOnBlur
        >
          {form => (
            <Form className="col-md-12 mb-4">
              <div className="d-flex justify-content-end pb-4">
                <Button
                  id="btn-voltar"
                  label="Voltar"
                  icon="arrow-left"
                  color={Colors.Azul}
                  border
                  className="mr-2"
                  onClick={() => onClickVoltar(form)}
                />
                <Button
                  id="btn-cancelar"
                  label="Cancelar"
                  color={Colors.Roxo}
                  border
                  className="mr-2"
                  onClick={() => onClickCancelar(form)}
                  disabled={!modoEdicao}
                />
                <Button
                  id="btn-excluir"
                  label="Excluir"
                  color={Colors.Vermelho}
                  border
                  className="mr-2"
                  disabled={novoRegistro}
                  onClick={onClickExcluir}
                />
                <Button
                  id="btn-salvar"
                  label={`${
                    idCompensacaoAusencia > 0 ? 'Alterar' : 'Cadastrar'
                  }`}
                  color={Colors.Roxo}
                  border
                  bold
                  className="mr-2"
                  onClick={() => validaAntesDoSubmit(form)}
                />
              </div>

              <div className="row">
                <div className="col-sm-12 col-md-8 col-lg-4 col-xl-4 mb-2">
                  <Loader loading={carregandoDisciplinas} tip="">
                    <SelectComponent
                      form={form}
                      id="disciplina"
                      label="Disciplina"
                      name="disciplina"
                      lista={listaDisciplinas}
                      valueOption="codigoComponenteCurricular"
                      valueText="nome"
                      onChange={onChangeDisciplina}
                      placeholder="Disciplina"
                      disabled={desabilitarDisciplina}
                    />
                  </Loader>
                </div>
                <div className="col-sm-12 col-md-4 col-lg-2 col-xl-2 mb-2">
                  <SelectComponent
                    form={form}
                    id="bimestre"
                    label="Bimestre"
                    name="bimestre"
                    valueOption="valor"
                    valueText="descricao"
                    onChange={onChangeCampos}
                    placeholder="Bimestre"
                    lista={listaBimestres}
                  />
                </div>
                <div className="col-sm-12 col-md-12 col-lg-6 col-xl-6 mb-2">
                  <CampoTexto
                    form={form}
                    label="Atividade"
                    placeholder="Atividade"
                    name="atividade"
                    onChange={onChangeCampos}
                    type="input"
                  />
                </div>
                {temRegencia && listaDisciplinasRegencia && (
                  <div className="col-sm-12 col-md-12 col-lg-5 col-xl-5 mb-2">
                    <Label text="Componente curricular" />
                    {listaDisciplinasRegencia.map((disciplina, indice) => {
                      return (
                        <Badge
                          key={disciplina.codigoComponenteCurricular}
                          role="button"
                          onClick={e => {
                            e.preventDefault();
                            selecionarDisciplina(indice);
                          }}
                          aria-pressed={disciplina.selecionada && true}
                          alt={disciplina.nome}
                          className="badge badge-pill border text-dark bg-white font-weight-light px-2 py-1 mr-2"
                        >
                          {disciplina.nome}
                        </Badge>
                      );
                    })}
                  </div>
                )}

                <div className="col-sm-12 col-md-12 col-lg-12 col-xl-12 mb-2">
                  <Editor
                    form={form}
                    name="detalhes"
                    onChange={onChangeCampos}
                    label="Detalhamento da atividade"
                  />
                </div>
              </div>
              <div className="row">
                <CompensacaoAusenciaListaAlunos
                  lista={alunos}
                  listaAusenciaCompensada={alunosCompensacao}
                />
              </div>
            </Form>
          )}
        </Formik>
        {exibirAuditoria ? (
          <Auditoria
            criadoEm={auditoria.criadoEm}
            criadoPor={auditoria.criadoPor}
            alteradoPor={auditoria.alteradoPor}
            alteradoEm={auditoria.alteradoEm}
          />
        ) : (
          ''
        )}
      </Card>
    </>
  );
};

export default CompensacaoAusenciaForm;
