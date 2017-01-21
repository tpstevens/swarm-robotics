using UnityEngine;

using System;
using System.Collections.Generic;

namespace Utilities
{
    public class CommMessage
    {
        public readonly uint messageId;
        public readonly int senderId, receiverId;
        public readonly string messageText;
        public readonly Vector3 origin;
        public readonly float sendTime;

        public float distanceTraveled = 0.0f;

        public CommMessage(uint messageId,
                           int senderId,
                           int receiverId,
                           string messageText,
                           Vector3 origin)
        {
            this.messageId = messageId;
            this.senderId = senderId;
            this.receiverId = receiverId;
            this.messageText = messageText;
            this.origin = origin;

            sendTime = Time.timeSinceLevelLoad;
        }

        // TODO add message ID
        public string ToFormattedString(int idLength, bool hexadecimal = false)
        {
            string ids;

            if (hexadecimal)
            {
                string receiverText;
                if (receiverId != Comm.ALL)
                    receiverText = "x" + receiverId.ToString("X4");
                else
                    receiverText = "ALL";

                ids = string.Format("{0,6} : x{1,4} -> {2,5} : ",
                                    "(x" + messageId.ToString("X4") + ")",
                                    senderId.ToString("X4"),
                                    receiverText);
            }
            else
            {
                int maxReceiverLength = Math.Max(3, idLength);
                ids = string.Format("{0,6} : {1," + idLength + "} -> "
                                        + "{2," + maxReceiverLength + "} : ",
                                    "(" + messageId.ToString() + ")",
                                    senderId.ToString(),
                                    receiverId != Comm.ALL ? receiverId.ToString() : "ALL");
            }

            return ids + messageText;
        }
    }

    public class Comm
    {
        public static int ALL = -1;


        private static volatile Comm comm;
        private static object mutex = new object();

        private readonly float MSG_SPEED = 10.0f; // message propagation speed, in m/s
        private readonly int numRobotIdDigits;
        private readonly MainInterface mainScript;

        private bool showInConsole;
        private LinkedList<CommMessage> activeMessages;
        private uint nextMessageId;

        private Comm()
        {
            showInConsole = true;
            nextMessageId = 0;
            activeMessages = new LinkedList<CommMessage>();

            GameObject mainObject = GameObject.Find("Scripts");
            if (mainObject != null)
            {
                mainScript = (MainInterface)mainObject.GetComponent(typeof(MainInterface));
                if (mainScript != null)
                {
                    string largestRobotId = (mainScript.getNumRobots() - 1).ToString();
                    numRobotIdDigits = largestRobotId.Length;

                    Config config = mainScript.getCurrentConfig();
                    MSG_SPEED = config.CommMessageSpeed;
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

        /// <summary>
        /// Register a message to be sent.
        /// </summary>
        /// <param name="senderId">The ID of the sender.</param>
        /// <param name="receiverId">
        ///     The ID of the intended recipient, or Comm.ALL for a broadcast.
        /// </param>
        /// <param name="messageText">The text of the message to be sent.</param>
        public static void sendMessage(int senderId, int receiverId, string messageText)
        {
            Vector3 origin;

            Comm comm = Instance();
            if (comm.mainScript.getRobotPosition(senderId, out origin))
            {
                CommMessage msg = new CommMessage(comm.nextMessageId++,
                                                  senderId,
                                                  receiverId,
                                                  messageText,
                                                  origin);
                comm.activeMessages.AddLast(msg);
                Log.d(LogTag.COMM, 
                      msg.ToFormattedString(comm.numRobotIdDigits), 
                      comm.showInConsole);
            }
        }

        public static void toggleShowInConsole()
        {
            Comm comm = Instance();
            comm.showInConsole = !comm.showInConsole;
            Log.w(LogTag.COMM, "Set showInConsole=" + comm.showInConsole);
        }

        public static void update(float frameTime)
        {
            // TODO: optimize so message checking and removal happen in the same pass, or at
            //       the very most in O(2n). Iterate in reverse if using List.removeAt(int i).

            Comm comm = Instance();
            List<CommMessage> toBeRemoved = new List<CommMessage>();
            Vector3 robotPosition;

            // Do we need to enforce first-in, first-out? Shouldn't matter...
            foreach (CommMessage msg in comm.activeMessages)
            {
                if (msg.receiverId != ALL)
                {
                    msg.distanceTraveled += frameTime * comm.MSG_SPEED;
                    if (comm.mainScript.getRobotPosition(msg.receiverId, out robotPosition))
                    {
                        if (Vector3.Distance(msg.origin, robotPosition) <= msg.distanceTraveled)
                        {
                            toBeRemoved.Add(msg);
                            comm.mainScript.notifyMessage(msg.receiverId, msg);

                            float fTime = Time.timeSinceLevelLoad - msg.sendTime;
                            float fDistance = msg.distanceTraveled;
                            string sTime = "time = " + fTime.ToString("F3");
                            string sDistance = "distance = " +  fDistance.ToString("F3");

                            string messageId = string.Format("{0,6}", "(" + msg.messageId + ")");
                            Log.d(LogTag.COMM, 
                                  messageId + " : delivered (" + sTime + ", " + sDistance + ")",
                                  comm.showInConsole);
                        }
                    }
                }
            }

            for (int i = toBeRemoved.Count - 1; i >= 0; --i)
            {

                comm.activeMessages.Remove(toBeRemoved[i]);
            }
        }

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
    }
}

/**
 * Could implement a message retransmission protocol for transmitting messages near the edge of
 * their range? Set an extra bit in the message to allow signal boosting (the sender should set
 * this if they don't receive an ACK in time. Use the same message ID so if the receiver did 
 * actually receive the message, they can ignore duplicates) by nearby robots, like a broadcast
 * but with a specific receiver. Implement some way for robots to delete their message transmission
 * caches and start over with messageId = 0.
 * 
 * Specify multiple receivers with Comm.ALL?
 * 
 * Add messages to Log? Add config options to show in Unity console and to show in written logs?
 * If don't show in written logs, should Log.writeToFile() automatically put them in their own
 * log?
 * 
 * Add timestamp to CommMessage.
 * 
 * Add configuration options for instant messages, message processing delay, message propagation
 * delay, etc.
 * 
 * Add visual display of message propagation. Filters for senders and receivers?
 * 
 * Periodically write messages to a file? Don't let Log handle it (add bool to disable Log storage)?
 * 
 * Implement multiple arrays of messages, and active messages in a list by int
 * When array gets too long, add a new array. When number of active messages in an array reaches 0, 
 * release memory and write to a file.
 * 
 * Implement protocol for 3-way handshake? Tie into state machine with callbacks to next state when
 * message is confirmed as delivered?
 */
