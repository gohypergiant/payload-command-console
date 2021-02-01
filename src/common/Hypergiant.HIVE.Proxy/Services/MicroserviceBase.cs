using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;

namespace Hypergiant.HIVE
{
    public abstract class MicroserviceBase
    {
        protected HttpClient HttpClient { get; }
        protected JsonSerializerOptions SerializerOptions { get; }

        public bool UsingDocker { get; }
        private string ServiceExe { get; }
        private string DockcerName { get; }
        private string InstanceMutexName { get; }
        private string StopEventName { get; }

        public MicroserviceBase(
            bool useDocker,
            string serviceExe,
            string dockerName,
            string instanceMutexName,
            string stopEventName)
        {
            HttpClient = new HttpClient();

            SerializerOptions = new JsonSerializerOptions
            {
                IgnoreNullValues = true,
                PropertyNameCaseInsensitive = true
            };
            SerializerOptions.Converters.Add(new JsonStringEnumConverter());

            UsingDocker = useDocker;
            ServiceExe = serviceExe;
            InstanceMutexName = instanceMutexName;
            StopEventName = stopEventName;

        }

        public void Start()
        {
            if (UsingDocker)
            {
                throw new NotImplementedException();
            }
            else
            {
                var dir = Path.GetDirectoryName(ServiceExe);

                // is the service running?
                if (!IsRunning)
                {
                    var p = new ProcessStartInfo
                    {
                        WorkingDirectory = dir,
                        FileName = ServiceExe,
                        Arguments = $"--mutex {InstanceMutexName} --stopEvent {StopEventName}"
                    };

                    try
                    {
                        Process.Start(p);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex.Message);
                    }
                }
            }

        }

        private bool IsRunning
        {
            get
            {
                if (UsingDocker)
                {
                    throw new NotImplementedException();
                }
                else
                {
                    using (var mutex = new Mutex(false, InstanceMutexName, out bool created))
                    {
                        return !created;
                    }
                }
            }
        }

        public void Stop()
        {
            if (UsingDocker)
            {
                throw new NotImplementedException();
            }
            else
            {
                var evt = new EventWaitHandle(false, EventResetMode.AutoReset, StopEventName);
                evt.Set();
            }
        }
    }
}
