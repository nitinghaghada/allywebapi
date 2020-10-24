﻿using System;
using System.Collections.Generic;
using System.Text;

namespace BusinessModel.Models
{
  public class RESTToken
  {
    public string token_type { get; set; }
    public int expires_in { get; set; }
    public string access_token { get; set; }
    public string refresh_token { get; set; }
  }
}
