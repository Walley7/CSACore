using CSACore.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace CSACore.Core {

    public class CommandLineArguments {
        //================================================================================
        public const string                         KEY_PREFIX = "-";
        public const int                            USAGE_DESCRIPTION_OFFSET = 26;
        public const int                            USAGE_WIDTH = 119;


        //================================================================================
        private Dictionary<string, List<string>>    mArguments = new Dictionary<string, List<string>>();


        //================================================================================
        //--------------------------------------------------------------------------------
        public CommandLineArguments(string[] args) {
            if (args != null && args.Length > 0)
                Parse(args);
        }


        // PARSING ================================================================================
        //--------------------------------------------------------------------------------
        protected virtual void Parse(string[] args) {
            // Variables
            string key = "";

            // Parse
            foreach (string a in args) {
                // Checks
                if (a == null)
                    continue;

                // Argument
                if (a.StartsWith(KEY_PREFIX)) {
                    key = a.ToLower();
                    if (!mArguments.ContainsKey(key))
                        mArguments.Add(key, new List<string>());
                }
                else
                    mArguments[key].Add(a);
            }
        }


        // ARGUMENTS ================================================================================
        //--------------------------------------------------------------------------------
        public string[] Arguments(string key) { return (mArguments.ContainsKey(key.ToLower()) ? mArguments[key.ToLower()].ToArray() : null); }
        public string[] this[string key] { get { return Arguments(key); } }
        public bool HasKey(string key) { return Arguments(key) != null; }


        // HELP ================================================================================
        //--------------------------------------------------------------------------------
        public string UsageString(string usage, params string[][] arguments) {
            // Usage
            StringBuilder builder = new StringBuilder();
            builder.AppendLine($"Usage: {AppDomain.CurrentDomain.FriendlyName} {usage}");

            // Arguments
            foreach (string[] a in arguments) {
                if (a.Length >= 2) {
                    // Argument
                    builder.Append("  ");
                    builder.Append(a[0]);
                    if (a[0].Length < USAGE_DESCRIPTION_OFFSET)
                        builder.Append(' ', USAGE_DESCRIPTION_OFFSET - a[0].Length - 2);
                    else {
                        builder.AppendLine();
                        builder.Append(' ', USAGE_DESCRIPTION_OFFSET);
                    }

                    // Description
                    string[] lines = UString.WordWrap(a[1], USAGE_WIDTH - USAGE_DESCRIPTION_OFFSET);
                    if (lines.Length > 0)
                        builder.AppendLine(lines[0]);
                    for (int i = 1; i < lines.Length; ++i) {
                        builder.Append(' ', USAGE_DESCRIPTION_OFFSET);
                        builder.AppendLine(lines[i]);
                    }
                }
            }

            // String
            return builder.ToString();
        }
    }

}
