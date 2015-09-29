/*
 * Copyright 2015 Dominick Baier, Brock Allen
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

using IdentityManager.AccessTokenValidation;
using Microsoft.Owin.Logging;
using Microsoft.Owin.Security.Jwt;
using Microsoft.Owin.Security.OAuth;
using System;
using System.IdentityModel.Tokens;
using System.Linq;

namespace Owin
{
    /// <summary>
    /// AppBuilder extensions for identity manager token validation
    /// </summary>
    public static class IdentityManagerBearerTokenValidationAppBuilderExtensions
    {
        /// <summary>
        /// Add identity manager token authentication to the pipeline.
        /// </summary>
        /// <param name="app">The application.</param>
        /// <param name="options">The options.</param>
        /// <returns></returns>
        public static IAppBuilder UseIdentityManagerBearerTokenAuthentication(this IAppBuilder app, IdentityManagerBearerTokenAuthenticationOptions options)
        {
            if (app == null) throw new ArgumentNullException("app");
            if (options == null) throw new ArgumentNullException("options");

            var loggerFactory = app.GetLoggerFactory();
            var middlewareOptions = new IdentityManagerOAuthBearerAuthenticationOptions();

            switch (options.ValidationMode)
            {
                case ValidationMode.Local:
                    middlewareOptions.LocalValidationOptions = ConfigureLocalValidation(options, loggerFactory);
                    break;
                case ValidationMode.ValidationEndpoint:
                    middlewareOptions.EndpointValidationOptions = ConfigureEndpointValidation(options, loggerFactory);
                    break;
                case ValidationMode.Both:
                    middlewareOptions.LocalValidationOptions = ConfigureLocalValidation(options, loggerFactory);
                    middlewareOptions.EndpointValidationOptions = ConfigureEndpointValidation(options, loggerFactory);
                    break;
                default:
                    throw new Exception("ValidationMode has invalid value");
            }

            if (options.TokenProvider != null)
            {
                middlewareOptions.TokenProvider = options.TokenProvider;
            }

            app.Use<IdentityManagerBearerTokenValidationMiddleware>(middlewareOptions, loggerFactory);

            if (options.RequiredScopes.Any())
            {
                app.Use<ScopeRequirementMiddleware>(options.RequiredScopes);
            }

            if (options.PreserveAccessToken)
            {
                app.Use<PreserveAccessTokenMiddleware>();
            }

            return app;
        }

        private static OAuthBearerAuthenticationOptions ConfigureEndpointValidation(IdentityManagerBearerTokenAuthenticationOptions options, ILoggerFactory loggerFactory)
        {
            if (options.EnableValidationResultCache)
            {
                if (options.ValidationResultCache == null)
                {
                    options.ValidationResultCache = new InMemoryValidationResultCache(options);
                }
            }

            var bearerOptions = new OAuthBearerAuthenticationOptions
            {
                AuthenticationMode = options.AuthenticationMode,
                AuthenticationType = options.AuthenticationType,
                AccessTokenProvider = new ValidationEndpointTokenProvider(options, loggerFactory),
                Provider = new ContextTokenProvider(),
            };

            return bearerOptions;
        }

        internal static OAuthBearerAuthenticationOptions ConfigureLocalValidation(IdentityManagerBearerTokenAuthenticationOptions options, ILoggerFactory loggerFactory)
        {
            JwtFormat tokenFormat = null;

            var audience = options.IssuerName.EnsureTrailingSlash();
            audience += "resources";

            var valParams = new TokenValidationParameters
            {
                ValidIssuer = options.IssuerName,
                ValidAudience = audience,
                NameClaimType = options.NameClaimType,
                RoleClaimType = options.RoleClaimType,
            };

            if (options.SigningCertificate != null)
            {
                valParams.IssuerSigningToken = new X509SecurityToken(options.SigningCertificate);
            }

            tokenFormat = new JwtFormat(valParams);

            var bearerOptions = new OAuthBearerAuthenticationOptions
            {
                AccessTokenFormat = tokenFormat,
                AuthenticationMode = options.AuthenticationMode,
                AuthenticationType = options.AuthenticationType,
                Provider = new ContextTokenProvider()
            };

            return bearerOptions;
        }
    }
}