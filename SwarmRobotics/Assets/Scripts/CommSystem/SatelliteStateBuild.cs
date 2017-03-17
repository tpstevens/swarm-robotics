using UnityEngine;

using System.Collections.Generic;

using Messages;
using Utilities;

namespace CommSystem
{
    public class SatelliteStateBuild
    {
        ////////////////////////////////////////////////////////////////////////
        // Member Variables
        ////////////////////////////////////////////////////////////////////////
        public readonly Queue<Vector2> waitQueue;
        public readonly string args;

        private int robotsInResourceCache = 0;
        private int totalRequiredResources = 0;
        private MainInterface mainScript;
        private Queue<CommMessage> pendingRequests;
        private Queue<Vector2> resourceOrigins;
        private Queue<Vector2> resourcePlacements;
        private Satellite satellite;

        ////////////////////////////////////////////////////////////////////////
        // Constructor and Functions
        ////////////////////////////////////////////////////////////////////////
        public SatelliteStateBuild(Satellite satellite, MainInterface mainScript, string args)
        {
            this.args = args;
            this.mainScript = mainScript;
            this.satellite = satellite;

            pendingRequests = new Queue<CommMessage>();

            // Initialize resource origins
            List<Vector2> resourceOriginList;
            mainScript.getResourcePositionsInCache(out resourceOriginList);
            resourceOriginList.Sort(delegate (Vector2 a, Vector2 b) {
                float difference;

                if (a.x == b.x)
                {
                    difference = a.y - b.y;
                }
                else
                {
                    difference = a.x - b.x;
                }

                return (difference == 0) ? 0 : ((difference > 0) ? 1 : -1);
            });

            resourceOrigins = new Queue<Vector2>(resourceOriginList);

            // Initialize wait queue
            waitQueue = new Queue<Vector2>();
            bool switchDirection = false;
            for (int i = 0; i < 6; ++i, switchDirection = !switchDirection)
            {
                for (int j = 0; j < 2; ++j)
                    waitQueue.Enqueue(new Vector2(i * 2, 20 + (switchDirection ? 1 - j : j) * 10));
            }

            if (args.StartsWith("\"") && args.EndsWith("\""))
            {
                resourcePlacements = new Queue<Vector2>(Words.getBuildOrder(args.Substring(1, args.Length - 2)));
            }
            else
            {
                resourcePlacements = new Queue<Vector2>();
            }

            Vector2 xMin = new Vector2(float.MaxValue, 15f);
            Vector2 xMax = new Vector2(float.MinValue, 15f);

            foreach (Vector2 v in resourcePlacements)
            {
                if (v.x < xMin.x)
                    xMin.x = v.x;

                if (v.x > xMax.x)
                    xMax.x = v.x;
            }

            xMax.x += 1.5f;
            xMin.x -= 1.5f;
            
            totalRequiredResources = resourcePlacements.Count;

            // start construction
            if (totalRequiredResources > 0)
                satellite.broadcastMessage(new MessageBuildStart(waitQueue, xMin, xMax).ToString());
        }

        public bool handleMessage(CommMessage msg)
        {
            bool handled = false;

            if (msg.text.StartsWith("build"))
            {
                string[] lines = msg.text.Split('\n');
                if (lines[1] == "request_task")
                {
                    handled = true;

                    if (totalRequiredResources > 0)
                    {
                        if (resourceOrigins.Count > 0)
                        {
                            --totalRequiredResources;
                            ++robotsInResourceCache;

                            bool lastTask = totalRequiredResources < mainScript.getNumRobots();
                            satellite.directMessage(msg.senderId,
                                                    new MessageBuildTask(resourceOrigins.Dequeue(),
                                                                         resourcePlacements.Dequeue(),
                                                                         lastTask).ToString());
                        }
                        else // must generate more resources
                        {
                            Log.w(LogTag.SATELLITE, "adding to pending requests");
                            pendingRequests.Enqueue(msg);
                        }
                    }
                    else
                    {
                        satellite.directMessage(msg.senderId, "build/finished");
                    }
                }
                else if (lines[1] == "left_cache")
                {
                    --robotsInResourceCache;

                    if (pendingRequests.Count > 0 && robotsInResourceCache == 0)
                    {
                        // place more resources in cache and refresh resourceOrigins
                        resourceOrigins = new Queue<Vector2>(mainScript.refillResourceCache());

                        while (pendingRequests.Count > 0)
                        {
                            --totalRequiredResources;
                            ++robotsInResourceCache;

                            CommMessage requestMessage = pendingRequests.Dequeue();
                            bool lastTask = totalRequiredResources < mainScript.getNumRobots();
                            satellite.directMessage(requestMessage.senderId,
                                                    new MessageBuildTask(resourceOrigins.Dequeue(),
                                                                         resourcePlacements.Dequeue(),
                                                                         lastTask).ToString());
                        }
                    }
                }
            }

            return handled;
        }
    }
}
