using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using BusinessModel.Models;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace AllyWebApi.Controllers
{
  public class AuthController : BaseController
  {
    //private readonly HttpContext _httpContext;
    public AuthController(IConfiguration configuration, ILogger<BaseController> log, IMemoryCache cache, IHttpContextAccessor httpContextAccessor) : base(configuration, log, cache, httpContextAccessor)
    {
      //_httpContext = httpContextAccessor.HttpContext;
    }

    [HttpPost]
    [Route("api/oauth/user/authenticate")]
    public ActionResult<UserSession> SignInUsingOAuth([FromBody]Dictionary<string, object> code)
    {
      UserSession loggedInUser = null;
      if (code != null)
      {
        var authCode = code["authCode"].ToString();
        RESTToken token;
        loggedInUser = Processor.SignInUsingOAuth(authCode, out token);
        Cache.Set(loggedInUser.userItemKey, token);

      }
      return loggedInUser;
    }

    [HttpPost]
    [Route("api/oauth/user/logout/{user}")]
    public IActionResult SignOut(string user)
    {
      //var userId = GetHeaderValue("__Id");
      if (!string.IsNullOrEmpty(user)) Cache.Remove(user);

      return Ok();
    }

    [HttpGet]
    [Route("api/oauth/user/authenticated")]
    public ActionResult<ApiResponseMessage> SessionAlive()
    {
      var userId = GetHeaderValue("__Id");
      if (!string.IsNullOrEmpty(userId) && Cache.Get<RESTToken>(userId) != null)
      {
        ApiResponseMessage responseMessage = new ApiResponseMessage
        {
          Data = true,
          Result = BusinessModel.Models.ActionResult.Success
        };
        return responseMessage;
      }

      return null;
    }
  }
}
