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
using Owin;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using IdentityServer3.Core.Models;
using IdentityServer3.Core.Services.InMemory;
using IdentityServer3.Core.Configuration;
using IdentityServer3.Core.Services;

namespace IdentityManager.Host.IdSvr
{
    public class IdSvrConfig
    {
        public static void Configure(IAppBuilder app, List<InMemoryUser> users)
        {
            var factory = new IdentityServerServiceFactory();

            factory.Register(new Registration<List<InMemoryUser>>(users));
            factory.UserService = new Registration<IUserService, InMemoryUserService>();

            var clients = GetClients();
            factory.Register(new Registration<IEnumerable<Client>>(clients));
            factory.ClientStore = new Registration<IClientStore>(typeof(InMemoryClientStore));
            factory.CorsPolicyService = new Registration<ICorsPolicyService>(new InMemoryCorsPolicyService(clients));

            factory.Register(new Registration<IEnumerable<Scope>>(GetScopes()));
            factory.ScopeStore = new Registration<IScopeStore>(typeof(InMemoryScopeStore));

            var idsrvOptions = new IdentityServerOptions
            {
                SiteName = "IdentityServer v3",
                SigningCertificate = Cert.Load(),
                Endpoints = new EndpointOptions {
                    EnableCspReportEndpoint = true
                },
                Factory = factory
            };
            app.UseIdentityServer(idsrvOptions);
        }

        static Client[] GetClients()
        {
            return new Client[]{
                new Client{
                    ClientId = "idmgr_client",
                    ClientName = "IdentityManager",
                    Enabled = true,
                    Flow = Flows.Implicit,
                    RequireConsent = false,
                    RedirectUris = new List<string>{
                        "https://localhost:44337",
                    },
                    PostLogoutRedirectUris = new List<string>{
                        "https://localhost:44337/idm"
                    },
                    IdentityProviderRestrictions = new List<string>(){IdentityServer3.Core.Constants.PrimaryAuthenticationType}
                },
            };
        }

        static Scope[] GetScopes()
        {
            return new Scope[] {
                StandardScopes.OpenId,
                 new Scope{
                    Name = "idmgr",
                    DisplayName = "IdentityManager",
                    Description = "Authorization for IdentityManager",
                    Type = ScopeType.Identity,
                    Claims = new List<ScopeClaim>{
                        new ScopeClaim(Constants.ClaimTypes.Name),
                        new ScopeClaim(Constants.ClaimTypes.Role)
                    }
                },
            };
        }
    }
}