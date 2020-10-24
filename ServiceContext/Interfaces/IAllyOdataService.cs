using System;
using System.Collections.Generic;
using System.Text;

namespace Decos.ServiceContext.Interfaces
{
  /// <summary>
  /// 
  /// </summary>
  public interface IAllyOdataService
  {
    Dictionary<string, object> GetItem(string entity, string itemId);
    Dictionary<string, object>[] GetEntityData(string entity);
    Dictionary<string, object>[] GetRelatedBookItems(string parententity, string entity, string key);
    string UpdateItem(string itemId, string entity, Dictionary<string, object> formData);
  }
}
