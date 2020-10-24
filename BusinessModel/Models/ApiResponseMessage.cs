using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace BusinessModel.Models
{
  public class ApiResponseMessage
  {
    public object Data;
    public string Key;
    public int Count;
    public ActionResult Result;
    public string Message { get; set; }
    public int StatusCode { get; set; }
    public string StatusDescription { get; set; }
  }

  public enum ActionResult
  {
    Success = 1,
    Fail = 0,
    Exception =2
  }
}
