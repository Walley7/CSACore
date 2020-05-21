using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace CSACore.Utility {

    public static class UDateTime {
        //================================================================================
        //--------------------------------------------------------------------------------
        public static string Datestamp(DateTime dateTime, string separator = "-") {
            return dateTime.Year.ToString("D4") + separator + dateTime.Month.ToString("D2") + separator + dateTime.Day.ToString("D2");
        }

        //--------------------------------------------------------------------------------
        public static string Datestamp(string separator = "-") { return Datestamp(DateTime.Now, separator); }

        //--------------------------------------------------------------------------------
        public static string Timestamp(DateTime dateTime, string separator = "-") {
            return dateTime.Year.ToString("D4") + separator + dateTime.Month.ToString("D2") + separator + dateTime.Day.ToString("D2") + separator +
                   dateTime.Hour.ToString("D2") + separator + dateTime.Minute.ToString("D2") + separator + dateTime.Second.ToString("D2");
        }

        //--------------------------------------------------------------------------------
        public static string Timestamp(string separator = "-") { return Timestamp(DateTime.Now, separator); }
        public static string UTCTimestamp(string separator = "-") { return Timestamp(DateTime.UtcNow, separator); }
    }

}
