using System.Collections.Generic;
using UnityEngine;

namespace Messages
{
    public sealed class MessageConstructionStart
    {
        public static readonly string TAG = "construction/start";

        public readonly Queue<Vector2> waitQueue;

        public MessageConstructionStart(Queue<Vector2> waitQueue)
        {
            this.waitQueue = waitQueue;
        }

        public static bool TryParse(string str, out MessageConstructionStart msg)
        {
            bool result = false;
            List<string> formattedLines = new List<string>();
            List<Vector2> waitQueueList = new List<Vector2>();
            string[] lines = str.Split('\n');

            for (int i = 0; i < lines.Length; ++i)
            {
                string s = lines[i].Trim();
                if (s.Length > 0)
                    formattedLines.Add(s);
            }

            result = (formattedLines.Count == 2)
                     && Parser.TryParseVector2List(formattedLines[1], out waitQueueList);

            msg = new MessageConstructionStart(new Queue<Vector2>(waitQueueList));

            return result;
        }

        public override string ToString()
        {
            string msg = TAG + "\n";

            // Add wait queue to line 1
            foreach (Vector2 v in waitQueue)
            {
                msg += v + "|";
            }

            return msg.Trim();
        }
    }
}
