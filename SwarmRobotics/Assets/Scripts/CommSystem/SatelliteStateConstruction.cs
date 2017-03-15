using UnityEngine;

using System.Collections.Generic;

using Messages;
using Utilities;

namespace CommSystem
{
    public class SatelliteStateConstruction
    {
        ////////////////////////////////////////////////////////////////////////
        // Private Classes
        ////////////////////////////////////////////////////////////////////////
        private class StructureLayer
        {
            public readonly List<Vector2> constructionPerimeter;
            public readonly Queue<Vector2> resourcePlacements;

            public StructureLayer(List<Vector2> constructionPerimeter,
                                  Queue<Vector2> resourcePlacements)
            {
                this.constructionPerimeter = constructionPerimeter;
                this.resourcePlacements = resourcePlacements;

                if (resourcePlacements.Count == 0)
                {
                    Log.a(LogTag.SATELLITE, "Attempted to initialize a StructureLayer with an empty set of resource placements.");
                }
            }
        }

        ////////////////////////////////////////////////////////////////////////
        // Member Variables
        ////////////////////////////////////////////////////////////////////////
        public readonly Queue<Vector2> resourceOrigins;
        public readonly Queue<Vector2> waitQueue;

        private int totalAvailableResources = 0;
        private int totalRequiredResources = 0;
        private MainInterface mainScript;
        private Queue<StructureLayer> structureLayers;
        private Satellite satellite;

