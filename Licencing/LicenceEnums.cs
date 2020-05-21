using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace CSACore.Licencing {
    
    //================================================================================
    public enum LicenceType {
        STANDARD,
        TRIAL
    }
    
    //--------------------------------------------------------------------------------
    public enum LicenceActivationStatus {
        NONE,
        ACTIVE,
        INVALID,
        EXPIRED
    }

}
