CREATE index if not exists aula_aula_pai_idx ON public.aula USING btree (aula_pai_id);