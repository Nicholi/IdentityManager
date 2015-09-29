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

using IdentityManager.Api.Filters;
using IdentityManager.Assets;
using IdentityManager.Configuration;
using System;
using System.Net.Http;
using System.Web.Http;

namespace IdentityManager.Api.Controllers
{
    [NoCache]
    [SecurityHeaders]
    public class PageController : ApiController
    {
        IdentityManagerOptions idmConfig;
        IHtmlFactory htmlFactory;
        public PageController(IdentityManagerOptions idmConfig, IHtmlFactory htmlFactory)
        {
            if (idmConfig == null) throw new ArgumentNullException("idmConfig");
            if (htmlFactory == null) throw new ArgumentNullException("htmlFactory");

            this.idmConfig = idmConfig;
            this.htmlFactory = htmlFactory;
        }

        [HttpGet]
        [AllowAnonymous]
        public IHttpActionResult Index()
        {
            return htmlFactory.GetResult(Request, "IdentityManager.Assets.Templates.index.html");
        }

        [HttpGet]
        [AllowAnonymous]
        public IHttpActionResult Logout()
        {
            idmConfig.SecurityConfiguration.SignOut(Request.GetOwinContext());
            return RedirectToRoute(Constants.RouteNames.Home, null);
        }
    }
}
