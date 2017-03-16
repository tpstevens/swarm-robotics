using UnityEngine;

using System.Collections;
using System.Collections.Generic;

using Utilities;

namespace CommSystem
{
    public class SatelliteStateForaging
    {
        private Vector2 currDest, oldDest;
        private Vector2 nextHomeLocation, location;
        private List<Vector2> untouchedResources;
        private List<Vector2> touchedResources;
        private HashSet<Vector2> aboutToBeTouchedResources;
        private Dictionary<uint, Vector2> robotResourceMap;
        private Queue<Vector2> resourcePlacements;
        private bool foraging = false;
        
        private Vector3 robotPosition;
        private Dictionary<uint, Vector3> robotPositions;
        private uint index;

        private Vector2 RH_location;
        private Vector3 return_to_base;
        private bool firstTimex = true;
        private bool firstTimey = true;

        private Satellite satellite;
        private MainInterface mainScript;

        public SatelliteStateForaging(Satellite satellite, MainInterface mainScript)
        {
            resourcePlacements = new Queue<Vector2>();

            untouchedResources = new List<Vector2>();
            touchedResources = new List<Vector2>();
            aboutToBeTouchedResources = new HashSet<Vector2>();
            resourcePlacements.Enqueue(new Vector2(29.5f, 29.5f));
            robotPositions = new Dictionary<uint, Vector3>();
            robotResourceMap = new Dictionary<uint, Vector2>();

            this.satellite = satellite;
            this.mainScript = mainScript;

            mainScript.getResourcePositions(out untouchedResources);
            scanForRobots(mainScript.getCurrentConfig());
            foraging = true;
            satellite.broadcastMessage("foraging\tstart");
        }

        public void handleMessage(CommMessage msg)
        {
            //for foraging
            if (msg.text == "resource_request")
            {
                if (untouchedResources.Count > 0)
                {
                    location = untouchedResources[0];
                    robotResourceMap[msg.senderId] = location;
                    satellite.directMessage(msg.senderId, "resource_location\t" + "" + location);
                    aboutToBeTouchedResources.Add(location);
                    untouchedResources.Remove(location);
                }
                else
                {
                    return_to_base = robotPositions[msg.senderId];
                    satellite.directMessage(msg.senderId, "go_to_base\t" + "" + return_to_base);
                }
            }
            if (msg.text == "resource_home_request")
            {
                RH_location = resourceDestenation();
                satellite.directMessage(msg.senderId, "resource_home\t" + "" + RH_location);
            }
            if (msg.text == "resource_delivered")
            {
                Vector2 resourceLocation = robotResourceMap[msg.senderId];
                //remove resource from pending
                aboutToBeTouchedResources.Remove(resourceLocation);
                //add resource to touchedResources
                touchedResources.Add(resourceLocation);
                if (untouchedResources.Count > 0)
                {
                    location = untouchedResources[0];
                    robotResourceMap[msg.senderId] = location;
                    satellite.directMessage(msg.senderId, "resource_location\t" + "" + location);
                    aboutToBeTouchedResources.Add(location);
                    untouchedResources.Remove(location);
                }
                else
                {
                    return_to_base = robotPositions[msg.senderId];
                    satellite.directMessage(msg.senderId, "go_to_base\t" + "" + return_to_base);
                }
            }
            // when untouchedResources and aboutToBeTouchedResources are empty, satellite will exit the foraging state
            if (untouchedResources == null && aboutToBeTouchedResources == null)
            {
                foraging = false;
            }
        }

        public bool isForaging()
        {
            return foraging;
        }

        private Vector2 resourceDestenation()
        {
            currDest = resourcePlacements.Dequeue();
            Vector2 test1 = currDest;
            oldDest = new Vector2(29.5f, 29.5f);

            if ((currDest.x - 2) < 20)
            {
                if (currDest.y - 2 < 20)
                {
                    Debug.Log("the resource home is full");
                    currDest.x = oldDest.x;
                    currDest.y = oldDest.y - 20f;
                }
                //oldDest = currDest;
                currDest.y = currDest.y - 2f;
                currDest.x = 29f;
            }
            else if (firstTimex && firstTimey)
            {
                currDest.x = currDest.x - 0.5f;
                currDest.y = currDest.y - 0.5f;
                firstTimex = false;
                firstTimey = false;
            }
            else
            {
                oldDest = new Vector2(29.5f, 29.5f);
                currDest.x = currDest.x - 2f;
            }
            resourcePlacements.Enqueue(currDest);
            return currDest;
        }

        private void scanForRobots(Config config)
        {
            for (index = 0; index < config.NumRobots; index++)
            {
                mainScript.getRobotPosition(index, out robotPosition);
                robotPositions[index] = robotPosition;
                Debug.Log("robotPositions[index]: " + robotPositions[index] + "\n");
            }
        }
    }
}
