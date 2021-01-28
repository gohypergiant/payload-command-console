using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: InternalsVisibleTo("TrekCfdpToolkit.Test")]

namespace Hypergiant.HIVE
{
    internal class TrekCfdpToolkitInterop
    {
        private const string CFDP_DLL_NAME = "trek_toolkit_cfdp_api";
        private const string DS_DLL_NAME = "trek_toolkit_ds_api";

        /*
        // ------------------ DEV NOTE ---------------------
        // Keep this code
        // This was removed because it's not yet working.  The Raspbian LD loader is not finding dependent functions that the CFDP library needs
        // even though the functions absolutely do exist and can be found and directly loaded by name from the ds_api.so library.  I don't understand why
        // but the code below is an attempt to figure it out.  Leaving this so when I return to try to solve the problem I don't have to start from zero again.
        // turning on the loaded helps show what is happening, but not why:
        //  export LD_DEBUG=all

        private static string TrekRoot { get; set; }

        static TrekCfdpToolkitInterop()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                TrekRoot = Environment.GetEnvironmentVariable("TREKROOT");
                if (string.IsNullOrWhiteSpace(TrekRoot))
                {
                    TrekRoot = "/opt/trek-5.1.1";
                }

                Console.WriteLine($"TREKROOT={TrekRoot}");

                // for some reason (unknown to me) managed code isn't loading functions from a dependent library
                // When it attempts to load the CFDP library it fails to find some functions which are in a separate lib.  If we manually get them it works.
                // This seems silly fragile, but it's all I can figure thus far.
                
                var lib = Path.Combine(TrekRoot, "lib/libtrek_toolkit_ds_api.so");
                Console.WriteLine($"Loading {lib}...");
                if (!NativeLibrary.TryLoad(lib, out IntPtr handle))
                {
                    Console.WriteLine("LOAD FAILED");
                }
                else
                {
                    NativeLibrary.TryGetExport(handle, "_ZTIN4trek13LibraryDeviceE", out IntPtr fn);
                    Console.WriteLine($"fn at {fn}");
                    NativeLibrary.TryGetExport(handle, "_ZTIN4trek9BpCollectE", out fn);
                    Console.WriteLine($"fn at {fn}");
                }

                NativeLibrary.SetDllImportResolver(typeof(TrekServerInterop).Assembly, LibraryResolve);
            }
        }

        private static IntPtr LibraryResolve(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
        {
            var hLib = IntPtr.Zero;
            if (libraryName == CFDP_DLL_NAME)
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    var libPath = Path.Combine(TrekRoot, "lib/libtrek_toolkit_cfdp_api.so");

                    Console.WriteLine($"Loading TReK CFDP library '{libPath}'");

                    try
                    {
                        hLib = NativeLibrary.Load(libPath);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to load: {ex.Message}");
                    }
                }
            }

            return hLib;
        }
        */

        // int EXPORT_THIS_TOOLKIT_CFDP_C_FUNCTION InitToolkitCfdp(const char* config_pathname);
        [DllImport(CFDP_DLL_NAME)]
        public static extern TrekErrorCode InitToolkitCfdp(string config_pathname);

        // int EXPORT_THIS_TOOLKIT_CFDP_C_FUNCTION PutComponentCFDP(const char *source_pathname, const char* destination_pathname, long long destination_eid, cfdp_class_of_service_type cfdp_class_of_service); /* set to CFDP_CLASS1 or CFDP_CLASS2 (CFDP_CLASS2 requires two way 
        [DllImport(CFDP_DLL_NAME)]
        public static extern TrekErrorCode PutComponentCFDP(string source_pathname, string destination_pathname, long destination_eid, CfdpClass cfdp_class_of_service);

        // int EXPORT_THIS_TOOLKIT_CFDP_C_FUNCTION AddPutComponentRequest(const char *source_pathname, const char* destination_pathname, long long destination_eid, cfdp_class_of_service_type cfdp_class_of_service);  /* set to CFDP_CLASS1 or CFDP_CLASS2 (CFDP_CLASS2 requires two way 
        [DllImport(CFDP_DLL_NAME)]
        public static extern TrekErrorCode AddPutComponentRequest(string source_pathname, string destination_pathname, long destination_eid, CfdpClass cfdp_class_of_service);

        // int EXPORT_THIS_TOOLKIT_CFDP_C_FUNCTION SendAllPutRequests();
        [DllImport(CFDP_DLL_NAME)]
        public static extern TrekErrorCode SendAllPutRequests();

        // void EXPORT_THIS_TOOLKIT_CFDP_C_FUNCTION RemoveAllPutRequests();
        [DllImport(CFDP_DLL_NAME)]
        public static extern TrekErrorCode RemoveAllPutRequests();

        // int EXPORT_THIS_TOOLKIT_CFDP_C_FUNCTION GetComponentCFDP(const char *source_pathname, const char* destination_pathname, long long source_eid, cfdp_class_of_service_type cfdp_class_of_service); /* set to CFDP_CLASS1 or CFDP_CLASS2 (CFDP_CLASS2 requires two way 
        [DllImport(CFDP_DLL_NAME)]
        public static extern TrekErrorCode GetComponentCFDP(string source_pathname, string destination_pathname, long destination_eid, CfdpClass cfdp_class_of_service);

        // int EXPORT_THIS_TOOLKIT_CFDP_C_FUNCTION AddGetComponentRequest(const char *source_pathname, const char* destination_pathname, long long destination_eid, cfdp_class_of_service_type cfdp_class_of_service);  /* set to CFDP_CLASS1 or CFDP_CLASS2 (CFDP_CLASS2 requires two way 
        [DllImport(CFDP_DLL_NAME)]
        public static extern TrekErrorCode AddGetComponentRequest(string source_pathname, string destination_pathname, long destination_eid, CfdpClass cfdp_class_of_service);

        // int EXPORT_THIS_TOOLKIT_CFDP_C_FUNCTION SendAllGetRequests();
        [DllImport(CFDP_DLL_NAME)]
        public static extern TrekErrorCode SendAllGetRequests();

        // void EXPORT_THIS_TOOLKIT_CFDP_C_FUNCTION RemoveAllGetRequests();
        [DllImport(CFDP_DLL_NAME)]
        public static extern TrekErrorCode RemoveAllGetRequests();

        // int EXPORT_THIS_TOOLKIT_DS_C_FUNCTION DSCleanUp();
        [DllImport(DS_DLL_NAME)]
        public static extern TrekErrorCode DSCleanUp();
    }
}
