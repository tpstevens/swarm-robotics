using UnityEngine;
using UnityEngine.AI;

using Utilities;

namespace Robots
{
    public class RobotStateMove : RobotState
    {
        private readonly float tolerance = 0.01f;
        private readonly float stoppingDistance;

        private NavMeshAgent robotAgent;
        private Vector3 targetPosition;

        public RobotStateMove(Vector2 targetPosition, float stoppingDistance = -1f)
        {
            this.stoppingDistance = stoppingDistance;
            this.targetPosition = new Vector3(targetPosition.x, 0, targetPosition.y);
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

                targetPosition.y = r.body.transform.position.y;
            }

            ////////////////////////////////////////////////////////////////////////////////////////
            // Reset variables if robot is returning from another state
            ////////////////////////////////////////////////////////////////////////////////////////
            if (resume)
            {
                resume = false;

                robotAgent = r.body.GetComponent<NavMeshAgent>();
                robotAgent.speed = r.VELOCITY;

                if (robotAgent == null)
                {
                    Log.a(LogTag.ROBOT, "Robot " + r.id + " does not have attached NavMeshAgent");
                    return;
                }
                else
                {
                    robotAgent.SetDestination(targetPosition);
                    robotAgent.avoidancePriority = (int)r.id + 1;
                }
            }

            ////////////////////////////////////////////////////////////////////////////////////////
            // Update: check if robot has reached its destination (within specified tolerance)
            ////////////////////////////////////////////////////////////////////////////////////////
            if (!robotAgent.pathPending && robotAgent.remainingDistance == 0)
            {
                if (Vector3.Distance(targetPosition, r.body.transform.position) < tolerance)
                {
                    finished = true;
                }
                else
                {
                    Log.d(LogTag.ROBOT, "Robot " + r.id + " cannot reach its destination. Waiting for path to open up.");
                    robotAgent.SetDestination(targetPosition);
                    r.pushState(new RobotStateSleep(1.0f));
                }
            }
            else if (stoppingDistance > 0 && Vector3.Distance(targetPosition, r.body.transform.position) < stoppingDistance)
            {
                finished = true;
                robotAgent.SetDestination(r.body.transform.position);
            }

            ////////////////////////////////////////////////////////////////////////////////////////
            // Process messages: no, should be handled in other states
            ////////////////////////////////////////////////////////////////////////////////////////

            ////////////////////////////////////////////////////////////////////////////////////////
            // "Clean up" robot state
            ////////////////////////////////////////////////////////////////////////////////////////
            if (finished)
            {
                // Log.d(LogTag.ROBOT, "Robot " + r.id + " has reached target position " + targetPosition);

                // Other robots must navigate around it
                robotAgent.avoidancePriority = 0;

                // Pop state off the stack
                r.popState();
            }
        }
    }
}