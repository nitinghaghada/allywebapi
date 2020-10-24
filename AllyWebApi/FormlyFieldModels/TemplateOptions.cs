namespace AllyWebApi.FormlyFieldModels
{
  /// <summary>
  /// 
  /// </summary>
  public class TemplateOptions
  {
    public string label { get; set; }
    public string placeholder { get; set; }
    public bool required { get; set; }
    public string type { get; set; }
    public string labelProp { get; set; }
    public string valueProp { get; set; }
    public bool disabled { get; set; }
    public bool readOnly { get; set; }
    public Options[] options { get; set; }

    public string changeExpr { get; set; }

  }
}
