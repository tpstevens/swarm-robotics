using UnityEngine;

using System.Collections.Generic;

using Utilities;

namespace CommSystem
{
    public class Comm
    {
        public static readonly uint SATELLITE = uint.MaxValue - 1;
        public static readonly uint RECEIVER_ALL = uint.MaxValue; // default receiver for broadcasts

        // Instance variable and mutex
        private static volatile Comm comm;
        private static object mutex = new object();

        // Parameters read from Config
        private readonly float MSG_DIST_LIMIT;   // message propagation limit, in m
        private readonly float MSG_FAILURE_RATE; // what % of messages will fail? 0 -> 1 (unused)
        private readonly float MSG_SPEED;        // message propagation speed, in m/s
        private bool showInUnityConsole;         // whether message send events appear in Unity console
        private bool showMsgIndicators;          // whether message indicators are rendered
        private int maxNumRobots;                // maximum number of robots a message could reach

        // Private cached objects
        private readonly GameObject msgIndicatorTemplate; // template for message indicators
        private readonly GameObject msgIndicatorObjects;  // parent of all message indicators
        private readonly MainInterface mainScript;        // cached reference to Main

        // Private objects
        private Dictionary<uint, CommMessage> activeMsgs;
        private Dictionary<uint, GameObject> activeMsgIndicators;
        private List<uint> msgsToBeDeleted; // TODO could create separate lists for physics thread that are
                                            // joined to this one in a single mutex lock, copy, and clear
        private List<KeyValuePair<uint,uint>> msgsToBeDelivered;
        private object listMutex = new object();
        private uint nextMsgId = 0;

        ////////////////////////////////////////////////////////////////////////
        // Public Static Methods
        ////////////////////////////////////////////////////////////////////////

        public static void broadcastMessage(uint senderId, string text)
        {
            broadcastMessage(senderId, 0, "", text);
        }

        public static void broadcastMessage(uint senderId, uint channel, string text)
        {
            broadcastMessage(senderId, channel, "", text);
        }

        public static void broadcastMessage(uint senderId, string tag, string text)
        {
            broadcastMessage(senderId, 0, tag, text);
        }

        public static void broadcastMessage(uint senderId, uint channel, string tag, string text)
        {
            bool validSenderPosition = true;
            Comm comm = Instance();
            Vector3 msgOrigin;

            if (senderId == SATELLITE)
            {
                if (!comm.mainScript.getSatellitePosition(out msgOrigin))
                {
                    Log.e(LogTag.COMM, "Failed to get satellite's position.");
                    validSenderPosition = false;
                }
            }
            else if (!comm.mainScript.getRobotPosition(senderId, out msgOrigin))
            {
                Log.e(LogTag.COMM, "Failed to get Robot " + senderId + "'s position.");
                validSenderPosition = false;
            }

            if (validSenderPosition)
            {
                uint msgId = comm.nextMsgId++;
                CommMessage msg = new CommMessage(msgId, senderId,
                                                  channel, tag,
                                                  msgOrigin, comm.MSG_DIST_LIMIT, comm.MSG_SPEED,
                                                  text);
                GameObject msgIndicator = comm.instantiateMsgIndicator(msg);

                comm.activeMsgs.Add(msgId, msg);
                comm.activeMsgIndicators.Add(msgId, msgIndicator);

                Log.d(LogTag.COMM, msg.ToString(), comm.showInUnityConsole);
            }
        }

        public static void directMessage(uint senderId, uint receiverId, string text)
        {
            bool validSenderPosition = true;
            Comm comm = Instance();
            Vector3 msgOrigin;

            if (senderId == SATELLITE)
            {
                if (!comm.mainScript.getSatellitePosition(out msgOrigin))
                {
                    Log.e(LogTag.COMM, "Failed to get satellite's position.");
                    validSenderPosition = false;
                }
            }
            else if (!comm.mainScript.getRobotPosition(senderId, out msgOrigin))
            {
                Log.e(LogTag.COMM, "Failed to get Robot " + senderId + "'s position.");
                validSenderPosition = false;
            }

            if (validSenderPosition)
            {
                uint msgId = comm.nextMsgId++;
                CommMessage msg = new CommMessage(msgId, senderId, receiverId, 
                                                 msgOrigin, comm.MSG_DIST_LIMIT, comm.MSG_SPEED,
                                                 text);
                GameObject msgIndicator = comm.instantiateMsgIndicator(msg);

                comm.activeMsgs.Add(msgId, msg);
                comm.activeMsgIndicators.Add(msgId, msgIndicator);

                Log.d(LogTag.COMM, msg.ToString(), comm.showInUnityConsole);
            }
        }

