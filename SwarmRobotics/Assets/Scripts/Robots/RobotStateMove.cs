using UnityEngine;
using UnityEngine.AI;

using Utilities;

namespace Robots
{
    public class RobotStateMove : RobotState
    {
        private readonly float tolerance = 0.01f;
        private readonly float stoppingDistance;
        
        private bool hasRotated = false;
        private NavMeshAgent robotAgent;
        private Vector3 targetPosition;

        public RobotStateMove(Vector2 targetPosition, float stoppingDistance = -1f)
        {
            this.stoppingDistance = stoppingDistance;
            this.targetPosition = new Vector3(targetPosition.x, 0, targetPosition.y);
        }

        public RobotStateMove(Vector3 targetPosition, float stoppingDistance = -1f)
        {
            this.stoppingDistance = stoppingDistance;
            this.targetPosition = targetPosition;
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

                robotAgent = r.body.GetComponent<NavMeshAgent>();

                if (robotAgent != null)
                {
                    robotAgent.avoidancePriority = (int)r.id + 1;
                    robotAgent.speed = r.VELOCITY;
                }
                else
                {
                    Log.a(LogTag.ROBOT, "Robot " + r.id + " does not have attached NavMeshAgent");
                    return;
                }

                targetPosition.y = r.body.transform.position.y;
            }

            ////////////////////////////////////////////////////////////////////////////////////////
            // Reset variables if robot is returning from another state
            ////////////////////////////////////////////////////////////////////////////////////////
            if (resume)
            {
                resume = false;

                if (!hasRotated)
                {
                    robotAgent.updateRotation = false;
                    r.pushState(new RobotStateTurn(new Vector2(targetPosition.x, targetPosition.z)));
                    hasRotated = true;
                    return;
                }
                else
                {
                    robotAgent.updateRotation = true;
                    robotAgent.SetDestination(targetPosition);
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
                    // robotAgent.SetDestination(targetPosition);
                    r.pushState(new RobotStateSleep(1.0f));
                }
            }
            else if (stoppingDistance > 0 && Vector3.Distance(targetPosition, r.body.transform.position) < stoppingDistance)
            {
                finished = true;
                robotAgent.SetDestination(r.body.transform.position);
                robotAgent.updateRotation = false;
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