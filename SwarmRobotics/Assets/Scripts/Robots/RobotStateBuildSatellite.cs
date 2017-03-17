using UnityEngine;

using System.Collections.Generic;

using CommSystem;
using Messages;
using Utilities;

namespace Robots
{
    public class RobotStateBuildSatellite : RobotState
    {
        private enum BuildState
        {
            WAITING_FOR_INITIALIZATION,
            QUEUING,
            FRONT_OF_QUEUE,
            FETCHING_RESOURCE,
            CARRYING_RESOURCE,
            PLACING_RESOURCE,
            RETURNING_QUEUE,
            FINISHED
        }

        private BuildState state;
        private CommMessage initialCommand;
        private Queue<Vector2> waitQueue;
        private Vector2 xMax, xMin;
        private Vector2 perimeterBreak, perimeterMin, perimeterMax;

        public RobotStateBuildSatellite(Robot r, CommMessage msg)
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
                state = BuildState.WAITING_FOR_INITIALIZATION;

                if (initialCommand.text.StartsWith(MessageBuildStart.TAG))
                {
                    MessageBuildStart msgData;
                    if (MessageBuildStart.TryParse(initialCommand.text, out msgData))
                    {
                        waitQueue = msgData.waitQueue;
                        xMax = msgData.xMax;
                        xMin = msgData.xMin;

                        r.pushState(new RobotStateQueue(new Queue<Vector2>(waitQueue), 1.5f));
                        state = BuildState.QUEUING;
                        r.pushState(new RobotStateSleep(2 * r.id));
                    }
                    else
                    {
                        Log.a(LogTag.ROBOT, "Failed to parse " + MessageBuildStart.TAG + " data.");
                        finished = true;
                    }
                }
                else
                {
                    Log.a(LogTag.ROBOT, "Failed to parse initial MessageBuildStart");
                    finished = true;
                }
            }

            ////////////////////////////////////////////////////////////////////////////////////////
            // Reset variables if robot is returning from another state
            ////////////////////////////////////////////////////////////////////////////////////////
            if (resume)
            {
                resume = false;

                switch (state) // resuming from this state
                {
                case BuildState.QUEUING:
                    {
                        state = BuildState.FRONT_OF_QUEUE;
                        Comm.directMessage(r.id, Comm.SATELLITE, "build\nrequest_task");
                        break;
                    }
                case BuildState.FINISHED:
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
                if (msg.text.StartsWith("build"))
                {
                    handledMessage = true;

                    if (msg.text.StartsWith(MessageBuildStart.TAG))
                    {
                        MessageBuildStart msgData;
                        if (MessageBuildStart.TryParse(msg.text, out msgData))
                        {
                            if (state == BuildState.WAITING_FOR_INITIALIZATION)
                            {
                                waitQueue = msgData.waitQueue;
                                xMax = msgData.xMax;
                                xMin = msgData.xMin;

                                r.pushState(new RobotStateQueue(new Queue<Vector2>(waitQueue), 1.5f));
                                state = BuildState.QUEUING;
                                r.pushState(new RobotStateSleep(2 * r.id));
                            }
                            else
                            {
                                Log.e(LogTag.ROBOT, "Received " + MessageBuildStart.TAG + " message outside of WAITING_FOR_INITIALIZATION state");
                            }
                        }
                        else
                        {
                            Log.a(LogTag.ROBOT, "Failed to parse " + MessageBuildStart.TAG + " data.");
                        }
                    }
                    else if (msg.text.StartsWith(MessageBuildTask.TAG))
                    {
                        MessageBuildTask msgData;
                        if (MessageBuildTask.TryParse(msg.text, out msgData))
                        {
                            if (state == BuildState.FRONT_OF_QUEUE)
                            {
                                perimeterMax = new Vector2(xMax.x, msgData.resourcePlacement.y - 4);
                                perimeterMin = new Vector2(xMin.x, msgData.resourcePlacement.y - 4);
                                perimeterBreak = new Vector2(msgData.resourcePlacement.x, perimeterMax.y);

                                Queue<Vector2> placementToEndPerimeter = new Queue<Vector2>();
                                placementToEndPerimeter.Enqueue(perimeterBreak);
                                placementToEndPerimeter.Enqueue(perimeterMin);

                                Queue<Vector2> startToPlacementPerimeter = new Queue<Vector2>();
                                startToPlacementPerimeter.Enqueue(perimeterMax);
                                startToPlacementPerimeter.Enqueue(perimeterBreak);

                                if (msgData.lastTask)
                                {
                                    r.pushState(new RobotStateMove(new Vector2(r.id * -1.5f, 31)));
                                    state = BuildState.FINISHED;
                                }
                                else
                                {
                                    r.pushState(new RobotStateQueue(new Queue<Vector2>(waitQueue), 2f));
                                    state = BuildState.QUEUING;
                                }

                                r.pushState(new RobotStateMove(xMin));
                                r.pushState(new RobotStateQueue(placementToEndPerimeter, 2f, 0.1f));
                                r.pushState(new RobotStatePlaceResource(msgData.resourcePlacement));
                                r.pushState(new RobotStateQueue(startToPlacementPerimeter, 2f, 0.1f));
                                r.pushState(new RobotStateSendMessage("build\nleft_cache", Comm.SATELLITE));
                                r.pushState(new RobotStateMove(xMax));
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
                    else if (msg.text == "build/finished")
                    {
                        r.pushState(new RobotStateMove(new Vector2(r.id * -1.5f, 31)));
                        state = BuildState.FINISHED;
                    }
                    else
                    {
                        Log.a(LogTag.ROBOT, "Unknown build message:\n" + msg.text);
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
                Log.d(LogTag.ROBOT, "Robot " + r.id + " has finished build (satellite).");

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
