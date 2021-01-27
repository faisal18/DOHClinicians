using ExcelDataReader;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DOHClinicians
{
    class ExceltoCSV
    {
        public static DataTable execute(string fileName, string sheetName)
        {
            DataTable dt = null;

            try
            {
                logger.Info("Opening file stream");
                using (FileStream stream = File.Open(fileName, FileMode.Open, FileAccess.ReadWrite))
                {
                    IExcelDataReader excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream);
                    DataSet result = excelReader.AsDataSet();
                    int tbl_count = result.Tables.Count;
                    for (int i = 0; i < tbl_count; i++)
                    {
                        if (result.Tables[i].ToString() == sheetName)
                        {
                            dt = result.Tables[i];
                        }
                    }

                    logger.Info("Closing file stream");
                    stream.Dispose();
                    stream.Close();
                }
            }
            catch(Exception ex)
            {
                logger.Info(ex);
            }
            return dt;
        }
    }
}
