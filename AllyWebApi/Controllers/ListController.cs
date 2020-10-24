using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using AllyWebApi.FormlyFieldModels;
using BusinessModel.Models;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using ActionResult = BusinessModel.Models.ActionResult;

namespace AllyWebApi.Controllers
{
  public class ListController : BaseController
  {
    /// <summary>
    /// 
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("api/entity/list/{entity}/{pageSize}/{skip}")]
    public ActionResult<ApiResponseMessage> Get(string entity, int pageSize, int skip, [FromQuery] string filter, [FromQuery] string orderby)
    {
      if (SetAuthorizeToken())
      {
        try
        {
          var items = Processor.GetList(entity, pageSize, skip, out var count, filter, orderby).ToArray();

          ApiResponseMessage responseMessage = new ApiResponseMessage
          {
            Data = items,
            Count = count,
            Result = ActionResult.Success
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
        }
      }
      return UnauthorizedResponse();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="mainItemKey"></param>
    /// <param name="entity"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("api/entity/rellist/{mainItemKey}/{entity}/{pageSize}/{skip}")]
    public ActionResult<ApiResponseMessage> GetRelatedList(string mainItemKey, string entity, int pageSize, int skip)
    {
      if (SetAuthorizeToken())
      {
        try
        {
          int count = 0;
          var items = String.Compare(entity, "WORKFLOWINSTANCES", StringComparison.OrdinalIgnoreCase) != 0
            ? Processor.GetRelatedList(mainItemKey, entity, pageSize, skip, out count)
            : Processor.GetLinkedWorkflowTasks(entity, mainItemKey);

          ApiResponseMessage responseMessage = new ApiResponseMessage
          {
            Data = items.ToArray(),
            Count = count,
            Result = ActionResult.Success
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
    /// <returns></returns>
    [HttpGet]
    [Route("api/entity/metadata/{entity}")]
    public ActionResult<ApiResponseMessage> GetMetaData(string entity)
    {
      if (SetAuthorizeToken())
      {
        var path = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase).Replace("file:\\", string.Empty);
        var jsonFile = Path.Combine(path, entity + ".json");
        try
        {
          if (!System.IO.File.Exists(jsonFile))
          {
            var data = Processor.GetMetaData(entity).ToArray();
            string jsonString = JsonConvert.SerializeObject(data);
            try
            {
              // If file exists, seek to the end of the file,
              // else create a new one.
              FileStream fileStream = System.IO.File.Open(jsonFile,
                FileMode.Append, FileAccess.Write);
              // Encapsulate the filestream object in a StreamWriter instance.
              StreamWriter fileWriter = new StreamWriter(fileStream);
              // Write the json string to the file
              fileWriter.WriteLine(jsonString);
              fileWriter.Flush();
              fileWriter.Close();
            }
            catch (IOException ioe)
            {
              Log.LogError(ioe, ioe.Message);
            }
          }

          List<FormlyFieldConfig> metaData = null;
          if (System.IO.File.Exists(jsonFile))
          {
            metaData = Processor.VerifyAndUpdateJson(jsonFile, entity);
          }
          ApiResponseMessage responseMessage = new ApiResponseMessage
          {
            Data = metaData,
            Result = ActionResult.Success
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
    /// <param name="value"></param>
    // POST api/values
    [HttpPost]
    [Route("api/entity/list/save")]
    public void Post([FromBody]string value)
    {
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <param name="value"></param>
    // PUT api/values/5
    [HttpPut]
    [Route("api/entity/list/update/{id}")]
    public void Put(int id, [FromBody]string value)
    {
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="configuration"></param>
    public ListController(IConfiguration configuration, ILogger<BaseController> log, IMemoryCache cache) : base(configuration, log, cache)
    {
    }

  }
}
