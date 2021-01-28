using System;
using System.Collections.Generic;
namespace Hypergiant.HIVE.Support
{
    public class Env
    {
        public static readonly Dictionary<string, string> defaults = new Dictionary<string, string>
            { 
                { "DYNAMO_ACCESS_ID", "ACCESS_ID" },
                { "DYNAMO_ACCESS_SECRET", "ACCESS_SECRET" },
                { "S3_ACCESS_ID", "ACCESS_ID" },
                { "S3_ACCESS_SECRET", "ACCESS_SECRET" },
            };

        public static string Get(string variableName, string fallback = "")
        {
            if (Environment.GetEnvironmentVariable(variableName) != null)
            {
                return Environment.GetEnvironmentVariable(variableName);
            }
            if(defaults.ContainsKey(variableName)) {
                return defaults[variableName];
            }
            return fallback;
        }
    }

}
