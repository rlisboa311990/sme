alter table encaminhamento_aee drop if exists usuario_id;

alter table encaminhamento_aee add column usuario_id int8 null; 

select
	f_cria_fk_se_nao_existir(
		'encaminhamento_aee',
		'encaminhamento_aee_usuario_fk',
		'FOREIGN KEY (usuario_id) REFERENCES usuario (id)'
	);

CREATE INDEX encaminhamento_aee_usuario_idx ON public.encaminhamento_aee USING btree (usuario_id);