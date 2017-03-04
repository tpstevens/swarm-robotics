using UnityEngine;
using UnityEngine.AI;

using Utilities;

namespace Robots
{
    public class RobotStateMove : RobotState
    {
        private readonly float speed;
        private readonly float tolerance = 0.01f;
        private readonly Vector3 targetPosition;

        private NavMeshAgent robotAgent;

        public RobotStateMove(Vector2 targetPosition, float speed)
        {
            this.speed = speed;
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
            }

            ////////////////////////////////////////////////////////////////////////////////////////
            // Reset variables if robot is returning from another state
            ////////////////////////////////////////////////////////////////////////////////////////
            if (resume)
            {
                resume = false;

                robotAgent = r.body.GetComponent<NavMeshAgent>();
                if (robotAgent == null)
                {
                    Log.a(LogTag.ROBOT, "Robot " + r.id + " does not have attached NavMeshAgent");
                    return;
                }
                else
                {
                    robotAgent.SetDestination(targetPosition);
                    robotAgent.avoidancePriority = 50;
                }
            }

            ////////////////////////////////////////////////////////////////////////////////////////
            // Update: check if robot has reached its destination (within specified tolerance)
            ////////////////////////////////////////////////////////////////////////////////////////
            if (!robotAgent.pathPending && robotAgent.remainingDistance < tolerance)
                finished = true;

            ////////////////////////////////////////////////////////////////////////////////////////
            // Process messages: no, should be handled in other states
            ////////////////////////////////////////////////////////////////////////////////////////

            ////////////////////////////////////////////////////////////////////////////////////////
            // "Clean up" robot state
            ////////////////////////////////////////////////////////////////////////////////////////
            if (finished)
            {
                Log.d(LogTag.ROBOT, "Robot " + r.id + " has reached target position " + targetPosition);

                // Other robots must navigate around it
                robotAgent.avoidancePriority = 0;

                // Pop state off the stack
                r.popState();
            }
        }
    }
}