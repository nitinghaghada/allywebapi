using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AllyWebApi.FormlyFieldModels;
using BusinessModel.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace AllyWebApi.Controllers
{
  public class FormController : BaseController
  {
    /// <summary>
    /// 
    /// </summary>
    /// <param name="configuration"></param>
    public FormController(IConfiguration configuration, ILogger<BaseController> log, IMemoryCache cache) : base(configuration, log, cache)
    {
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("api/entity/form/{entity}/{key}")]
    public ActionResult<ApiResponseMessage> Get(string entity, string key)
    {
      if (SetAuthorizeToken())
      {
        try
        {
          var items = Processor.GetEntityFormFromJoin(key);
          ApiResponseMessage responseMessage = new ApiResponseMessage
          {
            Data = items,
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
    /// <param name="entity"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("api/join/entitydata/{entity}/{key}")]
    public ActionResult<ApiResponseMessage> GetEntityWithProfile(string entity, string key)
    {
      if (SetAuthorizeToken())
      {
        try
        {
          List<FormlyFieldConfig> fields = null;
          var items = Processor.GetEntityWithProfile(entity, key, out fields);
          
          ApiResponseMessage responseMessage = new ApiResponseMessage
          {
            Data = new EntityData { Fields = fields, Data = items },
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
    /// Save entity registration information
    /// </summary>
    /// <param name="key">unique key of entity for which data need to save</param>
    /// <param name="entity">entity type</param>
    /// <param name="data">collection of data which need to save</param>
    /// <returns>Response object which contains saved entity or exception information if it fails</returns>
    [HttpPut]
    [Route("api/entity/save/{entity}/{key}")]
    public ActionResult<ApiResponseMessage> Save(string key, string entity, [FromBody]Dictionary<string, object> data)
    {
      if (SetAuthorizeToken())
      {
        try
        {
          var savedItem = Processor.SaveInJoin(entity, key, data);
          ApiResponseMessage responseMessage = new ApiResponseMessage
          {
            Data = savedItem,
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
    /// Create new entity registration
    /// </summary>
    /// <param name="entity">The entity type of the registration to be created</param>
    /// <returns>Response object which contains new created entity or exception information if it fails</returns>
    [HttpPost]
    [Route("api/entity/new/{entity}")]
    public ActionResult<ApiResponseMessage> New(string entity)
    {
      if (SetAuthorizeToken())
      {
        try
        {
          string itemKey = null;
          var savedItem = Processor.NewItem(entity, out itemKey);
          ApiResponseMessage responseMessage = new ApiResponseMessage
          {
            Data = savedItem,
            Key = itemKey,
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

  /// <summary>
  /// 
  /// </summary>
  public class EntityData
  {
    public List<FormlyFieldConfig> Fields;
    public Dictionary<string, object> Data;
  }
}
