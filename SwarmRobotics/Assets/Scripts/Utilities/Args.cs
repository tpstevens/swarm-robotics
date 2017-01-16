using System.IO;

namespace Utilities
{
    public class Args {

        public readonly string configFileName = "config.txt";

        /// <summary>
        /// Reads the args.txt to set program arguments (config file, etc.) If args.txt doesn't 
        /// exist, creates it and sets all options to defaults.
        /// </summary>
        public Args()
        {
            string filePath = FileUtilities.buildArgsPath("args.txt");

            if (File.Exists(filePath))
            {
                string[] args = File.ReadAllLines(filePath);
                for (int i = 0; i < args.Length; ++i)
                {
                    string option = args[i];
                    if (option.Trim().Length > 0)
                    {
                        string key = option.Trim();
                        int delimIndex = key.IndexOf('=');

                        if (delimIndex != -1)
                        {
                            string value = key.Substring(delimIndex + 1).Trim();
                            key = key.Substring(0, delimIndex).Trim().ToLower();
                            if (key != null)
                            {
                                if (key == "configname")
                                {
                                    configFileName = value;
                                }
                                // TODO add other options to if-else here
                            }
                        }
                        else
                        {
                            Log.e(LogTag.ARGS, "Invalid key-value format: " + key);
                        }
                    }
                }
            }
            else
            {
                Log.w(LogTag.ARGS, "Generating default args file");

                // Create default args file
                StreamWriter s = File.CreateText(filePath);
                s.WriteLine("configName=" + configFileName);
                s.Close();
            }
        }
    }
}
