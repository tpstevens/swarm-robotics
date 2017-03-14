using UnityEngine;

using System.IO;

namespace Utilities
{
    public class Config
    {
        public enum eSpawnShape { CIRCLE, SQUARE };

        private bool commShowInUnityConsole = false;
        private bool commShowMsgIndicators = false;
        private eSpawnShape spawnShape = eSpawnShape.SQUARE;
        private float commMsgDistanceLimit = 64.0f;
        private float commMsgSpeed = 10.0f;
        private float groundLength = 64.0f;
        private float spawnRadius = 5.0f; // applies to both square and circle spawn shapes
        private float robotRadarRange = 10.0f;
        private int numRobots = 1;
        private Rect resourceHomeRect = new Rect(new Vector2(20, 20), new Vector2(10, 10));
        private Vector2 spawnCenter = Vector2.zero;

        public bool CommShowInUnityConsole
        {
            get { return commShowInUnityConsole; }
            set { commShowInUnityConsole = value; }
        }

        public bool CommShowMsgIndicators
        {
            get { return commShowMsgIndicators; }
            set { commShowMsgIndicators = value; }
        }

        public float CommMsgDistanceLimit
        {
            get { return commMsgDistanceLimit; }
            set { if (value > 0f) { commMsgDistanceLimit = value; } }
        }

        public float CommMsgSpeed
        {
            get { return commMsgSpeed; }
            set { if (value > 0f) { commMsgSpeed = value; } }
        }

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
        /// The range of each robot's radar.
        /// </summary>
        public float RobotRadarRange
        {
            get { return robotRadarRange; }
            set { if (value > 0) { robotRadarRange = value; } }
        }

        public Rect ResourceHomeRect
        {
            get { return resourceHomeRect; }
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
        public Config(string configFile)
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
                                if (sKey == "commshowinunityconsole")
                                {
                                    if (!extractBool(sValue, ref commShowInUnityConsole))
                                        Log.w(LogTag.CONFIG, "Invalid Comm console state " + sValue);
                                }
                                else if (sKey == "commshowmsgindicator")
                                {
                                    if (!extractBool(sValue, ref commShowMsgIndicators))
                                        Log.w(LogTag.CONFIG, "Invalid Comm msg indicator state " + sValue);
                                }
                                else if (sKey == "groundlength")
                                {
                                    Log.w(LogTag.CONFIG, "Ignoring ground length from config (" + sValue + ") because of NavMesh");
                                    // if (!extractFloat(sValue, ref groundLength))
                                    //     Log.w(LogTag.CONFIG, "Invalid ground length " + sValue);
                                }
                                else if (sKey == "msgdistancelimit")
                                {
                                    if (!extractFloat(sValue, ref commMsgDistanceLimit))
                                        Log.w(LogTag.CONFIG, "Invalid Comm message limit " + sValue);
                                }
                                else if (sKey == "msgspeed")
                                {
                                    if (!extractFloat(sValue, ref commMsgSpeed))
                                        Log.w(LogTag.CONFIG, "Invalid Comm message speed " + sValue);
                                }
                                else if (sKey == "numrobots")
                                {
                                    if (!extractInt(sValue, ref numRobots))
                                        Log.w(LogTag.CONFIG, "Invalid number of robots " + sValue);
                                }
                                else if (sKey == "radarrange")
                                {
                                    if (!extractFloat(sValue, ref robotRadarRange))
                                        Log.w(LogTag.CONFIG, "Invalid Robot radar range " + sValue);
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

        private bool extractBool(string sValue, ref bool bValue)
        {
            bool result;
            bool b;

            if (result = bool.TryParse(sValue, out b))
                bValue = b;

            return result;
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
