CREATE TABLE IF NOT EXISTS public.consolidado_conselho_classe_aluno_turma_nota (
id int8 NOT NULL GENERATED ALWAYS AS IDENTITY,
consolidado_conselho_classe_aluno_turma_id int8,
bimestre int4 NOT NULL,
nota numeric(11, 2) NULL,
conceito_id int8 NULL,
componente_curricular_id int8 NULL,
CONSTRAINT consolidado_conselho_classe_aluno_turma_nota_pk PRIMARY KEY (id)
);

ALTER TABLE IF EXISTS public.consolidado_conselho_classe_aluno_turma_nota 
ADD CONSTRAINT consolidado_conselho_classe_aluno_turma_nota 
FOREIGN KEY (consolidado_conselho_classe_aluno_turma_id) 
REFERENCES public.consolidado_conselho_classe_aluno_turma(id);

ALTER TABLE IF EXISTS public.consolidado_conselho_classe_aluno_turma_nota 
ADD CONSTRAINT consolidado_conselho_classe_aluno_turma_nota_componente_Curricular 
FOREIGN KEY (componente_curricular_id) 
REFERENCES public.componente_curricular(id);

CREATE INDEX componente_curricular_idx ON public.consolidado_conselho_classe_aluno_turma_nota USING btree (componente_curricular_id);
CREATE INDEX consolidado_conselho_classe_aluno_turma_idx ON public.consolidado_conselho_classe_aluno_turma_nota USING btree (consolidado_conselho_classe_aluno_turma_id);


--> Removendo as linhas que são de bimestres e mantendo somente final
delete from consolidado_conselho_classe_aluno_turma where bimestre <> 0;

--> Removendo a coluna bimestre
alter table consolidado_conselho_classe_aluno_turma drop column if exists bimestre;