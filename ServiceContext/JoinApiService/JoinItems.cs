using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Decos.ServiceContext.JoinApiService
{
  public class JoinItems
  {
    [JsonProperty("count")]
    public int Count { get; set; }
    [JsonProperty("content")]
    public List<JoinItem> Items { get; set; }
    [JsonProperty("links")]
    public List<Link> Links { get; set; }
  }

  public class JoinItem
  {
    [JsonProperty("key")]
    public string Key;
    [JsonProperty("fields")]
    public Dictionary<string, object> Fields { get; set; }     
    //public JoinItemFields Fields;
    [JsonProperty("links")]
    public List<Link> Links;
    [JsonProperty("worklfowLinkInstanceType")]
    public string WorkflowLinkInstanceType;
    [JsonProperty("checklist")]
    public List<Checklist> checkList;
    [JsonProperty("outcomeslist")]
    public List<Dictionary<string, string>>  Outcomes;
    [JsonProperty("comment")]
    public string workflowHandleComment;
    [JsonProperty("outcome")]
    public string DecisionOutcome;

  }

  public class Link
  {
    [JsonProperty("rel")]
    public string Rel { get; set; }
    [JsonProperty("href")]
    public string HRef { get; set; }
  }

  public class JoinItemFields
  {
    public string itemtype_key { get; set; }
    public string itemtype_key2 { get; set; }
    public string parentKey { get; set; }

    [JsonProperty("archived")]
    public bool archived { get; set; }
    [JsonProperty("barcode")]
    public string barcode { get; set; }
    [JsonProperty("bookname")]
    public string bookname { get; set; }
    [JsonProperty("booksequence")]
    public string booksequence { get; set; }
    [JsonProperty("city")]
    public string city { get; set; }
    [JsonProperty("country")]
    public string country { get; set; }
    [JsonProperty("zipcode")]
    public string zipcode { get; set; }
    [JsonProperty("company")]
    public string company { get; set; }
    [JsonProperty("department")]
    public string department { get; set; }
    [JsonProperty("dfunction")]
    public string dfunction { get; set; }
    [JsonProperty("firstname")]
    public string firstname { get; set; }
    [JsonProperty("surname")]
    public string surname { get; set; }
    [JsonProperty("initials")]
    public string initials { get; set; }
    [JsonProperty("prefix")]
    public string prefix { get; set; }

    [JsonProperty("item_applies_to")]
    public string ItemAppliesTo { get; set; }
    [JsonProperty("legalid")]
    public string legalid { get; set; }
    [JsonProperty("mailaddress")]
    public string mailaddress { get; set; }
    [JsonProperty("mark")]
    public string mark { get; set; }
    [JsonProperty("memo")]
    public string memo { get; set; }
    [JsonProperty("processed")]
    public bool processed { get; set; }
    [JsonProperty("salutation")]
    public string salutation { get; set; }
    //[JsonProperty("sequence")]
    //public long sequence { get; set; }
    [JsonProperty("sex")]
    public string sex { get; set; }
    [JsonProperty("title")]
    public string title { get; set; }
    [JsonProperty("url")]
    public string url { get; set; }
    [JsonProperty("subject1")]
    public string subject1 { get; set; }
    [JsonProperty("subject2")]
    public string subject2 { get; set; }
    [JsonProperty("email1")]
    public string email1 { get; set; }
    [JsonProperty("email2")]
    public string email2 { get; set; }
    [JsonProperty("email3")]
    public string email3 { get; set; }
    [JsonProperty("phone1")]
    public string phone1 { get; set; }
    [JsonProperty("phone2")]
    public string phone2 { get; set; }
    [JsonProperty("phone3")]
    public string phone3 { get; set; }
    [JsonProperty("phone4")]
    public string phone4 { get; set; }
    [JsonProperty("phone5")]
    public string phone5 { get; set; }

    [JsonProperty("fax1")]
    public string fax1 { get; set; }
    [JsonProperty("fax2")]
    public string fax2 { get; set; }

    [JsonProperty("document_date")]
    public DateTime document_date { get; set; }
    [JsonProperty("received_date")]
    public DateTime received_date { get; set; }
    [JsonProperty("date1")]
    public DateTime? date1 { get; set; }
    [JsonProperty("date2")]
    public DateTime? date2 { get; set; }
    [JsonProperty("date3")]
    public DateTime? date3 { get; set; }
    [JsonProperty("date4")]
    public DateTime? date4 { get; set; }
    [JsonProperty("date5")]
    public DateTime? date5 { get; set; }
    [JsonProperty("date6")]
    public DateTime? date6 { get; set; }
    [JsonProperty("date7")]
    public DateTime? date7 { get; set; }
    [JsonProperty("date8")]
    public DateTime? date8 { get; set; }
    [JsonProperty("date9")]
    public DateTime? date9 { get; set; }
    [JsonProperty("date10")]
    public DateTime? date10 { get; set; }

    [JsonProperty("bol1")]
    public bool bol1 { get; set; }
    [JsonProperty("bol2")]
    public bool bol2 { get; set; }
    [JsonProperty("bol3")]
    public bool bol3 { get; set; }
    [JsonProperty("bol4")]
    public bool bol4 { get; set; }
    [JsonProperty("bol5")]
    public bool bol5 { get; set; }
    [JsonProperty("bol6")]
    public bool bol6 { get; set; }
    [JsonProperty("bol7")]
    public bool bol7 { get; set; }
    [JsonProperty("bol8")]
    public bool bol8 { get; set; }
    [JsonProperty("bol9")]
    public bool bol9 { get; set; }
    [JsonProperty("bol10")]
    public bool bol10 { get; set; }

    [JsonProperty("num1")]
    public double num1 { get; set; }
    [JsonProperty("num2")]
    public double num2 { get; set; }
    [JsonProperty("num3")]
    public double num3 { get; set; }
    [JsonProperty("num4")]
    public double num4 { get; set; }
    [JsonProperty("num5")]
    public double num5 { get; set; }
    [JsonProperty("num6")]
    public double num6 { get; set; }
    [JsonProperty("num7")]
    public double num7 { get; set; }
    [JsonProperty("num8")]
    public double num8 { get; set; }
    [JsonProperty("num9")]
    public double num9 { get; set; }
    [JsonProperty("num10")]
    public double num10 { get; set; }
    [JsonProperty("num11")]
    public double num11 { get; set; }
    [JsonProperty("num12")]
    public double num12 { get; set; }
    [JsonProperty("num13")]
    public double num13 { get; set; }
    [JsonProperty("num14")]
    public double num14 { get; set; }
    [JsonProperty("num15")]
    public double num15 { get; set; }
    [JsonProperty("num16")]
    public double num16 { get; set; }
    [JsonProperty("num17")]
    public double num17 { get; set; }
    [JsonProperty("num18")]
    public double num18 { get; set; }
    [JsonProperty("num19")]
    public double num19 { get; set; }
    [JsonProperty("num20")]
    public double num20 { get; set; }
    [JsonProperty("num21")]
    public double num21 { get; set; }
    [JsonProperty("num22")]
    public double num22 { get; set; }
    [JsonProperty("num23")]
    public double num23 { get; set; }
    [JsonProperty("num24")]
    public double num24 { get; set; }
    [JsonProperty("num25")]
    public double num25 { get; set; }

    [JsonProperty("num26")]
    public double num26 { get; set; }

    [JsonProperty("text1")]
    public string text1 { get; set; }
    [JsonProperty("text2")]
    public string text2 { get; set; }
    [JsonProperty("text3")]
    public string text3 { get; set; }
    [JsonProperty("text4")]
    public string text4 { get; set; }
    [JsonProperty("text5")]
    public string text5 { get; set; }
    [JsonProperty("text6")]
    public string text6 { get; set; }
    [JsonProperty("text7")]
    public string text7 { get; set; }
    [JsonProperty("text8")]
    public string text8 { get; set; }
    [JsonProperty("text9")]
    public string text9 { get; set; }
    [JsonProperty("text10")]
    public string text10 { get; set; }
    [JsonProperty("text11")]
    public string text11 { get; set; }
    [JsonProperty("text12")]
    public string text12 { get; set; }
    [JsonProperty("text13")]
    public string text13 { get; set; }
    [JsonProperty("text14")]
    public string text14 { get; set; }
    [JsonProperty("text15")]
    public string text15 { get; set; }
    [JsonProperty("text16")]
    public string text16 { get; set; }
    [JsonProperty("text17")]
    public string text17 { get; set; }
    [JsonProperty("text18")]
    public string text18 { get; set; }
    [JsonProperty("text19")]
    public string text19 { get; set; }
    [JsonProperty("text20")]
    public string text20 { get; set; }
    [JsonProperty("text21")]
    public string text21 { get; set; }
    [JsonProperty("text22")]
    public string text22 { get; set; }
    [JsonProperty("text23")]
    public string text23 { get; set; }
    [JsonProperty("text24")]
    public string text24 { get; set; }
    [JsonProperty("text25")]
    public string text25 { get; set; }
    [JsonProperty("text26")]
    public string text26 { get; set; }
    [JsonProperty("text27")]
    public string text27 { get; set; }
    [JsonProperty("text28")]
    public string text28 { get; set; }
    [JsonProperty("text29")]
    public string text29 { get; set; }
    [JsonProperty("text30")]
    public string text30 { get; set; }
    [JsonProperty("remainingDays")]
    public int remainingDays { get; set; }
    [JsonProperty("mainItemType")]
    public string mainItemType { get; set; }
  }

  public class JoinBookProfile
  {
    public string key { get; set; }
    public string itemProfileName { get; set; }
    public string fileNameMacro { get; set; }
    public string itemType { get; set; }
    public List<JoinItemProfileField> itemFields { get; set; }
  }
  public class JoinItemProfileField
  {
    public string field { get; set; }
    public string description { get; set; }
    public bool autoNum { get; set; }
    public bool defaultIsMacro { get; set; }
    public int displayWidthForm { get; set; }
    public int displayWidthList { get; set; }
    public int formSequence { get; set; }
    public bool limitedEntryField { get; set; }
    public int listSequence { get; set; }
    public bool mandatory { get; set; }
    public bool maxOneTableValue { get; set; }
    public bool passWordField { get; set; }
    public bool readOnly { get; set; }
    public bool showInForm { get; set; }
    public bool showInList { get; set; }
    public int maxLength { get; set; }
    public bool uniqueValue { get; set; }
    public bool onlyTableValues { get; set; }
    public string tableKey { get; set; }
  }
    public class Checklist
    {
        [JsonProperty("description")]
        public string description { get; set; }
        [JsonProperty("checked")]
        public bool Checked { get; set; }
        [JsonProperty("key")]
        public string key { get; set; }
        [JsonProperty("optional")]
        public bool optional { get; set; }
        [JsonProperty("automatic")]
        public bool automatic { get; set; }
    }

  public class ItemNote
  {
    [JsonProperty("fullname")]
    public string fullName { get; set; }
    [JsonProperty("date")]
    public DateTime date { get; set; }
    [JsonProperty("text")]
    public string text { get; set; }
    [JsonProperty("notetype")]
    public string noteType { get; set; }
    [JsonProperty("links")]
    public List<Link> Links;
  }
}
