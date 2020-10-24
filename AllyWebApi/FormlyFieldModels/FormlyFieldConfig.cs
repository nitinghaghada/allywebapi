using System;
using System.Collections.Generic;

namespace AllyWebApi.FormlyFieldModels
{
  /// <summary>
  /// 
  /// </summary>
  public class FormlyFieldConfig
  {
    public string key { get; set; }
    public string type { get; set; }
    public string className { get; set; }
    public string template { get; set; }
    public string wrapper { get; set; }
    public object defaultValue { get; set; }
    public TemplateOptions templateOptions { get; set; }
    public Validators validators { get; set; }
    public bool customField { get; set; }
    public Dictionary<string, string> expressionProperties { get; set; }
  }
}
