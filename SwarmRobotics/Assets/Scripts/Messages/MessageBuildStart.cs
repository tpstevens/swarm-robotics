using System.Collections.Generic;
using UnityEngine;

namespace Messages
{
    public sealed class MessageBuildStart
    {
        public static readonly string TAG = "build/start";

        public readonly Queue<Vector2> waitQueue;
        public readonly Vector2 xMax, xMin;

        public MessageBuildStart(Queue<Vector2> waitQueue, Vector2 xMin, Vector2 xMax)
        {
            this.waitQueue = waitQueue;
            this.xMax = xMax;
            this.xMin = xMin;
        }

        public static bool TryParse(string str, out MessageBuildStart msg)
        {
            bool result = false;
            List<string> formattedLines = new List<string>();
            List<Vector2> waitQueueList = new List<Vector2>();
            string[] lines = str.Split('\n');
            Vector2 xMax = new Vector2(float.MinValue, float.MinValue);
            Vector2 xMin= xMax;

            for (int i = 0; i < lines.Length; ++i)
            {
                string s = lines[i].Trim();
                if (s.Length > 0)
                    formattedLines.Add(s);
            }

            result = (formattedLines.Count == 4)
                     && Parser.TryParseVector2(formattedLines[1], out xMin)
                     && Parser.TryParseVector2(formattedLines[2], out xMax)
                     && Parser.TryParseVector2List(formattedLines[3], out waitQueueList);

            msg = new MessageBuildStart(new Queue<Vector2>(waitQueueList), xMin, xMax);

            return result;
        }

        public override string ToString()
        {
            string msg = TAG + "\n";

            msg += xMin + "\n";
            msg += xMax + "\n";

            // Add wait queue to line 3
            foreach (Vector2 v in waitQueue)
            {
                msg += v + "|";
            }

            return msg.Trim();
        }
    }
}
