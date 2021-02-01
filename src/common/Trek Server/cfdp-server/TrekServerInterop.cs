using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace cfdp_server
{
    public class TrekServerInterop
    {
        const string TREK_LIB_NAME = "trek_lib";

        static TrekServerInterop()
        {
            /*
            if (Environment.OSVersion.Platform == PlatformID.Unix)
            {
                var trekRoot = Environment.GetEnvironmentVariable("TREKROOT");
                if (string.IsNullOrWhiteSpace(trekRoot))
                {
                    trekRoot = "/trek-5.1.1";
                }
                // manged code won't load dependent libraries, so let's do that ourselves
                var lib = Path.Combine(trekRoot, "lib/libtrek_toolkit_ds_api.so");
                Console.WriteLine($"Loading {lib}...");
                if (!NativeLibrary.TryLoad(lib, out IntPtr handle))
                {
                    Console.WriteLine("LOAD FAILED");
                }
                else
                {
                    NativeLibrary.TryGetExport(handle, "_ZTIN4trek13LibraryDeviceE", out IntPtr fn);
                    Console.WriteLine($"fn at {fn}");
                }
            }
            */

            NativeLibrary.SetDllImportResolver(typeof(TrekServerInterop).Assembly, LibraryResolve);
        }

        private static IntPtr LibraryResolve(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
        {
            var hLib = IntPtr.Zero;
            if (libraryName == TREK_LIB_NAME)
            {
                string libPath;

                if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                {
                    libPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "trek-svc.dll");
                }
                else
                {
                    libPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "libtrek-svc-pi.so");
                }

                Console.WriteLine($"Resolving TReK Library from {libPath}");
                if (NativeLibrary.TryLoad(libPath, out hLib))
                {
                    Console.WriteLine($"Successfully loaded");
                }
                else
                {
                    Console.WriteLine($"Failed to load");
                }
            }

            return hLib;
        }

        private const int RTLD_LAZY = 0x00001; //Only resolve symbols as needed
        private const int RTLD_GLOBAL = 0x00100; //Make symbols available to libraries loaded later        [DllImport("dl")]

        [DllImport("dl")]
        private static extern IntPtr dlopen(string file, int mode);

        [DllImport(TREK_LIB_NAME)]
        public static extern IntPtr CreateServer(int port, [MarshalAs(UnmanagedType.LPStr)] string configFile);
        [DllImport(TREK_LIB_NAME)]
        public static extern int StartServer(IntPtr pServer);
        [DllImport(TREK_LIB_NAME)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsServerRunning(IntPtr pServer);
        [DllImport(TREK_LIB_NAME)]
        public static extern int StopServer(IntPtr pServer);
    }
}
