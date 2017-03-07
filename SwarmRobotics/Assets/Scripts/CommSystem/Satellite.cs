using UnityEngine;

using System.Collections.Generic;

using Utilities;

namespace CommSystem
{
    public class Satellite
    {
        public GameObject body;

        private Queue<CommMessage> unhandledMessages;

        public Satellite(GameObject body)
        {
            this.body = body;

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

        public void update()
        {
            while (unhandledMessages.Count > 0)
            {
                CommMessage msg = unhandledMessages.Dequeue();

                if (msg.text.StartsWith("construction"))
                {
                    string[] lines = msg.text.Split('\t');
                    if (lines[1] == "request_task")
                    {
                        directMessage(msg.senderId, "construction\tfinished");
                    }
                }
            }
        }
    }
}
