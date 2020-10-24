using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Xml;
using BusinessModel.Models;
using Decos.ServiceContext.Interfaces;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Decos.ServiceContext.JoinApiService
{
  /// <summary>
  /// 
  /// </summary>
  public enum RequestContentType
  {
    None,
    UrlEncoded,
    Json
  }

  /// <summary>
  /// 
  /// </summary>
  internal class JoinApiService : IJoinApiService
  {
    private readonly IConfiguration _configuration;
    private string _redirectUrl = "https://getpostman.com/oauth2/callback";
    private string BearerAuthorization = "Bearer {0}";
    private string _accessToken;
    private const string JoinSetting = "JoinApi";
    private readonly JsonSerializerSettings settings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
    private string _refreshToken = string.Empty;
    private string _tokenType;
    private string _expires;
    private string sessionUrl { get { return JoinApiUrl + "/aspx/api/v1/session"; } }

    public JoinApiService(IConfiguration configuration)
    {
      _configuration = configuration;
      //GetAccessToken();
    }

    #region private properties
    private string JoinApiUrl => _configuration[JoinSetting + ":ApiUrl"];
    private string RefreshToken
    {
      get => _refreshToken;
      set => _refreshToken = value;
    }
    private string ClientId => _configuration[JoinSetting + ":ClientId"];

    #endregion private properties

    #region public methods

    public UserSession GetAuthorizationCode()
    {
      string url = @"/auth/authorize";
      UserSession session = null;
      if (!string.IsNullOrEmpty(_accessToken))
      {
        var data = Get(url, true);
        session = JsonConvert.DeserializeObject<UserSession>(data, settings);
      }
      return session;
    }

    public UserSession GetUserSession()
    {
      string url = @"/session";
      UserSession session = null;
      if (!string.IsNullOrEmpty(_accessToken))
      {
        var data = Get(url, true);
        session = JsonConvert.DeserializeObject<UserSession>(data, settings);
      }
      return session;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="bookKey"></param>
    /// <param name="itemType"></param>
    /// <returns></returns>
    public JoinItems GetLinkedItems(string bookKey, string itemType, int pageSize, int skip, string filter = "", string orderBy = "")
    {
      return GetBookItems(itemType, bookKey, pageSize, skip, filter, orderBy);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="type"></param>
    /// <param name="parentBookId"></param>
    /// <returns></returns>
    public JoinItems GetBooks(string type, string parentBookId)
    {
      string url = string.Format(@"/items/{0}/containers", type.ToLowerInvariant());
      if (!string.IsNullOrEmpty(parentBookId))
      {
        url = string.Format(@"/items/{0}/containers", parentBookId);
      }
      var data = Get(url, true);
      JoinItems items = JsonConvert.DeserializeObject<JoinItems>(data, settings);
      return items;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public List<Dictionary<string, string>> GetTableValues(string id)
    {
      string url = string.Format(@"/tables/{0}/values?level=0", id);
      var data = Get(url, true);
      List<Dictionary<string, string>> tableValues = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(data, settings);
      return tableValues;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="type"></param>
    /// <param name="bookId"></param>
    /// <returns></returns>
    public JoinItems GetBookItems(string type, string bookId, int pageSize, int skip, string filter = "", string orderBy = "")
    {
      string url;
      if(string.IsNullOrEmpty(filter))
        url = string.Format(@"/items/{0}/{1}?oDataQuery.top={2}&oDataQuery.skip={3}&oDataQuery.orderBy={4}", bookId, type.ToLowerInvariant(), pageSize, skip, orderBy);
      else
        url = string.Format(@"/items/{0}/{1}?oDataQuery.top={2}&oDataQuery.skip={3}&oDataQuery.filter={4}&oDataQuery.orderBy={5}", bookId, type.ToLowerInvariant(), pageSize, skip, filter, orderBy);
      
      var data = Get(url, true);
      JoinItems items = JsonConvert.DeserializeObject<JoinItems>(data, settings);
      return items;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="type"></param>
    /// <param name="bookId"></param>
    /// <param name="filter"></param>
    /// <returns></returns>
    public JoinItems GetBookItemsWithFilters(string type, string bookId, string filter)
    {
      string url = string.Format(@"/items/{0}/{1}?properties=false&oDataQuery.filter={2}", bookId, type, filter);
      var data = Get(url, true);
      JoinItems items = JsonConvert.DeserializeObject<JoinItems>(data, settings);
      return items;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="itemId"></param>
    /// <returns></returns>
    public JoinItem GetItem(string itemId)
    {
      string url = string.Format(@"/items/{0}", itemId);
      var data = Get(url, true);
      JoinItem item = JsonConvert.DeserializeObject<JoinItem>(data, settings);
      return item;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="itemId"></param>
    /// <returns></returns>
    public JoinItems GetBlobItems(string itemId)
    {
      string url = string.Format(@"/items/{0}/blobs", itemId);
      var data = Get(url, true);
      JoinItems items = JsonConvert.DeserializeObject<JoinItems>(data, settings);
      return items;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="profileId"></param>
    /// <returns></returns>
    public JoinBookProfile GetBookProfile(string profileId)
    {
      string url = string.Format(@"/itemprofiles/{0}/form", profileId);
      var data = Get(url, true);
      JoinBookProfile item = JsonConvert.DeserializeObject<JoinBookProfile>(data, settings);
      return item;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    public JoinBookProfile GetBookProfileUsingUrl(string url)
    {
      var data = Get(url, true);
      JoinBookProfile item = JsonConvert.DeserializeObject<JoinBookProfile>(data, settings);
      return item;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="book"></param>
    /// <returns></returns>
    public JoinBookProfile GetBookProfile(JoinItem book)
    {
      if (book == null)
        return null;
      if (book.Links == null)
        return null;
      var profileLink = from link in book.Links where link.Rel == "itemprofile" select link;
      if (!profileLink.Any())
        return null;

      var profileUrl = profileLink.First().HRef;

      if (string.IsNullOrEmpty(profileUrl))
        return null;

      var data = Get(profileUrl, true);
      JoinBookProfile item = JsonConvert.DeserializeObject<JoinBookProfile>(data, settings);
      return item;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="parentBookId"></param>
    /// <param name="type"></param>
    /// <param name="itemToCreate"></param>
    /// <returns></returns>
    public JoinItem NewItem(string parentBookId, string type, JoinItem itemToCreate)
    {
      if (itemToCreate == null)
        return null;
      string content = JsonConvert.SerializeObject(itemToCreate, settings);
      string url = string.Format("/items/{0}/{1}", parentBookId, type.ToLowerInvariant());
      var data = Post(url, true, content, RequestContentType.Json);
      JoinItem item = JsonConvert.DeserializeObject<JoinItem>(data, settings);
      return item;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="itemId"></param>
    /// <param name="itemtoUpdate"></param>
    /// <returns></returns>
    public JoinItem UpdateItem(string itemId, JoinItem itemtoUpdate)
    {
      if (itemtoUpdate == null)
        return null;
      string content = JsonConvert.SerializeObject(itemtoUpdate, settings);
      string url = string.Format("/items/{0}", itemId);
      var data = Patch(url, true, content, RequestContentType.Json);
      JoinItem item = JsonConvert.DeserializeObject<JoinItem>(data, settings);
      return item;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="parentItemId"></param>
    /// <param name="fileDescription"></param>
    /// <param name="fileName"></param>
    /// <param name="fileContent"></param>
    /// <returns></returns>
    public JoinItem UploadNewFile(string parentItemId, string fileDescription, string fileName, byte[] fileContent)
    {
      if (string.IsNullOrEmpty(parentItemId) || string.IsNullOrEmpty(fileName) || fileContent == null)
        return null;

      JoinItem fileItem = new JoinItem();
      fileItem.Key = System.Guid.NewGuid().ToString("N").ToUpperInvariant();
      fileItem.Fields = new Dictionary<string, object>();
      //fileItem.Fields.text1 = fileDescription;
      fileItem.Fields.Add("text1", fileDescription);
      //fileItem.Fields.subject2 = fileName;
      fileItem.Fields.Add("subject2", fileName);

      string content = JsonConvert.SerializeObject(fileItem, settings);
      string url = string.Format("/items/{0}/BLOB", parentItemId);
      var data = Post(url, true, content, RequestContentType.Json);
      JoinItem fileItemCreated = JsonConvert.DeserializeObject<JoinItem>(data, settings);
      if (fileItemCreated == null)
        return null;
      if (string.IsNullOrEmpty(fileItemCreated.Key))
        return null;

      string fileUploadUrl = string.Format("/items/{0}/content", fileItemCreated.Key);

      Dictionary<string, object> postParameters = new Dictionary<string, object>();
      postParameters.Add("id", fileItemCreated.Key);
      postParameters.Add("postedFile", new FileParameter(fileContent, fileName));

      var data1 = RequestMultipartFormData("POST", fileUploadUrl, postParameters);
      return fileItemCreated;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="fileItemId"></param>
    /// <param name="fileName"></param>
    /// <param name="fileContent"></param>
    /// <returns></returns>
    public bool UpdateFile(string fileItemId, string fileName, byte[] fileContent)
    {
      if (string.IsNullOrEmpty(fileItemId) || string.IsNullOrEmpty(fileName) || fileContent == null)
        return false;

      string fileUploadUrl = string.Format("/items/{0}/content", fileItemId);

      Dictionary<string, object> postParameters = new Dictionary<string, object>();
      postParameters.Add("id", fileItemId);
      postParameters.Add("postedFile", new FileParameter(fileContent, fileName));

      RequestMultipartFormData("PUT", fileUploadUrl, postParameters);
      return true;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="itemId"></param>
    /// <param name="relItemType"></param>
    /// <param name="relItemId"></param>
    /// <returns></returns>
    public bool LinkItem(string itemId, string relItemType, string relItemId)
    {
      string relType = relItemType.ToUpperInvariant();
      if (string.IsNullOrEmpty(relType))
        return false;

      string url = string.Format(@"/items/{0}/{1}", itemId, relType);
      string postData = string.Format("'{0}'", relItemId);
      string data = Link(url, true, postData, RequestContentType.Json);
      if (string.IsNullOrEmpty(data))
        return false;
      if (data.EndsWith(string.Format(@"{0}""", url), StringComparison.OrdinalIgnoreCase))
        return true;

      return false;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="itemId"></param>
    /// <param name="noteText"></param>
    /// <returns></returns>
    public string AddItemNotes(string itemId, string noteText)
    {
      if (string.IsNullOrEmpty(noteText) && string.IsNullOrEmpty(itemId))
        return string.Empty;

      string url = string.Format(@"/items/{0}/itemnotes", itemId);
      string postData = string.Format("'{0}'", noteText);
      string data = Post(url, true, postData, RequestContentType.Json);
      return data;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="bookKey"></param>
    /// <param name="fieldName"></param>
    /// <returns></returns>
    public string GetItemType(string bookKey, string fieldName)
    {
      string url = string.Format("/items/{0}?select{1}", bookKey, fieldName);
      string data = Get(url, true);
      JoinItem item = JsonConvert.DeserializeObject<JoinItem>(data, settings);
      return item.Fields["itemtype_key2"].ToString();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public IList<ItemNote> ItemNotes(string key)
    {
      string url = string.Format("/items/{0}/itemnotes", key);
      string data = Get(url, true);
      var notes = JsonConvert.DeserializeObject<List<ItemNote>>(data, settings);
      return notes;
    }

    public RESTToken GetAuthToken(string code)
    {
      //string token = null;
      //string da = Get(AuthUrlWithClientID, true);
      //var code = "EB143A86C9AF27AA21AA9D913A357DB85B87B68B1B5A5C97162618E2D41562158152DF962718800F77DF49C734F64050830A0FA0A6E6AC2C714591E60E37CF3E49789652A8314F3D3C6F0F2EEA55BBAA0FDE501DB4054B1B60C6CA186047FD5468232A46D516AE36FDDB4C71F05976F0B6386F2607A4CFFE4B02501961189DC703BFC3457CDAAFF03CDF0B5A5CE626AE9FBA2B2B11127E363E80742C0B8024B3";
      string url = "/auth/token";
      string postData = string.Format("grant_type={0}&code={1}&client_id={2}&state={3}", "authorization_code", code, ClientId, "request-token");
      //string postData = string.Format("grant_type={0}&refresh_token={1}&client_id={2}&client_secret={3}", "refresh_token", System.Web.HttpUtility.UrlEncode(RefreshToken), ClientId, string.Empty);

      string data = Post(url, false, postData, RequestContentType.UrlEncoded);
      var token = JsonConvert.DeserializeObject<RESTToken>(data, settings);
      _accessToken = token.access_token;
      return token;
    }

    public bool SetAuthToken(RESTToken token)
    {
      if (token != null && !string.IsNullOrEmpty(token.access_token))
      {
        _accessToken = token.access_token;
        RefreshToken = token.refresh_token;
        return true;
      }
      return false;
    }
    #endregion

    #region private methods
    static HttpClient client = new HttpClient();

    private void GetAccessToken()
    {

      //string da = Get(AuthUrlWithClientID,true);
      //Code = "EB143A86C9AF27AA21AA9D913A357DB85B87B68B1B5A5C97162618E2D41562158152DF962718800F77DF49C734F64050830A0FA0A6E6AC2C714591E60E37CF3E49789652A8314F3D3C6F0F2EEA55BBAA0FDE501DB4054B1B60C6CA186047FD5468232A46D516AE36FDDB4C71F05976F0B6386F2607A4CFFE4B02501961189DC703BFC3457CDAAFF03CDF0B5A5CE626AE9FBA2B2B11127E363E80742C0B8024B3";
      //string url = "/auth/token";
      //string postData = string.Format("grant_type={0}&code={1}&client_id={2}", "authorization_code", Code, ClientID);
      //string data = Post(url, false, postData, RequestContentType.UrlEncoded);
      //JObject jData = JsonConvert.DeserializeObject(data, settings) as JObject;
      //if (jData != null)
      //{
      //TokenType = jData["token_type"]?.ToString();
      //Expires = jData["expires_in"]?.ToString();
      //AccessToken = jData["access_token"]?.ToString();
      //RefreshToken = jData["refresh_token"]?.ToString();
      // TokenType = "Bearer";
      //Expires = "2592000";
      //AccessToken = "kkqIYtVRlgWkqoqLoRD7c5+mL/Y0CXI71a1m9nZnAxrEWHktSrgxtNjsH0iSHF/hx/I+tYdfMAjrWfgNY0wghTtjzeufOAJozmx5CVNfL9dtGGy3ptr7uUU4G6+pkiCzGe/7lBnrkWCILaaSK6O7hnkwxT75AZnK3H0OAv/HLRqDnxmO2k2uA++Bpe/aN3zO3S6KAfvYrKWPEmdU612PEO/Kv+nXMIH43ztswuElsmc=6aORa1WxyrWzClglGv5Izw==";
      //RefreshToken = "ogUqOBxtDbfVO0Frino3hjmKYudog9//oaNjgSx+zZrg2jQYeoDBBEUKU4BIapnEzDPJoAa0fArMZftL/0Hyzt6BOtzqXNEb6T6bum3iyO87KhtFj9LlNrGe4zFEHsEFId9EnD1Bp3OqmCypgebC8kNWrQ81ynAKXexBO1LKsGZ0pUMzgV/SiiOdQvvnLGV3aF9t/O53A7mv/Err2fmRxVn9zI31U+qTTJqhdjtCnuwebZppaz3bZ7j4cBdq/UQfeCM1TFgcm4iCjv20bZZxAw==UjyrsniMcJl/Se/Jwp4pfA==";
      //// }
      RefreshAccessToken();
    }

    private void RefreshAccessToken()
    {
      string postData = string.Format("grant_type={0}&refresh_token={1}&client_id={2}&client_secret={3}", "refresh_token", System.Web.HttpUtility.UrlEncode(RefreshToken), ClientId, string.Empty);
      string url = "/auth/token";
      string data = Post(url, false, postData, RequestContentType.UrlEncoded);
      JObject jData = JsonConvert.DeserializeObject(data, settings) as JObject;
      {
        _tokenType = jData["token_type"]?.ToString();
        _expires = jData["expires_in"]?.ToString();
        _accessToken = jData["access_token"]?.ToString();
        RefreshToken = jData["refresh_token"]?.ToString();
      }
    }
    private HttpClient GetClient()
    {
      client.BaseAddress = new Uri(JoinApiUrl);
      client.DefaultRequestHeaders.Accept.Clear();
      client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
      return client;
    }

    private string Get(string requestUrl, bool needsAuthHeaders)
    {
      if (string.IsNullOrEmpty(requestUrl))
        throw new ArgumentNullException("requestUrl");

      HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requestUrl.StartsWith(JoinApiUrl) ? requestUrl : JoinApiUrl + requestUrl);
      request.Method = "GET";

      if (needsAuthHeaders)
        request.Headers["Authorization"] = string.Format(BearerAuthorization, _accessToken);

      HttpWebResponse response = (HttpWebResponse)request.GetResponse();
      var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
      response.Close();
      return responseString;
    }

    private string Link(string requestUrl, bool needsAuthHeaders, string postData, RequestContentType dataType)
    {
      return Request("LINK", requestUrl, needsAuthHeaders, postData, dataType);
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

      var request = (HttpWebRequest)WebRequest.Create(JoinApiUrl + requestUrl);

      var data = Encoding.ASCII.GetBytes(postData);
      request.Method = requestMethod;
      if (needsAuthHeaders)
        request.Headers["Authorization"] = string.Format(BearerAuthorization, _accessToken);
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


    #region file Upload

    private readonly Encoding encoding = Encoding.UTF8;
    private string RequestMultipartFormData(string requestMethod, string requestUrl, Dictionary<string, object> parameters)
    {
      string formDataBoundary = string.Format("----------{0:N}", Guid.NewGuid());
      string contentType = "multipart/form-data; boundary=" + formDataBoundary;

      byte[] formData = GetMultipartFormData(parameters, formDataBoundary);

      HttpWebResponse response = RequestWithFormData(requestMethod, requestUrl, contentType, formData);
      var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
      response.Close();
      return responseString;
    }
    private HttpWebResponse RequestWithFormData(string requestMethod, string requestUrl, string contentType, byte[] formData)
    {
      HttpWebRequest request = WebRequest.Create(JoinApiUrl + requestUrl) as HttpWebRequest;

      if (request == null)
      {
        throw new NullReferenceException("request is not a http request");
      }

      // Set up the request properties.
      request.Method = requestMethod; //"POST";            
      request.ContentType = contentType;
      request.UserAgent = "Someone";//??
      request.CookieContainer = new CookieContainer();
      request.ContentLength = formData.Length;

      request.Headers["Authorization"] = string.Format(BearerAuthorization, _accessToken);

      // Send the form data to the request.
      using (Stream requestStream = request.GetRequestStream())
      {
        requestStream.Write(formData, 0, formData.Length);
        requestStream.Close();
      }

      return request.GetResponse() as HttpWebResponse;
    }
    private byte[] GetMultipartFormData(Dictionary<string, object> postParameters, string boundary)
    {
      Stream formDataStream = new MemoryStream();
      bool needsCLRF = false;

      foreach (var param in postParameters)
      {
        // Thanks to feedback from commenters, add a CRLF to allow multiple parameters to be added.
        // Skip it on the first parameter, add it to subsequent parameters.
        if (needsCLRF)
          formDataStream.Write(encoding.GetBytes("\r\n"), 0, encoding.GetByteCount("\r\n"));

        needsCLRF = true;

        if (param.Value is FileParameter)
        {
          FileParameter fileToUpload = (FileParameter)param.Value;

          // Add just the first part of this param, since we will write the file data directly to the Stream
          string header = string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"; filename=\"{2}\"\r\nContent-Type: {3}\r\n\r\n",
              boundary,
              param.Key,
              fileToUpload.FileName ?? param.Key,
              fileToUpload.ContentType ?? "application/octet-stream");

          formDataStream.Write(encoding.GetBytes(header), 0, encoding.GetByteCount(header));

          // Write the file data directly to the Stream, rather than serializing it to a string.
          formDataStream.Write(fileToUpload.File, 0, fileToUpload.File.Length);
        }
        else
        {
          string postData = string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"\r\n\r\n{2}",
              boundary,
              param.Key,
              param.Value);
          formDataStream.Write(encoding.GetBytes(postData), 0, encoding.GetByteCount(postData));
        }
      }

      // Add the end of the request.  Start with a newline
      string footer = "\r\n--" + boundary + "--\r\n";
      formDataStream.Write(encoding.GetBytes(footer), 0, encoding.GetByteCount(footer));

      // Dump the Stream into a byte[]
      formDataStream.Position = 0L;
      byte[] formData = new byte[formDataStream.Length];
      formDataStream.Read(formData, 0, formData.Length);
      formDataStream.Close();

      return formData;
    }
    #endregion
    #endregion private methods
  }

  /// <summary>
  /// 
  /// </summary>
  public class FileParameter
  {
    public byte[] File { get; set; }
    public string FileName { get; set; }
    public string ContentType { get; set; }
    public FileParameter(byte[] file) : this(file, null) { }
    public FileParameter(byte[] file, string filename) : this(file, filename, null) { }
    public FileParameter(byte[] file, string filename, string contenttype)
    {
      File = file;
      FileName = filename;
      ContentType = contenttype;
    }
  }
}