using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using AllyWebApi.FormlyFieldModels;
using Decos.ServiceContext;
using Decos.ServiceContext.Interfaces;
using Decos.ServiceContext.JoinApiService;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using BusinessModel.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Caching.Memory;

namespace AllyWebApi
{
  /// <summary>
  /// 
  /// </summary>
  public class Processor : IProcessor
  {
    private ServiceContext _service;
    private readonly IConfiguration _configuration;
    readonly ILogger _log;
    private readonly IMemoryCache _cache;
    public Processor(IConfiguration configuration, IMemoryCache cache, ILogger log)
    {
      _configuration = configuration;
      _cache = cache;
      _log = log;
    }


    #region private functions
    private string FieldType(JoinItemProfileField field)
    {
      string fieldType = "input";
      try
      {
        if (field.field.StartsWith("NUM", StringComparison.InvariantCultureIgnoreCase)
            || field.field.IndexOf("SEQUENCE", StringComparison.InvariantCultureIgnoreCase) >= 0
            || field.field.Equals("ACTIVITYDUEDAYS", StringComparison.InvariantCultureIgnoreCase))
          fieldType = "number";
        else if (field.field.Equals("ITEM_CREATED", StringComparison.InvariantCultureIgnoreCase)
                 || field.field.EndsWith("_DELETED", StringComparison.InvariantCultureIgnoreCase)
                 || field.field.StartsWith("DATE", StringComparison.InvariantCultureIgnoreCase)
                 || field.field.EndsWith("_DATE", StringComparison.InvariantCultureIgnoreCase))
          fieldType = "datepicker";
        else if (field.field.StartsWith("BOL", StringComparison.InvariantCultureIgnoreCase)
                 || field.field.Equals("PROCESSED", StringComparison.InvariantCultureIgnoreCase)
                 || field.field.Equals("ARCHIVED", StringComparison.InvariantCultureIgnoreCase))
          fieldType = "checkbox";
        else if (!string.IsNullOrEmpty(field.tableKey))
          fieldType = "select";
      }
      catch (Exception e)
      {
        //Logger.ErrorFormat($"Error occured while executing FieldType {e}");
      }
      return fieldType;
    }
    #endregion
    #region public functions

    /// <summary>
    /// Gets the service context object
    /// </summary>
    public IServiceContext ServiceContext => _service ?? (_service = new ServiceContext(_configuration));

