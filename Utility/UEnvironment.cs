using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;



namespace CSACore.Utility {

    public static class UEnvironment {
        //================================================================================
        public const int                        CSIDL_COMMON_STARTMENU = 0x16;
        public const int                        CSIDL_COMMON_PROGRAMS = 0x17;
        public const int                        CSIDL_COMMON_APPDATA = 0x23;
        public const int                        CSIDL_LOCAL_APPDATA = 0x1c;


        // ENVIRONMENT ================================================================================
        //--------------------------------------------------------------------------------
        public static string SHGetFolderPath(int folder, bool create = false) {
            StringBuilder path = new StringBuilder(260);
            SHGetSpecialFolderPath(IntPtr.Zero, path, folder, create);
            return path.ToString();
        }

        //--------------------------------------------------------------------------------
        [DllImport("shell32.dll")]
        static extern bool SHGetSpecialFolderPath(IntPtr hwndOwner, [Out] StringBuilder lpszPath, int nFolder, bool fCreate);
    }

}
