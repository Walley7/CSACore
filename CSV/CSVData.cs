using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace CSACore.CSV {

    public class CSVData {
        //================================================================================
        private Dictionary<string, CSVReader.HeaderIndex>   mHeaderIndices = new Dictionary<string, CSVReader.HeaderIndex>();
        private List<string>                                mHeaders = new List<string>();
        private List<string>                                mCasedHeaders = new List<string>();

        private List<CSVDataRow>                            mRows = new List<CSVDataRow>();


        //================================================================================
        //--------------------------------------------------------------------------------
        public CSVData() { }

        //--------------------------------------------------------------------------------
        public CSVData(CSVReader reader) {
            ReadData(reader);
        }

        //--------------------------------------------------------------------------------
        public CSVData(string path) {
            CSVReader reader = new CSVReader(path);
            ReadData(reader);
            reader.Close();
        }

        //--------------------------------------------------------------------------------
        public CSVData(Stream stream) {
            CSVReader reader = new CSVReader(stream);
            ReadData(reader);
            reader.Close();
        }

        //--------------------------------------------------------------------------------
        public void Dispose() {
            mHeaders.Clear();
            mRows.Clear();
        }


        // READING ================================================================================
        //--------------------------------------------------------------------------------
        public void ReadData(CSVReader reader) {
            // Clear
            mHeaders.Clear();
            mHeaderIndices.Clear();
            mRows.Clear();

            // Header indices
            mHeaderIndices = new Dictionary<string, CSVReader.HeaderIndex>(reader.HeaderIndices);

            // Headers
            for (int i = 0; i < reader.HeaderIndices.Count; ++i) {
                mHeaders.Add(reader.Header(i));
                mCasedHeaders.Add(reader.CasedHeader(i));
            }

            // Rows
            int c = 0;
            while (reader.ReadNext()) {
                mRows.Add(new CSVDataRow(this, c++, reader));
            }
        }


        // HEADERS ================================================================================
        //--------------------------------------------------------------------------------
        public Dictionary<string, CSVReader.HeaderIndex> HeaderIndices { get { return mHeaderIndices; } }
        public List<string> Headers { get { return mHeaders; } }
        public List<string> CasedHeaders { get { return mCasedHeaders; } }
        public int ColumnCount { get { return mHeaders.Count; } }


        // ROWS ================================================================================
        //--------------------------------------------------------------------------------
        public List<CSVDataRow> Rows { get { return mRows; } }
    }

}
