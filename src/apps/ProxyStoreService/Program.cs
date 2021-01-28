using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ProxyStoreService
{
    class ServiceOptions
    {
        [Option('m', "mutex", HelpText = "Instance Mutex Name")]
        public string InstanceMutexName { get; set; }
        [Option('s', "stopEvent", HelpText = "Named Event used to Stop service")]
        public string StopEvent { get; set; }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            ServiceOptions opts = null;

            Parser.Default.ParseArguments<ServiceOptions>(args)
                .WithParsed<ServiceOptions>(o =>
                {
                    if (string.IsNullOrEmpty(o.InstanceMutexName))
                    {
                        o.InstanceMutexName = AppDomain.CurrentDomain.FriendlyName;
                    }
                    if (string.IsNullOrEmpty(o.StopEvent))
                    {
                        o.StopEvent = $"{AppDomain.CurrentDomain.FriendlyName}Stop";
                    }

                    opts = o;
                });

            if (opts == null)
            {
                return;
            }
            var mutex = new Mutex(true, opts.InstanceMutexName, out bool created);
            if (!created)
            {
                // already running
                return;
            }

            WaitHandle evt;

            // named events not supported on *nix platforms
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                evt = new EventWaitHandle(false, EventResetMode.AutoReset, opts.StopEvent);
            }
            else
            {
                // TODO: find a way to shut down on unix
                evt = new ManualResetEvent(false);
            }

            var t1 = Task.Run(() =>
            {
                // wait infinitely for the event
                evt.WaitOne();
            });

            var t2 = CreateHostBuilder(args)
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.AddJsonFile("appsettings.store.json");
                })
                .Build()
                .RunAsync();

            Task.WaitAny(t1, t2);
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
