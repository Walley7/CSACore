using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace CSACore.Utility {

    public static class UConvert {
        // STRINGS ================================================================================
        //--------------------------------------------------------------------------------
        public static T FromString<T>(string value, T failValue) {
            try {
                TypeConverter converter = TypeDescriptor.GetConverter(typeof(T));
                if (converter != null)
                    return (T)converter.ConvertFromString(value);
                else
                    return failValue;
            }
            catch (NotSupportedException) {
                return failValue;
            }
        }

        //--------------------------------------------------------------------------------
        public static T FromString<T>(string value) { return FromString<T>(value, default(T)); }

        //--------------------------------------------------------------------------------
        public static bool TryFromString<T>(string value, out T result) {
            // Default
            result = default(T);

            // Convert
            try {
                TypeConverter converter = TypeDescriptor.GetConverter(typeof(T));
                if (converter != null) {
                    result = (T)converter.ConvertFromString(value);
                    return true;
                }
                else
                    return false;
            }
            catch (NotSupportedException) {
                return false;
            }
        }
    }

}
