using UnityEngine;
using UnityEngine.AI;

using System.Collections.Generic;

using Utilities;

namespace Robots
{
    public class RobotStateQueue : RobotState
    {
        private bool enteredQueue;
        private float robotSpacing;
        private Queue<Vector2> queueWaypoints;
        private Vector2 lastWaypoint; // used to determine direction the robot will travel
        private Vector2 nextWaypoint; // used to determine which spot the robot will occupy next

        public RobotStateQueue(Queue<Vector2> queueWaypoints, float robotSpacing)
        {
            this.queueWaypoints = queueWaypoints;
            this.robotSpacing = robotSpacing;

            enteredQueue = false;
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
                nextWaypoint = queueWaypoints.Peek();

                if (robotSpacing < r.body.transform.localScale.x)
                {
                    Log.a(LogTag.ROBOT, "Cannot initialize queue with robot spacing that's less than 2x robot radius!");
                }
            }

            ////////////////////////////////////////////////////////////////////////////////////////
            // Reset variables if robot is returning from another state
            ////////////////////////////////////////////////////////////////////////////////////////
            if (resume)
            {
                resume = false;
                
                if (!enteredQueue)
                {
                    Log.d(LogTag.ROBOT, "Robot " + r.id + " is travelling to the the queue at " + queueWaypoints.Peek());
                    enteredQueue = true;
                    nextWaypoint = queueWaypoints.Peek();
                    r.pushState(new RobotStateMove(nextWaypoint));
                }
                else
                {
                    if (!enteredQueue)
                    {
                        enteredQueue = true;
                    }

                    if (Vector2.Distance(currentPosition, nextWaypoint) < 0.1f)
                    {
                        lastWaypoint = queueWaypoints.Dequeue();

                        if (queueWaypoints.Count == 0)
                        {
                            finished = true;
                        }
                        else
                        {
                            nextWaypoint = queueWaypoints.Peek();
                        }
                    }
                    
                    if (!finished)
                    {
                        bool moveToDestination = true;
                        float distanceToWaypoint = Vector2.Distance(currentPosition, nextWaypoint);
                        int layerMask = 1 << 8; // check Robot layer only
                        RaycastHit hitInfo;
                        Vector2 nextDestination = nextWaypoint;
                        Vector2 dir2d = nextDestination - currentPosition;
                        Vector3 dir3d = new Vector3(dir2d.x, 0, dir2d.y);
                        
                        if (Physics.Raycast(r.body.transform.position, dir3d, out hitInfo, distanceToWaypoint, layerMask))
                        {
                            float distanceBetweenRobots = hitInfo.distance + r.body.transform.localScale.x / 2.0f;
                            if (distanceBetweenRobots > robotSpacing)
                            {
                                dir2d.Normalize();
                                float nextDestinationDistance = distanceBetweenRobots - robotSpacing;
                                nextDestination = nextWaypoint - dir2d * (distanceToWaypoint - nextDestinationDistance);
                            }
                            else
                            {
                                moveToDestination = false;
                            }
                        }

                        if (moveToDestination)
                        {
                            r.pushState(new RobotStateMove(nextDestination));
                        }
                        else
                        {
                            r.pushState(new RobotStateSleep(0.1f));
                        }
                    }
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
