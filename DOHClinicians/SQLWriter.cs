using System;
using System.Data;
using System.IO;
using System.Text;

namespace DOHClinicians
{
    public class SQLWriter
    {
        public void run(string basedir, string filename, DataTable dataTable)
        {
            try
            {
                Wrtier(basedir, filename, dataTable);
            }
            catch (Exception ex)
            {
                logger.Info(ex);
            }
        }
        private void Wrtier(string basedir, string filename, DataTable dataTable)
        {
            StringBuilder sb = new StringBuilder();
            bool headerset = false;
            string result = string.Empty;

            string seperator = "^";
            string doubleqout = "";

            try
            {
                if (dataTable != null)
                {
                    if (dataTable.Rows.Count > 0)
                    {
                        if (!headerset)
                        {
                            logger.Info("Parsing Header");
                            WriteitDown(SetHeader(dataTable, seperator, doubleqout), basedir, filename);
                            headerset = true;
                        }
                        logger.Info("Parsing result");
                        for (int j = 0; j < dataTable.Rows.Count; j++)
                        {
                            sb.Append(doubleqout);
                            for (int k = 0; k < dataTable.Columns.Count; k++)
                            {
                                if (k != dataTable.Columns.Count - 1)
                                {
                                    sb.Append(string.Concat(Helper.CheckComma(dataTable.Rows[j][k].ToString()), seperator));
                                }
                                else if (k == dataTable.Columns.Count - 1)
                                {
                                    sb.Append(string.Concat(Helper.CheckComma(dataTable.Rows[j][k].ToString()), "\n"));
                                }
                            }
                        }
                    }
                    WriteitDown(sb.ToString(), basedir, filename);
                }
            }
            catch (Exception ex)
            {
                logger.Info(ex);
            }
        }
        private string SetHeader(DataTable dt, string seperator, string doubleqout)
        {
            StringBuilder stringBuilder = new StringBuilder();
            string empty = string.Empty;
            try
            {
                stringBuilder.Append(doubleqout);
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    stringBuilder.Append(string.Concat(dt.Columns[i].ColumnName, seperator));
                }
                empty = stringBuilder.ToString().Remove(stringBuilder.Length - 2);
                empty = string.Concat(empty, "\n");
            }
            catch (Exception exception)
            {
                logger.Info(exception);
            }
            return empty;
        }
        private void WriteitDown(string data, string basedir, string filename)
        {
            logger.Info(data);
            try
            {
                using (StreamWriter streamWriter = File.AppendText(string.Concat(basedir, filename)))
                {
                    streamWriter.Write(data);
                }
            }
            catch (Exception exception)
            {
                logger.Info(exception);
            }
        }
    }
}
