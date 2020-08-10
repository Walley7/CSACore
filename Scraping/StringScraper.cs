using System;
using System.Collections.Generic;
using System.Text;



namespace CSACore.Scraping {

    public class StringScraper {
        //================================================================================
        private string                          mString;

        private int                             mIndex;


        //================================================================================
        //--------------------------------------------------------------------------------
        public StringScraper(string value) {
            mString = value;
        }


        // STRING ================================================================================
        //--------------------------------------------------------------------------------
        public string String { get => mString; }
        public string RemainingString { get => (!EOF ? mString.Substring(mIndex) : ""); }


        // STATE ================================================================================
        //--------------------------------------------------------------------------------
        public int Index {
            set {
                if (value < 0 || value > mString.Length)
                    throw new IndexOutOfRangeException();
            }
            get { return mIndex; }
        }

        //--------------------------------------------------------------------------------
        public bool EOF { get => (mIndex == mString.Length); }


        // READING ================================================================================
        //--------------------------------------------------------------------------------
        public string ReadTo(string pattern, bool readIfNotFound = false) {
            // Checks
            if (EOF)
                throw new InvalidOperationException();

            // Pattern
            int patternIndex = mString.IndexOf(pattern, mIndex);
            if (patternIndex == -1) {
                if (!readIfNotFound)
                    return "";
                else
                    patternIndex = mString.Length;
            }

            // Read
            string result = mString.Substring(mIndex, patternIndex - mIndex);
            mIndex = patternIndex;
            return result;
        }

        //--------------------------------------------------------------------------------
        public bool ReadToCheck(string pattern, bool readIfNotFound = false) { return ReadTo(pattern, readIfNotFound) != ""; }

        //--------------------------------------------------------------------------------
        public string ReadPast(string pattern, bool readIfNotFound = false) {
            // Checks
            if (EOF)
                throw new InvalidOperationException();

            // Pattern
            int patternIndex = mString.IndexOf(pattern, mIndex);
            if (patternIndex == -1) {
                if (!readIfNotFound)
                    return "";
                else
                    patternIndex = mString.Length;
            }
            else
                patternIndex += pattern.Length;

            // Read
            string result = mString.Substring(mIndex, patternIndex - mIndex);
            mIndex = patternIndex;
            return result;
        }

        //--------------------------------------------------------------------------------
        public bool ReadPastCheck(string pattern, bool readIfNotFound = false) { return ReadPast(pattern, readIfNotFound) != ""; }

        //--------------------------------------------------------------------------------
        public string ReadPastAndPast(string pattern1, string pattern2, bool readIfNotFound = false) {
            if (ReadPastCheck(pattern1, readIfNotFound))
                return ReadPast(pattern2, readIfNotFound);
            else
                return "";
        }

        //--------------------------------------------------------------------------------
        public string ReadPastAndTo(string pastPattern, string toPattern, bool readIfNotFound = false) {
            if (ReadPastCheck(pastPattern, readIfNotFound))
                return ReadTo(toPattern, readIfNotFound);
            else
                return "";
        }
        
        //--------------------------------------------------------------------------------
        public string ReadPastAndToOrTo(string pastPattern, string toPattern1, string toPattern2, bool readIfNotFound = false) {
            if (ReadPastCheck(pastPattern, readIfNotFound)) {
                string result = ReadTo(toPattern1, readIfNotFound);
                if (result == "")    
                    result = ReadTo(toPattern2, readIfNotFound);
                return result;
            }
            else
                return "";
        }

        //--------------------------------------------------------------------------------
        public string ReadToAndTo(string pattern1, string pattern2, bool readIfNotFound = false) {
            if (ReadToCheck(pattern1, readIfNotFound))
                return ReadTo(pattern2, readIfNotFound);
            else
                return "";
        }

        //--------------------------------------------------------------------------------
        public string ReadToAndSkip(string pattern, bool readIfNotFound = false) {
            string result = ReadTo(pattern, readIfNotFound);
            ReadPast(pattern, readIfNotFound);
            return result;
        }


        // PEEKING ================================================================================
        //--------------------------------------------------------------------------------
    }

}
