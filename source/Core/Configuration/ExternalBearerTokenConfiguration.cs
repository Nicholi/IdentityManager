/*
 * Copyright 2014 Dominick Baier, Brock Allen
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
 
using Microsoft.Owin.Security.Jwt;
using Owin;
using System;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel.Security.Tokens;
using IdentityManager.AccessTokenValidation;

namespace IdentityManager.Configuration
{
    public class ExternalBearerTokenConfiguration : SecurityConfiguration
    {
        public ExternalBearerTokenConfiguration()
        {
            Scope = Constants.IdMgrScope;
        }

        public string Scope { get; set; }

        public Func<ClaimsPrincipal, ClaimsPrincipal> ClaimsTransformation { get; set; }

        public IdentityManagerBearerTokenAuthenticationOptions AuthenticationOptions { get; set; }

        internal override void Validate()
        {
            base.Validate();

            if (String.IsNullOrWhiteSpace(Scope)) throw new InvalidOperationException("OAuth2Configuration : Scope not configured");
            if (AuthenticationOptions == null) throw new InvalidOperationException("OAuth2Configuration : AuthenticationOptions not configured");
            if (String.IsNullOrWhiteSpace(AuthenticationOptions.Authority))
            {
                throw new InvalidOperationException("OAuth2Configuration : AuthenticationOptions.Authority not configured");
            }
            if (String.IsNullOrEmpty(AuthenticationOptions.IssuerName))
            {
                throw new InvalidOperationException("OAuth2Configuration : AuthenticationOptions.IssuerName not configured");
            }
            if (AuthenticationOptions.SigningCertificate == null)
            {
                throw new InvalidOperationException("OAuth2Configuration : AuthenticationOptions.SigningCertificate not configured");
            }
        }

        public override void Configure(IAppBuilder app)
        {
            // assure our IdentityManager scope is present
            if (!AuthenticationOptions.RequiredScopes.Any(x => String.Equals(x, Scope)))
            {
                var requiredScopes = AuthenticationOptions.RequiredScopes.ToList();
                requiredScopes.Add(Scope);
                AuthenticationOptions.RequiredScopes = requiredScopes;
            }
            app.UseIdentityManagerBearerTokenAuthentication(AuthenticationOptions);
            if (ClaimsTransformation != null)
            {
                app.Use(async (ctx, next) =>
                {
                    var user = ctx.Authentication.User;
                    if (user != null)
                    {
                        user = ClaimsTransformation(user);
                        ctx.Authentication.User = user;
                    }

                    await next();
                });
            }
        }
    }
}
