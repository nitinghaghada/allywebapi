using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BusinessModel.Models;
using Decos.ServiceContext.JoinApiService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace AllyWebApi.Controllers
{
  public class WorkflowController : BaseController
  {
    public WorkflowController(IConfiguration configuration, ILogger<BaseController> log, IMemoryCache cache) : base(configuration, log, cache)
    {
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("api/entity/tasks/{entity}/{key}")]
    public ActionResult<Tasks> GetTasks(string entity, string key)
    {
      Tasks item = null;
      if (SetAuthorizeToken())
      {
        item = Processor.GetTasks(entity, key);
      }

      return item;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("api/entity/task/{key}")]
    public ActionResult<JoinItem> GetTask(string key)
    {
        JoinItem item = null;
      if (SetAuthorizeToken())
      {
        item = Processor.GetTask(key);
      }

      return item;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="key">Join item key of workflow task</param>
    /// <param name="item"> Join workflow task item</param>
    /// <returns></returns>
    [HttpPut]
    [Route("api/handle/task/{key}")]
    public ActionResult<bool> HandleTask(string key, [FromBody]JoinItem item)
    {
      if (SetAuthorizeToken())
      {
        var handled = Processor.HandleTask(key, item);
        return handled;
      }

      return Unauthorized();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="key">Join item key of workflow task</param>
    /// <param name="note"> Join workflow task item</param>
    /// <returns></returns>
    [HttpPut]
    [Route("api/comments/task/{key}")]
    public ActionResult<bool> AddNote(string key, [FromBody]string note)
    {
      if (SetAuthorizeToken())
      {
        var result = Processor.AddNote(key, note);
        return result;
      }

      return Unauthorized();
    }

    [HttpGet]
    [Route("api/comments/task/{key}")]
    public ActionResult<IList<ItemNote>> GetComments(string key)
    {
      if (SetAuthorizeToken())
      {
        var notes = Processor.GetComments(key).ToArray();
        return notes;
      }

      return Unauthorized();
    }
  }
}
