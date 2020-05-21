using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;



namespace CSACore.Utility {

    public static class UNetwork {
        //================================================================================
        //--------------------------------------------------------------------------------
        public static string WebIP { get { return new WebClient().DownloadString("http://icanhazip.com"); } }
    }

}
