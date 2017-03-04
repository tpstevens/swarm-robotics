using UnityEngine;

using Utilities;

namespace Robots
{
    public class RobotStateMove : RobotState
    {
        private readonly float speed;
        private readonly float tolerance = 0.05f;
        private readonly Vector2 targetPosition;

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
                resume = true;  // Move state requires same behavior when initializing and resuming
            }

            ////////////////////////////////////////////////////////////////////////////////////////
            // Reset variables if robot is returning from another state
            ////////////////////////////////////////////////////////////////////////////////////////
            if (resume)
            {
                resume = false;

                Log.e(LogTag.ROBOT, "MOVE target = " + targetPosition);

                Vector3 target3dPosition = new Vector3(targetPosition.x, r.rigidbody.transform.position.y, targetPosition.y);
                Vector3 lookDirection = target3dPosition - r.rigidbody.transform.position;
                r.rigidbody.transform.rotation = Quaternion.LookRotation(lookDirection, Vector3.up);

                // Will this cause problems with moving to extremely close positions?
                Vector3 velocity = Vector3.Normalize(lookDirection) * speed;
                r.body.GetComponent<Rigidbody>().AddForce(velocity, ForceMode.VelocityChange);
            }

            ////////////////////////////////////////////////////////////////////////////////////////
            // Update: check if robot has reached its destination (within specified tolerance)
            ////////////////////////////////////////////////////////////////////////////////////////
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