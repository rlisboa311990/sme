﻿using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using SME.SGP.Dominio.Interfaces;
using SME.SGP.Infra;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;

namespace SME.SGP.Aplicacao.Servicos
{
    public class ServicoTokenJwt : IServicoTokenJwt
    {
        private readonly IConfiguration configuration;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IRepositorioCache cache;
        private string tokenGerado;

        public ServicoTokenJwt(IConfiguration configuration,
                               IHttpContextAccessor httpContextAccessor,
                               IRepositorioCache cache)
        {
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            this.httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            this.cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        public string GerarToken(string usuarioLogin, string usuarioNome, string codigoRf, Guid guidPerfil, IEnumerable<Permissao> permissionamentos)
        {
            IList<Claim> claims = new List<Claim>();

            claims.Add(new Claim(ClaimTypes.Name, usuarioLogin));
            claims.Add(new Claim("login", usuarioLogin));
            claims.Add(new Claim("nome", usuarioNome));
            claims.Add(new Claim("rf", codigoRf ?? string.Empty));
            claims.Add(new Claim("perfil", guidPerfil.ToString()));

            foreach (var permissao in permissionamentos)
            {
                claims.Add(new Claim("roles", permissao.ToString()));
            }

            var now = DateTime.Now;
            var token = new JwtSecurityToken(
                issuer: configuration.GetSection("JwtTokenSettings:Issuer").Value,
                audience: configuration.GetSection("JwtTokenSettings:Audience").Value,
                claims: claims,
                notBefore: now,
                expires: now.AddMinutes(double.Parse(configuration.GetSection("JwtTokenSettings:ExpiresInMinutes").Value)),
                signingCredentials: new SigningCredentials(
                    new SymmetricSecurityKey(
                        System.Text.Encoding.UTF8.GetBytes(configuration.GetSection("JwtTokenSettings:IssuerSigningKey").Value)),
                        SecurityAlgorithms.HmacSha256)
                );

            tokenGerado = new JwtSecurityTokenHandler()
                      .WriteToken(token);

            SalvarToken(usuarioLogin);

            return tokenGerado;
        }

        public bool TemPerfilNoToken(string guid)
        {
            throw new NotImplementedException();
        }

        private string ObterTokenAtual()
        {
            // Obtem token gerado ou o token da autenticação do contexto
            if (!string.IsNullOrEmpty(tokenGerado))
                return tokenGerado;

            var authorizationHeader = httpContextAccessor.HttpContext.Request.Headers["authorization"];

            return authorizationHeader == StringValues.Empty
                ? string.Empty
                : authorizationHeader.Single().Split(' ').Last();
        }

        public void RevogarToken(string login)
            => cache.RemoverAsync(ObterChaveLogin(login)).Wait();

        public bool TokenAtivo()
            => TokenAtivo(ObterTokenAtual());

        public bool TokenPresente()
        {
            var authorizationHeader = httpContextAccessor.HttpContext.Request.Headers["authorization"];

            return authorizationHeader == StringValues.Empty
                ? false
                : true;
        }

        public string ObterLogin()
            => ObterLoginDoToken(ObterTokenAtual());

        public Guid ObterPerfil()
            => ObterPerfilDoToken(ObterTokenAtual());

        public DateTime ObterDataHoraExpiracao()
        {
            var tokenStr = ObterTokenAtual();
            if (!string.IsNullOrEmpty(tokenStr))
            {
                var token = (new JwtSecurityTokenHandler()).ReadToken(tokenStr) as JwtSecurityToken;
                // Remove o fuso horario
                return token.ValidTo.AddHours(-3);
            }
            return DateTime.MinValue;
        }

        public DateTime ObterDataHoraCriacao()
            => ObterDataHoraCriacao(ObterTokenAtual());

        #region Private Methods
        private bool TokenAtivo(string token)
        {
            var tokenCache = cache.Obter(ObterChaveToken(token));

            // Quando não conseguir obter o token do cache assume que o atual é valido
            if (tokenCache == null)
                return true;

            var tokenAtual = ObterTokenAtual();
            if (tokenAtual == tokenCache)
                return true;

            // Verifica se o token atual é mais recente que o cache
            if ((tokenCache != token) && (ObterDataHoraCriacao(tokenAtual) > ObterDataHoraCriacao(tokenCache)))
            {
                tokenGerado = tokenAtual;

                SalvarToken(ObterLogin());
                return true;
            }

            return false;
        }

        private void SalvarToken(string usuarioLogin)
            => cache.SalvarAsync(ObterChaveLogin(usuarioLogin), tokenGerado).Wait();

        private string ObterChave(string nome)
            => $"token:{nome}";

        private string ObterChaveLogin(string usuarioLogin)
            => ObterChave(usuarioLogin);

        private string ObterChaveToken(string token)
            => ObterChave(ObterLoginDoToken(token));

        private IEnumerable<Claim> ObterClaims(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var tokenS = handler.ReadToken(token) as JwtSecurityToken;

            return tokenS.Claims;
        }

        private string ObterLoginDoToken(string token)
            => ObterClaims(token)
                .FirstOrDefault(claim => claim.Type == "login")?.Value ?? string.Empty;

        private Guid ObterPerfilDoToken(string token)
            => Guid.Parse(ObterClaims(token)
                .FirstOrDefault(claim => claim.Type == "perfil")?.Value 
                ?? string.Empty);

        private DateTime ObterDataHoraCriacao(string tokenStr)
        {
            if (!string.IsNullOrEmpty(tokenStr))
            {
                var token = (new JwtSecurityTokenHandler()).ReadToken(tokenStr) as JwtSecurityToken;
                // Remove o fuso horario
                return token.ValidFrom.AddHours(-3);
            }

            return DateTime.MinValue;
        }
        #endregion
    }
}