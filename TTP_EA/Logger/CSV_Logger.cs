using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TTP_EA.Logger
{
    public class CSV_Logger<TRecord> : IDisposable where TRecord : IRecord
    {
        public string FilePath { get; set; }
        private FileStream CsvStream { get; set; }
        private CsvWriter CsvWriter { get; set; }

        public CSV_Logger(string filePath)
        {
            FilePath = filePath;

            CsvStream = new FileStream(this.FilePath, FileMode.Create, FileAccess.Write, FileShare.Read);
            CsvWriter = new CsvWriter(new StreamWriter(CsvStream)
                , new CsvConfiguration(CultureInfo.CurrentCulture)
                {
                    Delimiter = ";",
                }
            );
            if (!CsvStream.CanWrite)
            {
                throw new IOException($"CSVLogger: Cannot create file for logging at {FilePath}");
            }
            CsvWriter.WriteHeader(typeof(TRecord));
            CsvWriter.NextRecord();
        }

        public void Log(TRecord record)
        {
            CsvWriter.WriteRecord(record);
            CsvWriter.NextRecord();
            CsvWriter.Flush();
        }

        public void Dispose()
        {
            CsvWriter.Dispose();
            CsvStream.Dispose();
            CsvStream.Close();
        }

    }
}
