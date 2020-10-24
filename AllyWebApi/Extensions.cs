using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AllyWebApi.FormlyFieldModels;
using Decos.ServiceContext.JoinApiService;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AllyWebApi
{
  public static class Extensions
  {
    /// <summary>
    /// 
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public static Dictionary<string, object> ToDictionary(this JoinItem item)
    {
      var model = new Dictionary<string, object>();
      try
      {
        if (item != null && item.Fields != null)
        {
          bool isWorkflowInstanceType = String.Compare(item.Fields["itemtype_key"].ToString(), "WORKFLOWLINKINSTANCE", StringComparison.OrdinalIgnoreCase) == 0;
          foreach (KeyValuePair<string, object> kvp in item.Fields)
          {
            if (kvp.Value != null)
            {
              var value = kvp.Value;
              if (String.Compare(kvp.Key,"MARK", StringComparison.OrdinalIgnoreCase) == 0 && isWorkflowInstanceType)
              {
                if (String.Compare(value.ToString(), "Actief", StringComparison.OrdinalIgnoreCase) == 0)
                  value = "Active";
                else if (String.Compare(value.ToString(), "Afgehandeld", StringComparison.OrdinalIgnoreCase) == 0)
                  value = "Completed";
              }
              model.Add(kvp.Key.ToLower(), value);
            }
          }
        }
      }
      catch (Exception e)
      {
        //Logger<>.ErrorFormat($"Error occured while executing ToDictionary : {e}");
        throw;
      }
      return model;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="values"></param>
    /// <returns></returns>
    public static Options[] ToFormlyOptions(this List<Dictionary<string, string>> values)
    {
      Options []options = null;
      try
      {
        if (values != null && values.Count > 0)
        {
          options = new Options[values.Count];
          int iCount = 0;
          foreach (var dict in values)
          {
            options[iCount] = new Options { label = dict["key"], value = dict["value"] };
            iCount++;
          }
        }
      }
      catch (Exception e)
      {
        //Console.WriteLine(e);
        throw;
      }
      return options;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="itemData"></param>
    /// <param name="bookKey"></param>
    /// <returns></returns>
    public static JoinItem ToJoinItem(this Dictionary<string, object> itemData, string bookKey = "")
    {
      try
      {
        var itemItem = new JoinItem();
        itemItem.Fields = new Dictionary<string, object>();
        var itemData1 = !string.IsNullOrEmpty(bookKey) ? itemData.Where(obj => obj.Key.Contains(bookKey)).ToList() : itemData.ToList();
        foreach (KeyValuePair<string, object> kvp in itemData1)
        {
          string[] keypair = kvp.Key.Split(new char[] { '_' }, StringSplitOptions.RemoveEmptyEntries);
          if (itemItem.Fields.ContainsKey("parentkey") && string.IsNullOrEmpty(itemItem.Fields["parentkey"].ToString()) && keypair.Length == 2)
            itemItem.Fields["parentkey"] = keypair[1];

          //PropertyInfo pi = itemItem.Fields.GetType().GetProperty(keypair[0]);
          //if (pi != null)
          {
            itemItem.Fields.Add(kvp.Key, kvp.Value);
          }
        }
        return itemItem;
      }
      catch (Exception e)
      {
        //Logger<>.ErrorFormat($"Error occured while executing ToJoinItem : {e}");
        throw;
      }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="itemData"></param>
    /// <param name="entityType"></param>
    /// <param name="outObject"></param>
    /// <returns></returns>
    public static T GetConvertedEntity<T>(this JoinItem itemData, string entityType, T outObject)
    {
        try
        {
            var path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase).Replace("file:\\", string.Empty);
            var jsonFile = System.IO.Path.Combine(path, entityType + ".json");
            if (File.Exists(jsonFile))
            {
                string jsonData = string.Empty;
                using (var streamReader = new StreamReader(jsonFile, Encoding.UTF8))
                {
                    jsonData = streamReader.ReadToEnd();
                    List<ModelMapper> ifsMapper = JsonConvert.DeserializeObject<List<ModelMapper>>(jsonData, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                    if (ifsMapper != null && ifsMapper.Count > 0)
                    {
                        foreach (var field in itemData.Fields)
                        {
                            Type myType = typeof(T);
                            string dbField = field.Key;
                            string propertyName = string.Empty;
                            var fieldDetails = ifsMapper.FirstOrDefault(i => String.Compare(i.Dbfield, dbField, StringComparison.OrdinalIgnoreCase) == 0);

                            if (fieldDetails != null)
                            {
                                propertyName = fieldDetails.PropertyName;
                                PropertyInfo propertyInfo = myType.GetProperty(propertyName);
                                if (propertyInfo != null)
                                {
                                    if (propertyInfo.PropertyType == typeof(decimal))
                                    {
                                        propertyInfo.SetValue(outObject, Convert.ToDecimal(field.Value));
                                    }
                                    else if(propertyInfo.PropertyType == typeof(Int32))
                                      propertyInfo.SetValue(outObject, CommonUtility.ConvertNullToInt32(field.Value));
                                    else
                                        propertyInfo.SetValue(outObject, field.Value);
                                }
                            }
                        }
                    }
                }
            }

        }
        catch (Exception e)
        {
            throw;
        }
        return outObject;
    }
  }
}
