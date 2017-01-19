using UnityEngine;

using System;

namespace Utilities
{
    public class Comm
    {
        private class CommMessage
        {
            public readonly uint messageId;
            public readonly int senderId, receiverId;
            public readonly string messageText;
            public readonly Vector3 origin;

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
            }

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

                    ids = string.Format("x{0,4} -> {1,5} : ",
                                        senderId.ToString("X4"),
                                        receiverText);
                }
                else
                {
                    int maxReceiverLength = Math.Max(3, idLength);
                    ids = string.Format("{0," + idLength + "} -> {1," + maxReceiverLength + "} : ", 
                                        senderId.ToString(),
                                        receiverId != Comm.ALL ? receiverId.ToString() : "ALL");
                }

                return ids + messageText;
            }
        }

        public static int ALL = -1;

        private static volatile Comm comm;
        private static object mutex = new object();

        private MainInterface mainScript;
        private uint nextMessageId;
        private int numRobotIdDigits;

        private Comm()
        {
            nextMessageId = 0;

            GameObject mainObject = GameObject.Find("Scripts");
            if (mainObject != null)
            {
                mainScript = (MainInterface)mainObject.GetComponent(typeof(MainInterface));
                if (mainScript != null)
                {
                    string largestRobotId = (mainScript.getNumRobots() - 1).ToString();
                    numRobotIdDigits = largestRobotId.Length;
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
                Log.w(LogTag.COMM, msg.ToFormattedString(comm.numRobotIdDigits));
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
 */
