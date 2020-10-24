using System.Collections.Generic;
using AllyWebApi.FormlyFieldModels;
using BusinessModel.Models;
using Decos.ServiceContext.Interfaces;
using Decos.ServiceContext.JoinApiService;
using Microsoft.AspNetCore.Mvc;

namespace AllyWebApi
{
  /// <summary>
  /// 
  /// </summary>
  public interface IProcessor
  {
    IServiceContext ServiceContext { get; }
    List<FormlyFieldConfig> GetMetaData(string entity);
    List<FormlyFieldConfig> VerifyAndUpdateJson(string jsonFile, string entity);
    IList<Dictionary<string, object>> GetList(string entity, int pageSize, int skip, out int count, string filter, string orderby = "");
    Dictionary<string, object> GetEntityForm(string entity, string itemId);
    Dictionary<string, object> GetEntityFormFromJoin(string itemId);
    IList<Dictionary<string, object>> GetStatusWiseEntityCount(string entityKey, string entityType, string statusField, string status);
    UserSession SignInUsingOAuth(string code, out RESTToken token);
    Dictionary<string, object> GetEntityWithProfile(string entity, string itemId, out List<FormlyFieldConfig> fields);
    string Save(string entity, string key, Dictionary<string, object> data);
    Dictionary<string, object> SaveInJoin(string entity, string key, Dictionary<string, object> data);
    IList<Dictionary<string, object>> GetRelatedList(string mainItemKey, string entity, int pageSize, int skip, out int count, string filter = "", string orderby="");
    IList<Dictionary<string, object>> GetLinkedWorkflowTasks(string entity, string key);
    IList<Dictionary<string, object>> GetMonthlyEntityCountGroupByStatus(string entityKey, string entityType, string statusField, string status, string dateField);
    Tasks GetTasks(string entity, string key);
    JoinItem GetTask(string key);
    bool HandleTask(string key, JoinItem item);
    IList<ItemNote> GetComments(string key);
    bool AddNote(string key, string note);
    Dictionary<string, object> NewItem(string entity, out string itemKey);
  }
}