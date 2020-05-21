using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace CSACore.CSV {

    public class CSVWriter {
        //================================================================================
        private StreamWriter                    mWriter;

        private List<string>                    mHeaders = new List<string>();

        private int                             mRowIndex = -1;
        private int                             mColumnIndex = 0;


        //================================================================================
        //--------------------------------------------------------------------------------
        public CSVWriter(StreamWriter streamWriter) {
            Open(streamWriter);
        }

        //--------------------------------------------------------------------------------
        public CSVWriter(string path) : this(new StreamWriter(path)) { }

        //--------------------------------------------------------------------------------
        public void Dispose() {
            Close();
        }


        // STATE ================================================================================
        //--------------------------------------------------------------------------------
        public void Open(StreamWriter writer) {
            // Close
            Close();

            // Open
            mWriter = writer;

            // Reset
            mHeaders.Clear();
            mRowIndex = -1;
        }

        //--------------------------------------------------------------------------------
        public void Close() {
            if (mWriter != null) {
                mWriter.Dispose();
                mWriter = null;
            }
        }


        // HEADERS ================================================================================
        //--------------------------------------------------------------------------------
        public void WriteHeader(string header) {
            // Checks
            if (mRowIndex != -1)
                throw new InvalidOperationException("No longer on the header row");
            if (string.IsNullOrEmpty(header))
                throw new ArgumentException("Header is empty");

            // Headers
            mHeaders.Add(header);

            // Write
            if (mColumnIndex > 0)
                mWriter.Write(",");

            if (header.Contains('"') || header.Contains(',') || header.Contains('\n'))
                mWriter.Write('"');
            mWriter.Write(header.Replace("\"", "\"\""));
            if (header.Contains('"') || header.Contains(',') || header.Contains('\n'))
                mWriter.Write('"');

            // Increment
            ++mColumnIndex;
        }

        //--------------------------------------------------------------------------------
        public string Header(int index) { return mHeaders[index]; }
        public int HeaderCount { get { return mHeaders.Count; } }


        // VALUES ================================================================================
        //--------------------------------------------------------------------------------
        public void WriteValue(string value) {
            // Checks
            if (mRowIndex == -1)
                throw new InvalidOperationException("On the header row");
            if (mColumnIndex >= mHeaders.Count)
                throw new InvalidOperationException("Row has already reached same column count has header row");

            // Null replacement
            if (value == null)
                value = "";

            // Write
            if (mColumnIndex > 0)
                mWriter.Write(",");

            if (value.Contains('"') || value.Contains(',') || value.Contains('\n'))
                mWriter.Write('"');
            mWriter.Write(value.Replace("\"", "\"\""));
            if (value.Contains('"') || value.Contains(',') || value.Contains('\n'))
                mWriter.Write('"');

            // Increment
            ++mColumnIndex;
        }


        // ROWS ================================================================================
        //--------------------------------------------------------------------------------
        public void WriteEndRow() {
            // Checks
            if (mColumnIndex == 0)
                throw new InvalidOperationException("Row is empty");
            if (mRowIndex >= 0 && mColumnIndex != mHeaders.Count)
                throw new InvalidOperationException("Row has less or more columns than header row");

            // Write
            mWriter.WriteLine();
            ++mRowIndex;
            mColumnIndex = 0;
        }
    }

}
