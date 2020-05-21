using CSACore.Utility;
using LumenWorks.Framework.IO.Csv;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace CSACore.CSV {

    public class CSVReader {
        //================================================================================
        private CsvReader                           mReader = null;
        private string                              mCopyPath = null;

        private Dictionary<string, HeaderIndex>     mHeaderIndices = new Dictionary<string, HeaderIndex>();

        private int                                 mRowCount;
        private int                                 mLineNumber = 1;
        
        //--------------------------------------------------------------------------------
        public event MissingColumnDelegate          MissingColumnEvent;
        public event MissingValueDelegate           MissingValueEvent;
        public event ParseErrorDelegate             ParseErrorEvent;
        public event TruncatedDelegate              TruncatedEvent;


        //================================================================================
        //--------------------------------------------------------------------------------
        private CSVReader(StreamReader streamReader) {
            Open(streamReader);
        }

        //--------------------------------------------------------------------------------
        public CSVReader(string path) {
            Open(path);
        }

        //--------------------------------------------------------------------------------
        public CSVReader(string path, string copyPath) {
            OpenIncrementalCopy(path, copyPath);
        }
        
        //--------------------------------------------------------------------------------
        public CSVReader(Stream stream) {
            Open(stream);
        }

        //--------------------------------------------------------------------------------
        public void Dispose() {
            Close();
        }

        
        // STATE ================================================================================
        //--------------------------------------------------------------------------------
        // We open the reader twice due to CsvReader.Count() reading to the end of the
        // file, without a convenient way to reset this.
        private void CountRows(StreamReader streamReader, bool dispose = true) {
            mReader = new CsvReader(streamReader, true);
            mRowCount = mReader.Count();
            if (dispose)
                mReader.Dispose(); // This closes the underlying stream, which we don't always want
            mReader = null;
        }

        //--------------------------------------------------------------------------------
        private void Open(StreamReader streamReader) {
            // CSV
            mReader = new CsvReader(streamReader, true);

            // Headers
            BuildHeaderIndices();
        }

        //--------------------------------------------------------------------------------
        public void Open(string path, bool close = true) {
            // Close
            if (close)
                Close();

            // Open
            CountRows(File.OpenText(path), false);
            Open(File.OpenText(path));
        }

        //--------------------------------------------------------------------------------
        public void Open(Stream stream) {
            // Close
            Close();

            // Open
            CountRows(new StreamReader(stream), false);
            stream.Seek(0, System.IO.SeekOrigin.Begin);
            Open(new StreamReader(stream));
        }

        //--------------------------------------------------------------------------------
        public void OpenIncrementalCopy(string path, string copyPath) {
            // Copy / open
            mCopyPath = Path.GetFullPath(UFile.IncrementalCopy(path, copyPath));
            Open(mCopyPath, false);
        }

        //--------------------------------------------------------------------------------
        public void Close(bool deleteCopy = true) {
            // Reader
            if (mReader != null) {
                mReader.Dispose();
                mReader = null;
            }

            // Copy path
            if (deleteCopy && mCopyPath != null && File.Exists(mCopyPath)) {
                File.Delete(mCopyPath);
                mCopyPath = null;
            }

            // Headers
            mHeaderIndices.Clear();
        }


        // PATHS ================================================================================
        //--------------------------------------------------------------------------------
        public string CopyPath { set { mCopyPath = value; } get { return mCopyPath; } }


        // READER ================================================================================
        //--------------------------------------------------------------------------------
        public CsvReader Reader { get { return mReader; } }

        //--------------------------------------------------------------------------------
        public bool ReadNext() {
            ++mLineNumber;
            return mReader.ReadNextRecord();
        }
        

        // ROWS ================================================================================
        //--------------------------------------------------------------------------------
        public int RowCount { get { return mRowCount; } }
        public int LineNumber { get { return mLineNumber; } }


        // HEADERS ================================================================================
        //--------------------------------------------------------------------------------
        private void BuildHeaderIndices() {
            string[] headers = mReader.GetFieldHeaders();
            mHeaderIndices.Clear();
            for (int i = 0; i < headers.Length; ++i) {
                mHeaderIndices.Add(headers[i].ToLower(), new HeaderIndex(i, headers[i]));
            }
        }

        //--------------------------------------------------------------------------------
        public Dictionary<string, HeaderIndex> HeaderIndices { get { return mHeaderIndices; } }

        //--------------------------------------------------------------------------------
        public string Header(int index) {
            IEnumerable<string> indices = from h in mHeaderIndices where h.Value.index == index select h.Key;
            return (indices.Count() > 0 ? indices.First() : null);
        }

        //--------------------------------------------------------------------------------
        public string CasedHeader(int index) {
            IEnumerable<string> indices = from h in mHeaderIndices where h.Value.index == index select h.Value.casedHeading;
            return (indices.Count() > 0 ? indices.First() : null);
        }

        //--------------------------------------------------------------------------------
        public string FindMissingColumn(params string[] columns) {
            foreach (string c in columns) {
                if (!mHeaderIndices.ContainsKey(c))
                    return c;
            }
            return null;
        }

        
        // FIELDS ================================================================================
        //--------------------------------------------------------------------------------
        public string FindMissingField(params string[] columns) {
            foreach (string c in columns) {
                string field = Field(c, false);
                if (field == null || field.Length == 0)
                    return c;
            }
            return null;
        }

        //--------------------------------------------------------------------------------
        public string Field(string name, bool notify = true) {
            string field = null;
            try { field = mReader[mHeaderIndices[name.ToLower()].index]; }
            catch (MissingFieldCsvException) {
                if (notify && MissingValueEvent != null)
                    MissingValueEvent(this, name);
            }
            catch (KeyNotFoundException) {
                if (notify && MissingColumnEvent != null)
                    MissingColumnEvent(this, name);
            }
            return field;
        }

        //--------------------------------------------------------------------------------
        public string Field(string name, int maxLength, bool notify = true) {
            string field = null;
            try { field = mReader[mHeaderIndices[name.ToLower()].index]; }
            catch (MissingFieldCsvException) {
                if (notify && MissingValueEvent != null)
                    MissingValueEvent(this, name);
            }
            catch (KeyNotFoundException) {
                if (notify && MissingColumnEvent != null)
                    MissingColumnEvent(this, name);
            }
            return Truncate(name, field, maxLength);
        }
        
        //--------------------------------------------------------------------------------
        public string FieldOrNull(string name, bool notify = true) {
            string field = Field(name, notify);
            if ((field != null) && (field.Length != 0))
                return mReader[mHeaderIndices[name.ToLower()].index];
            else
                return null;
        }
        
        //--------------------------------------------------------------------------------
        public string FieldOrNull(string name, int maxLength, bool notify = true) {
            string field = Field(name, maxLength, notify);
            if ((field != null) && (field.Length != 0))
                return Truncate(name, mReader[mHeaderIndices[name.ToLower()].index], maxLength);
            else
                return null;
        }

        //--------------------------------------------------------------------------------
        public T FieldAs<T>(string name) {
            // Field
            string fieldValue = Field(name);

            // Convert
            T result;
            if (!UConvert.TryFromString(fieldValue, out result)) {
                if (ParseErrorEvent != null)
                    ParseErrorEvent(this, name, typeof(int), fieldValue);
            }
            return result;
        }

        //--------------------------------------------------------------------------------
        public T FieldAs<T>(string name, int maxLength) {
            // Field
            string fieldValue = Field(name, maxLength);

            // Convert
            T result;
            if (!UConvert.TryFromString(fieldValue, out result)) {
                if (ParseErrorEvent != null)
                    ParseErrorEvent(this, name, typeof(int), fieldValue);
            }
            return result;
        }

        //--------------------------------------------------------------------------------
        public object DBField(string name) { return (object)Field(name) ?? DBNull.Value; }
        public object DBField(string name, int maxLength) { return (object)Field(name, maxLength) ?? DBNull.Value; }
        public object DBFieldOrNull(string name) { return (object)FieldOrNull(name) ?? DBNull.Value; }
        public object DBFieldOrNull(string name, int maxLength) { return (object)FieldOrNull(name, maxLength) ?? DBNull.Value; }
        
        //--------------------------------------------------------------------------------
        public object DBFieldBool(string name) {
            // Field
            string fieldValue = Field(name);
            if (string.IsNullOrEmpty(fieldValue))
                return DBNull.Value;
            
            // Parse
            return new string[] { "true", "t", "yes", "y" }.Contains(fieldValue.ToLower());
        }
        
        //--------------------------------------------------------------------------------
        public object DBFieldInt(string name) {
            // Field
            string fieldValue = Field(name);
            if (string.IsNullOrEmpty(fieldValue))
                return DBNull.Value;
            
            // Parse
            int result;
            if (Int32.TryParse(fieldValue, out result))
                return result;
            if (ParseErrorEvent != null)
                ParseErrorEvent(this, name, typeof(int), fieldValue);
            return DBNull.Value;
        }
        
        //--------------------------------------------------------------------------------
        public object DBFieldLong(string name) {
            // Field
            string fieldValue = Field(name);
            if (string.IsNullOrEmpty(fieldValue))
                return DBNull.Value;
            
            // Parse
            long result;
            if (Int64.TryParse(fieldValue, out result))
                return result;
            if (ParseErrorEvent != null)
                ParseErrorEvent(this, name, typeof(long), fieldValue);
            return DBNull.Value;
        }

        //--------------------------------------------------------------------------------
        public object DBFieldFloat(string name) {
            // Field
            string fieldValue = Field(name);
            if (string.IsNullOrEmpty(fieldValue))
                return DBNull.Value;
            
            // Parse
            float result;
            if (Single.TryParse(fieldValue, out result))
                return result;
            if (ParseErrorEvent != null)
                ParseErrorEvent(this, name, typeof(float), fieldValue);
            return DBNull.Value;
        }

        //--------------------------------------------------------------------------------
        public object DBFieldFloatFlexible(string name) {
            // Field
            string fieldValue = Field(name);
            if (string.IsNullOrEmpty(fieldValue))
                return DBNull.Value;
            
            // Modify
            string newFieldValue = new string((from c in fieldValue where (char.IsDigit(c) || (c == '.')) select c).ToArray());
            
            // Parse
            float result;
            if (Single.TryParse(newFieldValue, out result))
                return result;
            if (ParseErrorEvent != null)
                ParseErrorEvent(this, name, typeof(float), fieldValue);
            return DBNull.Value;
        }

        //--------------------------------------------------------------------------------
        public object DBFieldDecimal(string name) {
            // Field
            string fieldValue = Field(name);
            if (string.IsNullOrEmpty(fieldValue))
                return DBNull.Value;
            
            // Parse
            decimal result;
            if (Decimal.TryParse(fieldValue, out result))
                return result;
            if (ParseErrorEvent != null)
                ParseErrorEvent(this, name, typeof(decimal), fieldValue);
            return DBNull.Value;
        }

        //--------------------------------------------------------------------------------
        public object DBFieldDecimalFlexible(string name) {
            // Field
            string fieldValue = Field(name);
            if (string.IsNullOrEmpty(fieldValue))
                return DBNull.Value;

            // Modify
            string newFieldValue = new string((from c in fieldValue where (char.IsDigit(c) || (c == '.')) select c).ToArray());
            
            // Parse
            decimal result;
            if (Decimal.TryParse(newFieldValue, out result))
                return result;
            if (ParseErrorEvent != null)
                ParseErrorEvent(this, name, typeof(decimal), fieldValue);
            return DBNull.Value;
        }

        //--------------------------------------------------------------------------------
        public object DBFieldDateTime(string name, bool utcTime = false) {
            // Field
            string fieldValue = Field(name);
            if (string.IsNullOrEmpty(fieldValue))
                return DBNull.Value;

            // Parse
            DateTime result;
            if (DateTime.TryParse(fieldValue, out result))
                return (utcTime ? result.ToUniversalTime() : result);
            if (ParseErrorEvent != null)
                ParseErrorEvent(this, name, typeof(DateTime), fieldValue);
            return DBNull.Value;
        }
        
        //--------------------------------------------------------------------------------
        public DateTime? DBFieldDateTimeOrNull(string name) {
            object result = DBFieldDateTime(name);
            if (result != DBNull.Value)
                return (DateTime)result;
            else
                return null;
        }

        //--------------------------------------------------------------------------------
        public string Truncate(string name, string value, int maxLength) {
            if (string.IsNullOrEmpty(value))
                return value;
            if (value.Length <= maxLength)
                return value;
            else {
                if (TruncatedEvent != null)
                    TruncatedEvent(this, name, maxLength, value.Length);
                return value.Substring(0, maxLength);
            }
        }


        // EVENTS ================================================================================
        //--------------------------------------------------------------------------------
        public void FireOnMissingColumn(string name) {
            if (MissingColumnEvent != null)
                MissingColumnEvent(this, name);
        }

        //--------------------------------------------------------------------------------
        public void FireOnMissingValue(string name) {
            if (MissingValueEvent != null)
                MissingValueEvent(this, name);
        }

        //--------------------------------------------------------------------------------
        public void FireOnParseError(string name, Type type, string value) {
            if (ParseErrorEvent != null)
                ParseErrorEvent(this, name, type, value);
        }

        //--------------------------------------------------------------------------------
        public void FireOnTruncated(string name, int length, int valueLength) {
            if (TruncatedEvent != null)
                TruncatedEvent(this, name, length, valueLength);
        }


        //================================================================================
        //********************************************************************************
        public delegate void MissingColumnDelegate(CSVReader csv, string name);
        public delegate void MissingValueDelegate(CSVReader csv, string name);
        public delegate void ParseErrorDelegate(CSVReader csv, string name, Type type, string value);
        public delegate void TruncatedDelegate(CSVReader csv, string name, int length, int valueLength);

        //********************************************************************************
        public struct HeaderIndex {
            public int index;
            public string casedHeading;

            public HeaderIndex(int index, string casedHeading) {
                this.index = index;
                this.casedHeading = casedHeading;
            }
        }
    }

}
