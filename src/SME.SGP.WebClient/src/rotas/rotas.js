import React from 'react';
import { Switch, Route } from 'react-router-dom';
import BreadcrumbSgp from '../componentes-sgp/breadcrumb-sgp';
import Principal from '../paginas/Principal/principal';
import PlanoCiclo from '../paginas/Planejamento/PlanoCiclo/planoCiclo';
import PlanoAnual from '../paginas/Planejamento/PlanoAnual/planoAnual';

export default function Rotas() {

  return (
    <div>
      <BreadcrumbSgp/>
      <Switch>
        <Route path="/" exact component={Principal} />
        <Route path="/planejamento/plano-ciclo" exact component={PlanoCiclo} />
        <Route path="/planejamento/plano-anual" component={PlanoAnual} />
      </Switch>
    </div>
  );
}
