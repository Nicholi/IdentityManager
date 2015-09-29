using System;
using System.Net.Http;
using System.Web.Http;
using IdentityManager.Configuration;

namespace IdentityManager.Assets
{
    public class DefaultHtmlFactory : IHtmlFactory
    {
        private readonly SecurityConfiguration m_SecurityConfiguration;

        public DefaultHtmlFactory(SecurityConfiguration securityConfiguration)
        {
            m_SecurityConfiguration = securityConfiguration;
        }

        public IHttpActionResult GetResult(HttpRequestMessage request, String file)
        {
            return new EmbeddedHtmlResult(request, file, m_SecurityConfiguration);
        }
    }
}