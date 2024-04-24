using System;

namespace core.Utility
{
    public static class EnvironmentUtility
    {
        public static string FromEnvOrDefault(string name, string def = "")
        {
            string v = Environment.GetEnvironmentVariable(name);
            if (string.IsNullOrEmpty(v))
                return def;

            return v;
        }
    
        public static int FromEnvOrDefaultAsInt(string name, string def = "0")
        {
            return Convert.ToInt32(FromEnvOrDefault(name, def));
        }
    }
}