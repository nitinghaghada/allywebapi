using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using Decos.ServiceContext.Interfaces;
using Decos.ServiceContext.JoinApiService;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace Decos.ServiceContext.AllyOdataService
{
  /// <summary>
  /// 
  /// </summary>
  internal class AllyOdataService : IAllyOdataService
  {
    private readonly IConfiguration _configuration;
    private const string AllySetting = "AllyApi";
    private JsonSerializerSettings settings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };

    /// <summary>
    /// 
    /// </summary>
    /// <param name="configuration"></param>
    public AllyOdataService(IConfiguration configuration)
    {
      _configuration = configuration;
    }

    private string ApiUrl => _configuration[AllySetting + ":ApiUrl"];

    #region public methods
    /// <summary>
    /// 
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="itemId"></param>
    /// <returns></returns>
    public Dictionary<string, object> GetItem(string entity, string itemId)
    {
      string url = string.Format(@"/{0}({1})", entity, itemId);
      var data = Get(url, true);
      var item = JsonConvert.DeserializeObject<Dictionary<string, object>>(data, settings);
      if (item != null && item.Count > 0)
      {
        var data1 = item["d"].ToString().TrimStart('[').TrimEnd(']');
        item = JsonConvert.DeserializeObject<Dictionary<string, object>>(data1, settings);
      }
      return item;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public Dictionary<string, object>[] GetEntityData(string entity)
    {
      string url = string.Format(@"/{0}", entity);
      Dictionary<string, object>[] items = GetItems(url);
      return items;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="filter"></param>
    /// <returns></returns>
    public Dictionary<string, object>[] GetEntityData(string entity, string filter)
    {
      string url = string.Format(@"/{0}?$filter={1}", entity, filter);
      Dictionary<string, object>[] items = GetItems(url);
      return items;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    private Dictionary<string, object>[] GetItems(string url)
    {
      Dictionary<string, object>[] items = null;

      var data = Get(url, true);
      var item = JsonConvert.DeserializeObject<Dictionary<string, object>>(data, settings);
      if (item != null && item.Count > 0)
      {
        var data1 = item["d"].ToString();
        items = JsonConvert.DeserializeObject<Dictionary<string, object>[]>(data1, settings);
      }
      return items;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="parententity"></param>
    /// <param name="entity"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    public Dictionary<string, object>[] GetRelatedBookItems(string parententity, string entity, string key)
    {
      Dictionary<string, object>[] items = null;
      string url = string.Format(@"/{0}({1})?$expand={2}", parententity, key, entity);
      var data = Get(url, true);
      var item = JsonConvert.DeserializeObject<Dictionary<string, object>>(data, settings);
      if (item != null && item.Count > 0)
      {
        var data1 = item["d"].ToString().TrimStart('[').TrimEnd(']');
        var data2 = JsonConvert.DeserializeObject<Dictionary<string, object>>(data1, settings);
        if (data2 != null && data2.Count > 0)
        {
          items = JsonConvert.DeserializeObject<Dictionary<string, object>[]>(data2[entity].ToString(), settings);
        }
      }
      return items;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="formData"></param>
    /// <returns></returns>
    public string CreateItem(string entity, Dictionary<string, object> formData)
    {
      if (formData == null)
        return null;
      string content = JsonConvert.SerializeObject(formData, settings);
      return CreateItem(entity, content);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="content"></param>
    /// <returns></returns>
    public string CreateItem(string entity, string content)
    {
      if (string.IsNullOrEmpty(content) || string.IsNullOrEmpty(entity))
        return null;
      string url = string.Format("/{0}", entity);
      var data = Post(url, true, content, RequestContentType.Json);
      return data;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="itemId"></param>
    /// <param name="entity"></param>
    /// <param name="formData"></param>
    /// <returns></returns>
    public string UpdateItem(string itemId, string entity, Dictionary<string, object> formData)
    {
      if (formData == null)
        return null;
      string content = JsonConvert.SerializeObject(formData, settings);
      return UpdateItem(itemId, entity, content);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="itemId"></param>
    /// <param name="entity"></param>
    /// <param name="content"></param>
    /// <returns></returns>
    public string UpdateItem(string itemId, string entity, string content)
    {
      if (string.IsNullOrEmpty(content) || string.IsNullOrEmpty(entity))
        return null;
      string url = string.Format("/{0}({1})", entity, itemId);
      var data = Patch(url, true, content, RequestContentType.Json);
      return data;
    }

    #endregion public methods

    #region private methods
    private string Get(string requestUrl, bool needsAuthHeaders)
    {
      if (string.IsNullOrEmpty(requestUrl))
        throw new ArgumentNullException("requestUrl");

      HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requestUrl.StartsWith(ApiUrl) ? requestUrl : ApiUrl + requestUrl);
      request.Method = "GET";

      //if (needsAuthHeaders)
      //  request.Headers["Authorization"] = string.Format(BearerAuthorization, AccessToken);

      HttpWebResponse response = (HttpWebResponse)request.GetResponse();
      var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
      response.Close();
      return responseString;
    }
    private string Post(string requestUrl, bool needsAuthHeaders, string postData, RequestContentType dataType)
    {
      return Request("POST", requestUrl, needsAuthHeaders, postData, dataType);
    }

    private string Patch(string requestUrl, bool needsAuthHeaders, string postData, RequestContentType dataType)
    {
      return Request("PATCH", requestUrl, needsAuthHeaders, postData, dataType);
    }

    private string Request(string requestMethod, string requestUrl, bool needsAuthHeaders, string postData, RequestContentType dataType)
    {
      if (string.IsNullOrEmpty(requestUrl))
        throw new ArgumentNullException("requestUrl");

      var request = (HttpWebRequest)WebRequest.Create(ApiUrl + requestUrl);

      var data = Encoding.ASCII.GetBytes(postData);
      request.Method = requestMethod;
      //if (needsAuthHeaders)
      //  request.Headers["Authorization"] = string.Format(BearerAuthorization, AccessToken);
      if (dataType == RequestContentType.UrlEncoded || dataType == RequestContentType.None)
        request.ContentType = "application/x-www-form-urlencoded";
      else if (dataType == RequestContentType.Json)
        request.ContentType = "application/json";
      request.ContentLength = data.Length;
      using (var stream = request.GetRequestStream())
      {
        stream.Write(data, 0, data.Length);
      }
      var response = (HttpWebResponse)request.GetResponse();
      var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();

      response.Close();
      return responseString;
    }


    #endregion
  }
}