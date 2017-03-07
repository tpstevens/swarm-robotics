using UnityEngine;

using System.Collections.Generic;

using CommSystem;
using Utilities;

namespace Robots
{
    public class RobotStateConstructionSatellite : RobotState
    {
        private enum ConstructionState {
            QUEUING = 0,
            FRONT_OF_QUEUE = 1,
            FETCHING_RESOURCE = 2,
            CARRYING_RESOURCE = 3,
            PLACING_RESOURCE = 4,
            RETURNING_QUEUE = 5,
            FINISHED = 6
        }

        private ConstructionState state;
        private Queue<Vector2> waitQueue;

        public RobotStateConstructionSatellite(Queue<Vector2> waitQueue = null)
        {
            if (waitQueue == null)
            {
                bool switchDirection = false;
                this.waitQueue = new Queue<Vector2>();

                // keep first point out of the way of the queue
                // this.waitQueue.Enqueue(new Vector2(-5, 0));

                for (int i = 0; i < 4; ++i)
                {
                    for (int j = 0; j < 2; ++j)
                    {
                        this.waitQueue.Enqueue(new Vector2(i * 2, (switchDirection ? 1 - j : j) * 10));
                    }

                    switchDirection = !switchDirection;
                }
            }
            else
            {
                this.waitQueue = new Queue<Vector2>(waitQueue);
            }
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
                state = ConstructionState.QUEUING;

                r.pushState(new RobotStateQueue(waitQueue, 2.0f));
            }

            ////////////////////////////////////////////////////////////////////////////////////////
            // Reset variables if robot is returning from another state
            ////////////////////////////////////////////////////////////////////////////////////////
            if (resume)
            {
                resume = false;
                Log.d(LogTag.ROBOT, "Robot " + r.id + " is returning to construction (satellite)");

                switch(state) // we just left this state
                {
                    case ConstructionState.QUEUING:
                    {
                        state = ConstructionState.FRONT_OF_QUEUE;

                        Comm.directMessage(r.id, Comm.SATELLITE, "construction\trequest_task");
                        break;
                    }
                    case ConstructionState.FETCHING_RESOURCE:
                    {
                        break;
                    }
                    case ConstructionState.FINISHED:
                    {
                        finished = true;
                        break;
                    }
                    default:
                    {
                        break;
                    }
                }
            }

            ////////////////////////////////////////////////////////////////////////////////////////
            // Update
            ////////////////////////////////////////////////////////////////////////////////////////

            ////////////////////////////////////////////////////////////////////////////////////////
            // Process messages: TODO
            ////////////////////////////////////////////////////////////////////////////////////////
            int count = r.unhandledMessages.Count;
            for (int i = 0; i < count; ++i)
            {
                bool handledMessage = false;
                CommMessage msg = r.unhandledMessages.Dequeue();

                if (msg.text.StartsWith("construction"))
                {
                    string[] lines = msg.text.Split('\t');
                    if (lines.Length <= 1)
                    {
                        Log.a(LogTag.ROBOT, "Robot " + r.id + " processed empty or single-line message during: " + msg.text);
                    }
                    else
                    {
                        switch (state)
                        {
                            case ConstructionState.FRONT_OF_QUEUE:
                            {
                                if (lines[1] == "finished")
                                {
                                    Log.d(LogTag.ROBOT, "Robot " + r.id + " has is finished with construction.");
                                    state = ConstructionState.FINISHED;
                                    r.pushState(new RobotStateMove(new Vector2(r.id * 2, -24.0f)));
                                }
                                else
                                {
                                    // TODO
                                }

                                break;
                            }
                            default:
                            {
                                break;
                            }
                        }
                    }
                }
                
                if (!handledMessage) // didn't process message, leave for next state
                {
                    r.unhandledMessages.Enqueue(msg);
                }
            }

            ////////////////////////////////////////////////////////////////////////////////////////
            // "Clean up" robot state
            ////////////////////////////////////////////////////////////////////////////////////////
            if (finished)
            {
                Log.d(LogTag.ROBOT, "Robot " + r.id + " has finished construction (satellite).");

                // Pop state off the stack
                r.popState();
            }
        }
    }
}
