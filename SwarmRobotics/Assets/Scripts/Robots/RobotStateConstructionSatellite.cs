using UnityEngine;

using System.Collections.Generic;

using CommSystem;
using Messages;
using Utilities;

namespace Robots
{
    public class RobotStateConstructionSatellite : RobotState
    {
        private enum ConstructionState {
            WAITING_FOR_INITIALIZATION,
            QUEUING,
            FRONT_OF_QUEUE,
            FETCHING_RESOURCE,
            CARRYING_RESOURCE,
            PLACING_RESOURCE,
            RETURNING_QUEUE,
            FINISHED
        }

        private ConstructionState state;
        private CommMessage initialCommand;
        private Queue<Vector2> waitQueue;

        public RobotStateConstructionSatellite(Robot r, CommMessage msg)
        {
            initialCommand = msg;
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
                state = ConstructionState.WAITING_FOR_INITIALIZATION;

                if (initialCommand.text.StartsWith(MessageConstructionStart.TAG))
                {
                    MessageConstructionStart msgData;
                    if (MessageConstructionStart.TryParse(initialCommand.text, out msgData))
                    {
                        waitQueue = msgData.waitQueue;
                        r.pushState(new RobotStateQueue(new Queue<Vector2>(waitQueue), 1.5f));
                        state = ConstructionState.QUEUING;
                        r.pushState(new RobotStateSleep(2 * r.id));
                    }
                    else
                    {
                        Log.a(LogTag.ROBOT, "Failed to parse " + MessageConstructionStart.TAG + " data.");
                        finished = true;
                    }
                }
                else
                {
                    Log.a(LogTag.ROBOT, "Failed to parse initial MessageConstructionStart");
                    finished = true;
                }
            }

