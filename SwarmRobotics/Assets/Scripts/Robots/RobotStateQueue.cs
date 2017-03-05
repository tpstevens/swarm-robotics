using UnityEngine;

using System.Collections.Generic;

using Utilities;

namespace Robots
{
    public class RobotStateQueue : RobotState
    {
        private bool enteredQueue;
        private bool waiting;
        private float nextSpacing;
        private float queueSpacing;
        private Queue<Vector2> queueWaypoints;
        private Vector2 lastWaypoint; // used to determine direction the robot will travel
        private Vector2 nextPosition; // used to determine which spot the robot will occupy next

        public RobotStateQueue(Queue<Vector2> queueWaypoints, float queueSpacing)
        {
            this.queueWaypoints = queueWaypoints;
            this.queueSpacing = queueSpacing;

            enteredQueue = false;
            waiting = false;
        }

        /// <summary>
        /// Called every frame from Robot.update() if it's the current state (top of the stack)
        /// </summary>
        /// <param name="r">The robot to update</param>
        public override void update(Robot r)
        {
            bool finished = false;
            Vector2 currentPosition = new Vector2(r.body.transform.position.x,
                                                  r.body.transform.position.z);

            ////////////////////////////////////////////////////////////////////////////////////////
            // Initialize variables if necessary when first enter state
            ////////////////////////////////////////////////////////////////////////////////////////
            if (!initialized)
            {
                initialized = true;
                resume = true;

                lastWaypoint = currentPosition;
                nextPosition = queueWaypoints.Peek();
            }

            ////////////////////////////////////////////////////////////////////////////////////////
            // Reset variables if robot is returning from another state
            ////////////////////////////////////////////////////////////////////////////////////////
            if (resume)
            {
                resume = false;
                
                if (Vector2.Distance(currentPosition, nextPosition) < 0.01f || waiting)
                {
                    if (!enteredQueue)
                    {
                        Log.d(LogTag.ROBOT, "Robot " + r.id + " has entered the queue at " + queueWaypoints.Peek());
                        enteredQueue = true;
                    }

                    if (Vector2.Distance(currentPosition, queueWaypoints.Peek()) < 0.1f)
                    {
                        lastWaypoint = queueWaypoints.Dequeue();

                        if (queueWaypoints.Count == 0)
                        {
                            finished = true;
                        }
                    }
                    
                    if (!finished)
                    {
                        if (!waiting)
                        {
                            nextSpacing = Mathf.Min(Vector2.Distance(currentPosition, queueWaypoints.Peek()), queueSpacing);

                            if (nextSpacing < queueSpacing)
                            {
                                nextPosition = queueWaypoints.Peek();
                            }
                            else
                            {
                                Vector2 nextDirection = queueWaypoints.Peek() - lastWaypoint;
                                nextDirection.Normalize();
                                nextPosition = currentPosition + queueSpacing * nextDirection;
                            }
                        }

                        int layerMask = 1 << 8;
                        RaycastHit hitInfo;
                        Vector2 direction = nextPosition - currentPosition;
                        Vector3 nextDir3d = new Vector3(direction.x, 0, direction.y);
                        
                        if (!Physics.Raycast(r.body.transform.position, nextDir3d, out hitInfo, nextSpacing, layerMask))
                        {
                            r.pushState(new RobotStateMove(nextPosition));
                            waiting = false;
                        }
                        else
                        {
                            r.pushState(new RobotStateSleep(0.1f));
                            waiting = true;
                        }
                    }
                }
                else
                {
                    Log.d(LogTag.ROBOT, "Robot " + r.id + " is travelling to the the queue at " + queueWaypoints.Peek());
                    nextPosition = queueWaypoints.Peek();
                    r.pushState(new RobotStateMove(nextPosition));
                }
            }

            ////////////////////////////////////////////////////////////////////////////////////////
            // Update: TODO wait for other robots
            ////////////////////////////////////////////////////////////////////////////////////////

            ////////////////////////////////////////////////////////////////////////////////////////
            // Process messages: TODO
            ////////////////////////////////////////////////////////////////////////////////////////

            ////////////////////////////////////////////////////////////////////////////////////////
            // "Clean up" robot state
            ////////////////////////////////////////////////////////////////////////////////////////
            if (finished)
            {
                Log.d(LogTag.ROBOT, "Robot " + r.id + " has reached the front of the queue at " + lastWaypoint);
                r.popState();
            }
        }
    }
}
