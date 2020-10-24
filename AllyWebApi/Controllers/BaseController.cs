using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using BusinessModel.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860
//http://localhost:62527/api/entity/metadata/34/dd
namespace AllyWebApi.Controllers
{
  [ApiController]
  public class BaseController : ControllerBase
  {
    protected IProcessor Processor;
    private IConfiguration _configuration;
    protected readonly ILogger Log;
    protected IMemoryCache Cache;
    protected HttpContext httpContext;
    protected BaseController(IConfiguration configuration, ILogger log, IMemoryCache cache, IHttpContextAccessor httpContextAccessor)
    {
      Processor = new Processor(configuration, cache, log);
      _configuration = configuration;
      Log = log;
      Cache = cache;
      httpContext = httpContextAccessor.HttpContext;
      Log.LogInformation("Hello, world!");
    }

    protected BaseController(IConfiguration configuration, ILogger log, IMemoryCache cache)
    {
      Processor = new Processor(configuration, cache, log);
      _configuration = configuration;
      Log = log;
      Cache = cache;
    }

    protected string GetHeaderValue(string headerKey)
    {
      StringValues headerTokens;
      Request.Headers.TryGetValue(headerKey, out headerTokens);
      var val = headerTokens.FirstOrDefault();
      return val;
    }

    protected bool SetAuthorizeToken()
    {
      RESTToken token = null;// = GetHeaderValue("__AuthorizationToken");
      var userId = GetHeaderValue("__Id");
      //var cookie = Request.Cookies["Token"];
      if (userId != null)
        token = Cache.Get<RESTToken>(userId);
      if (token != null)
      {
        return Processor.ServiceContext.JoinApiService.SetAuthToken(new RESTToken
        {
          access_token = token.access_token,
          refresh_token = token.refresh_token
        });
      }
      return false;
    }

    protected ApiResponseMessage UnauthorizedResponse()
    {
      ApiResponseMessage responseMessage = new ApiResponseMessage
      {
        Result = BusinessModel.Models.ActionResult.Exception,
        StatusDescription = HttpStatusCode.Unauthorized.ToString(),
        StatusCode = (int)HttpStatusCode.Unauthorized
      };
      return responseMessage;
    }

    protected ApiResponseMessage WebExceptionResponse(WebException ex)
    {
      ApiResponseMessage responseMessage = new ApiResponseMessage
      {
        Result = BusinessModel.Models.ActionResult.Exception,
        Message = ex.Message,
        StatusCode = (int)((System.Net.HttpWebResponse)ex.Response).StatusCode,
        StatusDescription = ((System.Net.HttpWebResponse)ex.Response).StatusDescription
      };
      return responseMessage;
    }
  }
}
