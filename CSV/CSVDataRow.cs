using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace CSACore.CSV {

    public class CSVDataRow {
        //================================================================================
        private CSVData                         mCSVData;

        private int                             mRowIndex;

        private List<string>                    mValues = new List<string>();


        //================================================================================
        //--------------------------------------------------------------------------------
        public CSVDataRow(CSVData data, int rowIndex, CSVReader reader) {
            // CSV data
            mCSVData = data;

            // Row index
            mRowIndex = rowIndex;

            // Values
            for (int i = 0; i < data.ColumnCount; ++i) {
                mValues.Add(reader.FieldOrNull(data.Headers[i], false));
            }
        }


        // COLUMNS ================================================================================
        //--------------------------------------------------------------------------------
        public int ColumnCount { get { return mCSVData.ColumnCount; } }


        // ROW ================================================================================
        //--------------------------------------------------------------------------------
        public int RowIndex { get { return mRowIndex; } }


        // VALUES ================================================================================
        //--------------------------------------------------------------------------------
        public string Value(string column) { return mValues[mCSVData.HeaderIndices[column.ToLower()].index]; }
        public List<string> Values { get { return mValues; } }
    }

}
