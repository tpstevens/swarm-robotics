using System.Collections.Generic;
using UnityEngine;

namespace Messages
{
    public sealed class MessageBuildTask
    {
        public static readonly string TAG = "build/task_assignment";

        public readonly bool lastTask;
        public readonly Vector2 resourceOrigin;
        public readonly Vector2 resourcePlacement;

        public MessageBuildTask(Vector2 resourceOrigin,
                                Vector2 resourcePlacement,
                                bool lastTask)
        {
            this.lastTask = lastTask;
            this.resourceOrigin = resourceOrigin;
            this.resourcePlacement = resourcePlacement;
        }

        public static bool TryParse(string str, out MessageBuildTask msg)
        {
            bool lastTask = false;
            bool result = false;
            List<string> formattedLines = new List<string>();
            string[] lines = str.Split('\n');
            Vector2 resourceOrigin = new Vector2(float.MinValue, float.MinValue);
            Vector2 resourcePlacement = resourceOrigin;

            for (int i = 0; i < lines.Length; ++i)
            {
                string s = lines[i].Trim();
                if (s.Length > 0)
                    formattedLines.Add(s);
            }

            result = (formattedLines.Count == 4)
                     && Parser.TryParseVector2(formattedLines[1], out resourceOrigin)
                     && Parser.TryParseVector2(formattedLines[2], out resourcePlacement)
                     && Parser.TryParseBool(formattedLines[3], out lastTask);

            msg = new MessageBuildTask(resourceOrigin, resourcePlacement, lastTask);

            return result;
        }

        public override string ToString()
        {
            string msg = TAG + "\n";

            // Add resource origin to line 1
            msg += resourceOrigin + "\n";

            // Add resource placement to line 2
            msg += "\n" + resourcePlacement;

            // Add last task flag to line 3
            msg += "\n" + lastTask.ToString();

            return msg;
        }
    }
}
