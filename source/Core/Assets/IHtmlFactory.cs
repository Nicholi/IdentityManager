using IdentityManager.Configuration;
using System;
using System.Net.Http;
using System.Web.Http;

namespace IdentityManager.Assets
{
    public interface IHtmlFactory
    {
        IHttpActionResult GetResult(HttpRequestMessage request, String file);
    }
}