using UnityEngine;

using System.Collections.Generic;

using Messages;
using Utilities;

namespace CommSystem
{
    public class Satellite
    {
        public GameObject body;

        private MainInterface mainScript;
        private Queue<CommMessage> unhandledMessages;
        private List<Vector2> constructionPerimeter;
        private Queue<Vector2> resourceOrigins;
        private Queue<Vector2> resourcePlacements;
        private Queue<Vector2> waitQueue;

        public Satellite(GameObject body, MainInterface mainScript)
        {
            this.body = body;
            this.mainScript = mainScript;

            unhandledMessages = new Queue<CommMessage>();
        }

        public void directMessage(uint receiverId, string text)
        {
            Comm.directMessage(Comm.SATELLITE, receiverId, text);
        }

        public void broadcastMessage(string text)
        {
            Comm.broadcastMessage(Comm.SATELLITE, text);
        }

        public Vector3 getPosition()
        {
            if (body != null)
                return body.transform.position;

            return Vector3.zero;
        }

        /// <summary>
        /// Notify the satellite that it has receieved a message. The message will be added to
        /// the end of the queue and handled inside the update() function, not here.
        /// </summary>
        /// <param name="msg"></param>
        public void queueMessage(CommMessage msg)
        {
            Log.d(LogTag.COMM, "satellite.queueMessage: " + msg);
            unhandledMessages.Enqueue(msg);
        }

        public void startConstruction()
        {
            List<Vector2> resourceOriginList;
            mainScript.getResourcePositions(out resourceOriginList);
            resourceOrigins = new Queue<Vector2>(resourceOriginList);

            float placementSpacing = 1.5f;
            resourcePlacements = new Queue<Vector2>();
            for (int i = -1; i < 2; ++i)
            {
                resourcePlacements.Enqueue(new Vector2(i * placementSpacing, placementSpacing));
                resourcePlacements.Enqueue(new Vector2(i * placementSpacing, -1 * placementSpacing));
                if (i != 0)
                    resourcePlacements.Enqueue(new Vector2(i * placementSpacing, 0));
            }

            constructionPerimeter = new List<Vector2>();
            constructionPerimeter.Add(new Vector2(-1 * placementSpacing, 2 * placementSpacing));
            constructionPerimeter.Add(new Vector2(2 * placementSpacing, 2 * placementSpacing));
            constructionPerimeter.Add(new Vector2(2 * placementSpacing, -2f * placementSpacing));
            constructionPerimeter.Add(new Vector2(-2 * placementSpacing, -2f * placementSpacing));
            constructionPerimeter.Add(new Vector2(-2 * placementSpacing, 2 * placementSpacing));

            waitQueue = new Queue<Vector2>();
            bool switchDirection = false;
            for (int i = 0; i < 6; ++i, switchDirection = !switchDirection)
            {
                for (int j = 0; j < 2; ++j)
                    waitQueue.Enqueue(new Vector2(i * 2, 20 + (switchDirection ? 1 - j : j) * 10));
            }

            broadcastMessage(new MessageConstructionStart(waitQueue).ToString());
        }

        public void update()
        {
            while (unhandledMessages.Count > 0)
            {
                CommMessage msg = unhandledMessages.Dequeue();

                if (msg.text.StartsWith("construction"))
                {
                    string[] lines = msg.text.Split('\n');
                    if (lines[1] == "request_task")
                    {
                        if (resourcePlacements.Count > 0 && resourceOrigins.Count > 0)
                        {
                            bool lastTask = resourceOrigins.Count <= mainScript.getNumRobots() || resourcePlacements.Count <= mainScript.getNumRobots();
                            directMessage(msg.senderId,
                                          new MessageConstructionTask(resourceOrigins.Dequeue(),
                                                                      constructionPerimeter,
                                                                      resourcePlacements.Dequeue(),
                                                                      lastTask).ToString());
                        }
                        else
                        {
                            directMessage(msg.senderId, "construction/finished");
                        }
                    }
                }
            }
        }
    }
}
