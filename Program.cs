
using System.Linq;

namespace FindDuplicates
{
    internal class Program
    {
        public static byte[] CalculateHash(System.IO.FileInfo fileInfo)
        {
            byte[] hash = { };

            using (System.IO.FileStream fileStream = fileInfo.OpenRead())
            {
                using (System.IO.BufferedStream bFileStream = new System.IO.BufferedStream(fileStream, (1024 * 1024)))
                {
                    using (System.Security.Cryptography.SHA1 sha = System.Security.Cryptography.SHA1.Create())
                    {
                        bFileStream.Position = 0;
                        hash = sha.ComputeHash(bFileStream);
                    }
                }
            }

            return hash;
        }

        static void Main(string[] args)
        {
            System.Diagnostics.Debug.Assert(args.Length > 0, "FAIL, expected at least 1 argument");
            foreach (string arg in args)
            {
                System.Diagnostics.Debug.Assert(System.IO.Directory.Exists(arg), "FAIL, directory does not exist: " + arg);
                System.Diagnostics.Debug.Assert(arg.EndsWith("" + System.IO.Path.DirectorySeparatorChar) == false, "FAIL, directory path ends with a directory seperator character: " + arg);
            }

            // store as a dictionary keyed by the unique hash, with the value being a list of the files with the unique hash

            System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<string>> hashedFiles = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<string>>();
            System.Collections.Generic.List<string> skippedAbnormalFiles = new System.Collections.Generic.List<string>();
            System.Collections.Generic.List<string> skippedEmptyFiles = new System.Collections.Generic.List<string>();

            foreach (string inputPath in args)
            {
                System.Console.WriteLine("** starting " + inputPath + " **");
                
                System.Collections.Generic.IEnumerable<string> filePathsEnumeration = System.IO.Directory.EnumerateFiles(inputPath, "*.*", System.IO.SearchOption.AllDirectories);
                foreach (string filePath in filePathsEnumeration)
                {
                    // skip empty files
                    System.IO.FileInfo fileInfo = new System.IO.FileInfo(filePath);
                    if (fileInfo.Length == 0)
                    {
                        skippedEmptyFiles.Add(filePath);
                        System.Console.WriteLine("  empty (skipped): " + filePath);
                        continue;
                    }

                    byte[] _hash = CalculateHash(fileInfo); // compute hash
                    string hash = System.BitConverter.ToString(_hash);

                    // duplicate found, add to existing entry in dictionary
                    if (hashedFiles.ContainsKey(hash))
                    {
                        hashedFiles[hash].Add(filePath);
                        System.Console.WriteLine("  duplicate: ("+ hash + ") " + filePath);
                        continue;
                    }
                    
                    // unique found, add new entry in dictionary
                    hashedFiles.Add(hash, new System.Collections.Generic.List<string> { filePath });
                    System.Console.WriteLine("  unique: ("+ hash + ") " + filePath);
                }

                System.Console.WriteLine("** finished " + inputPath + " **");
            }

            int quantityOfUniquesFound = hashedFiles.Count(e => e.Value.Count() == 1);
            int quantityOfDuplicatesFound = hashedFiles.Count( e => e.Value.Count() > 1 );

            System.Console.WriteLine("Finished, press ENTER to quit");
            System.Console.ReadLine();
        }
    }
}
