using UnityEngine;
using UnityEngine.AI;
using Utilities;

namespace Robots
{
    public class RobotStateMove : RobotState
    {
        private readonly float speed;
        private readonly float tolerance = 0.05f;
        private readonly Vector2 targetPosition;

        private NavMeshAgent robotAgent;
        private Vector3 targetPosition3d;

        public RobotStateMove(Vector2 targetPosition, float speed)
        {
            this.speed = speed;
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
                targetPosition3d = new Vector3(targetPosition.x, r.body.transform.position.y, targetPosition.y);

                robotAgent = r.body.GetComponent<NavMeshAgent>();
                if (robotAgent == null)
                {
                    Log.a(LogTag.ROBOT, "Robot " + r.id + " does not have attached NavMeshAgent");
                    finished = true;
                }
            }

            ////////////////////////////////////////////////////////////////////////////////////////
            // Reset variables if robot is returning from another state
            ////////////////////////////////////////////////////////////////////////////////////////
            if (resume)
            {
                resume = false;
            }

            ////////////////////////////////////////////////////////////////////////////////////////
            // Update: check if robot has reached its destination (within specified tolerance)
            ////////////////////////////////////////////////////////////////////////////////////////
            if (robotAgent != null)
                robotAgent.SetDestination(targetPosition3d);

            Vector2 position2d = new Vector2(r.body.transform.position.x, r.body.transform.position.z);
            if (Vector2.Distance(position2d, targetPosition) <= tolerance)
                finished = true;

            ////////////////////////////////////////////////////////////////////////////////////////
            // Process messages: TODO
            ////////////////////////////////////////////////////////////////////////////////////////

            ////////////////////////////////////////////////////////////////////////////////////////
            // "Clean up" robot state
            ////////////////////////////////////////////////////////////////////////////////////////
            if (finished)
            {
                Log.d(LogTag.ROBOT, "Robot " + r.id + " has reached target position " + targetPosition);
                
                // Move robot onto the position exactly and reset its velocity
                r.body.transform.position = new Vector3(targetPosition.x, 
                                                        r.body.transform.position.y, 
                                                        targetPosition.y);
                r.body.GetComponent<Rigidbody>().AddForce(-1 * r.rigidbody.velocity, 
                                                          ForceMode.VelocityChange);

                // Pop state off the stack
                r.popState();
            }
        }
    }
}