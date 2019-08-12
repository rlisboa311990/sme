﻿using Dommel;
using SME.SGP.Dados.Contexto;
using SME.SGP.Dominio;
using SME.SGP.Dominio.Entidades;
using SME.SGP.Dominio.Interfaces;
using System;
using System.Collections.Generic;

namespace SME.SGP.Dados.Repositorios
{
    public abstract class RepositorioBase<T> : IRepositorioBase<T> where T : EntidadeBase
    {
        protected readonly ISgpContext database;

        protected RepositorioBase(ISgpContext database)
        {
            this.database = database;
        }

        public void Auditar(string usuario, long identificador)
        {
            database.Insert<Auditoria>(new Auditoria()
            {
                Data = DateTime.Now,
                Entidade = typeof(T).Name.ToLower(),
                Chave = identificador,
                Usuario = usuario
            });
        }

        public virtual IEnumerable<T> Listar()
        {
            return database.Conexao().GetAll<T>();
        }

        public virtual T ObterPorId(long id)
        {
            return database.Conexao().Get<T>(id);
        }

        public virtual void Remover(long id)
        {
            var entidade = database.Conexao().Get<T>(id);
            database.Conexao().Delete(entidade);
        }

        public virtual long Salvar(T entidade)
        {
            if (entidade.Id > 0)
            {
                entidade.AlteradoEm = DateTime.Now;
                entidade.AlteradoPor = "usuário logado";
                database.Conexao().Update(entidade);
            }
            else
            {
                entidade.CriadoPor = "usuário logado";
                entidade.Id = (long)database.Conexao().Insert(entidade);
            }
            return entidade.Id;
        }
    }
}