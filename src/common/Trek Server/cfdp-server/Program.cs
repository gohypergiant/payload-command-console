using System;
using System.Diagnostics;
using System.IO;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;

namespace cfdp_server
{
    class Program
    {
        static void Main(string[] args)
        {
//            System.Runtime.InteropServices.dl
//            System.Runtime.InteropServices.NativeLibrary.
            Console.WriteLine("Hello CFDP!");
            var runTime = 60;

            var path = new FileInfo("cfdp-config.txt");

            var ptr = TrekServerInterop.CreateServer(5000, path.FullName);
            Console.WriteLine($"Running: {TrekServerInterop.IsServerRunning(ptr)}");
            Task.Run(() => TrekServerInterop.StartServer(ptr));
            Console.WriteLine($"Running for {runTime} seconds");
            for (int i = 0; i < runTime; i++)
            {
                Thread.Sleep(1000);
            }
            Console.WriteLine($"Stopping");
            TrekServerInterop.StopServer(ptr);
            Thread.Sleep(1000);
            Console.WriteLine($"Running: {TrekServerInterop.IsServerRunning(ptr)}");
        }
    }
}
