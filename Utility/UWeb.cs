using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace CSACore.Utility {

    public static class UWeb {
        //================================================================================
        //--------------------------------------------------------------------------------
        public static string BaseURL(string url, int maximumLength = -1) {
            // Base
            int argumentsIndex = url.IndexOf('?');
            if (argumentsIndex >= 0)
                url = url.Substring(0, argumentsIndex);

            // Maximum length
            if (maximumLength >= 0 && url.Length > maximumLength)
                url = url.Substring(0, maximumLength);

            // Return
            return url;
        }
    }

}
