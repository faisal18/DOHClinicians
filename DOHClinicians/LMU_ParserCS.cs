using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Text;
using Newtonsoft.Json.Linq;

namespace DOHClinicians
{
    public class LMU_ParserCS
    {


        public static void LMU_Controller(string filepath)
        {
            try
            {
                Upload_File(LMU_Parser(filepath));
            }
            catch (Exception ex)
            {
                logger.Info(ex);
            }
        }

        private static string LMU_Parser(string filepath)
        {
            string baseDir = ConfigurationManager.AppSettings.Get("baseDir");
            string newfilename = "Clinicians_DOH_" + DateTime.Now.ToString("yyyyMMdd") + "_LMUPassed.csv";

            try
            {
                logger.Info("Parsing file with LMU");

                string[] lines = File.ReadAllLines(filepath);

                StringBuilder sb = new StringBuilder();
                sb.Append("License,name,Username,Password,Facility License,Facility Name,area,Active From,Active To,is active,source," +
                            "Specialty ID 1,Specialty,Gender,Nationality,Email,Phone,Specialty ID 2,Specialty ID 3,type,Old License\n");

                string latestversion = GetLMULatest();
                string Specialitiesversion = GetLMUSpecialitesLatest();

                if (lines.Length > 0)
                {
                    for (int i = 1; i < lines.Length; i++)
                    {


                        string[] data = lines[i].Split(',');
                        logger.Info("LMU iteration " + i + " clinician license " + data[0]);


                        string license = data[0];
                        string license_start = data[7];
                        string license_end = data[8];
                        string isActive = data[9];
                        string source = data[10];

                        //string SpecialityID = data[11];
                        //string Speciality = data[12];




                        GateParams obj = GetClinicianRecord(license, latestversion);
                        obj.DHA_Input = CheckActive_Reverse(isActive);
                        obj = GetTruthTable(obj);
                        isActive = obj.Out_isActive;
                        source = obj.Out_Source;

                        //Speciality = GetLMURecordForSpecialities(SpecialityID, Specialitiesversion);

                        license_start = ConvertDate_LMU(license_start);
                        license_end = ConvertDate_LMU(license_end);



                        sb.Append(
                            data[0] + "," +
                            data[1] + "," +
                            data[2] + "," +
                            data[3] + "," +
                            data[4] + "," +
                            data[5] + "," +
                            data[6] + "," +
                            license_start + "," +
                            license_end + "," +
                            isActive + "," +
                            source + "," +
                            data[11] + "," +

                            //Speciality + "," +
                            data[12] + "," +

                            data[13] + "," +
                            data[14] + "," +
                            data[15] + "," +
                            data[16] + "," +
                            data[17] + "," +
                            data[18] + "," +
                            data[19] + "," +
                            data[20] + "\n"
                            );
                    }


                    Console.WriteLine("Writting LMU file");


                    using (StreamWriter wrtier = File.CreateText(baseDir + "\\" + newfilename))
                    {
                        wrtier.Write(sb.ToString());
                    }

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return baseDir + newfilename;
        }
        private static string GetLMULatest()
        {
            string result = string.Empty;
            try
            {
                string url = ConfigurationManager.AppSettings.Get("LMU_URL") + ConfigurationManager.AppSettings.Get("LMU_Clinician_Latest");
                string username = ConfigurationManager.AppSettings.Get("LMU_Username");
                string token = ConfigurationManager.AppSettings.Get("LMU_Token");
                result = PostCall_ByBody(url, "", token, username, false);
                //result = "7539";
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return result;
        }
        private static string GetLMUSpecialitesLatest()
        {
            string result = string.Empty;
            try
            {
                string url = ConfigurationManager.AppSettings.Get("LMU_URL") + ConfigurationManager.AppSettings.Get("LMU_Specialities_Latest");
                string username = ConfigurationManager.AppSettings.Get("LMU_Username");
                string token = ConfigurationManager.AppSettings.Get("LMU_Token");
                result = PostCall_ByBody(url, "", token, username, false);
                //result = "7539";
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return result;
        }
        private static GateParams GetClinicianRecord(string data, string latestversion)
        {
            string result = string.Empty;
            GateParams obj = new GateParams();

            try
            {
                string url = ConfigurationManager.AppSettings.Get("LMU_URL") + ConfigurationManager.AppSettings.Get("LMU_Clinician"); ;
                string username = ConfigurationManager.AppSettings.Get("LMU_Username");
                string token = ConfigurationManager.AppSettings.Get("LMU_Token");
                string license = data;


                //string body = "{\r\n \"oldVersion\": 0,\"targetVersion\": " + GetLMULatest() + ",\r\n \"param\" : \"license=" + license + "\" \r\n}";
                //string body = "{\r\n \"oldVersion\": 0,\"targetVersion\": " + latestversion + ",\r\n \"param\" : \"license=" + license + "\" \r\n}";

                result = PostCall_ByBody(url, data, token, username, true);



                JObject yo = JObject.Parse(result);
                if (yo["content"] != null)
                {
                    if (yo["content"].First != null)
                    {
                        string status = yo["content"].First["status"].ToString();
                        string isActive = yo["content"].First["values"]["isActive"].ToString();
                        string source = yo["content"].First["values"]["source"].ToString();

                        obj.LMU_isActive = isActive;
                        obj.LMU_Source = source;
                        obj.LMU_Status = status;
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return obj;
        }
        private static string GetLMURecordForSpecialities(string data, string Specialitiesversion)
        {
            string result = string.Empty;
            string description = string.Empty;
            try
            {
                string url = ConfigurationManager.AppSettings.Get("LMU_URL") + ConfigurationManager.AppSettings.Get("LMU_Specialities"); ;
                string username = ConfigurationManager.AppSettings.Get("LMU_Username");
                string token = ConfigurationManager.AppSettings.Get("LMU_Token");
                string body = "{\r\n \"oldVersion\": 0,\"targetVersion\": " + Specialitiesversion + ",\r\n \"param\" : \"specialtyId=" + data + "\" \r\n}";


                if (data != null)
                {
                    if (data.Length > 1)
                    {
                        result = PostCall_ByBody(url, body, token, username, false);
                        logger.Info(result);
                        JObject yo = JObject.Parse(result);
                        if (yo["content"] != null)
                        {
                            if (yo["content"].First != null)
                            {
                                string specialty = yo["content"].First["values"]["specialty"].ToString();
                                description = specialty;
                            }
                        }
                    }
                }



            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return description;
        }

        private static GateParams GetTruthTable(GateParams obj)
        {
            try
            {


                if (obj.LMU_isActive != null && obj.LMU_Source != null)
                {

                    if (obj.DHA_Input.ToUpper() == "TRUE" && obj.LMU_isActive.ToUpper() == "TRUE" && obj.LMU_Source.ToUpper() == "ALL")
                    {
                        obj.Out_isActive = "TRUE";
                        obj.Out_Source = "ALL";
                    }

                    else if (obj.DHA_Input.ToUpper() == "FALSE" && obj.LMU_isActive.ToUpper() == "TRUE" && obj.LMU_Source.ToUpper() == "ALL")
                    {
                        obj.Out_isActive = "TRUE";
                        obj.Out_Source = "HAAD";
                    }


                    else if (obj.DHA_Input.ToUpper() == "TRUE" && obj.LMU_isActive.ToUpper() == "FALSE" && obj.LMU_Source.ToUpper() == "ALL")
                    {
                        obj.Out_isActive = "TRUE";
                        obj.Out_Source = "DHA";
                    }

                    else if (obj.DHA_Input.ToUpper() == "TRUE" && obj.LMU_isActive.ToUpper() == "FALSE" && obj.LMU_Source.ToUpper() == "DHA")
                    {
                        obj.Out_isActive = "TRUE";
                        obj.Out_Source = "DHA";
                    }

                    else if (obj.DHA_Input.ToUpper() == "FALSE" && obj.LMU_isActive.ToUpper() == "TRUE" && obj.LMU_Source.ToUpper() == "DHA")
                    {
                        obj.Out_isActive = "FALSE";
                        obj.Out_Source = "DHA";
                    }

                    else if (obj.DHA_Input.ToUpper() == "FALSE" && obj.LMU_isActive.ToUpper() == "FALSE" && obj.LMU_Source.ToUpper() == "DHA")
                    {
                        obj.Out_isActive = "FALSE";
                        obj.Out_Source = "DHA";
                    }

                    else if (obj.DHA_Input.ToUpper() == "TRUE" && obj.LMU_isActive.ToUpper() == "TRUE" && obj.LMU_Source.ToUpper() == "DHA")
                    {
                        obj.Out_isActive = "TRUE";
                        obj.Out_Source = "DHA";
                    }

                    else if (obj.DHA_Input.ToUpper() == "TRUE" && obj.LMU_isActive.ToUpper() == "TRUE" && obj.LMU_Source.ToUpper() == "HAAD")
                    {
                        obj.Out_isActive = "TRUE";
                        obj.Out_Source = "ALL";
                    }

                    else if (obj.DHA_Input.ToUpper() == "FALSE" && obj.LMU_isActive.ToUpper() == "TRUE" && obj.LMU_Source.ToUpper() == "HAAD")
                    {
                        obj.Out_isActive = "TRUE";
                        obj.Out_Source = "HAAD";
                    }

                }


                else if (obj.DHA_Input.ToUpper() == "TRUE")
                {
                    obj.Out_isActive = "TRUE";
                    obj.Out_Source = "DHA";
                }


                else if (obj.DHA_Input.ToUpper() == "FALSE")
                {
                    obj.Out_isActive = "FALSE";
                    obj.Out_Source = "DHA";
                }




            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return obj;
        }
        private static string CheckActive_Reverse(string data)
        {
            string active = string.Empty;

            if (data.ToString() == "Deactivated")
                active = "False";
            if (data.ToString() == "Active")
                active = "True";
            else
                active = "False";

            return active;
        }
        private static string PostCall_ByBody(string URL, string postdata, string accessKey, string username, bool isGet)
        {
            string result = string.Empty;
            bool OKAY = false;
            int counter = 0;
            //int counter_limit = int.Parse(ConfigurationManager.AppSettings.Get("counter_limit"));
            int counter_limit = 1;

            try
            {
                while (!OKAY)
                {
                    try
                    {
                        using (WebClient wc = new WebClient())
                        {
                            wc.Headers[HttpRequestHeader.ContentType] = "application/json";
                            //wc.Headers[HttpRequestHeader.Authorization] = staticToken;

                            wc.Headers.Set("username", username);
                            wc.Headers.Set("access-key", accessKey);

                            logger.Info("LMU Clinicians called at " + DateTime.Now);
                            if (!isGet)
                            {
                                result = wc.UploadString(URL, postdata);
                            }
                            if (isGet)
                            {
                                string complete = URL + "?queryString=values.license|EQ|" + postdata;
                                result = wc.DownloadString(complete);
                            }

                            //CustomLog.Info(result);
                            //Console.WriteLine(result);

                            if (result != null)
                                if (result.Length > 0)
                                {
                                    //JObject yo = JObject.Parse(result);
                                    //if (yo["ReturnCode"] != null)
                                    //{
                                    //    Console.WriteLine("Return Code: " + yo["ReturnCode"].ToString());
                                    //    if (yo["ReturnCode"].ToString() == "00")
                                    //    {
                                    //        OKAY = true;
                                    //        //("Results found for data: " + postdata);
                                    //        //CustomLog.Info("Results found for data: " + postdata);
                                    //    }
                                    //    if (yo["ReturnCode"].ToString() == "10" || yo["ReturnCode"].ToString() == "07")
                                    //    {
                                    //        OKAY = true;
                                    //        //("Results found for data: " + postdata);
                                    //        //CustomLog.Info(yo["ReturnMessage"].ToString());
                                    //    }
                                    //}
                                    //else
                                    //{
                                    //    Console.WriteLine("Result: " + yo.ToString());
                                    //    //CustomLog.Info(yo.ToString());
                                    //    Console.WriteLine(yo.ToString());

                                    //}

                                    OKAY = true;

                                }
                                else if (counter > counter_limit)
                                {
                                    OKAY = true;
                                    Console.WriteLine("Tried hitting the service " + counter_limit + " times but no results found");

                                }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.Write(ex.Message);
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception Occured in PostCall_ByBody !\n" + result);
                return ex.Message;
            }
        }
        private static string ConvertDate_LMU(string date)
        {
            string resultdate = date;
            try
            {
                if (date != null)
                {
                    if (date.Length > 0)
                    {
                        resultdate = Convert.ToDateTime(date).ToString("yyyy-MM-dd");
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return resultdate;
        }

        private static void Upload_File(string local_file_path)
        {
            string ftpUserName = System.Configuration.ConfigurationManager.AppSettings["FTPUsername"];
            string ftpPassword = System.Configuration.ConfigurationManager.AppSettings["FTPPassword"];
            string FTPServerPath = System.Configuration.ConfigurationManager.AppSettings["FTPLocalPath"];
            string ftpServerIP = System.Configuration.ConfigurationManager.AppSettings["FTPHost"];
            try
            {

                logger.Info("Uploading file " + local_file_path + " to address " + ftpServerIP + FTPServerPath + "/" + Path.GetFileName(local_file_path));
                string requestpath = @"ftp://" + ftpServerIP + FTPServerPath + "/" + Path.GetFileName(local_file_path);
                using (var client = new WebClient())
                {
                    client.Credentials = new NetworkCredential(ftpUserName, ftpPassword);
                    client.UploadFile(requestpath, WebRequestMethods.Ftp.UploadFile, local_file_path);
                    logger.Info("File uploaded successfully on path : "+requestpath);
                }
            }
            catch (Exception ex)
            {
                logger.Info(ex);
            }

        }
    }
}
