using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace CSACore.Profiling {

    public class Profiler {
        //================================================================================
        public const char                           ENTRY_NAME_DELIMITER = '.';


        //================================================================================
        private Dictionary<string, ProfilerEntry>   mEntries = new Dictionary<string, ProfilerEntry>();


        //================================================================================
        //--------------------------------------------------------------------------------
        public Profiler() { }


        // ENTRIES ================================================================================
        //--------------------------------------------------------------------------------
        public ProfilerEntry Start(string name) {
            // Entry
            ProfilerEntry entry;
            if (!mEntries.ContainsKey(name)) {
                entry = new ProfilerEntry(name);
                mEntries.Add(name, entry);
            }
            else
                entry = mEntries[name];

            // Start
            entry.Start();
            return entry;
        }
        
        //--------------------------------------------------------------------------------
        public ProfilerEntry Stop(string name) {
            // Entry
            if (!mEntries.ContainsKey(name))
                throw new KeyNotFoundException();
            ProfilerEntry entry = mEntries[name];

            // Stop
            entry.Stop();
            return entry;
        }

        //--------------------------------------------------------------------------------
        public void Clear() { mEntries.Clear(); }
        public ProfilerEntry Entry(string name) { return mEntries[name]; }
        public IEnumerable<ProfilerEntry> Entries { get { return mEntries.Values; } }


        // RESULTS ================================================================================
        //--------------------------------------------------------------------------------
        public string ResultsString {
            get {
                StringBuilder builder = new StringBuilder();
                BuildResultsString(builder);
                return builder.ToString();
            }
        }

        //--------------------------------------------------------------------------------
        private void BuildResultsString(StringBuilder builder, string parentsName = "", int depth = 0, List<bool> atEndList = null) {
            // At end list
            if (atEndList == null)
                atEndList = new List<bool>();
            atEndList.Add(false);

            // Dimensions
            int resultsNameWidth = ResultNamesWidth;

            // Header
            if (depth == 0) {
                builder.AppendLine(new string('-', resultsNameWidth + 1 + 9 + 1 + 13 + 1 + 13));
                builder.Append(new string(' ', resultsNameWidth));
                builder.Append(" ");
                builder.Append(" Quantity");
                builder.Append(" ");
                builder.Append("      Average");
                builder.Append(" ");
                builder.Append("        Total");
                builder.AppendLine();
            }

            // Entries
            List<ProfilerEntry> entries = (from e in mEntries.Values where e.ParentsName.Equals(parentsName) select e).ToList();
            entries.Sort((a, b) => a.LocalName.CompareTo(b.LocalName));

            // Results
            for (int i = 0; i < entries.Count; ++i) {
                // Tree
                if (depth > 0) {
                    for (int j = 1; j < depth; ++j) {
                        builder.Append(!atEndList[j] ? "│  " : "   ");
                    }
                    builder.Append(i < entries.Count - 1 ? "├─ " : "└─ ");
                }

                // At end
                if (i == entries.Count - 1)
                    atEndList[depth] = true;

                // Entry
                builder.Append(entries[i].LocalName);
                builder.Append(new string(' ', resultsNameWidth - (depth * 3 + entries[i].LocalName.Length)));
                builder.Append(" ");
                builder.Append($"{entries[i].RunCount, 9}");
                builder.Append(" ");
                builder.Append($"{entries[i].AverageDuration, 13:F4}");
                builder.Append(" ");
                builder.Append($"{entries[i].TotalDuration, 13:F4}");
                builder.AppendLine();

                // Next depth
                BuildResultsString(builder, entries[i].Name, depth + 1, atEndList);
            }
            
            // Line
            if (depth == 0)
                builder.AppendLine(new string('-', resultsNameWidth + 1 + 9 + 1 + 13 + 1 + 13));

            // At end list
            atEndList.RemoveAt(atEndList.Count - 1);
        }

        //--------------------------------------------------------------------------------
        private int ResultNamesWidth {
            get {
                int depth = 0;
                int longestNameLength = 0;
                foreach (ProfilerEntry e in mEntries.Values) {
                    if (e.Depth > depth)
                        depth = e.Depth;
                    if (e.LocalName.Length > longestNameLength)
                        longestNameLength = e.LocalName.Length;
                }

                return depth * 3 + longestNameLength;
            }
        }
    }

}
