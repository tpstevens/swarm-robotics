using UnityEngine;

using Utilities;

namespace Robots
{
    public class RobotStateTurn : RobotState
    {
        private readonly Vector2 targetPosition;

        public RobotStateTurn(Vector2 targetPosition)
        {
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
                resume = true;  // Turn state requires same behavior when initializing and resuming
            }

            ////////////////////////////////////////////////////////////////////////////////////////
            // Reset variables if robot is returning from another state
            ////////////////////////////////////////////////////////////////////////////////////////
            if (resume)
            {
                resume = false;
                Log.d(LogTag.ROBOT, "TURN Robot " + r.id + " is turning towards target position " + targetPosition);

                // Turn instantaneously
                Vector3 target = new Vector3(targetPosition.x, r.rigidbody.transform.position.y, targetPosition.y);
                r.rigidbody.transform.rotation = Quaternion.LookRotation(target, Vector3.up);
                finished = true;
            }

            ////////////////////////////////////////////////////////////////////////////////////////
            // Update: no, already reached target angle
            ////////////////////////////////////////////////////////////////////////////////////////

            ////////////////////////////////////////////////////////////////////////////////////////
            // Process messages: no, should happen in another state
            ////////////////////////////////////////////////////////////////////////////////////////

            ////////////////////////////////////////////////////////////////////////////////////////
            // "Clean up" robot state
            ////////////////////////////////////////////////////////////////////////////////////////
            if (finished)
            {
                // Pop state off the stack
                r.popState();
            }
        }
    }
}