        /// <summary>
        /// Clear the comm without writing it to the file.
        /// </summary>
        public static void clear()
        {
            lock (mutex)
            {
                comm = null;
            }
        }

        public static void notifyDeletion(uint msgId)
        {
            Comm comm = Instance();

            lock(comm.listMutex)
            {
                comm.msgsToBeDeleted.Add(msgId);
            }
        }

        public static void notifyDelivery(uint msgId, uint receiverId)
        {
            Comm comm = Instance();

            lock (comm.listMutex)
            {
                comm.msgsToBeDelivered.Add(new KeyValuePair<uint, uint>(msgId, receiverId));
            }
        }

        public static void toggleShowMsgIndicators()
        {
            Comm comm = Instance();
            comm.showMsgIndicators = !comm.showMsgIndicators;
            
            foreach (GameObject g in comm.activeMsgIndicators.Values)
            {
                Renderer r = g.GetComponent<Renderer>();
                if (r != null)
                    r.enabled = comm.showMsgIndicators;
            }

            Log.w(LogTag.COMM, "Set showMessageIndicators=" + comm.showMsgIndicators);
        }

        public static void toggleShowInUnityConsole()
        {
            Comm comm = Instance();
            comm.showInUnityConsole = !comm.showInUnityConsole;
            Log.w(LogTag.COMM, "Set showInUnityConsole=" + comm.showInUnityConsole);
        }

        public static void update(float frameTime)
        {
            Comm comm = Instance();
            Vector3 robotPosition;

            lock(comm.listMutex)
            {
                // Update direct message travel distances
                CommMessage msg;
                GameObject msgIndicator;
                foreach (KeyValuePair<uint, CommMessage> p in comm.activeMsgs)
                {
                    if (p.Value.receiverId != Comm.RECEIVER_ALL)
                    {
                        msg = p.Value;
                        msg.distanceTraveled += frameTime * msg.propagationSpeed;

                        if (comm.mainScript.getRobotPosition(msg.receiverId, out robotPosition)
                            && Vector3.Distance(msg.origin, robotPosition) <= msg.distanceTraveled)
                        {
                            comm.msgsToBeDelivered.Add(new KeyValuePair<uint, uint>(msg.id, msg.receiverId));
                            comm.msgsToBeDeleted.Add(msg.id);
                        }

                        if (comm.activeMsgIndicators.TryGetValue(msg.id, out msgIndicator))
                        {
                            Vector3 msgScale = new Vector3();
                            msgScale.x = msg.distanceTraveled * 2.0f;
                            msgScale.y = msg.distanceTraveled * 2.0f;
                            msgScale.z = msg.distanceTraveled * 2.0f;
                            msgIndicator.transform.localScale = msgScale;
                        }
                    }
                }

                // Deliver messages that have reached (one of) their receiver(s)
                foreach (KeyValuePair<uint, uint> p in comm.msgsToBeDelivered) // TODO specify what the key and value is
                {
                    comm.deliverMessage(p.Key, p.Value);
                }

                comm.msgsToBeDelivered.Clear();

                // Delete direct messages that have been delivered to all recipients, or any message
                // that has exceeded its travel distance
                GameObject g;
                foreach (uint id in comm.msgsToBeDeleted)
                {
                    comm.activeMsgs.Remove(id);
                    if (comm.activeMsgIndicators.TryGetValue(id, out g))
                    {
                        comm.activeMsgIndicators.Remove(id);
                        Object.Destroy(g);
                    }
                } 
            }
        }

        ////////////////////////////////////////////////////////////////////////
        // Private Methods
        ////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Return the instance of the Comm singleton, creating it if necessary.
        /// </summary>
        /// <returns>The instance of Comm.</returns>
        private static Comm Instance()
        {
            if (comm == null)
            {
                lock (mutex)
                {
                    comm = new Comm(); // Failure in constructor will be handled by Log.a()
                }
            }

            return comm;
        }

