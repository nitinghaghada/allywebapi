using System;
using System.Collections.Generic;
using System.Text;

namespace BusinessModel.Models
{
    public class UserSession
    {
      public string userName { get; set; }
      public string fullName { get; set; }
      public bool limitedEntryUser { get; set; }
      public string language { get; set; }
      public int listPageSize { get; set; }
      public int mobileAppMaxFormButtons { get; set; }
      public string signature { get; set; }
      public string userItemKey { get; set; }
      public string pendingItemRootBookKey { get; set; }
      public bool sendEmailNotifications { get; set; }
      public string emailAddress { get; set; }
      public bool mailClassifierEnabled { get; set; }
      //public RESTToken token { get; set; }
  }
}
