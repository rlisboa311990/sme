import React from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { JoditEditor } from '~/componentes';
import { setQuestoesItinerancia } from '~/redux/modulos/itinerancia/action';

const EditoresTexto = () => {
  const dispatch = useDispatch();
  const dados = useSelector(store => store.itinerancia.questoesItinerancia);

  const setAcompanhamentoSituacao = (valor, questao) => {
    questao.resposta = valor;
    dispatch(setQuestoesItinerancia([...dados]));
  };

  return (
    <>
      {dados &&
        dados.map(questao => {
          return (
            <div className="row mb-4" key={questao.id}>
              <div className="col-12">
                <JoditEditor
                  label={questao.descricao}
                  value=""
                  name={questao.descricao + questao.questaoId}
                  id={questao.questaoId}
                  onChange={e => setAcompanhamentoSituacao(e, questao)}
                />
              </div>
            </div>
          );
        })}
    </>
  );
};

export default EditoresTexto;
