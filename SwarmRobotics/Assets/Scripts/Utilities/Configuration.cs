using UnityEngine;

using System.IO;

namespace Utilities
{
    public class Configuration
    {
        public enum eSpawnShape { CIRCLE, SQUARE };

        private eSpawnShape spawnShape = eSpawnShape.SQUARE;
        private float groundLength = 25.0f;
        private float spawnRadius = 5.0f; // applies to both square and circle spawn shapes
        private int numRobots = 16;
        private Vector2 spawnCenter = Vector2.zero;

        /// <summary>
        /// Side length of ground, assuming when it's square.
        /// </summary>
        public float GroundLength
        {
            get { return groundLength; }
            set { if (value > 0f) { groundLength = value; } }
        }

        /// <summary>
        /// Number of robots to be spawned.
        /// </summary>
        public int NumRobots
        {
            get { return numRobots; }
            set { if (value > 0) { numRobots = value; } }
        }

        /// <summary>
        /// The center of the spawn area.
        /// </summary>
        public Vector2 SpawnCenter
        {
            get { return spawnCenter; }
            set { spawnCenter = value; } // Check that it's within the ground radius? would require
                                         // some sort of order check.
        }

        /// <summary>
        /// Shape of the robot spawn area. Though circles are possible, they are single-layer only.
        /// </summary>
        public eSpawnShape SpawnShape
        {
            get { return spawnShape; }
            set { spawnShape = value; }
        }

        /// <summary>
        /// Radius of the robot spawn area.
        /// </summary>
        public float SpawnRadius
        {
            get { return spawnRadius; }
            set { spawnRadius = value; }
        }

        /// <summary>
        /// Read a given configuration file from the config/ folder, setting default arguments if 
        /// necessary. Options are generally given, each on their own line, as "key=value"
        /// </summary>
        /// <param name="configFile">The config filename.</param>
        public Configuration(string configFile)
        {
            string filePath = FileUtilities.buildConfigPath(configFile);

            if (File.Exists(filePath))
            {
                string sKey, sValue;
                string[] configOptions = File.ReadAllLines(filePath);

                for (int i = 0; i < configOptions.Length; ++i)
                {
                    string option = configOptions[i];
                    if (!option.StartsWith("#") && option.Trim().Length > 0)
                    {
                        sKey = option.Trim();
                        int delimIndex = sKey.IndexOf('=');

                        if (delimIndex != -1)
                        {
                            sValue = sKey.Substring(delimIndex + 1).Trim();
                            sKey = sKey.Substring(0, delimIndex).Trim().ToLower();

                            if (sKey != null)
                            {
                                if (sKey == "groundlength")
                                {
                                    if (!extractFloat(sValue, ref groundLength))
                                        Log.w(LogTag.CONFIG, "Invalid ground length " + sValue);
                                }
                                else if (sKey == "numrobots")
                                {
                                    if (!extractInt(sValue, ref numRobots))
                                        Log.w(LogTag.CONFIG, "Invalid number of robots " + sValue);
                                }
                                else if (sKey == "spawncenter")
                                {
                                    if (!extractVector2(sValue, ref spawnCenter))
                                         Log.w(LogTag.CONFIG, "Invalid spawn center: " + sValue);
                                }
                                else if (sKey == "spawnradius")
                                {
                                    if (!extractFloat(sValue, ref spawnRadius))
                                        Log.w(LogTag.CONFIG, "Invalid spawn radius " + sValue);
                                }
                                else if (sKey == "spawnshape")
                                {
                                    switch (sValue.ToLower())
                                    {
                                    case "circle":
                                        spawnShape = eSpawnShape.CIRCLE;
                                        break;
                                    case "square":
                                        spawnShape = eSpawnShape.SQUARE;
                                        break;
                                    default:
                                        Log.w(LogTag.CONFIG, "Invalid spawn shape" + sValue);
                                        break;
                                    }
                                }
                            }
                        }
                        else
                        {
                            Log.e(LogTag.CONFIG, "Invalid key-value format: " + sKey);
                        }
                    }
                }
            }
            else
            {
                Log.e(LogTag.CONFIG, "Config file \"" + filePath + "\" not found");
            }
        }

        private bool extractFloat(string sValue, ref float fValue)
        {
            bool result;
            float f;

            if (result = float.TryParse(sValue, out f))
                fValue = f;

            return result;
        }

        private bool extractInt(string sValue, ref int iValue)
        {
            bool result;
            int i;

            if (result = int.TryParse(sValue, out i))
                iValue = i;

            return result;
        }

        private bool extractVector2(string sValue, ref Vector2 vValue)
        {
            bool result = false;
            float x, z;
            string[] coordinates = sValue.Trim(new char[] { '{', '}' }).Split(',');

            if (coordinates.Length == 2 &&
                float.TryParse(coordinates[0].Trim(), out x) &&
                float.TryParse(coordinates[1].Trim(), out z))
            {
                result = true;
                vValue = new Vector2(x, z);
            }

            return result;
        }
    }
}
