using System;

namespace DOHClinicians
{
    public class Helper
    {
        public static string ApplyFacilityFormula(string data)
        {
            //if (data[0].ToString() == "0")
            //{
            //    data = "DHA-F-" + data.Substring(1);
            //}
            //else
            //{
            //    data = "DHA-F-" + data;
            //}
            if (data != "NULL" && data != "\\n" && data != string.Empty)
            {
                data = "DHA-F-" + data;
            }


            return data;
        }
        public static string CheckActive(string data)
        {
            string active = string.Empty;

            if (data.ToString() == "Cancelled" || data.ToString() == "InActive" || data.ToString() == "Suspended" || data.ToString() == "Revoked")
                active = "Deactivated";
            if (data.ToString() == "Active")
                active = "Active";

            //if (data.ToString() == "0")
            //    active = "FALSE";
            //if (data.ToString() == "1")
            //    active = "TRUE";


            return active;
        }
        public static string CheckNull(string data)
        {
            string result = string.Empty;
            if (data == null || data == "\n" || data == string.Empty || data == "\\n      " || data == "--")
            {
                result = string.Empty;
            }
            else
            {
                result = data;
            }
            return result;
        }
        public static string CheckComma(string data)
        {
            string result = string.Empty;
            result = data;
            if (data != null)
            {
                if (data.Length > 0)
                {
                    if (data.Contains(","))
                    {
                        result = data.Replace(",", " ");
                    }
                    if (result.Contains("\n"))
                    {
                        result = result.Replace("\n", "\\n");
                    }
                    if (result.Contains("\r"))
                    {
                        result = result.Replace("\r", " ");
                    }
                    if (result.Contains("'"))
                    {
                        result = result.Replace("'", "''");
                    }
                    if (result.Contains("|"))
                    {
                        result = result.Replace("|", "");
                    }
                    if (result.Contains("\t"))
                    {
                        result = result.Replace("\t", "\\t");
                    }
                    if (result.Contains("\""))
                    {
                        result = result.Replace("\"", "");
                    }

                }
                else
                {
                    result = "NULL";
                }
            }
            else
            {
                result = data;
            }
            return result;
        }
        public static string ConvertDate(string Rdate)
        {

            string[] formats = {
                        "dd/MM/yyyy hh:mm:ss tt"
                     ,  "dd/MM/yyyy hh:mm:ss"
                     ,  "dd/MM/yyyy hh:mm:ss t"
                     ,  "dd/MM/yyyy h:m:s t"
                     ,  "dd/MM/yyyy HH:mm:ss"
                     ,  "dd/MM/yyyy HH:mm:ss tt"
                     ,  "dd/MM/yyyy H:m:s t"
                     ,  "dd/MM/yyyy"
                     ,  "d/M/yy hh:mm:ss tt"
                     ,  "M/d/yyyy h:mm:ss tt"
                     ,  "M/d/yyyy h:mm tt"
                     ,  "MM/dd/yyyy hh:mm:ss"
                     ,  "M/d/yyyy h:mm:ss"
                     ,  "M/d/yyyy hh:mm tt"
                     ,  "M/d/yyyy hh tt"
                     ,  "M/d/yyyy h:mm"
                     ,  "M/d/yyyy h:mm"
                     ,  "MM/dd/yyyy hh:mm"
                     ,  "M/dd/yyyy hh:mm"
                     ,  "dd-MM-yyyy"
                     ,  "dd-MM-yyyy HH:mm:ss"
                     ,  "yyyy-MM-dd HH:mm:ss.SSS"
              };

            //Logger.Info("Transforming date " + Rdate);
            string activetoDatedata = string.Empty;
            bool isconverted = false;
            DateTime getDatefromString = DateTime.Now;

            try
            {
                //CustomLog.Info("Converting Date: " + Rdate);
                if (DateTime.TryParseExact(Rdate, formats, new System.Globalization.CultureInfo("en-US"), System.Globalization.DateTimeStyles.None, out getDatefromString))
                {
                    //Logger.Info("Converted '" + Rdate + "' to " + getDatefromString + ".");
                    //CustomLog.Info("Converted Date: " + Rdate + " to date: " + getDatefromString);
                    isconverted = true;
                }
                else
                {
                    //Logger.Info("Unable to convert '" + Rdate + "' to a date.");
                    //CustomLog.Info("Not able to parse date: " + Rdate);
                }

                if (isconverted)
                {
                    activetoDatedata = getDatefromString.ToString("MM/dd/yyyy hh:mm:ss");
                }
                else
                {
                    DateTime tmpDate = DateTime.Now;
                    DateTime dtNeedParsing = DateTime.TryParse(Rdate, out tmpDate) == true ? DateTime.Parse(Rdate) : tmpDate;

                    if (tmpDate == dtNeedParsing)
                    {
                        //Logger.Info(" ~~~~~~~~ not able to parse the date correctly ~~~~~~~~~~~~ ");
                        //CustomLog.Info("Not able to parse date: " + Rdate);
                        activetoDatedata = tmpDate.ToString();
                    }
                    else
                    {
                        activetoDatedata = dtNeedParsing.ToString("MM/dd/yyyy hh:mm:ss");
                    }
                }
                //CustomLog.Info("Final Date: " + activetoDatedata);
                return activetoDatedata;
            }

            catch (Exception ex)
            {
                //CustomLog.Info(ex.Message);
                return null;
            }
        }
        public static string CheckGender(string data)
        {
            string result = string.Empty;
            try
            {
                if (data != null)
                {
                    if (data.ToLower() == "male")
                    {
                        result = "Male";
                    }
                    else if (data.ToLower() == "female")
                    {
                        result = "Female";
                    }
                    else
                    {
                        result = "";
                    }

                }

            }
            catch (Exception ex)
            {
                logger.Info(ex.Message);
            }

            return result;
        }
    }
}
