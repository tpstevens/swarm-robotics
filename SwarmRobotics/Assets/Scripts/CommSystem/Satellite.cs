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
        private SatelliteStateConstruction constructionState = null;

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
            if (constructionState == null)
            {
                constructionState = new SatelliteStateConstruction(this, mainScript);
            }
            else
            {
                Log.e(LogTag.SATELLITE, "Satellite already in construction state.");
            }
        }

        public void update()
        {
            while (unhandledMessages.Count > 0)
            {
                CommMessage msg = unhandledMessages.Dequeue();

                if (msg.text.StartsWith("construction") && constructionState != null)
                {
                    constructionState.handleMessage(msg);
                }
            }
        }
    }
}
