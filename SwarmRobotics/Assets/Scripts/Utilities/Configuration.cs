using System.IO;

namespace Utilities
{
    public class Configuration
    {
        private float groundLength = 25.0f;

        /// <summary>
        /// Side length of ground, assuming when it's square.
        /// </summary>
        public float GroundLength
        {
            get { return groundLength; }
            set { if (value > 0f) { groundLength = value; } }
        }

        /// <summary>
        /// Read a given configuration file from the config/ folder, setting default arguments if 
        /// necessary. Options are generally given, each on their own line, as "key=value"
        /// </summary>
        /// <param name="configFile"></param>
        public Configuration(string configFile)
        {
            string filePath = FileUtilities.buildConfigPath(configFile);

            if (File.Exists(filePath))
            {
                float fValue;

                string[] configOptions = File.ReadAllLines(filePath);
                for (int i = 0; i < configOptions.Length; ++i)
                {
                    string option = configOptions[i];
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
                                if (key == "groundlength")
                                {
                                    if (float.TryParse(value, out fValue))
                                    {
                                        groundLength = fValue;
                                    }
                                    else
                                    {
                                        Log.w(LogTag.CONFIG, "Invalid ground length " + value);
                                    }
                                }
                                // TODO add other options to if-else here
                            }
                        }
                        else
                        {
                            Log.e(LogTag.CONFIG, "Invalid key-value format: " + key);
                        }
                    }
                }
            }
            else
            {
                Log.e(LogTag.CONFIG, "Config file \"" + filePath + "\" not found");
            }
        }
    }
}
