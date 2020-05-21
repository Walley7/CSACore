using CSACore.Licencing;
using CSACore.Logging;
using CSACore.Profiling;
using CSACore.Server;
using CSACore.Utility;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace CSACore.Core {

    public static class CSA {
        //================================================================================
        private static CommandLineArguments     sCommandLineArguments = null;

        private static JObject                  sConfigurationJson;

        private static Logger                   sLogger = null;

        private static Profiler                 sProfiler = new Profiler();

        private static CSAServer                sServer = new CSAServer();

        private static Licencer                 sLicencer = new Licencer();


        //================================================================================
        //--------------------------------------------------------------------------------
        public static void Initialise(string[] commandLineArguments, string configurationPath = "") {
            // Command line arguments
            if (commandLineArguments != null)
                sCommandLineArguments = new CommandLineArguments(commandLineArguments);

            // Configuration
            if (!string.IsNullOrWhiteSpace(configurationPath))
                InitialiseConfiguration(configurationPath);
        }

        //--------------------------------------------------------------------------------
        public static void Initialise(string configurationPath = "") { Initialise(null, configurationPath); }
        public static void Initialise() { Initialise(null, null); }

        //--------------------------------------------------------------------------------
        public static void Shutdown() { }


        // COMMAND LINE ARGUMENTS ================================================================================
        //--------------------------------------------------------------------------------
        public static CommandLineArguments Arguments { get { return sCommandLineArguments; } }


        // CONFIGURATION ================================================================================
        //--------------------------------------------------------------------------------
        private static void InitialiseConfiguration(string configurationPath) {
            // Read
            StreamReader stream = new StreamReader(configurationPath);
            string json = stream.ReadToEnd();
            stream.Close();

            // Json
            sConfigurationJson = JObject.Parse(json);
        }

        //--------------------------------------------------------------------------------
        public static string Setting(string settingName, bool exceptionOnBlank = true, bool exceptionOnNull = true) {
            string settingValue = (string)sConfigurationJson.SelectToken(settingName);
            if ((exceptionOnBlank && (settingValue != null) && string.IsNullOrWhiteSpace(settingValue)) || (exceptionOnNull && (settingValue == null)))
                throw new InvalidDataException("Missing setting - '" + settingName + "'");
            return settingValue;
        }


        // LOGGING ================================================================================
        //--------------------------------------------------------------------------------
        public static void OpenLog(string logPath, bool logStart = true, bool addTimestamp = true) {
            // Close
            CloseLog();

            // Timestamp
            if (addTimestamp)
                logPath = logPath.Insert(Math.Max(logPath.LastIndexOf('.'), 0), "-" + UDateTime.Datestamp());

            // Open
            sLogger = new Logger(logPath);
            if (logStart)
                sLogger.LogStart();
        }
        
        //--------------------------------------------------------------------------------
        public static void CloseLog(bool logEnd = true) {
            if (sLogger != null) {
                if (logEnd)
                    sLogger.LogEnd();
                sLogger.Dispose();
                sLogger = null;
            }
        }

        //--------------------------------------------------------------------------------
        public static Logger Logger { get { return sLogger; } }


        // PROFILING ================================================================================
        //--------------------------------------------------------------------------------
        public static Profiler Profiler { get { return sProfiler; } }


        // LICENCING ================================================================================
        //--------------------------------------------------------------------------------
        public static CSAServer Server { get { return sServer; } }


        // LICENCING ================================================================================
        //--------------------------------------------------------------------------------
        public static Licencer Licencer { get { return sLicencer; } }
    }

}
