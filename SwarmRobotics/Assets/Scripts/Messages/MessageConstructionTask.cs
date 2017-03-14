using System.Collections.Generic;
using UnityEngine;

namespace Messages
{
    public sealed class MessageConstructionTask 
    {
        public static readonly string TAG = "construction/task_assignment";

        public readonly bool lastTask;
        public readonly List<Vector2> constructionPerimeter;
        public readonly Vector2 resourceOrigin;
        public readonly Vector2 resourcePlacement;

        public MessageConstructionTask(Vector2 resourceOrigin,
                                       List<Vector2> constructionPerimeter,
                                       Vector2 resourcePlacement,
                                       bool lastTask)
        {
            this.lastTask = lastTask;
            this.constructionPerimeter = constructionPerimeter;
            this.resourceOrigin = resourceOrigin;
            this.resourcePlacement = resourcePlacement;
        }

        public static bool TryParse(string str, out MessageConstructionTask msg)
        {
            bool lastTask = false;
            bool result = false;
            List<string> formattedLines = new List<string>();
            List<Vector2> constructionPerimeter = new List<Vector2>();
            string[] lines = str.Split('\n');
            Vector2 resourceOrigin = new Vector2(float.MinValue, float.MinValue);
            Vector2 resourcePlacement = resourceOrigin;

            for (int i = 0; i < lines.Length; ++i)
            {
                string s = lines[i].Trim();
                if (s.Length > 0)
                    formattedLines.Add(s);
            }

            result = (formattedLines.Count == 5)
                     && Parser.TryParseVector2(formattedLines[1], out resourceOrigin)
                     && Parser.TryParseVector2List(formattedLines[2], out constructionPerimeter)
                     && Parser.TryParseVector2(formattedLines[3], out resourcePlacement)
                     && Parser.TryParseBool(formattedLines[4], out lastTask);

            msg = new MessageConstructionTask(resourceOrigin, constructionPerimeter, resourcePlacement, lastTask);

            return result;
        }

        public override string ToString()
        {
            string msg = TAG + "\n";

            // Add resource origin to line 2
            msg += resourceOrigin + "\n";

            // Add construction perimeter to line 3
            foreach (Vector2 v in constructionPerimeter)
            {
                msg += v + "|";
            }

            // Add resource placement to line 4
            msg += "\n" + resourcePlacement;

            msg += "\n" + lastTask.ToString();

            return msg;
	    }
    }
}
