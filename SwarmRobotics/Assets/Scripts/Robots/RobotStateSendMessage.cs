using UnityEngine;

using System.Collections.Generic;

using CommSystem;
using Utilities;

namespace Robots
{
    public class RobotStateSendMessage : RobotState
    {
        private string text;
        private uint receiverId;

        public RobotStateSendMessage(string text, uint receiverId)
        {
            this.receiverId = receiverId;
            this.text = text;
        }

        /// <summary>
        /// Called every frame from Robot.update() if it's the current state (top of the stack)
        /// </summary>
        /// <param name="r">The robot to update</param>
        public override void update(Robot r)
        {
            bool finished = false;

            ////////////////////////////////////////////////////////////////////////////////////////
            // Initialize variables if necessary when first enter state
            ////////////////////////////////////////////////////////////////////////////////////////
            if (!initialized)
            {
                initialized = true;
                resume = true;
            }

            ////////////////////////////////////////////////////////////////////////////////////////
            // Reset variables if robot is returning from another state
            ////////////////////////////////////////////////////////////////////////////////////////
            if (resume)
            {
                Log.e(LogTag.ROBOT, "sent message " + text);

                if (receiverId == Comm.RECEIVER_ALL)
                    Comm.broadcastMessage(r.id, text);
                else
                    Comm.directMessage(r.id, receiverId, text);

                finished = true;
            }

            ////////////////////////////////////////////////////////////////////////////////////////
            // Update: intentionally empty
            ////////////////////////////////////////////////////////////////////////////////////////

            ////////////////////////////////////////////////////////////////////////////////////////
            // Process messages: intentionally empty
            ////////////////////////////////////////////////////////////////////////////////////////

            ////////////////////////////////////////////////////////////////////////////////////////
            // "Clean up" robot state
            ////////////////////////////////////////////////////////////////////////////////////////
            if (finished)
            {
                r.popState();
            }
        }
    }
}
