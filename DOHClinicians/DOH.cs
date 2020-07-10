using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace DOHClinicians
{
    class DOH
    {
        public string baseDir = ConfigurationManager.AppSettings.Get("baseDir");
        public string filename = "DOH_" + DateTime.Now.ToString("yyyyMMdd") + ".xlsx";
        public string Sheetname = ConfigurationManager.AppSettings.Get("Sheetname");
        public bool to_filter = bool.Parse(ConfigurationManager.AppSettings.Get("to_filter"));

        public string CurrentFilePath = string.Empty;
        public string OldFilePath = string.Empty;
        public string filterfilename = "Result";

        public DOH()
        {
            Controller();
        }

        public void Controller()
        {
            logger.Info("DOH CLINICIAN INTEGRATION STARTED");
            CurrentFilePath = baseDir + filename;
            OldFilePath = baseDir + "DOH_" + DateTime.Now.AddDays(-1).ToString("yyyyMMdd") + ".csv";

            try
            {

                if (DownloadFile(CurrentFilePath))
                {
                    if (ConvertFiletoCSV(CurrentFilePath, Sheetname))
                    {
                        CurrentFilePath = baseDir + Path.GetFileNameWithoutExtension(filename) + ".csv";
                        if (to_filter)
                        {
                            if (FilterFile(OldFilePath, CurrentFilePath))
                            {
                                if (CreateFile(baseDir + filterfilename + ".csv"))
                                {

                                }
                            }
                        }
                        else
                        {
                            if (CreateFile(CurrentFilePath))
                            {

                            }
                        }
                    }
                }

                logger.Info("File Downloaded successfully");

            }
            catch (Exception ex)
            {
                logger.Info(ex);
            }
        }

        private bool CreateFile(string FullPath)
        {
            bool result = false;
            logger.Info("Creating file in " + FullPath);
            try
            {
                string header = "License,name,Facility License,Facility Name,area,Active From,Active To,is active,source,Specialty ID 1,Username,Password,Gender,Nationality,Specialty ID 2,Specialty ID 3,type,Email,Phone,Specialty,Specialty Field ID,Specialty Field,major,profession,HAAD_Category,Current Status ,Old License\n";
                List<DOHFile> list_doh = new List<DOHFile>();
                logger.Info("Objectifying file " + CurrentFilePath);
                foreach (string line in File.ReadAllLines(CurrentFilePath))
                {
                    string[] rows = line.Split('^');

                    DOHFile obj_file = new DOHFile();
                    obj_file.ClinicianLicense = Helper.CheckNull(Helper.CheckComma(rows[0]));
                    obj_file.ClinicianName = Helper.CheckNull(Helper.CheckComma(rows[1]));
                    obj_file.Major = Helper.CheckNull(Helper.CheckComma(rows[2]));
                    obj_file.Profession = Helper.CheckNull(Helper.CheckComma(rows[3]));
                    obj_file.Category = Helper.CheckNull(Helper.CheckComma(rows[4]));
                    obj_file.Gender = Helper.CheckNull(Helper.CheckGender(rows[5]));
                    obj_file.FacilityName = Helper.CheckNull(Helper.CheckComma(rows[6]));
                    obj_file.FacilityLicense = Helper.CheckNull(rows[7]);
                    obj_file.Location = Helper.CheckNull(Helper.CheckComma(rows[8]));
                    obj_file.FacilityType = Helper.CheckNull(Helper.CheckComma(rows[9]));
                    obj_file.Status = Helper.CheckNull(Helper.CheckActive(rows[10]));
                    obj_file.From = Helper.CheckNull(LMU_ParserCS.ConvertDate_LMU(Helper.ConvertDate(rows[11])));
                    obj_file.To = Helper.CheckNull(LMU_ParserCS.ConvertDate_LMU(Helper.ConvertDate(rows[12])));

                    logger.Info("Number of objects loaded : " + list_doh.Count);
                    list_doh.Add(obj_file);
                }

                StringBuilder sb = new StringBuilder();
                sb.Append(header);
                string seperator = ",";
                logger.Info("Appending to new file");

                for (int i = 0; i < list_doh.Count; i++)
                {
                    sb.Append(
                        list_doh[i].ClinicianLicense + seperator +
                        list_doh[i].ClinicianName + seperator +
                        list_doh[i].FacilityLicense + seperator +
                        list_doh[i].FacilityName + seperator +
                        list_doh[i].Location + seperator +
                        list_doh[i].From + seperator +
                        list_doh[i].To + seperator +
                        list_doh[i].Status + seperator +
                        "Internal (HAAD)" + seperator +
                        "TBD" + seperator +
                        "" + seperator +
                        "" + seperator +
                        list_doh[i].Gender + seperator +
                        "N/A" + seperator +
                        "N/A" + seperator +
                        "N/A" + seperator +
                        "Internal (HAAD)" + seperator +
                        "N/A" + seperator +
                        "N/A" + seperator +
                        "TBD" + seperator +
                        "N/A" + seperator +
                        "N/A" + seperator +
                        list_doh[i].Major + seperator +
                        list_doh[i].Profession + seperator +
                        list_doh[i].Category + seperator +
                        "N/A" + seperator +
                        "N/A" + "\n"
                        );
                }

                using (StreamWriter writer = File.CreateText(FullPath))
                {
                    writer.Write(sb.ToString());
                }
                result = true;
            }
            catch (Exception ex)
            {
                logger.Info(ex);
            }
            return result;
        }
        private bool DownloadFile(string FullPath)
        {
            bool result = false;
            logger.Info("Downloading file");

            try
            {
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                string URL = ConfigurationManager.AppSettings.Get("URL");
                using (WebClient wc = new WebClient())
                {
                    wc.DownloadFile(URL, FullPath);
                    result = true;
                }
                logger.Info("File downloaded successfully");

            }
            catch (Exception ex)
            {
                logger.Info(ex);
            }
            return result;
        }
        private bool ConvertFiletoCSV(string FullPath, string sheetname)
        {
            bool result = false;
            logger.Info("Convert excel file to CSV");
            try
            {
                DataTable sheetTable = loadSingleSheet(FullPath, sheetname + "$");

                if(sheetTable!=null)
                {
                    if (sheetTable.Rows.Count > 1) 
                    {
                        SQLWriter yo = new SQLWriter();
                        logger.Info("Writing Datatable to file");
                        yo.run(baseDir, Path.GetFileNameWithoutExtension(FullPath) + ".csv", sheetTable);
                        result = true;
                    }
                }

               

                //Microsoft.Office.Interop.Excel.Application xlapp;
                //xlapp = new Microsoft.Office.Interop.Excel.Application();
                //xlapp.Visible = true;
                //Microsoft.Office.Interop.Excel.Workbook xlworkbook = xlapp.Workbooks.Open(CurrentFilePath);
                //xlworkbook = xlapp.ActiveWorkbook;
                //Microsoft.Office.Interop.Excel.Worksheet xlsheet = xlapp.ActiveSheet;
                //xlsheet.SaveAs(baseDir + Path.GetFileNameWithoutExtension(CurrentFilePath) + ".csv", Microsoft.Office.Interop.Excel.XlFileFormat.xlCSV);
                //result = true;
                //CurrentFilePath = baseDir + Path.GetFileNameWithoutExtension(CurrentFilePath) + ".csv";
            }
            catch (Exception ex)
            {
                logger.Info(ex);
            }
            return result;
        }
        private System.Data.OleDb.OleDbConnection returnConnection(string fileName)
        {

            //"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + path + ";Extended Properties=Excel 12.0;";
            return new System.Data.OleDb.OleDbConnection("Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + fileName + ";Extended Properties=\"Excel 12.0;\"");

            //return new System.Data.OleDb.OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + fileName + "; Jet OLEDB:Engine Type=5;Extended Properties=\"Excel 8.0;\"");
        }
        private DataTable loadSingleSheet(string fileName, string sheetName)
        {
            DataTable sheetData = new DataTable();
            try
            {
                using (System.Data.OleDb.OleDbConnection conn = this.returnConnection(fileName))
                {
                    conn.Open();
                    System.Data.OleDb.OleDbDataAdapter sheetAdapter = new System.Data.OleDb.OleDbDataAdapter("select * from [" + sheetName + "]", conn);
                    sheetAdapter.Fill(sheetData);
                }
            }
            catch (Exception ex)
            {
                logger.Info(ex);
            }
            return sheetData;
        }
        private bool FilterFile(string oldfilenmae, string newfilename)
        {
            bool result = false;
            try
            {
                logger.Info("filtering out the file");

                string OlderFile = oldfilenmae;
                string currentFile = newfilename;

                string[] NEW = File.ReadAllLines(Path.Combine(baseDir, currentFile));
                string[] OLD = File.ReadAllLines(Path.Combine(baseDir, OlderFile));
                IEnumerable<String> NEWonly = NEW.Except(OLD);
                File.WriteAllLines(Path.Combine(baseDir, filterfilename + ".csv"), NEWonly);
                result = true;

            }
            catch (Exception ex)
            {
                logger.Info(ex);
            }
            return result;
        }
    }
}
