import React, { useState } from 'react';
import { Provider } from 'react-redux';
import { Router } from 'react-router-dom';
import './configuracao/ReactotronConfig';
import history from './servicos/history';
import Alert from './componentes/alert';
import GlobalStyle from './estilos/global';
import Navbar from './componentes-sgp/navbar';
import Sider from './componentes-sgp/sider';
import { store } from './redux';
import Conteudo from './componentes-sgp/conteudo';

const notificacoes = { alertas: [] };

function App() {

  //const MenuStore = useSelector(store => store.menu);
  const [collapsed, setCollapsed] = useState(false);
  console.log(store.menu);

  return (
    <Provider store={store}>
      <Router history={history}>
        <GlobalStyle />
        <Navbar />
        <div className="container-fluid h-100">
          <div className="row h-100">
            <Sider />
            <Conteudo />
          </div>
        </div>
      </Router>
    </Provider>
  );
}

export default App;
