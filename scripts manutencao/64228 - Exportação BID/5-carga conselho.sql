do $$
declare
	ues record;
	anoLetivo int := 2021;
begin	
	for ues in
		select id, nome from ue order by id
	loop
		raise notice 'Escola % - %', ues.id, ues.nome;
		call RELATORIO_BID_CONSELHO(anoLetivo, ues.Id);
		commit;
	end loop;
end $$