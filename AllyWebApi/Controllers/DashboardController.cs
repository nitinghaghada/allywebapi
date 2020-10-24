using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using BusinessModel.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace AllyWebApi.Controllers
{
  public class DashboardController : BaseController
  {
    public DashboardController(IConfiguration configuration, ILogger<BaseController> log, IMemoryCache cache, IHttpContextAccessor httpContextAccessor) : base(configuration, log, cache, httpContextAccessor)
    {
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="entityKey"></param>
    /// <param name="entityType"></param>
    /// <param name="statusField"></param>
    /// <param name="status"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("api/statuswisecount/{entitykey}/{entitytype}/{statusfield}/{status}")]
    public ActionResult<ApiResponseMessage> GetStatusWiseEntityCount(string entityKey, string entityType, string statusField, string status)
    {
      if (SetAuthorizeToken())
      {
        try
        {
          IList<Dictionary<string, object>> data = new List<Dictionary<string, object>>();
          data = Processor.GetStatusWiseEntityCount(entityKey, entityType, statusField, status);
          ApiResponseMessage responseMessage = new ApiResponseMessage
          {
            Data = data.ToArray(),
            Result = BusinessModel.Models.ActionResult.Success
          };
          return responseMessage;

        }
        catch (Exception ex)
        {
          Log.LogError(ex, ex.Message);
          if (ex is WebException exception)
          {
            return WebExceptionResponse(exception);
          }
          throw;
        }
      }
      return UnauthorizedResponse();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="entityKey"></param>
    /// <param name="entityType"></param>
    /// <param name="statusField"></param>
    /// <param name="status"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("api/statuswisecount/{entitykey}/{entitytype}/{statusfield}/{status}/{datefield}")]
    public ActionResult<ApiResponseMessage> GetMonthlyEntityCountGroupByStatus(string entityKey, string entityType, string statusField, string status, string dateField)
    {
      if (SetAuthorizeToken())
      {
        try
        {
          IList<Dictionary<string, object>> data = new List<Dictionary<string, object>>();
          data = Processor.GetMonthlyEntityCountGroupByStatus(entityKey, entityType, statusField, status, dateField);
          ApiResponseMessage responseMessage = new ApiResponseMessage
          {
            Data = data.ToArray(),
            Result = BusinessModel.Models.ActionResult.Success
          };
          return responseMessage;

        }
        catch (Exception ex)
        {
          Log.LogError(ex, ex.Message);
          if (ex is WebException exception)
          {
            return WebExceptionResponse(exception);
          }
          throw;
        }
      }
      return UnauthorizedResponse();
    }
  }
}
