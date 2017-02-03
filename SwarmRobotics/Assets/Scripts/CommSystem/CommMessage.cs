using UnityEngine;

namespace CommSystem
{
    public class CommMessage
    {
        public readonly float distanceLimit;        // how far messages travel before dissipating
        public readonly float propagationSpeed;     // how fast messages travel, in m/s
        public readonly float sendTime;             // in seconds (time since level load)
        public readonly string tag = "";
        public readonly string text;
        public readonly uint channel = 0;           // default 0 for broadcast/point-to-point
        public readonly uint id;
        public readonly uint receiverId = Comm.RECEIVER_ALL; // default (Comm.ALL) is broadcast message
        public readonly uint senderId;
        public readonly Vector3 origin;

        public float distanceTraveled = 0.0f;

        public CommMessage(uint id, uint senderId,
                           uint channel, string tag, 
                           Vector3 origin, float distanceLimit, float propagationSpeed, 
                           string text)
        {
            this.id = id;
            this.senderId = senderId;
            this.channel = channel;
            this.tag = tag;
            this.origin = origin;
            this.distanceLimit = distanceLimit;
            this.propagationSpeed = propagationSpeed;
            this.text = text;

            sendTime = Time.timeSinceLevelLoad;
        }

        public CommMessage(uint id, uint senderId, uint receiverId,
                           Vector3 origin, float distanceLimit, float propagationSpeed, 
                           string text)
        {
            this.id = id;
            this.senderId = senderId;
            this.receiverId = receiverId;
            this.origin = origin;
            this.distanceLimit = distanceLimit;
            this.propagationSpeed = propagationSpeed;
            this.text = text;

            sendTime = Time.timeSinceLevelLoad;
        }

        public override string ToString()
        {
            string metadata = "";

            if (receiverId != Comm.RECEIVER_ALL)
            {
                metadata = string.Format("{0} : x{1} -> {2} : ",
                                         "(x" + id.ToString("X4") + ")",
                                         senderId.ToString("D4"),
                                         receiverId.ToString("D4"));
            }
            else
            {
                metadata = string.Format("{0} : x{1} -> channel({2}) tag({3}) : ",
                                         "(x" + id.ToString("X4") + ")",
                                         senderId.ToString("D4"),
                                         channel,
                                         tag);
            }

            return metadata + text;
        }
    }
}
