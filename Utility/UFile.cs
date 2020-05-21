using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace CSACore.Utility {

    public static class UFile {
        //================================================================================
        //--------------------------------------------------------------------------------
        public static string IncrementalFreePath(string path) {
            // Free path
            if (!File.Exists(path) && !Directory.Exists(path))
                return path;

            // Parts
            string prefix = Path.GetDirectoryName(path) + "/" + Path.GetFileNameWithoutExtension(path);
            string extension = Path.GetExtension(path);

            // Find free filename
            int counter = 2;
            while (File.Exists(prefix + "_" + counter + extension) || Directory.Exists(prefix + "_" + counter + extension)) {
                ++counter;
            }

            // Return
            return (prefix + "_" + counter + extension).Replace(@"\", "/");
        }

        //--------------------------------------------------------------------------------
        public static string IncrementalMove(string sourceFilePath, string destinationFilePath) {
            string freePath = IncrementalFreePath(destinationFilePath);
            File.Move(sourceFilePath, freePath);
            return freePath;
        }

        //--------------------------------------------------------------------------------
        public static string IncrementalCopy(string sourceFilePath, string destinationFilePath) {
            string freePath = IncrementalFreePath(destinationFilePath);
            File.Copy(sourceFilePath, freePath);
            return freePath;
        }
    }
}
