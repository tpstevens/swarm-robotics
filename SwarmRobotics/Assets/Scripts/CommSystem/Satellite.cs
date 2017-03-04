using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
            unhandledMessages.Enqueue(msg);
        }

        public void update()
        {
            // TODO: handle messages
        }
    }
}
