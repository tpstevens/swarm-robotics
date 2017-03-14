using UnityEngine;

using System.Collections.Generic;

using Utilities;

namespace Messages
{
    class Parser
    {
        protected internal static bool TryParseBool(string str, out bool result)
        {
            return bool.TryParse(str.Trim(), out result);
        }

        protected internal static bool TryParseVector2(string str, out Vector2 result)
        {
            bool success = true;
            float x, y;
            string[] coordinates = str.Trim(new char[] { '(', ')' }).Split(',');

            if (coordinates.Length == 2 &&
                float.TryParse(coordinates[0].Trim(), out x) &&
                float.TryParse(coordinates[1].Trim(), out y))
            {
                result = new Vector2(x, y);
            }
            else
            {
                Log.e(LogTag.MESSAGEPARSER, "Failed to parse \"" + str.Trim() + "\" as a Vector2");
                result = new Vector2(float.MinValue, float.MinValue);
                success = false;
            }

            return success;
        }

        protected internal static bool TryParseVector2List(string str, out List<Vector2> result)
        {
            bool success = true;
            string[] vectors = str.Split('|');

            result = new List<Vector2>();

            Vector2 temp;
            for (int i = 0; i < vectors.Length; ++i)
            {
                if (vectors[i].Length > 0)
                {
                    if (TryParseVector2(vectors[i], out temp))
                    {
                        result.Add(temp);
                    }
                    else
                    {
                        Log.e(LogTag.MESSAGEPARSER, "Failed to parse \"" + str.Trim() + "\" as a Queue<Vector2>");
                        success = false;
                        result.Clear();
                    }
                }
            }

            return success;
        }
    }
}
