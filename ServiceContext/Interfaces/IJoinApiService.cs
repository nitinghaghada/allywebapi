using System.Collections.Generic;
using BusinessModel.Models;
using Decos.ServiceContext.JoinApiService;

namespace Decos.ServiceContext.Interfaces
{
  /// <summary>
  /// 
  /// </summary>
  public interface IJoinApiService
  {
    JoinItems GetLinkedItems(string bookKey, string itemType, int pageSize, int skip, string filter = "", string orderBy = "");
    JoinItems GetBooks(string type, string parentBookId);
    List<Dictionary<string, string>> GetTableValues(string id);
    JoinItems GetBookItems(string type, string bookId, int pageSize, int skip, string filter = "", string orderBy = "");
    JoinItems GetBookItemsWithFilters(string type, string bookId, string filter);
    JoinItem GetItem(string itemId);
    JoinBookProfile GetBookProfile(string profileId);
    JoinBookProfile GetBookProfile(JoinItem book);
    JoinItem UpdateItem(string itemId, JoinItem itemtoUpdate);
    bool LinkItem(string itemId, string relItemType, string relItemId);
    string GetItemType(string bookKey, string fieldName);
    JoinBookProfile GetBookProfileUsingUrl(string url);
    string AddItemNotes(string itemId, string noteText);
    IList<ItemNote> ItemNotes(string key);
    RESTToken GetAuthToken(string code);
    UserSession GetUserSession();
    bool SetAuthToken(RESTToken token);
    JoinItem NewItem(string parentKey, string entity, JoinItem itemToCreate);
  }
}