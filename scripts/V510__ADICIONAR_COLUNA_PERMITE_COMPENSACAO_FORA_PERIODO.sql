insert
	into
	parametros_sistema
(nome,
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
values('PermiteCompensacaoForaPeriodo',
83,
'Permitir Compensação Fora do Período',
'true',
2021,
true,
now(),
'SISTEMA',
null,
null,
'0',
null);