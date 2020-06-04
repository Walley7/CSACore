using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace CSACore.Profiling {

    public class ProfilerEntry {
        //================================================================================
        private string                          mName;
        private string[]                        mNameParts;

        private Stopwatch                       mStopwatch = new Stopwatch();
        private int                             mStartCount = 0;

        private long                            mLastStartTime;
        private List<decimal>                   mDurations = new List<decimal>();


        //================================================================================
        //--------------------------------------------------------------------------------
        public ProfilerEntry(string name) {
            mName = name;
            mNameParts = name.Split(Profiler.ENTRY_NAME_DELIMITER);
        }


        // NAME ================================================================================
        //--------------------------------------------------------------------------------
        public string Name { get { return mName; } }
        public string[] NameParts { get { return mNameParts; } }
        public string LocalName { get { return mNameParts[mNameParts.Length - 1]; } }
        public string ParentsName { get { return (mNameParts.Length > 1 ? string.Join(".", mNameParts, 0, mNameParts.Length - 1): ""); } }
        public int Depth { get { return Math.Max(mNameParts.Length - 1, 0); } }


        // TIMING ================================================================================
        //--------------------------------------------------------------------------------
        public bool Start() {
            if (Started)
                return false;
            mLastStartTime = mStopwatch.ElapsedTicks;
            mStopwatch.Start();
            ++mStartCount;
            return true;
        }

        //--------------------------------------------------------------------------------
        public bool Stop() {
            if (!Started)
                return false;
            mStopwatch.Stop();
            mDurations.Add((decimal)(mStopwatch.ElapsedTicks - mLastStartTime) / TimeSpan.TicksPerMillisecond);
            return true;
        }

        //--------------------------------------------------------------------------------
        public bool Started { get { return mStopwatch.IsRunning; } }

        //--------------------------------------------------------------------------------
        public int RunCount { get { return mStartCount; } }
        public decimal TotalDuration { get { return (decimal)mStopwatch.ElapsedTicks / TimeSpan.TicksPerMillisecond; } }
        public decimal AverageDuration { get { return (RunCount > 0 ? TotalDuration / RunCount : 0.0m); } }
        public List<decimal> Durations { get { return mDurations; } }
    }

}