        private Comm()
        {
            GameObject mainObject = GameObject.Find("Scripts");
            if (mainObject != null)
            {
                mainScript = mainObject.GetComponent<MainInterface>();
                if (mainScript != null)
                {
                    activeMsgs = new Dictionary<uint, CommMessage>();
                    activeMsgIndicators = new Dictionary<uint, GameObject>();
                    msgsToBeDeleted = new List<uint>();
                    msgsToBeDelivered = new List<KeyValuePair<uint, uint>>();
                    maxNumRobots = mainScript.getNumRobots();

                    msgIndicatorTemplate = retrieveMsgIndicatorTemplate();

                    GameObject DebugObjects = GameObject.Find("Debug");
                    if (DebugObjects == null)
                        DebugObjects = new GameObject("Debug");

                    msgIndicatorObjects = new GameObject("Message Indicators");
                    msgIndicatorObjects.transform.parent = DebugObjects.transform;

                    Config config = mainScript.getCurrentConfig();

                    MSG_DIST_LIMIT = config.CommMsgDistanceLimit;
                    MSG_FAILURE_RATE = 0.0f; // (unused)
                    MSG_SPEED = config.CommMsgSpeed;

                    showInUnityConsole = config.CommShowInUnityConsole;
                    showMsgIndicators = config.CommShowMsgIndicators;
                }
                else
                {
                    Log.a(LogTag.COMM, "Failed to initialize Comm: main script not found.");
                }
            }
            else
            {
                Log.a(LogTag.COMM, "Failed to initialize Comm: Scripts header not found.");
            }
        }

        private void deliverMessage(uint messageId, uint receiverId)
        {
            CommMessage msg;
            if (activeMsgs.TryGetValue(messageId, out msg))
            {
                comm.mainScript.notifyMessage(receiverId, msg);
                string type = (msg.receiverId == RECEIVER_ALL ? "broadcast" : "direct");
                string recipient = (receiverId == SATELLITE) ? "Satellite" : ("Robot " + receiverId);

                Log.d(LogTag.COMM, 
                      "Delivering " + type + " message " + messageId + " to " + recipient,
                      showInUnityConsole);
            }
            else
            {
                Log.e(LogTag.COMM, "Failed to deliver message " + messageId);
            }
        }

        private GameObject retrieveMsgIndicatorTemplate()
        {
            GameObject template = mainScript.getMessageIndicatorPrefab();

            if (template != null)
            {
                // set other properties here?
            }
            else
            {
                Log.e(LogTag.COMM, "Creating message indicator template in code");

                template = GameObject.CreatePrimitive(PrimitiveType.Cylinder);

                template.transform.localScale = new Vector3(0.0f, 0.001f, 0.0f);
                template.transform.hideFlags = HideFlags.HideInHierarchy;
                template.layer = ApplicationManager.LAYER_MESSAGES;

                template.GetComponent<Renderer>().material.color = Color.cyan;
            }

            return template;
        }

        private GameObject instantiateMsgIndicator(CommMessage msg)
        {
            bool broadcast = msg.receiverId == Comm.RECEIVER_ALL;

            GameObject msgIndicator = GameObject.Instantiate(Instance().msgIndicatorTemplate);
            msgIndicator.hideFlags = HideFlags.None;
            msgIndicator.transform.position = msg.origin;

            if (!broadcast)
            {
                // Remove collider because Comm will calculate collisions manually
                Object.Destroy(msgIndicator.GetComponentInChildren<Collider>());
                Object.Destroy(msgIndicator.GetComponentInChildren<CommMessageBroadcast>());
            }
            else
            {
                int numRobots = (msg.senderId == SATELLITE) ? maxNumRobots : maxNumRobots - 1;
                msgIndicator.GetComponentInChildren<CommMessageBroadcast>().initialize(msg, numRobots);
            }

            msgIndicator.name = "Message " + msg.id;
            msgIndicator.transform.parent = msgIndicatorObjects.transform;
            msgIndicator.GetComponent<Renderer>().enabled = comm.showMsgIndicators;

            return msgIndicator;
        }
    }
}

/**
 * TODO:
 * - different colors of message indicators for different types of messages? Channels, etc.
 * - special ACK type message?
 */