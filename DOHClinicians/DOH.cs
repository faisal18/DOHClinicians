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

        public string Clinician_filename = "Clinician_" + DateTime.Now.ToString("yyyyMMdd") + ".xlsx";
        public string Clinician_Transformed_filename = "Clinician_Transformed_" + DateTime.Now.ToString("yyyyMMdd") + ".csv";
        public string ClinicianHistory_filename = "ClinicianHistory_" + DateTime.Now.ToString("yyyyMMdd") + ".xlsx";

        public string ClinicianSheetname = ConfigurationManager.AppSettings.Get("ClinicianSheetname");
        public string ClinicianHistorySheetname = ConfigurationManager.AppSettings.Get("ClinicianHistorySheetname");
        public bool to_filter = bool.Parse(ConfigurationManager.AppSettings.Get("to_filter"));

        public string Clinician_FilePath = string.Empty;
        public string ClinicianHistory_FilePath = string.Empty;
        public string Clinician_Transformed_FilePath = string.Empty;

        public string OldFilePath = string.Empty;
        public string filterfilename = "Result";

        public DOH()
        {
            Controller();
        }
        public void Controller()
        {
            //OldFilePath = baseDir + "Clinician_" + DateTime.Now.AddDays(-1).ToString("yyyyMMdd") + ".csv";

            logger.Info("DOH CLINICIAN INTEGRATION STARTED");
            Clinician_FilePath = baseDir + Clinician_filename;
            ClinicianHistory_FilePath = baseDir + ClinicianHistory_filename;
            Clinician_Transformed_FilePath = baseDir + Clinician_Transformed_filename;

            try
            {
                CreateOutputFile(GetActiveCliniciansController(Clinician_FilePath, ClinicianHistory_FilePath), Clinician_Transformed_FilePath);
                logger.Info("File Downloaded successfully");
            }
            catch (Exception ex)
            {
                logger.Info(ex);
            }
        }
        private void CreateOutputFile(List<Clinicians> list_doh,string FullPath)
        {
            bool result = false;
            try
            {
                StringBuilder sb = new StringBuilder();
                string header = "License,name,Facility License,Facility Name,area,Active From,Active To,is active,source,Specialty ID 1,Username,Password,Gender,Nationality,Specialty ID 2,Specialty ID 3,type,Email,Phone,Specialty,Specialty Field ID,Specialty Field,major,profession,HAAD_Category,Current Status ,Old License\n";
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
        }


        private List<Clinicians> GetActiveCliniciansController(string CliniciansPath,string CliniciansHistoryPath)
        {
            List<Clinicians> lst_Clinician_new = new List<Clinicians>();
            try
            {
                string Clinician_URL = ConfigurationManager.AppSettings.Get("CliniciansURL");
                DownloadFile(CliniciansPath, Clinician_URL);
                ConvertFiletoCSV(CliniciansPath, ClinicianSheetname);
                List<Clinicians> lst_Clinician = ListofClinicians(baseDir + Clinician_filename + ".csv");

                string ClinicianHistory_URL = ConfigurationManager.AppSettings.Get("CliniciansHistoryURL");
                DownloadFile(CliniciansHistoryPath, ClinicianHistory_URL);
                ConvertFiletoCSV(CliniciansHistoryPath, CliniciansHistoryPath);
                List<CliniciansHistory> lst_ClinicianHistory = ListofCliniciansHistory(baseDir + ClinicianHistory_filename + ".csv");


                if (lst_Clinician.Count > 0 && lst_ClinicianHistory.Count > 0) 
                {
                    lst_Clinician = lst_Clinician.Where(x => x.Status.ToUpper() == "ACTIVE").ToList();
                    foreach (Clinicians obj in lst_Clinician)
                    {
                        if(lst_ClinicianHistory.Any(x => x.LicenseNumber == obj.ClinicianLicense))
                        {
                            lst_Clinician_new.Add(obj);
                        }
                    }
                }
                else
                {
                    logger.Info("Something went wrong");
                }

            }
            catch (Exception ex)
            {
                logger.Info(ex);
            }

            return lst_Clinician_new;
        }

        private List<Clinicians> ListofClinicians(string FullPath)
        {
            bool result = false;
            List<Clinicians> list_doh = new List<Clinicians>();

            logger.Info("Creating file in " + FullPath);
            try
            {
                
                logger.Info("Objectifying file " + FullPath);
                int row = 0;
                foreach (string line in File.ReadAllLines(FullPath))
                {
                    row = row + 1;
                    if(row==1)
                    {
                        continue;
                    }

                    string[] rows = line.Split('^');

                    Clinicians obj_file = new Clinicians();
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
                    obj_file.From = DateTime.ParseExact(Helper.CheckNull(rows[11]), "d/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
                    obj_file.To = DateTime.ParseExact(Helper.CheckNull(rows[12]), "d/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);

                    logger.Info("Number of Clinicians parsed : " + list_doh.Count);
                    list_doh.Add(obj_file);
                }

                list_doh = list_doh.Where(x => x.Status.ToUpper() == "ACTIVE").ToList();
                
            }
            catch (Exception ex)
            {
                logger.Info(ex);
            }
            return list_doh;
        }
        private List<CliniciansHistory> ListofCliniciansHistory(string FullPath)
        {
            List<CliniciansHistory> lst_obj = new List<CliniciansHistory>();
            try
            {
                //take latest only
                //dd-MMM-yyyy
                int row = 0;
                foreach(string line in File.ReadAllLines(FullPath))
                {
                    row = row + 1;
                    if (row == 1)
                    {
                        continue;
                    }
                    string[] rows = line.Split('^');
                    CliniciansHistory obj = new CliniciansHistory();

                    obj.LicenseNumber = Helper.CheckNull(Helper.CheckComma(rows[0]));
                    obj.FacilityLicenseNumber = Helper.CheckNull(Helper.CheckComma(rows[1]));
                    obj.EffectiveDate = DateTime.ParseExact(Helper.CheckNull(rows[2]), "d-MMM-yy", System.Globalization.CultureInfo.InvariantCulture);
                    obj.Status = Helper.CheckNull(Helper.CheckComma(rows[3]));
                    lst_obj.Add(obj);
                }

                lst_obj = lst_obj.GroupBy(x => x.LicenseNumber).Select(x => x.OrderByDescending(y => y.EffectiveDate).First()).ToList();
                lst_obj = lst_obj.Where(x => x.Status.ToUpper() == "ACTIVE").ToList();
            }
            catch (Exception ex)
            {
                logger.Info(ex);
            }
            return lst_obj;
        }

        private bool DownloadFile(string FullPath,string URL)
        {
            bool result = false;
            logger.Info("Downloading file");

            try
            {
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
               
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
