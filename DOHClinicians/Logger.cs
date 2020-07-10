using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.IO;


namespace DOHClinicians
{
    class logger
    {
        public static string baseDir = ConfigurationManager.AppSettings.Get("baseDir");

        public static void Info(string data)
        {
            Console.WriteLine(data);
            using (StreamWriter streamWriter = File.AppendText(string.Concat(baseDir, "\\Infolog.csv")))
            {
                streamWriter.Write(string.Concat(new object[] { DateTime.Now, " : ", data, "\n" }));
            }
        }

        public static void Info(Exception data)
        {
            Console.WriteLine(data);
            using (StreamWriter streamWriter = File.AppendText(string.Concat(baseDir, "\\Infolog.csv")))
            {
                streamWriter.Write(string.Concat(new object[] { DateTime.Now, " : ", data, "\n" }));
            }
        }
    }
}