            ////////////////////////////////////////////////////////////////////////////////////////
            // Reset variables if robot is returning from another state
            ////////////////////////////////////////////////////////////////////////////////////////
            if (resume)
            {
                resume = false;
                Log.d(LogTag.ROBOT, "Robot " + r.id + " is returning to construction (satellite)");

                switch(state) // resuming from this state
                {
                    case ConstructionState.QUEUING:
                    {
                        state = ConstructionState.FRONT_OF_QUEUE;

                        Comm.directMessage(r.id, Comm.SATELLITE, "construction\nrequest_task");
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

                // TODO: this if-block is more repugnant than someone kicking a puppy
                if (msg.text.StartsWith("construction"))
                {
                    handledMessage = true;

                    if (msg.text.StartsWith(MessageConstructionStart.TAG))
                    {
                        MessageConstructionStart msgData;
                        if (MessageConstructionStart.TryParse(msg.text, out msgData))
                        {
                            if (state == ConstructionState.WAITING_FOR_INITIALIZATION)
                            {
                                waitQueue = msgData.waitQueue;
                                r.pushState(new RobotStateQueue(new Queue<Vector2>(waitQueue), 2f));
                                state = ConstructionState.QUEUING;
                            }
                            else
                            {
                                Log.e(LogTag.ROBOT, "Received " + MessageConstructionStart.TAG + " message outside of WAITING_FOR_INITIALIZATION state");
                            }
                        }
                        else
                        {
                            Log.a(LogTag.ROBOT, "Failed to parse " + MessageConstructionStart.TAG + " data.");
                        }
                    }
                    else if (msg.text.StartsWith(MessageConstructionTask.TAG))
                    {
                        MessageConstructionTask msgData;
                        if (MessageConstructionTask.TryParse(msg.text, out msgData))
                        {
                            if (state == ConstructionState.FRONT_OF_QUEUE)
                            {
                                Queue<Vector2> startToPlacementPerimeter;
                                Queue<Vector2> placementToEndPerimeter;
                                calculateBreakPosition(msgData.constructionPerimeter, 
                                                       msgData.resourcePlacement, 
                                                       out startToPlacementPerimeter, 
                                                       out placementToEndPerimeter);

                                if (msgData.lastTask)
                                {
                                    r.pushState(new RobotStateMove(new Vector2(r.id * -1.5f, 31)));
                                    state = ConstructionState.FINISHED;
                                }
                                else
                                {
                                    r.pushState(new RobotStateQueue(new Queue<Vector2>(waitQueue), 2f));
                                    state = ConstructionState.QUEUING;
                                }

                                r.pushState(new RobotStateQueue(placementToEndPerimeter, 2f));
                                r.pushState(new RobotStatePlaceResource(msgData.resourcePlacement));
                                r.pushState(new RobotStateQueue(startToPlacementPerimeter, 2f));
                                r.pushState(new RobotStateMove(msgData.resourceOrigin + new Vector2(0, -10)));
                                r.pushState(new RobotStateRetrieveResource(msgData.resourceOrigin));
                            }
                            else
                            {
                                Log.e(LogTag.ROBOT, "Received " + MessageConstructionTask.TAG + " message outside of FRONT_OF_QUEUE state");
                            }
                        }
                        else
                        {
                            Log.a(LogTag.ROBOT, "Failed to parse " + MessageConstructionTask.TAG + " data.");
                        }
                    }
                    else if (msg.text == "construction/finished")
                    {
                        r.pushState(new RobotStateMove(new Vector2(r.id * -1.5f, 31)));
                        state = ConstructionState.FINISHED;
                    }
                    else
                    {
                        Log.a(LogTag.ROBOT, "Unknown construction message:\n" + msg.text);
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

        private void calculateBreakPosition(List<Vector2> constructionPerimeter,
                                            Vector2 resourcePlacement,
                                            out Queue<Vector2> startToPlacementPerimeter,
                                            out Queue<Vector2> placementToEndPerimeter)
        {
            float closestDistance = float.MaxValue;
            int closestSegmentIndex = -1;
            Vector2 closestSegmentBreak = new Vector2(float.MinValue, float.MinValue);

            for (int i = 0; i < constructionPerimeter.Count - 1; ++i)
            {
                Vector2 segmentBreak;

                float distance = calculateSegmentToPointDistance(constructionPerimeter[i],
                                                                 constructionPerimeter[i + 1],
                                                                 resourcePlacement,
                                                                 out segmentBreak);

                if (distance < closestDistance) // using '<=' instead of '<' should get us closer to the end, 
                                                // but doesn't match up nicely with how perimeter is defined
                {
                    closestDistance = distance;
                    closestSegmentIndex = i;
                    closestSegmentBreak = segmentBreak;
                }
            }

            startToPlacementPerimeter = new Queue<Vector2>();
            placementToEndPerimeter = new Queue<Vector2>();

            if (closestDistance != float.MaxValue)
            {
                for (int i = 0; i <= closestSegmentIndex; ++i)
                {
                    startToPlacementPerimeter.Enqueue(constructionPerimeter[i]);
                }

                startToPlacementPerimeter.Enqueue(closestSegmentBreak);
                placementToEndPerimeter.Enqueue(closestSegmentBreak);

                for (int i = closestSegmentIndex + 1; i < constructionPerimeter.Count; ++i)
                {
                    placementToEndPerimeter.Enqueue(constructionPerimeter[i]);
                }
            }
        }

        private float calculateSegmentToPointDistance(Vector2 a, Vector2 b, Vector2 point, out Vector2 segmentBreak)
        {
            float distance;
            Vector2 temp;

            if (a.y == b.y)
            {
                if (a.x > b.x)
                {
                    temp = a;
                    a = b;
                    b = temp;
                }

                if (point.x < a.x)
                {
                    distance = Vector2.Distance(a, point);
                    segmentBreak = a;
                }
                else if (point.x > b.x)
                {
                    distance = Vector2.Distance(b, point);
                    segmentBreak = b;
                }
                else
                {
                    distance = Mathf.Abs(a.y - point.y);
                    segmentBreak = new Vector2(point.x, a.y);
                }
            }
            else if (a.x == b.x)
            {
                if (a.y > b.y)
                {
                    temp = a;
                    a = b;
                    b = temp;
                }

                if (point.y < a.y)
                {
                    distance = Vector2.Distance(a, point);
                    segmentBreak = a;
                }
                else if (point.y > b.y)
                {
                    distance = Vector2.Distance(b, point);
                    segmentBreak = b;
                }
                else
                {
                    distance = Mathf.Abs(a.x - point.x);
                    segmentBreak = new Vector2(a.x, point.y);
                }
            }
            else
            {
                distance = float.MaxValue;
                segmentBreak = new Vector2(float.MinValue, float.MinValue);
                Log.a(LogTag.ROBOT, "Cannot calculate segment-to-point distance with non-simple segments");
            }

            return distance;
        }
    }
}
