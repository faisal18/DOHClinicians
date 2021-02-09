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
        #region GlobalVariables
        public string baseDir = ConfigurationManager.AppSettings.Get("baseDir");

        public string Clinician_filename = "Clin_" + DateTime.Now.ToString("yyyyMMdd") + ".xlsx";
        public string ClinicianHistory_filename = "ClinHist_" + DateTime.Now.ToString("yyyyMMdd") + ".xlsx";

        public string Clinician_Transformed_filename = "ClinTrans_" + DateTime.Now.ToString("yyyyMMdd") + ".csv";
        public string OldFileName = "ClinTrans_" + DateTime.Now.AddDays(-1).ToString("yyyyMMdd") + ".csv";
        public string filterfilename = "ClinFiltered_" + DateTime.Now.ToString("yyyyMMdd") + ".csv";

        public string ClinicianSheetname = ConfigurationManager.AppSettings.Get("ClinicianSheetname");
        public string ClinicianHistorySheetname = ConfigurationManager.AppSettings.Get("ClinicianHistorySheetname");

        public bool to_filter = bool.Parse(ConfigurationManager.AppSettings.Get("to_filter"));
        public bool LMU_Upload = bool.Parse(ConfigurationManager.AppSettings.Get("LMU_Upload"));

        public string Clinician_FilePath = string.Empty;
        public string ClinicianHistory_FilePath = string.Empty;
        public string Clinician_Transformed_FilePath = string.Empty;
        public string Clinician_Transformed_Old_FilePath = string.Empty;
        public string Clinician_Filtered_FilePath = string.Empty;




        #endregion

        #region MainFunctions

        public void Controller()
        {
            
            logger.Info("DOH CLINICIAN INTEGRATION STARTED");
            Clinician_FilePath = baseDir + Clinician_filename;
            ClinicianHistory_FilePath = baseDir + ClinicianHistory_filename;
            Clinician_Transformed_FilePath = baseDir + Clinician_Transformed_filename;
            Clinician_Transformed_Old_FilePath = baseDir + OldFileName;
            Clinician_Filtered_FilePath = baseDir + filterfilename;

            try
            {

                //if(true)
                if (CreateOutputFile(GetActiveCliniciansController(Clinician_FilePath, ClinicianHistory_FilePath), Clinician_Transformed_FilePath))

                {
                    logger.Info("File Downloaded successfully");

                    bool uploaded = false;

                    if (to_filter)
                    {
                        if (File.Exists(Clinician_Transformed_Old_FilePath))
                        {
                            if (FilterFile(Clinician_Transformed_Old_FilePath, Clinician_Transformed_FilePath))
                            {
                                if (LMU_Upload)
                                {
                                    RemoveDuplicates(Clinician_Filtered_FilePath);
                                    LMU_ParserCS.LMU_Controller(Clinician_Filtered_FilePath);
                                    uploaded = true;

                                }
                            }
                        }
                    }

                    if (LMU_Upload == true && uploaded == false)
                    {
                        RemoveDuplicates(Clinician_Filtered_FilePath);
                        LMU_ParserCS.LMU_Controller(Clinician_Transformed_FilePath);
                    }
                }


                
            }
            catch (Exception ex)
            {
                logger.Info(ex);
            }
        }
        private bool CreateOutputFile(List<Clinicians> list_doh, string FullPath)
        {
            bool result = false;
            try
            {

                StringBuilder sb = new StringBuilder();
                //string header = "License,name,Username,Password,Facility License,Facility Name,area,Active From,Active To,is active,source,Specialty ID 1,Specialty,Gender,Nationality,Email,Phone,Specialty ID 2,Specialty ID 3,type,Old License,SpecialtyFieldID,SpecialtyField,Major,Profession,Haad Category,Current Status\n";
                string header = "License,name,Username,Password,Facility License,Facility Name,area,Active From,Active To,is active,source,Specialty ID 1,Specialty,Gender,Nationality,Email,Phone,Specialty ID 2,Specialty ID 3,type,Old License,Specialty Field ID,Specialty Field,major,profession,HAAD_Category,Current Status \n";

                sb.Append(header);
                string seperator = ",";
                logger.Info("Appending to Main file");

                for (int i = 0; i < list_doh.Count; i++)
                {
                    logger.Info("Adding record " + list_doh[i].ClinicianLicense);
                    sb.Append(
                        list_doh[i].ClinicianLicense + seperator +
                        list_doh[i].ClinicianName + seperator +
                        "" + seperator + // Username
                        "" + seperator + // Password
                        list_doh[i].FacilityLicense + seperator +
                        list_doh[i].FacilityName + seperator +
                        list_doh[i].Location + seperator +
                        list_doh[i].From + seperator +
                        list_doh[i].To + seperator +
                        list_doh[i].Status + seperator +
                        "HAAD" + seperator + // Source
                        "" + seperator + // SP ID 1
                        "" + seperator + // Speciality
                        list_doh[i].Gender + seperator + // Gender
                        "" + seperator + // Nationality
                        "" + seperator + // Email
                        "" + seperator + // Phone
                        "" + seperator + // SP ID 2
                        "" + seperator + // SP ID 3
                        "Internal (HAAD)" + seperator + // Type
                        "" + seperator + // Oldlicense

                        //EXTRA FIELDS from HAAD
                        "" + seperator + // SPF ID
                        "" + seperator + //SPF
                        list_doh[i].Major + seperator + // Major
                        list_doh[i].Profession + seperator + // Profession
                        list_doh[i].Category + seperator + // Category
                        "" + "\n" // CurrentStatus


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

        #endregion

        #region MakeAList
        private List<Clinicians> GetActiveCliniciansController(string CliniciansPath, string CliniciansHistoryPath)
        {
            List<Clinicians> lst_Clinician_new = new List<Clinicians>();
            List<Clinicians> lst_Clinician = null;
            List<CliniciansHistory> lst_ClinicianHistory = null;

            string Clinician_URL = ConfigurationManager.AppSettings.Get("CliniciansURL");
            string ClinicianHistory_URL = ConfigurationManager.AppSettings.Get("CliniciansHistoryURL");

            try
            {

                if (DownloadFile(CliniciansPath, Clinician_URL))
                {
                    ConvertFiletoCSV(CliniciansPath, ClinicianSheetname);
                    lst_Clinician = ListofClinicians(baseDir + Path.GetFileNameWithoutExtension(Clinician_filename) + ".csv");
                }

                if (DownloadFile(CliniciansHistoryPath, ClinicianHistory_URL))
                {
                    ConvertFiletoCSV(CliniciansHistoryPath, ClinicianHistorySheetname);
                    lst_ClinicianHistory = ListofCliniciansHistory(baseDir + Path.GetFileNameWithoutExtension(ClinicianHistory_filename) + ".csv");
                }

                //lst_Clinician = ListofClinicians(baseDir + Path.GetFileNameWithoutExtension(Clinician_filename) + ".csv");
                //lst_ClinicianHistory = ListofCliniciansHistory(baseDir + Path.GetFileNameWithoutExtension(ClinicianHistory_filename) + ".csv");

                if (lst_Clinician.Count > 1 && lst_ClinicianHistory.Count > 1)
                {
                    logger.Info("Both lists Parsed succssesufully");

                    lst_Clinician = lst_Clinician.Where(x => x.Status.ToUpper() == "ACTIVE").ToList();
                    foreach (Clinicians obj in lst_Clinician)
                    {


                        logger.Info("Mapping active clinician " + obj.ClinicianLicense + " to active clinician history");
                        if (lst_ClinicianHistory.Any(x => x.LicenseNumber == obj.ClinicianLicense))
                        {
                            logger.Info("Clinician " + obj.ClinicianLicense + " added to the main list");
                            lst_Clinician_new.Add(obj);
                        }
                        else
                        {
                            logger.Info("Clinician " + obj.ClinicianLicense + " not added to the main list");
                        }

                        //if (obj.ClinicianLicense != "GD25903")
                        //{
                        //    continue;
                        //}
                        //List<CliniciansHistory> lst_ClinicianHistory2 = lst_ClinicianHistory.Where(x => x.LicenseNumber == "GD25903").ToList();
                        //if (lst_ClinicianHistory2.Any(x => x.LicenseNumber == obj.ClinicianLicense))
                        //{
                        //    logger.Info("Clinician " + obj.ClinicianLicense + " added to the main list");
                        //    lst_Clinician_new.Add(obj);
                        //}
                        //else
                        //{
                        //    logger.Info("Clinician " + obj.ClinicianLicense + " not added to the main list");
                        //}

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
            //logger.Info("Creating file in " + FullPath);

            try
            {

                logger.Info("Objectifying file " + FullPath);
                int row = 0;
                try
                {
                    foreach (string line in File.ReadAllLines(FullPath))
                    {
                        row = row + 1;
                        if (row == 1)
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
                        //obj_file.From = DateTime.ParseExact(Helper.CheckNull(rows[11]), "d/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
                        //obj_file.To = DateTime.ParseExact(Helper.CheckNull(rows[12]), "d/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);

                        obj_file.From = Helper.DateParser(rows[11]);
                        obj_file.To = Helper.DateParser(rows[12]);


                        logger.Info("Number of Clinicians parsed : " + list_doh.Count + " having license : " + obj_file.ClinicianLicense);
                        list_doh.Add(obj_file);
                    }
                }
                catch (Exception ex)
                {
                    logger.Info("Inner loop exception in Clinician on parsing record " + (list_doh.Count + 1) + " exception:" + ex.Message);
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
                foreach (string line in File.ReadAllLines(FullPath))
                {
                    try
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

                        //obj.EffectiveDate = DateTime.ParseExact(Helper.CheckNull(rows[2]), "d-MMM-yy", System.Globalization.CultureInfo.InvariantCulture);
                        //obj.EffectiveDate = Helper.ConvertDate(rows[2]);
                        obj.EffectiveDate = Convert.ToDateTime(Helper.ConvertDate(rows[2]));

                        obj.Status = Helper.CheckNull(Helper.CheckComma(rows[3]));
                        lst_obj.Add(obj);

                        logger.Info("Number of CliniciansHistory parsed : " + lst_obj.Count + " having license : " + obj.LicenseNumber);

                    }
                    catch (Exception ex)
                    {
                        logger.Info("Inner loop exception in CliniciansHistory on parsing record " + (lst_obj.Count + 1) + " exception:" + ex.Message);
                    }
                }

                logger.Info("Process complete");

                lst_obj = lst_obj.GroupBy(x => x.LicenseNumber).Select(x => x.OrderByDescending(y => y.EffectiveDate).First()).ToList();
                lst_obj = lst_obj.Where(x => x.Status.ToUpper() == "ACTIVE").ToList();
            }
            catch (Exception ex)
            {
                logger.Info(ex);
            }
            return lst_obj;
        }
        private List<CliniciansTransformed> ObjectifyTransformedfiles(string filepath)
        {
            List<CliniciansTransformed> list = new List<CliniciansTransformed>();

            try
            {

                string[] lines = File.ReadAllLines(filepath);
                foreach (string line in lines)
                {
                    string[] data = line.Split(',');

                    list.Add(
                        new CliniciansTransformed
                        {
                            License = data[0],
                            name = data[1],
                            Username = data[2],
                            Password = data[3],
                            FacilityLicense = data[4],
                            FacilityName = data[5],
                            area = data[6],
                            ActiveFrom = data[7],
                            ActiveTo = data[8],
                            isactive = data[9],
                            source = data[10],
                            SpecialtyID1 = data[11],
                            Specialty = data[12],
                            Gender = data[13],
                            Nationality = data[14],
                            Email = data[15],
                            Phone = data[16],
                            SpecialtyID2 = data[17],
                            SpecialtyID3 = data[18],
                            type = data[19],
                            OldLicense = data[20],
                            SpecialtyFieldID = data[21],
                            SpecialtyField = data[22],
                            major = data[23],
                            profession = data[24],
                            HAAD_Category = data[25],
                            CurrentStatus = data[26]
                        }
                    );
                }
            }
            catch (Exception ex)
            {
                logger.Info(ex);
            }

            return list;
        }

        #endregion

        #region Custom
        private static void RemoveDuplicates(string filepath)
        {
            logger.Info("Removing Duplicates");
            try
            {
                string[] linese = File.ReadAllLines(filepath);
                File.WriteAllLines(filepath, linese.Distinct().ToArray());
            }
            catch (Exception ex)
            {
                logger.Info(ex);
            }
            logger.Info("Duplicates Removed");

        }
        private bool DownloadFile(string FullPath, string URL)
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
            logger.Info("Converting excel file to CSV");
            try
            {
                DataTable sheetTable = ExceltoCSV.execute(FullPath, sheetname);

                if (sheetTable != null)
                {
                    if (sheetTable.Rows.Count > 1)
                    {
                        SQLWriter yo = new SQLWriter();
                        logger.Info("Writing Datatable to file");
                        yo.run(baseDir, Path.GetFileNameWithoutExtension(FullPath) + ".csv", sheetTable);
                        result = true;
                        logger.Info("Converion successfull");
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Info(ex);
            }
            return result;
        }
        private bool FilterFile(string OldFilePath, string NewFilePath)
        {
            bool result = false;
            string header = "License,name,Username,Password,Facility License,Facility Name,area,Active From,Active To,is active,source,Specialty ID 1,Specialty,Gender,Nationality,Email,Phone,Specialty ID 2,Specialty ID 3,type,Old License,Specialty Field ID,Specialty Field,major,profession,HAAD_Category,Current Status \n";

            try
            {
                logger.Info("filtering out the file");
                string[] NEW = File.ReadAllLines(NewFilePath);
                string[] OLD = File.ReadAllLines(OldFilePath);
                IEnumerable<String> NEWonly = NEW.Except(OLD);
                File.WriteAllText(Clinician_Filtered_FilePath, header);
                File.AppendAllLines(Clinician_Filtered_FilePath, NEWonly);
                //File.AppendAllLines()
                result = true;

            }
            catch (Exception ex)
            {
                logger.Info(ex);
            }
            return result;
        }
        #endregion


    }
}
