using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace CSACore.Logging {

    public class Logger {
        //================================================================================
        public enum Category {
            NONE,
            START,
            END,
            ERROR,
            WARNING
        }

        //--------------------------------------------------------------------------------
        public const int                        INDENT_LENGTH = 2;

            
        //================================================================================
        private StreamWriter                    mStreamWriter = null;

        private string                          mIndent = "";

        private DateTime                        mLogStartTime = DateTime.Now;


        //================================================================================
        //--------------------------------------------------------------------------------
        public Logger(string path) {
            Open(path);
        }

        //--------------------------------------------------------------------------------
        public void Dispose() {
            Close();
        }
        

        // STATE ================================================================================
        //--------------------------------------------------------------------------------
        public void Open(string path) {
            // Close
            Close();

            // Open
            mStreamWriter = File.AppendText(path);
            mStreamWriter.AutoFlush = true;
        }
        
        //--------------------------------------------------------------------------------
        public void Close() {
            if (mStreamWriter != null) {
                mStreamWriter.Close();
                mStreamWriter = null;
            }
        }
        
        //--------------------------------------------------------------------------------
        public bool IsOpen { get { return mStreamWriter != null; } }


        // LOGGING ================================================================================
        //--------------------------------------------------------------------------------
        public void Log(string text) {
            if (!IsOpen)
                throw new InvalidOperationException("The log is not open.");
            Console.WriteLine(text);
            mStreamWriter.WriteLine(text);
        }

        //--------------------------------------------------------------------------------
        public void Log(Category category, string text) {
            Log("[" + CategoryString(category) + "] " + mIndent + text);
        }

        //--------------------------------------------------------------------------------
        public void LogDoubleBreak() { Log("================================================================================"); }
        public void LogBreak() { Log("--------------------------------------------------------------------------------"); }
        public void LogBlankLine() { Log(""); }
        
        //--------------------------------------------------------------------------------
        public void LogInfo(string text) { Log(Category.NONE, text); }

        //--------------------------------------------------------------------------------
        public void LogStart() {
            LogDoubleBreak();
            mLogStartTime = DateTime.Now;
            Log(Category.START, mLogStartTime.ToString());
        }
        
        //--------------------------------------------------------------------------------
        public void LogEnd() {
            LogBreak();
            Log(Category.END, $"Done (duration: {(DateTime.Now - mLogStartTime).ToString("hh\\:mm\\:ss")})");
            LogBlankLine();
        }

        //--------------------------------------------------------------------------------
        public void LogError(string text) { Log(Category.ERROR, text); }
        public void LogWarning(string text) { Log(Category.WARNING, text); }


        // CATEGORIES ================================================================================
        //--------------------------------------------------------------------------------
        public static string CategoryString(Category category) {
            switch (category) {
                case Category.START:    return " START ";
                case Category.END:      return "  END  ";
                case Category.ERROR:    return " ERROR ";
                case Category.WARNING:  return "WARNING";
                default:                return "       ";
            }
        }


        // INDENT ================================================================================
        //--------------------------------------------------------------------------------
        public void IncreaseIndent(int count = 1) {
            for (int i = 0; i < count; ++i) {
                mIndent += new string(' ', INDENT_LENGTH);
            }
        }

        //--------------------------------------------------------------------------------
        public void DecreaseIndent(int count = 1) {
            for (int i = 0; i < count; ++i) {
                if (mIndent.Length == 0)
                    break;
                mIndent = mIndent.Remove(mIndent.Length - 2);
            }
        }

        //--------------------------------------------------------------------------------
        public void SetIndent(int count) {
            mIndent = "";
            IncreaseIndent(count);
        }

        //--------------------------------------------------------------------------------
        public void ResetIndent() { SetIndent(0); }
        public string Indent { get { return mIndent; } }
        public int IndentCount { get { return mIndent.Length / INDENT_LENGTH; } }
    }

}
