﻿CREATE TABLE IF NOT EXISTS public.wf_aprovacao_nivel
(
    id bigint NOT NULL generated always as identity,
    usuario_id varchar(15) NOT NULL,
    status int NOT NULL,
    descricao varchar(100) NULL,    
	nivel int NOT NULL,
	wf_aprovacao_id bigint not null,
    criado_em timestamp without time zone NOT NULL,
    criado_por character varying(200) COLLATE pg_catalog."default" NOT NULL,
    alterado_em timestamp without time zone,
    alterado_por character varying(200) COLLATE pg_catalog."default",
    criado_rf character varying(200) COLLATE pg_catalog."default" NOT NULL,
    alterado_rf character varying(200) COLLATE pg_catalog."default",
    CONSTRAINT wf_aprova_nivel_nivel_pk PRIMARY KEY (id)

);

ALTER TABLE public.wf_aprovacao_nivel ADD CONSTRAINT wf_aprovacao_nivel_wf_aprovacao_fk FOREIGN KEY (wf_aprovacao_id) REFERENCES wf_aprovacao(id);
CREATE INDEX wf_aprovacao_nivel_usuario_idx ON public.wf_aprovacao_nivel (usuario_id);


