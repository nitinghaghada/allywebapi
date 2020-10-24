using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace AllyWebApi
{
  /// Imported class from Decos Core library (JOIN)
  public static class CommonUtility
  {
    /// <summary>
    /// Converts a string to have a beginning uppercase letter
    /// </summary>
    /// <param name="sInput"></param>
    /// <returns></returns>
    public static string FirstUpper(string sInput)
    {
      if (sInput == null) return string.Empty; // Berend 20180102: NullReferenceException seen in aspx log.
      if ((sInput.StartsWith("&")) && (sInput.Length >= 2))
      {
        return (sInput.Substring(0, 2).ToUpper() + sInput.Substring(2)); // Erik DDWE/4269
      }
      else if (sInput.Length > 0)
      {
        return (sInput.Substring(0, 1).ToUpper() + sInput.Substring(1)); // Erik DDWE/4269
      }
      return sInput;
    }

    /// <summary>
    /// Converts any object into a string, returns empty string
    /// for null objects
    /// </summary>
    /// <param name="o"></param>
    /// <returns></returns>
    public static string ConvertNullAndTrim(object o)
    {
      if (o != null)
      {
        string sRet;
        Type t = o.GetType(); // GJG 20050720 - Changed 0 (zero) to o (object)

        //sRet = o.ToString().TrimEnd();
        sRet = o.ToString().Trim(); //Paresh DW.Net/130

        if ((t != typeof(string)) && (t != typeof(DateTime)))
        {
          int iDecimal = sRet.LastIndexOf(System.Threading.Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator);
          if (iDecimal > 0)
          {
            for (int i = sRet.Length - 1; i >= iDecimal; i--)
            {
              if ((sRet[i] == '0') || (i == iDecimal))
                sRet = sRet.Substring(0, i);
              else
                break;
            }
          }
        }
        return sRet;
      }
      else
        return "";

    }


    /// <summary>
    /// ConvertNullToInt64 converts database value in a type-safe way to a long integer, returns 0 on failure.
    /// </summary>
    /// <param name="o"></param>
    /// <returns></returns>
    public static long ConvertNullToInt64(object o)
    {
      long iRet = 0;
      try
      {
        if (o != System.DBNull.Value)
        {
          // Berend 20100217: added check for numeric string to prevent format exception
          if (!(o is string) || IsNumeric((string)o))
            iRet = Convert.ToInt64(o);
        }
      }
      catch { }
      return iRet;
    }

    /// <summary>
    /// ConvertNullToInt32 converts database value in a type-safe way to an integer, returns 0 on failure.
    /// </summary>
    /// <param name="o"></param>
    /// <returns></returns>
    public static int ConvertNullToInt32(object o)
    {
      int iRet = 0;
      try
      {
        if (o != System.DBNull.Value)
        {
          // Berend 20100217: added check for numeric string to prevent format exception
          if (!(o is string) || IsNumeric((string)o))
            iRet = Convert.ToInt32(o);
        }
      }
      catch { }
      return iRet;
    }

    /// <summary>
    /// ConvertNullToBool converts the specified value to a boolean, returning false if the specified value is null
    /// </summary>
    /// <param name="o"></param>
    /// <returns></returns>
    public static bool ConvertNullToBool(object o)
    {
      bool bRet = false;
      try
      {
        if (o != null && o != DBNull.Value) // JW 5835: Convert.ToBoolean caused invalidcastexception for DbNull.Value
        {
          if (o.GetType() != typeof(string))
          {
            try
            {
              bRet = Convert.ToBoolean(o);
            }
            catch { }
          }
          if (!bRet)
          {
            string sValue = ConvertNullAndTrim(o).ToUpper();
            switch (sValue)
            {
              case "1":
              case "J":
              case "Y":
              case "TRUE":
              case "WAHR":
              case "WAAR":
              case "VRAI":
              case "YES":
              case "JA":
              case "ON": // Gerhard 20100526
                bRet = true;
                break;
            }
          }
        }
        else
          return false;
      }
      catch { }
      return bRet;
    }


    /// <summary>
    /// Converts a string to a num without any exception
    /// </summary>
    /// <param name="sNum"></param>
    /// <returns></returns>
    public static double ConvertStrToNum(string sNum)
    {
      return ConvertStrToNum(sNum, "");
    }

    /// <summary>
    /// ConvertNullToDate converts the specified value to a datetime, returning DateTime.MinValue if the specified value is null
    /// </summary>
    /// <param name="o"></param>
    /// <returns></returns>
    public static DateTime ConvertNullToDate(object o)
    {
      DateTime dtRet = System.DateTime.MinValue;
      try
      {
        // > JW 7718
        if (o != null && o != System.DBNull.Value)
        {
          if (o is string)
            dtRet = StringToDate(o.ToString());
          else if (o is DateTime)
            dtRet = (DateTime)o;
          // < JW 7718
        }
      }
      catch 
      {
      }
      return dtRet;
    }

    //> Berend DWE/8068
    /// <summary>
    /// ConvertNullToDouble converts database value in a type-safe way to a double, returns 0.0 on failure.
    /// </summary>
    /// <param name="o"></param>
    /// <returns></returns>
    public static double ConvertNullToDouble(object o)
    {
      double dblRet = 0.0;
      try
      {
        if (o != System.DBNull.Value)
        {
          if (o is string)
          {
            string sValue = (string)o;
            if (IsNumeric(sValue))
            {
              string sDecimalSep = string.Empty;
              int iComma = sValue.LastIndexOf(',');
              int iDot = sValue.LastIndexOf('.');
              if (iComma > iDot) sDecimalSep = ",";
              if (iDot > iComma) sDecimalSep = ".";
              dblRet = ConvertStrToNum(sValue, sDecimalSep);
            }
          }
          else
            dblRet = Convert.ToDouble(o);
        }
      }
      catch { }
      return dblRet;
    }

    /// <summary>
    /// Converts a string to a num without any exception
    /// </summary>
    /// <param name="sNum"></param>
    /// <param name="sDecimalSeparator"></param>
    /// <returns></returns>		
    public static double ConvertStrToNum(string sNum, string sDecimalSeparator)
    {
      //const string sValidChars = "-.,0123456789Ee"; // DDWE/4032 Vikas	
      //> Berend 20081030: did not work if decimal separator was passed by caller
      string sCurrentCultureDecimalSeparator = System.Threading.Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator;
      //< Berend 20081030
      try
      {
        //> Berend DWE/7808: return 0 for item keys
        if ((sNum.Length == 32) && IsHexaDecimal(sNum) && !IsNumeric(sNum))
          return 0;
        //< Berend DWE/7808
        string sValid = "";
        if ((sNum != null) && (sNum.Length > 0))
        {
          //>> DDWE/4032 Vikas			
          sValid = NumericPart(sNum);
          if (sValid == null || sValid.Length == 0) return 0; // JW 7749
          //<< DDWE/4032 Vikas
          //> Berend 20081030
          if (string.Compare(sDecimalSeparator, sCurrentCultureDecimalSeparator) != 0)
          {
            if (sDecimalSeparator.Length == 0) // >> JW DW.Net 341
            {
              if (string.Compare(sCurrentCultureDecimalSeparator, ",") == 0)
                sValid = sValid.Replace(".", sCurrentCultureDecimalSeparator);
              else
                sValid = sValid.Replace(",", sCurrentCultureDecimalSeparator);
            } // << JW DW.Net 341
            else
              sValid = sValid.Replace(sDecimalSeparator, sCurrentCultureDecimalSeparator);
          }
          //< Berend 20081030
          try
          {
            //> Berend web/5325
            int i = 1;
            if (sValid.EndsWith("%"))
            {
              sValid = sValid.Replace("%", string.Empty);
              i = 100;
            }
            if (IsNumeric(sValid)) // Berend 20171012
              return Convert.ToDouble(sValid) / i;
            //< Berend web/5325
          }
          catch
          {
            //System.Diagnostics.Trace.WriteLine(e); // JW 20060510
            return 0;
          }
        }
      }
      catch
      {
        //System.Diagnostics.Trace.WriteLine(e); // JW 20060510
      }
      return 0;
    }

    /// <summary>
    /// Checks whether given string is a valid hexadecimal value.
    /// </summary>
    /// <param name="sHexValue">
    /// The value to check. If the value starts with a number sign (#) but does 
    /// not start with "#x", it is assumed not to be hexadecimal (e.g. "#39" 
    /// would return false, but "#x39" and "39" both return true).
    /// </param>
    /// <returns>True if the value is a valid hexadecimal value.</returns>
    public static bool IsHexaDecimal(string sHexValue)
    {
      //> Berend IDP/1: incorrectly returned true for base64 encoded string
      if (sHexValue == null)
        return false;
      if (sHexValue.StartsWith("#")) //>> Ruben 8364
      {
        sHexValue = sHexValue.Substring(1);
        if (!sHexValue.StartsWith("x"))
          return false;
      } //<< Ruben 8364

      int iStart = sHexValue.StartsWith("x") ? 1 : 0;
      if (sHexValue.Length == iStart)
        return false;
      for (int i = iStart; i < sHexValue.Length; i++)
      {
        char c = sHexValue[i];
        if (!(char.IsDigit(c) || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F')))
          return false;
      }
      return true;
      //< Berend IDP/1
    }

    private static Regex m_rexNumeric1 = new Regex("^[^0-9|.|,|-]*");
    private static Regex m_rexNumeric2 = new Regex("[^0-9|.|,|-|\\%]*$"); 
    /// <summary>
    /// returns the remainder of the string with leading and trailing characters stripped
    /// </summary>
    /// <param name="sNum">Input string</param>
    /// <returns>Numeric Part of the input string</returns>
    public static string NumericPart(string sNum)
    {
      try
      {
        sNum = m_rexNumeric1.Replace(sNum, ""); // JW 7830 // Berend issue 1154
        sNum = m_rexNumeric2.Replace(sNum, ""); // JW 7830 // Berend issue 1154
        //sNum = System.Text.RegularExpressions.Regex.Replace(sNum, "[\\~#%&*${}/:<>?|\"-]",""); //Nitin/HandlerUI/185
        return sNum.Replace(" ", ""); // Berend 20100316: remove embedded spaces, e.g., "30 000 000" should be allowed
      }
      catch
      {
        return "";
      }
    }

    /// <summary>
    /// Returns if the given string contains numbers only
    /// </summary>
    /// <param name="sNumber">String to check</param>
    /// <returns></returns>
    public static bool IsNumeric(string sNumber)
    {
      return IsNumeric(sNumber, IsNumericCheck.Default);
    }
    /// <summary>
    /// Returns if the given string contains numbers only
    /// </summary>
    /// <param name="sNumber">String to check</param>
    /// <param name="checkNumberStyle">Numeric style to be checked</param>
    /// <returns></returns>
    public static bool IsNumeric(string sNumber, IsNumericCheck checkNumberStyle)
    {
      // Berend 20100217: empty string is not numeric
      if ((sNumber == null) || (sNumber.Length == 0))
        return false;

      char[] ca = sNumber.ToCharArray();
      int iDecimalSeparatorCount = 0; //NB Issue/2571
      for (int i = 0; i < ca.Length; i++)
      {
        if (!char.IsDigit(ca[i]))
        {
          switch (ca[i])
          {
            //> Berend web/5325: treat trailing percentage sign as a kind of decimal separator
            case '%':
              if ((i < (ca.Length - 1)) || ((checkNumberStyle & IsNumericCheck.AllowDecimalSeparator) != IsNumericCheck.AllowDecimalSeparator))
                return false;
              break;
            //< Berend web/5325
            case '.':
            case ',':
              iDecimalSeparatorCount++;  //NB Issue/2571
              if ((checkNumberStyle & IsNumericCheck.AllowDecimalSeparator) != IsNumericCheck.AllowDecimalSeparator)
                return false;
              break;
            case '-':
              if ((i > 0) || ((checkNumberStyle & IsNumericCheck.AllowSign) != IsNumericCheck.AllowSign))
                return false;
              break;
            default:
              return false;
          }
          //>NB Issue/2571
          if (iDecimalSeparatorCount > 1)  //Number should not have more than one decimal separtors
            return false;
          //<NB Issue/2571
        }
      }
      return true;
    }

    /// <summary>
    /// Converts a string into Date without any exception.
    /// Function considers both Timestamop 
    /// </summary>
    /// <param name="sDateValue">string representation of date.</param>
    /// <returns>Date object with parsed value. If date is not able to be parsed then it return minimum date.</returns>
    public static DateTime StringToDate(string sDateValue)
    {
      DateTime dtValue = DateTime.MinValue;
      try
      {
        if (IsTimestamp(sDateValue))
          dtValue = TimestampToDate(sDateValue);
        else
          dtValue = DecosDate.StringToDate(sDateValue);
      }
      catch (Exception ex)
      {
        Trace.WriteLine(ex);
      }
      return dtValue;
    }

    /// <summary>
    /// Converts a timestamp to a DateTime
    /// </summary>
    /// <param name="sTimeStamp">A string in yyyyMMddhhmmss format</param>
    /// <returns>The converted DateTime, or MinValue on error</returns>
    public static DateTime TimestampToDate(string sTimeStamp)
    {
      try
      {
        if ((sTimeStamp != null) && (sTimeStamp.Length >= 14)) // Berend 20130409
        {
          int year = System.Convert.ToInt16(sTimeStamp.Substring(0, 4));
          int month = System.Convert.ToInt16(sTimeStamp.Substring(4, 2));
          int day = System.Convert.ToInt16(sTimeStamp.Substring(6, 2));
          int hour = System.Convert.ToInt16(sTimeStamp.Substring(8, 2));
          int minutes = System.Convert.ToInt16(sTimeStamp.Substring(10, 2));
          int seconds = System.Convert.ToInt16(sTimeStamp.Substring(12, 2));
          return new DateTime(year, month, day, hour, minutes, seconds, 00);
        }
      }
      catch { }
      return DateTime.MinValue;
    }

    /// <summary>
    /// Returns true if the input string appears to be a timestamp.
    /// </summary>
    /// <param name="sDateValue">string value</param>
    /// <returns>true if timestamp</returns>
    public static bool IsTimestamp(string sDateValue)
    {
      return (sDateValue != null) && (sDateValue.Length == 14)
                                  && IsNumeric(sDateValue, IsNumericCheck.AllowOnlyDigits);
    }
  }

  /// <summary>
  /// Determines the characters that will be seen as numeric by IsNumeric
  /// </summary>
  public enum IsNumericCheck
  {
    /// <summary>Only digits will be seen as numeric</summary>
    AllowOnlyDigits = 0,
    /// <summary>Decimal separator (either "." or ",") is allowed</summary>
    AllowDecimalSeparator = 1,
    /// <summary>Sign character "-" is allowed</summary>
    AllowSign = 2,
    /// <summary>By default all number formats are allowed</summary>
    Default = AllowDecimalSeparator | AllowSign
  }

  /// Imported class from Decos Core library (JOIN)
  /// TODO : Fetch User var and global var setting from JOIN DB using JOIN API and use it in below library functions (wherever it require)
  /// <summary>
  /// Public class for manipulating dates
  /// </summary>
  public class DecosDate
  {
    private int year;
    private int month;
    private int day;
    private int hour;
    private int minutes;
    private int seconds;
    private DateTime date_value;

    /// <summary>
    /// Contains the names of the months in English
    /// </summary>
    public static string[] MONTHNAMES_EN = { "JAN", "FEB", "MAR", "APR", "MAY", "JUN", "JUL", "AUG", "SEP", "OCT", "NOV", "DEC" };
    /// <summary>
    /// Contains the names of the months in Dutch
    /// </summary>
    public static string[] MONTHNAMES_NL = { "JAN", "FEB", "MAA", "APR", "MEI", "JUN", "JUL", "AUG", "SEP", "OKT", "NOV", "DEC" };
    /// <summary>
    /// Contains the names of the months in Dutch (2nd option, march differs (MAA/MRT)
    /// </summary>
    public static string[] MONTHNAMES_NL2 = { "JAN", "FEB", "MRT", "APR", "MEI", "JUN", "JUL", "AUG", "SEP", "OKT", "NOV", "DEC" };
    /// <summary>
    /// Contains the names of the months in German
    /// </summary>
    public static string[] MONTHNAMES_DE = { "JAN", "FEB", "MAR", "APR", "MAI", "JUN", "JUL", "AUG", "SEP", "OKT", "NOV", "DEZ" };
    //>>DDWE/6945- Pallavi
    /// <summary>
    /// Contains the names of the months in German (2nd option, march differs (MAR/MRZ)
    /// </summary>
    public static string[] MONTHNAMES_DE2 = { "JAN", "FEB", "MRZ", "APR", "MAI", "JUN", "JUL", "AUG", "SEP", "OKT", "NOV", "DEZ" };
    //<<DDWE/6945- Pallavi
    /// <summary>
    /// Contains the names of the months in French
    /// </summary>
    public static string[] MONTHNAMES_FR = { "JAN", "FÉV", "MAR", "AVR", "MAI", "JUIN", "JUIL.", "AOÛ", "SEP", "OCT", "NOV", "DÉC" }; //Paresh DWNet/262: Rev-01 [For language FR, change Month name to JUI from JUL] //DWE/10121-Dipali
    /// <summary>
    /// Contains the names of the months in French (2nd option, without acute and circumflex accents)
    /// </summary>
    public static string[] MONTHNAMES_FR2 = { "JAN", "FEV", "MAR", "AVR", "MAI", "JUIN", "JUI", "AOÛ", "SEP", "OCT", "NOV", "DEC" }; //DWE/10121-Dipali
    //>> DDWE/3735 - Dipti
    /// <summary>
    /// Contains the full months name in English
    /// </summary>
    public static string[] MONTHNAMES_FULL_EN = { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" };
    /// <summary>
    /// Contains the full months name in Dutch
    /// </summary>
    public static string[] MONTHNAMES_FULL_NL = { "januari", "februari", "maart", "april", "mei", "juni", "juli", "augustus", "september", "oktober", "november", "december" };
    /// <summary>
    /// Contains the full months name in French
    /// </summary>
    public static string[] MONTHNAMES_FULL_FR = { "janvier", "février", "mars", "avril", "mai", "juin", "juillet", "août", "septembre", "octobre", "novembre", "décembre" };
    /// <summary>
    /// Contains the full months name in German
    /// </summary>
    public static string[] MONTHNAMES_FULL_DE = { "Januar", "Februar", "März", "April", "Mai", "Juni", "Juli", "August", "September", "Oktober", "November", "Dezember" };
    /// <summary>
    /// Contains formats for DATE
    /// </summary>
    public static string[] DATE_FORMAT = { "M D, Y", "D M Y" };
    //<< DDWE/3735 - Dipti
    /// <summary>
    /// Contains default time stamp value. It is 1st January 1900 00:00:00
    /// </summary>
    public static string DEFAULT_TIMESTAMP = "19000101000000";
    private static readonly DateTime m_dtEpochZero = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc); // DateTime "constant" used for Epoch calculations

    private static string SixDigitDateFormat
    {
      get
      {
        return "ddMMyy"; 
      }
    }
    private static string EightDigitDateFormat
    {
      get
      {
        return "ddMMyyyy"; 
      }
    }
    //// << Ddwe/4399 - Sanket, Berend

    /// <summary>
    /// Constructs a DecosDate object from a free string
    /// </summary>
    /// <param name="sStringDate">string that contains the date</param>
    public DecosDate(string sStringDate)
    {
      bool bHasTimePart = false;
      date_value = StringToDate(sStringDate, ref bHasTimePart);
      if (date_value == DateTime.MinValue)
      {
        if (sStringDate.Length.Equals(14))
        {
          year = System.Convert.ToInt16(sStringDate.Substring(0, 4));
          month = System.Convert.ToInt16(sStringDate.Substring(4, 2));
          day = System.Convert.ToInt16(sStringDate.Substring(6, 2));
          hour = System.Convert.ToInt16(sStringDate.Substring(8, 2));
          minutes = System.Convert.ToInt16(sStringDate.Substring(10, 2));
          seconds = System.Convert.ToInt16(sStringDate.Substring(12, 2));
          date_value = new DateTime(year, month, day, hour, minutes, seconds, 00);
        }
        else
        {
          //> Berend web/5720
          if (sStringDate == null)
            sStringDate = string.Empty;
          string[] sArray = sStringDate.Split('|');
          if (sArray.Length > 5)
          {
            //< Berend web/5720
            year = System.Convert.ToInt16(sArray[0]);
            month = System.Convert.ToInt16(sArray[1]);
            day = System.Convert.ToInt16(sArray[2]);
            hour = System.Convert.ToInt16(sArray[3]);
            minutes = System.Convert.ToInt16(sArray[4]);
            seconds = System.Convert.ToInt16(sArray[5]);
            date_value = new DateTime(year, month, day, hour, minutes, seconds, 00);
          }
          //> Berend web/5720
          else
            throw new FormatException("The string '" + sStringDate + "' does not represent a valid date/time");
          //< Berend web/5720
        }
      }
    }

    /// <summary>
    /// Constructs a DecosDate from numeric parameters
    /// </summary>
    /// <param name="iYear">The year for the date</param>
    /// <param name="iMonth">The month for the date</param>
    /// <param name="iDay">The day for the date</param>
    /// <param name="iHour">The hours for the date</param>
    /// <param name="iMinute">The minutes for the date</param>
    /// <param name="iSeconds">The seconds for the date</param>
    public DecosDate(int iYear, int iMonth, int iDay, int iHour, int iMinute, int iSeconds)
    {
      date_value = new DateTime(iYear, iMonth, iDay, iHour, iMinute, iSeconds, 00);
      year = date_value.Year;
      month = date_value.Month;
      day = date_value.Day;
      hour = date_value.Hour;
      minutes = date_value.Minute;
      seconds = date_value.Second;
    }

    /// <summary>
    /// Constructs a DecosDate from a System.DateTime
    /// </summary>
    /// <param name="dtValue">System.DateTime with the date</param>
    public DecosDate(DateTime dtValue)
    {
      date_value = dtValue;
      year = dtValue.Year;
      month = dtValue.Month;
      day = dtValue.Day;
      hour = dtValue.Hour;
      minutes = dtValue.Minute;
      seconds = dtValue.Second;
    }

    /// <summary>
    /// Converts a free string to a System.DateTime
    /// </summary>
    /// <param name="oValue">Value to be converted</param>
    /// <returns>DateTime object representing the same date/time as the input object oValue</returns>
    public static System.DateTime StringToDate(object oValue)
    {
      bool bTimeUsed = false;
      return StringToDate(oValue, ref bTimeUsed);
    }

    /// <summary>
    /// Converts a given object into a DateTime object
    /// </summary>
    /// <param name="oValue">Value to be converted</param>
    /// <param name="bUsedTime">Returns true if time data is found and converted</param>
    /// <returns>DateTime object representing the same date/time as the input object oValue</returns>
    public static System.DateTime StringToDate(object oValue, ref bool bUsedTime)
    {
      //> Berend web/5696: added optional bIsMonth output parameter
      bool bIsMonth;
      return StringToDate(oValue, out bUsedTime, out bIsMonth);
    }

    //> Berend web/6164
    /// <summary>
    /// Converts a given object into a DateTime object
    /// </summary>
    /// <param name="oValue">Value to be converted</param>
    /// <param name="sFormat">Format used to format the date or empty string</param>
    /// <returns>DateTime object representing the same date/time as the input object oValue</returns>
    public static System.DateTime StringToDate(object oValue, string sFormat)
    {
      bool bUsedTime, bIsMonth;
      return StringToDate(oValue, sFormat, out bUsedTime, out bIsMonth);
    }

    /// <summary>
    /// Converts a given object into a DateTime object
    /// </summary>
    /// <param name="oValue">Value to be converted</param>
    /// <param name="bUsedTime">Returns true if time data is found and converted</param>
    /// <param name="bIsMonth">Returns true if only year and month were specified in input</param>
    /// <returns>DateTime object representing the same date/time as the input object oValue</returns>
    public static System.DateTime StringToDate(object oValue, out bool bUsedTime, out bool bIsMonth)
    {
      return StringToDate(oValue, string.Empty, out bUsedTime, out bIsMonth);
    }

    /// <summary>
    /// Converts a given object into a DateTime object
    /// </summary>
    /// <param name="oValue">Value to be converted</param>
    /// <param name="sFormat">Format used to format the date or empty string</param>
    /// <param name="bUsedTime">Returns true if time data is found and converted</param>
    /// <param name="bIsMonth">Returns true if only year and month were specified in input</param>
    /// <returns>DateTime object representing the same date/time as the input object oValue</returns>
    public static System.DateTime StringToDate(object oValue, string sFormat, out bool bUsedTime, out bool bIsMonth)
    {
      //< Berend web/6164
      //< Berend web/5696
      System.DateTime dRet;
      string sStart;
      string sTime;
      string[] aParts;
      string sMonth;
      int i;
      int iVal;
      int iDay;
      int iMonth;
      int iYear = -1; // Berend web/5564
      int iSecond = 0;
      int iMinute = 0;
      int iHour = 0;
      bool bPM = false;

      dRet = System.DateTime.MinValue;
      NormalizeJavascriptDateTime(ref oValue); // Berend .Net/608
      sStart = oValue.ToString().Trim(); // Berend .Net/608: moved down
      bUsedTime = false;
      //> Berend web/5696
      bIsMonth = false;
      while (sStart.IndexOf("  ") >= 0)
        sStart = sStart.Replace("  ", " ");
      //< Berend web/5696
      if (sStart.Length > 0)
      {
        //> Berend web/6168: support for string value using custom format
        if ((sFormat != null) && (sFormat.Length > 0))
        {
          try
          {
            dRet = DateTime.ParseExact(sStart, sFormat, Thread.CurrentThread.CurrentCulture);
          }
          catch
          {
            dRet = System.DateTime.MinValue;
          }
          if (dRet != System.DateTime.MinValue)
          {
            bUsedTime = HasTimePart(dRet);
            return dRet;
          }
        }
        //< Berend web/6168        
        try // DD/136 : Parashar : Moved up here
        {
          if (sStart.IndexOf(':') >= 0)
          {
            int iSpacePos = sStart.LastIndexOf(' ');
            //> Berend web/5564: support for xml date/time format yyyy-MM-dd'T'HH:mm:ss
            if (iSpacePos < 0)
            {
              iSpacePos = sStart.IndexOf(':') - 3;
              if ((iSpacePos >= 0) && (sStart[iSpacePos] != 'T'))
                iSpacePos = -1;
            }
            //< Berend web/5564
            if (iSpacePos >= 0)
            {
              sTime = sStart.Substring(iSpacePos + 1);
              if ("|AM|PM|".IndexOf("|" + sTime.Trim().ToUpper() + "|") >= 0)
              {
                bPM = (sTime.Trim().ToUpper().Equals("PM"));
                //>> DD/136 : Parashar
                int iSpacePos2 = sStart.LastIndexOf(" ", iSpacePos - 1);
                sTime = sStart.Substring(iSpacePos2 + 1, iSpacePos - iSpacePos2);
                //> Berend web/5950
                if ((sTime.Length > 3) && sTime.StartsWith("12:") && !bPM)
                  sTime = "00" + sTime.Substring(2);
                //< Berend web/5950
                sStart = sStart.Substring(0, iSpacePos2);
                //<< DD/136 : Parashar
              }
              else
              {
                sStart = (iSpacePos > 0) ? sStart.Substring(0, iSpacePos) : string.Empty; // Berend 20070321 .Net/125
              }
              aParts = sTime.Split(':');
              sTime = string.Empty;
              for (i = 0; i < aParts.Length; i++)
              {
                iVal = CommonUtility.ConvertNullToInt32(aParts[i]);
                if (bPM && (i == 0) && (iVal < 12)) // Berend web/5950
                {
                  iVal += 12;
                }
                sTime += iVal.ToString("00") + ":";
              }
              sTime = sTime.Substring(0, sTime.Length - 1);
              aParts = sTime.Split(':');
              iHour = (int)CommonUtility.ConvertStrToNum(aParts[0]);
              iMinute = (int)CommonUtility.ConvertStrToNum(aParts[1]);
              if (aParts.Length > 2) iSecond = (int)CommonUtility.ConvertStrToNum(aParts[2]);
              bUsedTime = true;
            }
          }

          //> Berend web/4739
          char[] aSep = new char[] { '-', '/', '.', ' ', (char)160 }; // Berend D5.1 issue 264: added space as separator // Erik Issue 1528 - also support nbsp (160)
          aParts = null;
          for (int iSep = 0; iSep < aSep.Length; iSep++)
          {
            aParts = sStart.Split(aSep[iSep]);
            AddThirdDatePart(ref aParts, ref bIsMonth); // Berend web/5696
            if (aParts.Length == 3)
              break;
          }
          //< Berend web/4739
          if ((aParts == null) || (aParts.Length < 3))
            aParts = sStart.Split(' ');

          if (aParts.Length > 3)
          {
            ArrayList alParts = new ArrayList();
            for (i = 0; i < aParts.Length; i++)
            {
              if (aParts[i].Length > 0)
              {
                alParts.Add(alParts[i]);
              }
            }
            for (i = 0; i <= 2; i++)
            {
              aParts[i] = alParts[i].ToString();
            }
          }
          else if (aParts.Length == 1 && sStart.Length == 8 && CommonUtility.ConvertNullToInt32(sStart) >= 1011900) // Berend web/3861
          {
            aParts = new String[3];
            // >> Ddwe/4399 - Sanket
            aParts[0] = DatePart(sStart, 'D');
            aParts[1] = DatePart(sStart, 'M');
            aParts[2] = DatePart(sStart, 'Y');
            // << Ddwe/4399 - Sanket
          }
          //> Berend web/3861
          else if (aParts.Length == 1 && sStart.Length == 6 && CommonUtility.ConvertNullToInt32(sStart) >= 10100)
          {
            aParts = new String[3];
            // >> Ddwe/4399 - Sanket
            aParts[0] = DatePart(sStart, 'D');
            aParts[1] = DatePart(sStart, 'M');
            aParts[2] = DatePart(sStart, 'Y');
            // << Ddwe/4399 - Sanket
            if (string.Compare(aParts[2], "79") > 0)
            {
              aParts[2] = "19" + aParts[2];
            }
            else
            {
              aParts[2] = "20" + aParts[2];
            }
          }
          else if (aParts.Length == 1 && sStart.Length == 4 && CommonUtility.ConvertNullToInt32(sStart) >= 1753) //SDA/55712-Sunny // JW/SDA/55712 [web/6857]
          {
            aParts = new String[3];
            aParts[0] = "1";
            aParts[1] = "1";
            aParts[2] = sStart;
          }
          //< Berend web/3861
          else if (aParts.Length == 1 && sStart.Length == 14)
          {
            aParts = new String[6];
            aParts[0] = sStart.Substring(6, 2);
            aParts[1] = sStart.Substring(4, 2);
            aParts[2] = sStart.Substring(0, 4);
            iHour = CommonUtility.ConvertNullToInt32(sStart.Substring(8, 2));
            iMinute = CommonUtility.ConvertNullToInt32(sStart.Substring(10, 2));
            iSecond = CommonUtility.ConvertNullToInt32(sStart.Substring(12, 2));
          }
          // >> Ddwe/4399 - Sanket, Berend
          else if ((aParts.Length == 3) &&
                   CommonUtility.IsNumeric(aParts[0]) &&
                   CommonUtility.IsNumeric(aParts[1]) &&
                   CommonUtility.IsNumeric(aParts[2]))
          {
            //> Berend web/5564
            if ((aParts[0].Length == 4) && (CommonUtility.ConvertNullToInt32(aParts[0]) >= 1000))
            {
              string sYear = aParts[0];
              aParts[0] = aParts[2];
              aParts[2] = sYear;
            }
            else
            {
              //< Berend web/5564
              if (aParts[0].Length < 2) aParts[0] = aParts[0].PadLeft(2, '0');
              if (aParts[1].Length < 2) aParts[1] = aParts[1].PadLeft(2, '0');
              string sTempDate = aParts[0] + aParts[1] + aParts[2];
              if (sTempDate.Length == 8 || sTempDate.Length == 6)
              {
                aParts = new String[3];
                aParts[0] = DatePart(sTempDate, 'D');
                aParts[1] = DatePart(sTempDate, 'M');
                aParts[2] = DatePart(sTempDate, 'Y');
              }
            } // Berend web/5564
          }
          // << Ddwe/4399 - Sanket, Berend
          // >> DWNET/600[1] - Sanket	
          string sMonthPart = string.Empty, sDayPart = string.Empty;
          if (aParts.Length > 1) // Berend 20110722
          {
            if (!CommonUtility.IsNumeric(aParts[0])) // Berend .Net/608: removed test for comma, comma is optional
            {
              aParts[1] = aParts[1].Replace(",", string.Empty);	 // US locale's date format. MMM dd, YYYY.					
              sDayPart = aParts[1];
              sMonthPart = aParts[0];
            }
            else
            {
              //> Berend web/5564
              if ((aParts[0].Length == 4) && (CommonUtility.ConvertNullToInt32(aParts[0]) >= 1000))
              {
                iYear = CommonUtility.ConvertNullToInt32(aParts[0]);
                sMonthPart = aParts[1];
                sDayPart = aParts[2];
              }
              else
              {
                //< Berend web/5564
                sDayPart = aParts[0];
                sMonthPart = aParts[1];
              } // Berend web/5564
            }
          }
          iDay = CommonUtility.ConvertNullToInt32(sDayPart);
          if (CommonUtility.IsNumeric(sMonthPart))
          {
            iMonth = CommonUtility.ConvertNullToInt32(sMonthPart);
          }
          else
          {
            iMonth = 0;
          }
          // << DWNET/600[1] - Sanket
          if ((iYear == -1) && (aParts.Length > 2)) // Berend web/5564 // Berend 20110722
          {
            if (aParts[2].Length == 2)
            {
              iYear = CommonUtility.ConvertNullToInt32(aParts[2]);
              if (iYear < 80)
              {
                iYear += 2000;
              }
              else if (iYear < 100)
              {
                iYear += 1900;
              }
            }
            else
            {
              iYear = CommonUtility.ConvertNullToInt32(aParts[2]);
            }
          } // Berend web/5564
          // Berend .Net/922: removed "fuzzy logic" code that was part of DWNET/600[1]
          if ((iMonth == 0) && (sMonthPart != null) && (sMonthPart.Length > 2)) //DDWE/6103-Ashish
          {
            sMonth = sMonthPart.ToUpper().Trim().Substring(0, 3); // DWNET/600[1] - Sanket
            iMonth = Array.IndexOf(MONTHNAMES_EN, sMonth) + 1;
            if (iMonth == 0) { iMonth = Array.IndexOf(MONTHNAMES_EN, sMonth) + 1; }
            if (iMonth == 0) { iMonth = Array.IndexOf(MONTHNAMES_NL, sMonth) + 1; }
            if (iMonth == 0) { iMonth = Array.IndexOf(MONTHNAMES_NL2, sMonth) + 1; }
            if (iMonth == 0) { iMonth = Array.IndexOf(MONTHNAMES_DE, sMonth) + 1; }
            if (iMonth == 0) { iMonth = Array.IndexOf(MONTHNAMES_DE2, sMonth) + 1; } //DDWE/6945- Pallavi
            if (iMonth == 0) { iMonth = Array.IndexOf(MONTHNAMES_FR, sMonth) + 1; }
            if (iMonth == 0) { iMonth = Array.IndexOf(MONTHNAMES_FR2, (string.Compare(sMonthPart, "JUIN", true) == 0 ? sMonthPart.ToUpper() : sMonth)) + 1; } //DWE/10121-Dipali
          }
          //>>DDWE/6103-Ashish
          if ((iMonth == 0) && (iDay == 0) && (iSecond == 1))
          {
            iMonth = 1;
            bUsedTime = true;
          }
          if ((iDay == 0) && ((iSecond == 59) || (iSecond == 1)))
          {
            iDay = 1;
            bUsedTime = true;
          }
          if ((iMonth == 0) && (iDay == 0) && (iSecond == 0))
          {
            iMonth = 1;
            iDay = 1;
            iSecond = 1;
            bUsedTime = true;
          }
          if ((iDay == 0) && (iSecond == 0))
          {
            iDay = 1;
            iSecond = 59;
            bUsedTime = true;
          }
          //<<DDWE/6103-Ashish
          dRet = (iYear > 0) ? new DateTime(iYear, iMonth, iDay, iHour, iMinute, iSecond) : System.DateTime.MinValue; // Berend DWE/7808
        }
        catch
        {
          dRet = System.DateTime.MinValue;
        }
      }
      return dRet;
    }

    /// <summary>
    /// AddThirdDatePart adds the missing part to a 2-part date:
    /// - If only month and day are specified, the current year is assumed and added
    /// - If year and month are specified, day 1 is added and bIsMonth is set to true.
    /// </summary>
    /// <param name="aParts">IN/OUT: string array with date parts</param>
    /// <param name="bIsMonth">Set to true if only year and month were specified</param>
    private static void AddThirdDatePart(ref string[] aParts, ref bool bIsMonth)
    {
      if ((aParts.Length == 2)
        && (CommonUtility.IsNumeric(aParts[0], IsNumericCheck.AllowOnlyDigits)
        || CommonUtility.IsNumeric(aParts[1], IsNumericCheck.AllowOnlyDigits)))
      {
        string[] aPartsOut = new string[3];

        if ((aParts[0].Length == 4) && CommonUtility.IsNumeric(aParts[0], IsNumericCheck.AllowOnlyDigits))
        {
          bIsMonth = true;
          aPartsOut[0] = aParts[0];
          aPartsOut[1] = aParts[1];
          aPartsOut[2] = "1";
        }
        else if ((aParts[1].Length == 4) && CommonUtility.IsNumeric(aParts[1], IsNumericCheck.AllowOnlyDigits))
        {
          bIsMonth = true;
          aPartsOut[0] = "1";
          aPartsOut[1] = aParts[0];
          aPartsOut[2] = aParts[1];
          if (CommonUtility.IsNumeric(aPartsOut[1], IsNumericCheck.AllowOnlyDigits))
          {
            // Use month abbreviation instead of number to make return array unambiguous if user uses M/D/Y
            int iMonth = CommonUtility.ConvertNullToInt32(aPartsOut[1]);
            if ((iMonth >= 1) && (iMonth <= 12))
              aPartsOut[1] = "JANFEBMARAPRMAYJUNJULAUGSEPOCTNOVDEC".Substring((iMonth - 1) * 3, 3);
          }
        }
        else
        {
          aPartsOut[0] = aParts[0];
          aPartsOut[1] = aParts[1];
          aPartsOut[2] = DateTime.Now.Year.ToString();
        }
        aParts = aPartsOut;
      }
    }

    /// <summary>
    /// Converts javascript date/time format into a date/time format that we can process.
    /// </summary>
    /// <param name="oValue">
    /// IN: may be javascript date/time string
    /// OUT: normalized date/time stirng if input was recongnized as javascript date/time
    /// </param>
    private static void NormalizeJavascriptDateTime(ref object oValue)
    {
      try
      {
        string sDateTime = CommonUtility.ConvertNullAndTrim(oValue);
        if (sDateTime.Length >= 32)
        {
          int iPos = sDateTime.IndexOf("GMT");
          if (iPos < 0) iPos = sDateTime.IndexOf("UTC");
          if (iPos > 0)
          {
            string[] asDate = sDateTime.Split(' ');
            if (asDate.Length >= 6)
            {
              // skip weekday, assumed to have index 0
              sDateTime = asDate[1] + " " + asDate[2];
              string sYear = string.Empty;
              string sTime = string.Empty;
              for (int i = 3; i < asDate.Length; i++)
              {
                if (((asDate[i].Length == 2) || (asDate[i].Length == 4)) &&
                    CommonUtility.IsNumeric(asDate[i], IsNumericCheck.AllowOnlyDigits))
                  sYear = asDate[i];
                else if ((asDate[i].Length == 8) && (asDate[i].IndexOf(':') == 2))
                  sTime = asDate[i];
              }
              oValue = (sDateTime + " " + sYear + " " + sTime).TrimEnd();
            }
          }
        }
      }
      catch (Exception ex)
      {
        Trace.Write(ex);
      }
    }

    // >> Ddwe/4399 - Sanket
    /// <summary>
    /// Return Date part from given date string. date string must be in either 6 or 8 digits format.
    /// </summary>
    /// <param name="sDateValue">date value of which part is requested.</param>
    /// <param name="cDatePart">Type part requested. e.g. M,D,Y</param>
    /// <returns>returns string value of Date part</returns>
    private static string DatePart(string sDateValue, char cDatePart)
    {
      string sRetVal = string.Empty;
      string sDateFormat = string.Empty;
      string sDatePhrase = string.Empty;
      int iStart = -1, iLen = -1;
      try
      {
        if (sDateValue.Length == 8)
          sDateFormat = EightDigitDateFormat;
        else if (sDateValue.Length == 6)
          sDateFormat = SixDigitDateFormat;
        sDateFormat = sDateFormat.ToUpper();
        switch (char.ToUpper(cDatePart))
        {
          case 'Y':
            sDatePhrase = (sDateValue.Length == 8 ? "YYYY" : "YY");
            iLen = (sDateValue.Length == 8 ? 4 : 2);
            break;
          case 'D':
            sDatePhrase = "DD";
            iLen = 2;
            break;
          case 'M':
            sDatePhrase = "MM";
            iLen = 2;
            break;
        }
        sRetVal = string.Empty;
        if (sDatePhrase.Length > 0)
        {
          iStart = sDateFormat.IndexOf(sDatePhrase);
          if (iStart != -1 && iLen != -1)
            sRetVal = sDateValue.Substring(iStart, iLen);
        }
      }
      catch (Exception ex)
      {
        Trace.WriteLine(ex);
      }
      return sRetVal;
    }
    // << Ddwe/4399 - Sanket

    /// <summary>
    /// Formats a date into well formatted string
    /// </summary>
    /// <param name="dDate">Date to convert</param>
    /// <param name="bUseTime">Specifies if time should also be converted</param>
    /// <returns></returns>
    public static string DateFormat(System.DateTime dDate, bool bUseTime)
    {
      string sLanguage;
      sLanguage = "EN";

      return DateFormat(dDate, bUseTime, true, sLanguage);
    }

    /* JW DecosDirect: Commented this function since we can't know the current user's language
    /// <summary>
    /// Formats a date into well formatted string for the current user's language
    /// </summary>
    /// <param name="dDate">Date to use</param>
    /// <param name="bUseTime">If true, time data is also inserted</param>
    /// <param name="bUseLanguage">If true, language specifics are used (e.g. month names)</param>
    /// <returns></returns>
    public static string DateFormat(System.DateTime dDate, bool bUseTime, bool bUseLanguage)
    {
      return DateFormat(dDate,bUseTime,bUseLanguage,"EN");
    }*/

    /// <summary>
    /// Formats a date into well formatted string for the given language
    /// </summary>
    /// <param name="dDate">Date to use</param>
    /// <param name="bUseTime">If true, time data is also inserted</param>
    /// <param name="bUseLanguage">If true, language specifics are used (e.g. month names)</param>
    /// <param name="sLanguage">The language to use</param>
    /// <returns></returns>
    public static string DateFormat(System.DateTime dDate, bool bUseTime, bool bUseLanguage, string sLanguage)
    {
      // >> DW.NET-132 Pankil
      string sResult = string.Empty;
      string sSeparator = string.Empty;
      bool bIsExceptionalDate = false;//DDWE/6103-Ashish
      // << DW.NET-132 Pankil

      if (!bUseLanguage)
      {
        sLanguage = "EN";
      }
      // > JW 9040: removed DW.NET-132
      switch (sLanguage.ToUpper())
      {
        case "EN":
        case "FR":
        case "HI":
        default:
          sSeparator = "-";
          break;
        case "DE":
          sSeparator = ".";
          break;
        case "NL":
          sSeparator = " ";
          break;
      }
      // < JW 9040
      sResult = "0" + dDate.Day.ToString();
      sResult = sResult.Substring(sResult.Length - 2);
      //>>DDWE/6103-Ashish
      bIsExceptionalDate = DecosDate.IsExceptionalDate(dDate);
      if (bIsExceptionalDate && (dDate.Second == 59))
      {
        sResult = "00";
        bUseTime = false;
      }
      //<<DDWE/6103-Ashish
      string sDay = sResult;//DDWE/6591[1]-Ashish
      sResult += sSeparator;  // DW.NET-132 Pankil
      string sMonth = CommonUtility.FirstUpper(MONTHNAMES_EN[dDate.Month - 1].ToLower());  // DW.NET-132 Pankil // JW 20080625 //DDWE/6591[1]-Ashish
      switch (sLanguage)
      {
        case "NL":
          sResult += MONTHNAMES_NL[dDate.Month - 1].ToLower();
          break;
        case "HI": // JW 9040: It's common to display Hindi dates in EN format
        case "EN":
          sResult += sMonth; //DDWE/6591[1]-Ashish Moved code up
          break;
        case "DE":
          sResult += CommonUtility.FirstUpper(MONTHNAMES_DE[dDate.Month - 1].ToLower()); // JW 20080625
          break;
        case "FR":
          sResult += MONTHNAMES_FR[dDate.Month - 1].ToLower();
          break;
      }
      //>>DDWE/6103-Ashish
      if (bIsExceptionalDate && (dDate.Second == 1))
      {
        sResult = "00" + sSeparator + "00";
        bUseTime = false;
        sDay = sMonth = "00"; //DDWE/6591[1]-Ashish
      }
      //<<DDWE/6103-Ashish
      sResult += sSeparator;  // DW.NET-132 Pankil
      sResult += dDate.Year.ToString();
      //>>DDWE/6591[1]-Ashish
      if (string.Compare(sLanguage, "EN", true) == 0)
        sResult = sMonth + " " + sDay + ", " + dDate.Year.ToString();
      //<<DDWE/6591[1]-Ashish

      if (bUseTime)
      {
        dDate = DecosDate.ServerToClientDate(dDate); // DDWE/5129 - Sanket
        if (!bIsExceptionalDate)
        {
          //>> Nikunj DDWE/5128
          if ((string.Compare(sLanguage, "EN", true) == 0)) // DDWE/5374 - Ashish
            sResult += " " + String.Format("{0:hh:mm:ss tt}", dDate);
          else
            sResult += " " + String.Format("{0:HH:mm:ss}", dDate);
          //<< Nikunj DDWE/5128
        }
      }
      return sResult;
    }

    /// <summary>
    /// Returns true if the specified date object has a non-zero time part.
    /// </summary>
    /// <param name="dDate">Date to be investigated</param>
    /// <returns>true if time part is present</returns>
    public static bool HasTimePart(System.DateTime dDate)
    {
      return (dDate.TimeOfDay.Ticks > 0);
    }

    /// <summary>
    /// CustomDateFormat returns the date in a specified format.
    /// Format can be selected from DATE_FORMAT.
    /// </summary>
    /// <param name="dDate">Date to use</param>
    /// <param name="sLanguage">The language to use</param>
    /// <param name="bUseTime">If true, time data is also inserted</param>
    /// <returns></returns>
    /// DDWE/3735 - Dipti
    public static string CustomDateFormat(System.DateTime dDate, string sLanguage, bool bUseTime)
    {
      return CustomDateFormat(dDate, sLanguage, bUseTime, false); // DDWE/5129 - Sanket
    }

    // >> DDWE/5129 - Sanket
    /// <summary>
    /// CustomDateFormat returns the date in a specified format.
    /// Format can be selected from DATE_FORMAT.
    /// </summary>
    /// <param name="dDate">Date to use</param>
    /// <param name="sLanguage">The language to use</param>
    /// <param name="bUseTime">If true, time data is also inserted</param>
    /// <param name="bUseClientTimeOffset">True if Date should be converted based on Client Time Offset.</param>
    /// <returns>Formated date as string.</returns>
    public static string CustomDateFormat(System.DateTime dDate, string sLanguage, bool bUseTime, bool bUseClientTimeOffset)
    {
      string sResult = "";
      string sFormat = "";

      bool bIsExceptionalDate = DecosDate.IsExceptionalDate(dDate);//DDWE/6103[1]-Ashish
      // Convert to Client Date before processing. DDWE/5129
      if (bUseTime && bUseClientTimeOffset) // Berend 20090529
        dDate = DecosDate.ServerToClientDate(dDate);
      if (string.Compare(sLanguage, "EN") == 0)
        sFormat = DATE_FORMAT[0];
      else
        sFormat = DATE_FORMAT[1];
      for (int i = 0; i <= sFormat.Length - 1; i++)
      {
        switch (sFormat.Substring(i, 1).ToUpper())
        {
          case "D":
            //>>DDWE/6103[1]-Ashish
            if (bIsExceptionalDate)
              sResult += "00";
            else
              sResult += dDate.Day.ToString().Trim();
            break;
          case "M":
            if (bIsExceptionalDate && dDate.Second == 1)
              sResult += "00 ";
            else
            {
              switch (sLanguage)
              {
                case "NL":
                  sResult += MONTHNAMES_FULL_NL[dDate.Month - 1];
                  break;
                case "EN":
                  sResult += MONTHNAMES_FULL_EN[dDate.Month - 1];
                  break;
                case "DE":
                  sResult += MONTHNAMES_FULL_DE[dDate.Month - 1];
                  break;
                case "FR":
                  sResult += MONTHNAMES_FULL_FR[dDate.Month - 1];
                  break;
              }
            }
            //<<DDWE/6103[1]-Ashish
            break;
          case "Y":
            sResult += dDate.Year.ToString();
            break;
          default:
            sResult += sFormat.Substring(i, 1);
            break;
        }

      }
      if (bUseTime && !bIsExceptionalDate) //DDWE/6103[1]-Ashish
      {
        // Berend 20090512: removed second call to DecosDate.ServerToClientDate

        //>> Nikunj DDWE/5128
        if ((string.Compare(sLanguage, "EN", true) == 0)) // DDWE/5374 - Ashish
          sResult += " " + String.Format("{0:hh:mm:ss tt}", dDate);
        else
          sResult += " " + String.Format("{0:HH:mm:ss}", dDate);
        //<< Nikunj DDWE/5128        
      }

      return sResult;
    }
    // << DDWE/5129 - Sanket

    //>>DDWE/6103-Ashish
    /// <summary>
    /// Format Exceptional Date with custom format
    /// </summary>
    /// <param name="dtValue">Exceptional Date value</param>
    /// <param name="Culture">Current user Culture</param>
    /// <param name="sFormat">Format in which Exceptional date should be formated</param>
    /// <param name="bUsetime">If true, time data is also used</param>
    /// <returns>Formated date as string</returns>
    public static string FormatExceptionalDate(DateTime dtValue, CultureInfo Culture, string sFormat, bool bUsetime)
    {
      string sRet = string.Empty;
      if (sFormat != null && sFormat.Length > 0)
      {
        if (dtValue.Second == 59)
        {
          sFormat = sFormat.Replace("d", "0");
        }
        else if (dtValue.Second == 1)
        {
          sFormat = sFormat.Replace("d", "0");
          sFormat = sFormat.Replace("M", "0");
        }
        sRet = dtValue.ToString(sFormat, Culture);
      }
      else
        sRet = DateFormat(dtValue, bUsetime, true, Culture.Parent.Name.ToUpper());
      return sRet;
    }
    /// <summary>
    /// Return Whether given date is exceptional date or not.
    /// </summary>
    /// <param name="dtValue">Date to use</param>
    /// <returns>Whether date is exceptional or not</returns>
    public static bool IsExceptionalDate(DateTime dtValue)
    {
      bool bReturn = false;
      if (dtValue != DateTime.MinValue)
      {
        if ((dtValue.Day == 1) && (dtValue.Hour == 0)
          && (dtValue.Minute == 0) && (dtValue.Second == 1))
        {
          bReturn = true;
        }
        else if ((dtValue.Day == 1) && (dtValue.Hour == 0)
          && (dtValue.Minute == 0) && (dtValue.Second == 59))
        {
          bReturn = true;
        }
      }
      return bReturn;
    }
    //<<DDWE/6103-Ashish

    /// <summary>
    /// Converts the current date to a date in the format of
    /// YYYYMMDDHHNNSS
    /// </summary>
    /// <returns></returns>
    public string yyyymmddhhmmss()
    {
      string sReturn;

      sReturn = Value.ToString("yyyyMMddHHmmss");

      return sReturn;
    }

    /// <summary>
    /// Returns the current DecosDate as valid MySQL date
    /// </summary>
    /// <returns></returns>
    public string MySqlDateTime()
    {
      string sReturn;
      sReturn = Value.ToString("yyyy-MM-dd HH:mm:ss");

      return sReturn;
    }

    // > MM/6499
    /// <summary>
    /// IsoDateFormat is internally used in SQL date encoding. Most database servers allow
    /// yyyy-MM-dd format, which is the ISO 8601 date formatting standard.
    /// If the date has a time part, the time will automatically be included in the output.
    /// An empty string is returned for DateTime.MinValue (the date that we consider empty).
    /// </summary>
    /// <param name="dt"></param>
    /// <returns></returns>
    public static string IsoDateTime(DateTime dt)
    {
      return IsoDateTime(dt, HasTimePart(dt));
    }

    /// <summary>
    /// IsoDateFormat is internally used in SQL date encoding. Most database servers allow
    /// yyyy-MM-dd format, which is the ISO 8601 date formatting standard.
    /// An empty string is returned for DateTime.MinValue (the date that we consider empty).
    /// </summary>
    /// <param name="dt"></param>
    /// <param name="bUseTime"></param>
    /// <returns></returns>
    public static string IsoDateTime(DateTime dt, bool bUseTime)
    {
      if (dt != DateTime.MinValue)
      {
        string sFormat = "yyyy-MM-dd";
        if (bUseTime)
          sFormat += " HH:mm:ss";
        return dt.ToString(sFormat);
      }
      return string.Empty;
    }
    // < MM/6499

    /// <summary>
    /// Returns the date value of the current DecosDate
    /// </summary>
    public DateTime Value
    {
      get
      {
        return date_value;
      }
    }

    /// <summary>
    /// Converts the current DecosDate to a string
    /// </summary>
    /// <returns>Formatted date</returns>
    public override string ToString()
    {
      return ToString("EN", false); // Berend 20090512
    }

    /// <summary>
    /// Converts the current DecosDate to a string for a given language
    /// </summary>
    /// <param name="sLanguage">Language code to convert date for</param>
    /// <returns>Formatted date</returns>
    public string ToString(string sLanguage)
    {
      return ToString(sLanguage, false); // Berend 20090512
    }

    //> Berend 20090512
    /// <summary>
    /// Converts the current DecosDate to a string for a given language, optionally
    /// shows time part.
    /// </summary>
    /// <param name="sLanguage">Language code to convert date for</param>
    /// <param name="bUseTime">True to use time</param>
    /// <returns>Formatted date/time</returns>
    public string ToString(string sLanguage, bool bUseTime)
    {
      return DateFormat(date_value, bUseTime, true, sLanguage);
    }
    //< Berend 20090512

    public const int INVALIDCLIENTTIMEOFFSET = 32767;

    public static int ClientTimeOffset
    {
      get
      {
        return 0; //TODO :ClientTimeOffset need to fetch it from JOIN User var table
      }
    }


    // >> DDWE/5129 - Sanket
    /// <summary>
    /// Derives client date from server date based on Client time offset.
    /// </summary>
    /// <param name="dtOnServer">Server date to be used to derive client side date.</param>
    /// <returns>Date object which can be used to show it on client interface. If no Client Time offset is provided then same date will be returned.</returns>
    public static DateTime ServerToClientDate(DateTime dtOnServer)
    {
      DateTime dtOnClient = dtOnServer;
      try
      {
        if ((ClientTimeOffset != INVALIDCLIENTTIMEOFFSET) && HasTimePart(dtOnServer)) // Berend D5.1 issue 576
        {
          dtOnClient = dtOnServer.AddMinutes(ClientTimeOffset);
        }
      }
      catch (Exception ex)
      {
        Trace.WriteLine(ex);
      }
      return dtOnClient;
    }
    /// <summary>
    /// Derives server date from client date based on Client time offset.
    /// </summary>
    /// <param name="dtOnClient">Client date to be used to derive server side date.</param>
    /// <returns>Date object which can be used on server. If no Client Time offset is provided then same date will be returned.</returns>
    public static DateTime ClientToServerDate(DateTime dtOnClient)
    {
      DateTime dtOnServer = dtOnClient;
      try
      {
        if ((ClientTimeOffset != INVALIDCLIENTTIMEOFFSET) && HasTimePart(dtOnClient)) // Berend D5.1 issue 576
        {
          dtOnServer = dtOnClient.AddMinutes(-ClientTimeOffset);
        }
      }
      catch (Exception ex)
      {
        Trace.WriteLine(ex);
      }
      return dtOnServer;
    }
    // << DDWE/5129 - Sanket

    //>>DDWE/6646-Nikunj
    /// <summary>
    /// Finds the next date whose day of the week equals the specified day of the week.
    /// </summary>
    /// <param name="dtStartDate">The date to begin the search.</param>
    /// <param name="desiredDay">The desired day of the week whose date will be returned.</param>
    /// <returns>The returned date is on the given day of this week. If the given day is before given date, the date for the following week's desired day is returned.</returns>
    public static DateTime GetNextDateForDay(DateTime dtStartDate, DayOfWeek desiredDay)
    {
      int iAddDays = ((desiredDay - dtStartDate.DayOfWeek + 6) % 7) + 1;
      return dtStartDate.AddDays(iAddDays);
    }
    //<<DDWE/6646-Nikunj

    //> Berend DWE/9067: moved from Decos.WOPI.Library
    /// <summary>
    /// Returns the date/time for the specified number of seconds since 1970.
    /// </summary>
    /// <param name="lEpochSeconds">Input epoch seconds</param>
    /// <returns>Date/time</returns>
    public static DateTime GetEpochDate(long lEpochSeconds)
    {
      return m_dtEpochZero.AddSeconds(lEpochSeconds);
    }
    //< Berend DWE/9067: moved from Decos.WOPI.Library

    // > JW/HandlerUI/55
    /// <summary>
    /// Returns the abbreviation of the month in the specified date in the correct language 
    /// </summary>
    /// <param name="dt">Date</param>
    /// <param name="sLanguage">Language to be used</param>
    /// <returns></returns>
    public static string MonthAbr(DateTime dt, string sLanguage)
    {
      try
      {
        string sMonth = string.Empty;
        int iMonth = dt.Month - 1;
        switch (sLanguage)
        {
          default:
          case "EN":
            sMonth = DecosDate.MONTHNAMES_EN[iMonth];
            break;
          case "NL":
            sMonth = DecosDate.MONTHNAMES_NL[iMonth];
            break;
          case "FR":
            sMonth = DecosDate.MONTHNAMES_FR[iMonth];
            break;
          case "DE":
            sMonth = DecosDate.MONTHNAMES_DE[iMonth];
            break;
        }
        return sMonth.ToUpper();
      }
      catch (Exception ex)
      {
        System.Diagnostics.Trace.Write(ex);
      }
      return string.Empty;
    }
    // < JW/HandlerUI/55
  }

}
