using System;
using System.IO;
using System.Threading.Tasks;

namespace Hypergiant.HIVE
{
    public class JsonTextFilePublisher : ITelemetryPublisher
    {
        private FileInfo m_fi;

        public JsonTextFilePublisher(string filepath)
        {
            if (string.IsNullOrEmpty(filepath))
            {
                throw new ArgumentException("Invalid path");
            }

            Initialize(new FileInfo(filepath));
        }

        public JsonTextFilePublisher(FileInfo fi)
        {
            if (fi == null)
            {
                throw new ArgumentNullException(nameof(fi));
            }

            Initialize(fi);
        }

        private void Initialize(FileInfo fi)
        {
            if (fi.Exists)
            {
                throw new ArgumentException("Target file already exists");
            }

            Console.WriteLine($">> RECORDING DATA TO {fi.FullName}");

            if (!fi.Directory.Exists)
            {
                fi.Directory.Create();
            }

            m_fi = fi;
        }

        public void QueueTelemetry(string path, TelemetryDataValue value)
        {
            using (var writer = m_fi.AppendText())
            {
                writer.WriteLine(value.ToJson());
            }
        }
    }
}
