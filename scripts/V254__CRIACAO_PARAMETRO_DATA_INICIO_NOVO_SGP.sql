insert into public.parametros_sistema (nome, 
                                        tipo, 
                                        descricao, 
                                        valor, 
                                        ano, 
                                        ativo, 
                                        criado_em, 
                                        criado_por, 
                                        alterado_em, 
                                        alterado_por, 
                                        criado_rf, 
                                        alterado_rf)
                                values ('DataInicioSGP',
                                        29,
                                        'Data de início do Novo SGP',
                                        '2020',
                                        null,
                                        true,
                                        now(), 						 		 
                                        'Carga Inicial',
                                        null,
                                        null,
                                        'Carga Inicial',
                                        null);