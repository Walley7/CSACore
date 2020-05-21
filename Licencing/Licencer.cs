using CSACore.Core;
using Standard.Licensing;
using Standard.Licensing.Security.Cryptography;
using Standard.Licensing.Validation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace CSACore.Licencing {

    public class Licencer {
        //================================================================================
        private string                          mPublicKey;
        private License                         mLicence = null;

        private Task<bool>                      mServerValidationTask = null;
        private bool                            mPassedServerValidation = true;


        // LICENCE ================================================================================
        //--------------------------------------------------------------------------------
        public void LoadLicence(string publicKey, string licence, bool serverValidation = true) {
            // Clear
            mLicence = null;

            // Checks
            if (string.IsNullOrWhiteSpace(licence))
                return;

            // Parts
            string[] licenceParts = licence.Split(':');
            if (licenceParts.Length != 5)
                return;

            // Public key
            mPublicKey = publicKey;

            // ID
            string id = licenceParts[0];

            // Type
            string type = licenceParts[1].Equals("s") ? "Standard" : "Trial";

            // Maximum users
            string quantity = licenceParts[2];

            // Expiry
            DateTime expiry = DateTime.MaxValue;
            //string expiryDayOfWeek = "";
            if (!string.IsNullOrWhiteSpace(licenceParts[3]))
                DateTime.TryParse(licenceParts[3], out expiry);

            // Signature
            string signature = licenceParts[4];

            // Convert to XML
            string xmlLicence = $"<License>" +
                                $"\n  <Id>{id}</Id>" +
                                $"\n  <Type>{type}</Type>" +
                                (!string.IsNullOrWhiteSpace(quantity) ? $"\n  <Quantity>{quantity}</Quantity>" : "") +
                                (expiry.Year < 9999 ? $"\n  <Expiration>{expiry.ToString("ddd, dd MMM yyyy HH:mm:ss")} GMT</Expiration>" : "") +
                                $"\n  <Signature>{signature}</Signature>" +
                                $"\n</License>";
           
            // Load
            mLicence = License.Load(xmlLicence);

            // Server validation
            BeginServerValidation(LicenceID);
        }

        //--------------------------------------------------------------------------------
        public void LoadLicenceFromFile(string publicKey, string licencePath, bool serverValidation = true) {
            // Checks
            if (!File.Exists(licencePath)) {
                BeginServerValidation("");
                return;
            }

            // Licence
            string licence = File.ReadAllText(licencePath);
            CSA.Licencer.LoadLicence(publicKey, licence, serverValidation);
        }
        
        //--------------------------------------------------------------------------------
        public string LicenceID { get { return mLicence.Id.ToString(); } }
        public LicenceType LicenceType { get { return mLicence.Type == LicenseType.Standard ? LicenceType.STANDARD : LicenceType.TRIAL; } }

        //--------------------------------------------------------------------------------
        public string LicenceTypeString {
            get {
                switch (LicenceType) {
                    case LicenceType.STANDARD:  return "Standard";
                    case LicenceType.TRIAL:     return "Trial";
                    default:                    return "Standard";
                }
            }
        }

        //--------------------------------------------------------------------------------
        public DateTime LicenceExpiration { get { return mLicence.Expiration; } }
        public string LicenceString { get { return LicenceCompactString(mLicence); } }

        //--------------------------------------------------------------------------------
        // Example of validating entire licence in one statement:
        //     validationResults = sLicence.Validate()
        //         .ExpirationDate().When(lic => lic.Type == LicenseType.Trial) // .When is a condition applied to the previous validation = in this case only validate expiration date when type is trial
        //         .And().Signature(PUBLIC_LICENCE_KEY)
        //         .AssertValidLicense().ToList();
        //     return (validationResults.Count() == 0);
        public LicenceActivationStatus LicenceStatus {
            get {
                // No licence
                if (mLicence == null)
                    return LicenceActivationStatus.NONE;

                // Invalid
                if (mLicence.Validate().Signature(mPublicKey).AssertValidLicense().Count() > 0 || !mPassedServerValidation)
                    return LicenceActivationStatus.INVALID;

                // Expired
                if (mLicence.Validate().ExpirationDate().AssertValidLicense().Count() > 0)
                    return LicenceActivationStatus.EXPIRED;

                // Active
                return LicenceActivationStatus.ACTIVE;
            }
        }

        //--------------------------------------------------------------------------------
        public string LicenceStatusString {
            get {
                switch (LicenceStatus) {
                    case LicenceActivationStatus.ACTIVE:    return "Active";
                    case LicenceActivationStatus.INVALID:   return "Invalid";
                    case LicenceActivationStatus.EXPIRED:   return "Expired";
                    default:                                return "Activation Required";
                }
            }
        }

        //--------------------------------------------------------------------------------
        public bool LicenceActive { get { return LicenceStatus == LicenceActivationStatus.ACTIVE; } }
        public bool HasLicence { get { return mLicence != null; } }
        
        //--------------------------------------------------------------------------------
        // OLD LICENCE CODE:
        //public static string LicenceDescription {
        //    get {
        //        // Checks
        //        if (sLicence == null)
        //            return "ACTIVATION REQUIRED";
        //
        //        // Variables
        //        string description = "";
        //
        //        // Validity
        //        bool valid = Settings.LicenceIsValid(out List<IValidationFailure> validationResults);
        //        if (!valid) {
        //            foreach (IValidationFailure v in validationResults) {
        //                description += (!string.IsNullOrEmpty(description) ? "\r\n\r\n" : "") + v.Message.ToUpper() + "\r\n" + v.HowToResolve;
        //            }
        //        }
        //        
        //        // Description
        //        description += (!string.IsNullOrEmpty(description) ? "\r\n\r\n" : "") + "Licenced To: " + sLicence.Customer.Name +
        //                       "\r\nType: " + sLicence.Type +
        //                       (sLicence.Expiration.Year < 3000 ? "\r\nUntil: " + sLicence.Expiration.ToShortDateString() : "");
        //        return description;
        //    }
        //}


        // APPLICATION KEYS ================================================================================
        //--------------------------------------------------------------------------------
        public void GenerateApplicationKeys(string passPhrase, out string privateKey, out string publicKey) {
            // Keys
            KeyGenerator keyGenerator = KeyGenerator.Create();
            KeyPair keyPair = keyGenerator.GenerateKeyPair();
            privateKey = keyPair.ToEncryptedPrivateKeyString(passPhrase);
            publicKey = keyPair.ToPublicKeyString();
        }


        // LICENCE GENERATION ================================================================================
        //--------------------------------------------------------------------------------
        public string GenerateLicence(string privateKey, string passPhrase, LicenceType type, int maximumUsers, DateTime? expiry) {
            // Licence type
            LicenseType licenceType = (type == LicenceType.STANDARD ? LicenseType.Standard : LicenseType.Trial);

            // Licence builder
            ILicenseBuilder licenceBuilder = License.New()
                .WithUniqueIdentifier(Guid.NewGuid())
                .As(licenceType);

            // Maximum users
            if (maximumUsers > 0)
                licenceBuilder.WithMaximumUtilization(maximumUsers);

            // Expiry
            if (expiry != null)
                licenceBuilder.ExpiresAt(((DateTime)expiry).ToUniversalTime().Date); // Remove the time component, we just want midnight UTC/GMT

            // Licence
            License licence = licenceBuilder.CreateAndSignWithPrivateKey(privateKey, passPhrase);
            return LicenceCompactString(licence);
        }

        //--------------------------------------------------------------------------------
        public string GenerateLicence(string privateKey, string passPhrase, LicenceType type) { return GenerateLicence(privateKey, passPhrase, type, 0, null); }
        public string GenerateLicence(string privateKey, string passPhrase, LicenceType type, int maximumUsers) { return GenerateLicence(privateKey, passPhrase, type, 0, null); }
        public string GenerateLicence(string privateKey, string passPhrase, LicenceType type, DateTime expiry) { return GenerateLicence(privateKey, passPhrase, type, 0, null); }

        //--------------------------------------------------------------------------------
        private string LicenceCompactString(License licence) {
            string id = licence.Id.ToString();
            string type = (licence.Type == LicenseType.Standard ? "s" : "t");
            string quantity = licence.Quantity > 0 ? licence.Quantity.ToString() : "";
            string expiry = licence.Expiration.Year < 9999 ? licence.Expiration.ToShortDateString() : "";
            string signature = licence.Signature;
            return $"{id}:{type}:{quantity}:{expiry}:{signature}";
        }


        // SERVER VALIDATION ================================================================================
        //--------------------------------------------------------------------------------
        private void BeginServerValidation(string licenceID) {
            EndServerValidation();
            mServerValidationTask = Task.Run(() => { return CSA.Server.ValidateLicenceAsync(licenceID); }); // Task.Run is required to run it on a new thread
            mPassedServerValidation = true;
        }

        //--------------------------------------------------------------------------------
        private void EndServerValidation() {
            if (mServerValidationTask != null) {
                mServerValidationTask.Wait();
                mServerValidationTask.Dispose();
                mServerValidationTask = null;
            }
        }

        //--------------------------------------------------------------------------------
        public bool PollServerValidation() {
            // Checks
            if (mServerValidationTask == null || !mServerValidationTask.IsCompleted)
                return false;

            // Poll
            mPassedServerValidation = mServerValidationTask.Result;
            EndServerValidation();
            return true;
        }

        //--------------------------------------------------------------------------------
        public bool ValidatingWithServer { get { return (mServerValidationTask != null); } }
    }

}
