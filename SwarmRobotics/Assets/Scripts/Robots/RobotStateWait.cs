using UnityEngine;

using System.Collections.Generic;

using CommSystem;
using Utilities;

namespace Robots
{
    public class RobotStateWait : RobotState
    {
        public RobotStateWait()
        {
            // Intentionally empty
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
                resume = false;
                Log.d(LogTag.ROBOT, "Robot " + r.id + " is waiting");
            }

            ////////////////////////////////////////////////////////////////////////////////////////
            // Update: intentionally empty
            ////////////////////////////////////////////////////////////////////////////////////////

            ////////////////////////////////////////////////////////////////////////////////////////
            // Process messages: TODO
            ////////////////////////////////////////////////////////////////////////////////////////
            while (r.unhandledMessages.Count > 0)
            {
                CommMessage msg = r.unhandledMessages.Dequeue();
                if (msg.senderId == Comm.SATELLITE)
                {
                    if (msg.text.StartsWith("construction/start"))
                    {
                        r.pushState(new RobotStateConstructionSatellite(r, msg));
                    }
                    else if (msg.text == "test")
                    {
                        Log.w(LogTag.ROBOT, "Robot " + r.id + " performing test.");
                        r.pushState(new RobotStateMove(Vector2.zero));
                        r.pushState(new RobotStateSleep(0.5f));
                        r.pushState(new RobotStateMove(new Vector2(0, 5)));
                        r.pushState(new RobotStateSleep(0.5f * r.id));
                    }
                    else if (msg.text == "test_queue")
                    {
                        Queue<Vector2> waypointQueue = new Queue<Vector2>();
                        bool switchDirection = false;

                        waypointQueue.Enqueue(new Vector2(0, -2)); // to attempt to avoid blocking the queue start

                        for (int i = 0; i < 4; ++i)
                        {
                            for (int j = 0; j < 2; ++j)
                            {
                                if (switchDirection)
                                    waypointQueue.Enqueue(new Vector2(i * 2, (1 - j) * 10));
                                else
                                    waypointQueue.Enqueue(new Vector2(i * 2, j * 10));
                            }

                            switchDirection = !switchDirection;
                        }

                        r.pushState(new RobotStateQueue(waypointQueue, 2.0f));
                    }
                    else
                    {
                        Log.w(LogTag.ROBOT, "Robot " + r.id + " processed unknown message " + msg.text + " from " + msg.senderId);
                    }
                }
                else
                {
                    Log.w(LogTag.ROBOT, "Robot " + r.id + " processed unknown message " + msg.text + " from " + msg.senderId);
                }
            }

            ////////////////////////////////////////////////////////////////////////////////////////
            // "Clean up" robot state
            ////////////////////////////////////////////////////////////////////////////////////////
            if (finished)
            {
                // Intentionally empty because this should never happen
            }
        }
    }
}
