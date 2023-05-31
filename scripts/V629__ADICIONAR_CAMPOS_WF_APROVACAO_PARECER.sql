ALTER TABLE public.wf_aprovacao_parecer_conclusivo ADD column IF NOT exists conselho_classe_parecer_id_anterior int8 NULL;
ALTER TABLE public.wf_aprovacao_parecer_conclusivo ADD CONSTRAINT wf_aprovacao_parecer_conclusivo_parecer_anterior_fk FOREIGN KEY (conselho_classe_parecer_id_anterior) REFERENCES conselho_classe_parecer(id);
ALTER TABLE public.wf_aprovacao_parecer_conclusivo ADD column if not exists excluido bool NOT NULL DEFAULT false;