    /// <summary>
    /// 
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public List<FormlyFieldConfig> GetMetaData(string entity)
    {
      List<FormlyFieldConfig> formlyFieldConfigs = new List<FormlyFieldConfig>();
      try
      {
        var bookProfileKey = _configuration[entity + ":JoinProfileKey"];
        if (!string.IsNullOrEmpty(bookProfileKey))
        {
          JoinBookProfile itemProfile = ServiceContext.JoinApiService.GetBookProfile(bookProfileKey);
          formlyFieldConfigs = ToFormlyFieldConfig(itemProfile, entity);
        }
      }
      catch (Exception e)
      {
        //Logger.ErrorFormat($"Error occured while executing GetMetaData {e}");
        //Microsoft.Extensions.Logging.DebugLoggerFactoryExtensions.AddDebug()
        throw e;
      }
      return formlyFieldConfigs;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="itemProfile"></param>
    /// <param name="entity"></param>
    /// <returns></returns>
    private List<FormlyFieldConfig> ToFormlyFieldConfig(JoinBookProfile itemProfile, string entity)
    {
      List<FormlyFieldConfig> formlyFieldConfigs = new List<FormlyFieldConfig>();

      bool workflowLinkInstance = String.Compare(itemProfile.itemType, "WORKFLOWLINKINSTANCE", StringComparison.OrdinalIgnoreCase) == 0;
      var readOnlyFields = _configuration[entity + ":ReadOnlyFields"];
      foreach (JoinItemProfileField field in itemProfile.itemFields.Where(obj => obj.limitedEntryField).ToList())
      {
        var property = entity + ":" + field.field;
        if (!string.IsNullOrEmpty(_configuration[property]))
        {
          FormlyFieldConfig formlyFieldConfig = new FormlyFieldConfig { key = _configuration[property] };
          TemplateOptions templateOptions = new TemplateOptions
          {
            label = field.description,
            placeholder = field.description,
            required = field.mandatory,
            disabled = field.readOnly,
            readOnly = field.readOnly
          };

          //TODO : this will change to set field property based on user rights
          if (readOnlyFields != null && readOnlyFields.IndexOf("|" + formlyFieldConfig.key + "|", StringComparison.Ordinal) >= 0)
          {
            templateOptions.readOnly = true;
            templateOptions.disabled = true;
          }

          string fieldType = FieldType(field);
          if (!string.IsNullOrEmpty(field.tableKey))
          {
            var tableValues = ServiceContext.JoinApiService.GetTableValues(field.tableKey);
            templateOptions.options = tableValues.ToFormlyOptions();
          }

          if (fieldType.ToLower().Equals("number"))
          {
            formlyFieldConfig.type = "input";
            templateOptions.type = fieldType;
          }
          else
            formlyFieldConfig.type = fieldType;

          formlyFieldConfig.className = "col-6"; //default class to render form field 
          formlyFieldConfig.customField = false;
          formlyFieldConfig.templateOptions = templateOptions;
          formlyFieldConfigs.Add(formlyFieldConfig);
        }
      }

      return formlyFieldConfigs;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="jsonFile"></param>
    /// <param name="entity"></param>
    /// <returns></returns>
    public List<FormlyFieldConfig> VerifyAndUpdateJson(string jsonFile, string entity)
    {
      List<FormlyFieldConfig> metaData = null;
      bool bChanged = false;
      var data = GetMetaData(entity);
      using (var streamReader = new StreamReader(jsonFile, Encoding.UTF8))
      {
        var text = streamReader.ReadToEnd();
        metaData = JsonConvert.DeserializeObject<List<FormlyFieldConfig>>(text, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
        if (metaData != null && metaData.Any())
        {
          //Remove fields which are removed from new data list except custom template fields added in json (returned from join item profile)
          bChanged = (metaData.RemoveAll(m => !data.Exists(d => d.key == m.key) && m.template == null && m.customField == false) > 0);

          foreach (FormlyFieldConfig fieldConfig in data)
          {
            FormlyFieldConfig formlyFieldConfig = metaData.FirstOrDefault(fld => fld.key == fieldConfig.key);
            if (formlyFieldConfig != null)
            {
              if (!formlyFieldConfig.customField)
              {
                foreach (PropertyInfo pi in fieldConfig.GetType().GetProperties())
                {
                  if ("|classname|validators|changeExpr|customField|".IndexOf("|" + pi.Name + "|", StringComparison.OrdinalIgnoreCase) >=
                      0) continue;
                  PropertyInfo filePi = formlyFieldConfig.GetType().GetProperty(pi.Name);
                  if (filePi != null && (pi.GetValue(fieldConfig) != filePi.GetValue(formlyFieldConfig)))
                  {
                    filePi.SetValue(formlyFieldConfig, pi.GetValue(fieldConfig));
                  }
                }
              }
            }
            else
            {
              fieldConfig.className = "col-12";
              bChanged = true;
              metaData.Add(fieldConfig);
            }
          }
        }
      }
      if (bChanged)
      {
        var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase).Replace("file:\\", string.Empty);
        string newFile = Path.Combine(path, entity + DateTime.Now.ToString("yyyyMMddHHmmssffff") + ".json");
        File.Copy(jsonFile, newFile);

        var jsonData = File.ReadAllText(jsonFile);
        jsonData = JsonConvert.SerializeObject(metaData);
        File.WriteAllText(jsonFile, jsonData);
      }
      return metaData;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public IList<Dictionary<string, object>> GetList(string entity, int pageSize, int skip, out int count, string filter, string orderby = "")
    {
      //Dictionary<string, object>[] models = null;
      IList<Dictionary<string, object>> models = new List<Dictionary<string, object>>();
      count = 0;
      if (!string.IsNullOrEmpty(entity))
      {
        var bookKey = _configuration[entity + ":JoinBookKey"];
        var entityType = _configuration[entity + ":EntityType"];
        models = GetRelatedList(bookKey, entityType, pageSize, skip, out count, filter, orderby);
      }
      return models;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="itemId"></param>
    /// <returns></returns>
    public Dictionary<string, object> GetEntityForm(string entity, string itemId)
    {
      Dictionary<string, object> formData = null;
      if (!string.IsNullOrEmpty(entity))
      {
        formData = ServiceContext.AllyODataService.GetItem(entity, itemId);
      }
      return formData;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="itemId"></param>
    /// <returns></returns>
    public Dictionary<string, object> GetEntityFormFromJoin(string itemId)
    {
      Dictionary<string, object> formData = null;
      if (!string.IsNullOrEmpty(itemId))
      {
        var item = ServiceContext.JoinApiService.GetItem(itemId);
        formData = item.ToDictionary();
      }
      return formData;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="itemId"></param>
    /// <param name="fields"></param>
    /// <returns></returns>
    public Dictionary<string, object> GetEntityWithProfile(string entity, string itemId, out List<FormlyFieldConfig> fields)
    {
      Dictionary<string, object> formData = null;
      fields = null;
      if (!string.IsNullOrEmpty(itemId))
      {
        var item = ServiceContext.JoinApiService.GetItem(itemId);
        if (item != null)
        {
          formData = item.ToDictionary();
          if (item.Links != null && item.Links.Count > 0)
          {
            var profileUrlRef = item.Links.FirstOrDefault(l => l.Rel == "itemprofile")?.HRef;
            var profile = ServiceContext.JoinApiService.GetBookProfileUsingUrl(profileUrlRef);
            fields = ToFormlyFieldConfig(profile, entity);

            if (item.Fields != null)
            {
              //Fill outcome field options with decision table
              bool workflowInstanceType = String.Compare(item.Fields["itemtype_key"].ToString(), "WORKFLOWLINKINSTANCE", StringComparison.OrdinalIgnoreCase) == 0;

              if (workflowInstanceType && string.Compare(formData["mark"].ToString(), "Completed", StringComparison.OrdinalIgnoreCase) == 0)
              {
                fields?.Where(f => f.key != "mark").ToList().ForEach(x => x.templateOptions.disabled = true);
              }
              else
              {
                if (String.Compare(item.WorkflowLinkInstanceType, "ManualDecision", StringComparison.OrdinalIgnoreCase) == 0)
                {
                  var tableUrlRef = item.Links.FirstOrDefault(l => l.Rel == "table")?.HRef;
                  if (tableUrlRef != null)
                  {
                    var tableKey = tableUrlRef.Substring(tableUrlRef.LastIndexOf("/", StringComparison.Ordinal));
                    var tablevalues = ServiceContext.JoinApiService.GetTableValues(tableKey);

                    var field = fields?.FirstOrDefault(f => string.Compare(f.key, "text5", StringComparison.OrdinalIgnoreCase) == 0); //TODO : TEXT5 should be replaced with mapped property 
                    if (field != null)
                    {
                      field.templateOptions.options = tablevalues.ToFormlyOptions();
                      field.type = "select";
                      field.templateOptions.required = true;
                    }
                  }
                }
              }
            }
          }
        }

      }
      return formData;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="key"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    public string Save(string entity, string key, Dictionary<string, object> data)
    {
      if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(entity) && data != null)
      {
        if (data.Count > 0)
        {
          return ServiceContext.AllyODataService.UpdateItem(entity, key, data);
        }
      }

      return string.Empty;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="key"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    public Dictionary<string, object> SaveInJoin(string entity, string key, Dictionary<string, object> data)
    {
      if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(entity) && data != null)
      {
        if (data.Count > 0)
        {
          return ServiceContext.JoinApiService.UpdateItem(key, data.ToJoinItem()).ToDictionary();
        }
      }
      return null;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="mainItemKey"></param>
    /// <param name="entity"></param>
    /// <returns></returns>
    public IList<Dictionary<string, object>> GetRelatedList(string mainItemKey, string entity, int pageSize, int skip, out int count, string filter = "", string orderby="")
    {
      IList<Dictionary<string, object>> list = new List<Dictionary<string, object>>();
      count = 0;
      if (!string.IsNullOrEmpty(mainItemKey) && !string.IsNullOrEmpty(entity))
      {
        JoinItems items = ServiceContext.JoinApiService.GetLinkedItems(mainItemKey, entity, pageSize, skip, filter, orderby);
        if (items != null)
        {
          count = items.Count;
          foreach (var item in items.Items)
          {
            var model = item.ToDictionary();
            model.Add("itemkey", item.Key);
            list.Add(model);
          }
        }
      }
      return list;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="entity">AB_ACTIVE_ADMINISTRATOR</param>
    /// <param name="itemKey">workflowlinkinstances</param>
    /// <returns>tasks{counts,tasks}</returns>
    public Tasks GetTasks(string entity, string itemKey)
    {
      Tasks data = null;
      List<Task> modelslist = new List<Task>();
      if (!string.IsNullOrEmpty(entity))
      {
        JoinItems items = ServiceContext.JoinApiService.GetLinkedItems(entity, itemKey, 50, 0);
        if (items.Count > 0)
        {

          foreach (var item in items.Items)
          {
            Task taskitem = new Task();
            item.GetConvertedEntity("Task", taskitem);
            taskitem.ActivityType = item.Fields.ContainsKey("itemtype_key2") ? CommonUtility.ConvertNullAndTrim(item.Fields["itemtype_key2"]) : "";
            taskitem.ItemKey = item.Key;
            if (String.Compare(taskitem.ActivityType, "CHECKLIST", StringComparison.OrdinalIgnoreCase) != 0)
            {
              if (item.Fields.ContainsKey("bol1") && CommonUtility.ConvertNullToBool(item.Fields["bol1"]) &&
                  item.Fields.ContainsKey("num1") && Convert.ToInt16(item.Fields["num1"]) == 1) taskitem.ActivityType = "MANUALDECISION";
            }
            modelslist.Add(taskitem);
          }
          data = new Tasks
          {
            Count = items.Count,
            TaskItems = modelslist
          };
        }
      }
      return data;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="itemKey"></param>
    /// <returns></returns>
    public JoinItem GetTask(string itemKey)
    {
      JoinItem item = ServiceContext.JoinApiService.GetItem(itemKey);
      if (item != null)
      {
        if (String.Compare(item.WorkflowLinkInstanceType, "ManualDecision", StringComparison.OrdinalIgnoreCase) == 0)
        {
          var tableUrlRef = item.Links.FirstOrDefault(l => l.Rel == "table")?.HRef;
          if (tableUrlRef != null)
          {
            var tableKey = tableUrlRef.Substring(tableUrlRef.LastIndexOf("/", StringComparison.Ordinal));
            List<Dictionary<string, string>> tablevalues = ServiceContext.JoinApiService.GetTableValues(tableKey);
            item.Outcomes = tablevalues;
          }
        }
      }
      return item;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <param name="item"></param>
    /// <returns></returns>
    public bool HandleTask(string key, JoinItem item)
    {
      bool handle = false;
      if (item != null)
      {
        try
        {
          if (!string.IsNullOrEmpty(item.workflowHandleComment))
            ServiceContext.JoinApiService.AddItemNotes(item.Fields["url"].ToString(), item.workflowHandleComment);
        }
        catch (Exception e)
        {
          Console.WriteLine(e);
        }

        if (String.Compare(item.WorkflowLinkInstanceType, "ManualDecision", StringComparison.OrdinalIgnoreCase) == 0)
        {
          if(!string.IsNullOrEmpty(item.DecisionOutcome))
          {
            item.Fields["text5"] = item.DecisionOutcome;
            handle = true;
          }
        }
        else handle = true;

        if (handle)
        {
          item.Fields["mark"] = "HANDLED";
          if (item.Fields.ContainsKey("date2") && (item.Fields["date2"] == null || Convert.ToDateTime(item.Fields["date2"]) == DateTime.MinValue))
            item.Fields["date2"] = DateTime.Now;
          item = ServiceContext.JoinApiService.UpdateItem(key, item);
          handle = (String.Compare(item.Fields["mark"].ToString(), "Afgehandeld", StringComparison.OrdinalIgnoreCase) == 0 || String.Compare(item.Fields["mark"].ToString(), "HANDLED", StringComparison.OrdinalIgnoreCase) == 0);
        }

      }
      return handle;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="itemKey"></param>
    /// <returns></returns>
    public IList<Dictionary<string, object>> GetLinkedWorkflowTasks(string entity, string itemKey)
    {
      List<Dictionary<string, object>> models = new List<Dictionary<string, object>>();
      string relItemType = "WORKFLOWINSTANCE";
      if (!string.IsNullOrEmpty(itemKey))
      {
        JoinItems items = ServiceContext.JoinApiService.GetBookItems(entity, itemKey, 50, 0);
        List<JoinItem> joinItems = new List<JoinItem>();
        foreach (JoinItem item in items.Items)
        {
          var workflowLinks = ServiceContext.JoinApiService.GetBookItemsWithFilters("WORKFLOWLINKINSTANCE", item.Key, "bol1 eq null or bol1 eq true");
          if (workflowLinks?.Items != null && workflowLinks.Items.Count > 0)
            joinItems.AddRange(workflowLinks.Items);
        }
        items = new JoinItems() { Items = joinItems };
        Dictionary<string, object> model = null;
        foreach (JoinItem item in items.Items)
        {
          model = item.ToDictionary();
          model.Add("itemkey", item.Key);
          models.Add(model);
        }
      }
      return models;

    }

    public IList<ItemNote> GetComments(string key)
    {
      IList<ItemNote> notes = new List<ItemNote>();
      if (!string.IsNullOrEmpty(key))
      {
        notes = ServiceContext.JoinApiService.ItemNotes(key);
      }
      return notes;
    }

    public bool AddNote(string key, string note)
    {
      bool result = false;
      if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(note))
      {
        ServiceContext.JoinApiService.AddItemNotes(key, note);
        result = true;
      }
      return result;
    }

    public IList<Dictionary<string, object>> GetStatusWiseEntityCount(string entityKey, string entityType, string statusField, string status)
    {
      IList<Dictionary<string, object>> data = new List<Dictionary<string, object>>();
      if (!string.IsNullOrEmpty(entityKey) && !string.IsNullOrEmpty(status))
      {
        var statuses = status.Split('|');
        if (statuses.Length > 0)
        {
          foreach (var state in statuses)
          {
            var stateCondition = statusField + " eq '" + state + "'";
            var items = ServiceContext.JoinApiService.GetBookItemsWithFilters(entityType, entityKey, stateCondition);
            if (items != null)
            {
              Dictionary<string, object> dict = new Dictionary<string, object>();
              dict.Add("name", state);
              dict.Add("value", items.Count);
              data.Add(dict);
            }
          }
        }
      }
      return data;
    }

    private object GetPropertyValue(object obj, string propertyName)
    {
      PropertyInfo property = obj.GetType().GetProperty(propertyName);
      return property.GetValue(obj);
    }

    public IList<Dictionary<string, object>> GetMonthlyEntityCountGroupByStatus(string entityKey, string entityType, string statusField, string status, string dateField)
    {
      IList<Dictionary<string, object>> data = new List<Dictionary<string, object>>();
      if (!string.IsNullOrEmpty(entityKey) && !string.IsNullOrEmpty(status))
      {
        var statuses = status.Split('|');
        if (statuses.Length > 0)
        {
          foreach (var state in statuses)
          {
            var stateCondition = statusField + " eq '" + state + "'";
            string[] months = {"Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"};
            for(var i=0; i< months.Length ; i++)
            {
              var startDate = new DateTime(DateTime.Now.Year, i + 1, 1);
              var endDate = startDate.AddMonths(1).AddDays(-1);

              stateCondition = statusField + " eq '" + state + "' and " + dateField + " gt '" + startDate + "' and " + dateField + " lt '" + endDate + "'";
              var items = ServiceContext.JoinApiService.GetBookItemsWithFilters(entityType, entityKey, stateCondition);

              if (items != null)
              {
                Dictionary<string, object> dict = new Dictionary<string, object>
                {
                  { "name", months[i] },
                  { "value", items.Count }// ? 0 : filteredItems.Count()}
                };
                data.Add(dict);
              }
            }

          }
        }
      }
      return data;
    }

    public UserSession SignInUsingOAuth(string code, out RESTToken token)
    {
      UserSession session = null;
      token = ServiceContext.JoinApiService.GetAuthToken(code);
      
      if (token != null)
      {
        session = ServiceContext.JoinApiService.GetUserSession();
        //if(session != null)
        //  session.token = token;
      }
      return session;
    }

    /// <summary>
    /// Create new entity registration
    /// </summary>
    /// <param name="entity">The entity type of the registration to be created</param>
    /// <returns>Dictionary key value pair which contains new created entity information</returns>
    public Dictionary<string, object> NewItem(string entity, out string itemKey)
    {
      itemKey = null;
      if (!string.IsNullOrEmpty(entity))
      {
        var bookKey = _configuration[entity + ":JoinBookKey"];
        var type = _configuration[entity + ":EntityType"];
        if (!string.IsNullOrEmpty(bookKey))
        {
          var item = ServiceContext.JoinApiService.NewItem(bookKey, type, null);
          if (item != null)
          {
            itemKey = item.Key;
            return item.ToDictionary();
          }
        }
      }
      return null;
    } 
    #endregion

  }
}
