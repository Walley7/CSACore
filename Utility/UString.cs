using F23.StringSimilarity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace CSACore.Utility {

    public static class UString {
        //================================================================================
        private static JaroWinkler              sSimilarityJaroWinkler = new JaroWinkler();


        // OPERATIONS ================================================================================
        //--------------------------------------------------------------------------------
        public static string Join(string separator, params string[] strings) {
            // Join
            StringBuilder builder = new StringBuilder();
            foreach (string s in strings) {
                if (!string.IsNullOrWhiteSpace(s)) {
                    if (builder.Length > 0)
                        builder.Append(separator);
                    builder.Append(s);
                }
            }
            
            // Return
            return builder.ToString();
        }


        // FORMATTING ================================================================================
        //--------------------------------------------------------------------------------
        public static string[] WordWrap(string text, int width) {
            // Variables
            List<string> lines = new List<string>();
            char[] whitespace = new[] { ' ', '\r', '\n', '\t' };
            char[] delimiters = new[] { ' ', ',', '.', '?', '!', ':', ';', '-', '\n', '\r', '\t' };
            int index;
            int lastWrapIndex = 0;

            // Wrap
            do {
                if (lastWrapIndex + width > text.Length)
                    index = text.Length;
                else
                    index = text.LastIndexOfAny(delimiters, Math.Min(text.Length - 1, lastWrapIndex + width)) + 1;
                lines.Add(text.Substring(lastWrapIndex, index - lastWrapIndex).Trim(whitespace));
                lastWrapIndex = index;
            }
            while (index < text.Length);

            // Return
            return lines.ToArray();
        }

        
        // DISTANCE ================================================================================
        //--------------------------------------------------------------------------------
        public static double DistanceJaroWrinkler(string string1, string string2) { return sSimilarityJaroWinkler.Distance(string1, string2); }


        // NUMBERS ================================================================================
        //--------------------------------------------------------------------------------
        public static string SanitisedNumber(string number) {
            // Initialise
            StringBuilder builder = new StringBuilder();
            number = number.Trim();
            bool hasSign = (number.First() == '-');
            bool hasDecimalPoint = false;

            // Sanitise
            foreach (char c in number) {
                if (!hasDecimalPoint && c == '.') {
                    if (builder.Length == 0)
                        builder.Append('0');
                    builder.Append(c);
                    hasDecimalPoint = true;
                }
                else if (char.IsDigit(c))
                    builder.Append(c);
            }

            // Sign
            if (hasSign)
                builder.Insert(0, '-');

            // Return
            return builder.ToString();
        }
    }

}