        ////////////////////////////////////////////////////////////////////////
        // Constructor and Functions
        ////////////////////////////////////////////////////////////////////////
        public SatelliteStateConstruction(Satellite satellite, MainInterface mainScript)
        {
            this.mainScript = mainScript;
            this.satellite = satellite;

            structureLayers = new Queue<StructureLayer>();

            // Initialize resource origins
            List<Vector2> resourceOriginList;
            mainScript.getResourcePositions(out resourceOriginList);
            resourceOrigins = new Queue<Vector2>(resourceOriginList);
            totalAvailableResources = resourceOrigins.Count;

            // Initialize wait queue
            waitQueue = new Queue<Vector2>();
            bool switchDirection = false;
            for (int i = 0; i < 6; ++i, switchDirection = !switchDirection)
            {
                for (int j = 0; j < 2; ++j)
                    waitQueue.Enqueue(new Vector2(i * 2, 20 + (switchDirection ? 1 - j : j) * 10));
            }

            float placementSpacing = 1.5f;
            List<Vector2> constructionPerimeter;
            Queue<Vector2> resourcePlacements;

            // Initialize resource placements for first layer
            {
                placementSpacing = 1.0f;
                resourcePlacements = new Queue<Vector2>();
                Stack<Vector2> reversePlacements = new Stack<Vector2>();

                for (int i = -1; i < 2; ++i)
                    reversePlacements.Push(new Vector2(i * placementSpacing, placementSpacing));

                reversePlacements.Push(new Vector2(placementSpacing, 0));

                for (int i = 1; i >= -1; --i)
                    reversePlacements.Push(new Vector2(i * placementSpacing, -1 * placementSpacing));

                reversePlacements.Push(new Vector2(-1 * placementSpacing, 0));

                while (reversePlacements.Count > 0)
                    resourcePlacements.Enqueue(reversePlacements.Pop());
            }

            // Initialize construction perimeter for first layer
            {
                placementSpacing = 1.5f;
                constructionPerimeter = new List<Vector2>();
                constructionPerimeter.Add(new Vector2(-1 * placementSpacing, 3 * placementSpacing));
                constructionPerimeter.Add(new Vector2(-1 * placementSpacing, 2 * placementSpacing));
                constructionPerimeter.Add(new Vector2(2 * placementSpacing, 2 * placementSpacing));
                constructionPerimeter.Add(new Vector2(2 * placementSpacing, -2f * placementSpacing));
                constructionPerimeter.Add(new Vector2(-2 * placementSpacing, -2f * placementSpacing));
                constructionPerimeter.Add(new Vector2(-2 * placementSpacing, placementSpacing));
                constructionPerimeter.Add(new Vector2(-5 * placementSpacing, placementSpacing));
                constructionPerimeter.Add(new Vector2(-5 * placementSpacing, 5 * placementSpacing));
            }

            // Initialize first layer and add to layers list
            structureLayers.Enqueue(new StructureLayer(constructionPerimeter, resourcePlacements));
            totalRequiredResources += resourcePlacements.Count;

            // Initialize resource placements for the second layer
            {
                placementSpacing = 1.5f;
                resourcePlacements = new Queue<Vector2>();
                Stack<Vector2> reversePlacements = new Stack<Vector2>();

                for (int i = -2; i <= 2; ++i)
                    reversePlacements.Push(new Vector2(i * placementSpacing, 2 * placementSpacing));

                for (int i = 1; i >= -1; --i)
                    reversePlacements.Push(new Vector2(2 * placementSpacing, i * placementSpacing));

                for (int i = 2; i >= -2; --i)
                    reversePlacements.Push(new Vector2(i * placementSpacing, -2 * placementSpacing));

                for (int i = -1; i <= 1; ++i)
                    reversePlacements.Push(new Vector2(-2 * placementSpacing, i * placementSpacing));

                while (reversePlacements.Count > 0)
                    resourcePlacements.Enqueue(reversePlacements.Pop());
            }

            // Initialize construction perimeter for second layer
            {
                placementSpacing *= 2;
                constructionPerimeter = new List<Vector2>();
                constructionPerimeter.Add(new Vector2(-1 * placementSpacing, 3 * placementSpacing));
                constructionPerimeter.Add(new Vector2(-1 * placementSpacing, 2 * placementSpacing));
                constructionPerimeter.Add(new Vector2(2 * placementSpacing, 2 * placementSpacing));
                constructionPerimeter.Add(new Vector2(2 * placementSpacing, -2f * placementSpacing));
                constructionPerimeter.Add(new Vector2(-2 * placementSpacing, -2f * placementSpacing));
                constructionPerimeter.Add(new Vector2(-2 * placementSpacing, 2 * placementSpacing));
            }

            // Initialize second layer and add to layers list
            structureLayers.Enqueue(new StructureLayer(constructionPerimeter, resourcePlacements));
            totalRequiredResources += resourcePlacements.Count;

            // start construction
            satellite.broadcastMessage(new MessageConstructionStart(waitQueue).ToString());
        }

        public bool handleMessage(CommMessage msg)
        {
            bool handled = false;

            if (msg.text.StartsWith("construction"))
            {
                string[] lines = msg.text.Split('\n');
                if (lines[1] == "request_task")
                {
                    handled = true;

                    if (structureLayers.Count > 0 && resourceOrigins.Count > 0)
                    {
                        List<Vector2> constructionPerimeter = structureLayers.Peek().constructionPerimeter;
                        Vector2 resourceOrigin = resourceOrigins.Dequeue();
                        Vector2 resourcePlacement = structureLayers.Peek().resourcePlacements.Dequeue();

                        --totalRequiredResources;

                        if (structureLayers.Peek().resourcePlacements.Count == 0)
                        {
                            structureLayers.Dequeue();
                        }

                        bool lastTask = structureLayers.Count == 0 || resourceOrigins.Count == 0 || totalRequiredResources < mainScript.getNumRobots();
                        satellite.directMessage(msg.senderId,
                                                new MessageConstructionTask(resourceOrigin,
                                                                            constructionPerimeter,
                                                                            resourcePlacement,
                                                                            lastTask).ToString());
                    }
                    else
                    {
                        satellite.directMessage(msg.senderId, "construction/finished");
                    }
                }
            }

            return handled;
        }
    }
}
