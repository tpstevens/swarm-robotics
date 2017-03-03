using UnityEngine;

using Utilities;

namespace Robots
{
    public class RobotStateTurn : RobotState
    {
        private readonly float targetAngle;

        public RobotStateTurn(float targetAngle)
        {
            this.targetAngle = targetAngle;
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
                Log.d(LogTag.ROBOT, "TURN Robot " + r.id + " is turning towards target angle " + targetAngle);

                // Turn instantaneously
                r.rigidbody.transform.rotation = Quaternion.AngleAxis(targetAngle, Vector3.up);
            }

            ////////////////////////////////////////////////////////////////////////////////////////
            // Update: check if robot has reached its target angle
            ////////////////////////////////////////////////////////////////////////////////////////
            if (Mathf.Abs(r.getOrientation() - targetAngle) < 0.0001f)
                finished = true;
            else
                Log.e(LogTag.ROBOT, "TURN current = " + r.getOrientation() + ", target = " + targetAngle);

            ////////////////////////////////////////////////////////////////////////////////////////
            // Process messages: no, should happen in another state
            ////////////////////////////////////////////////////////////////////////////////////////

            ////////////////////////////////////////////////////////////////////////////////////////
            // "Clean up" robot state
            ////////////////////////////////////////////////////////////////////////////////////////
            if (finished)
            {
                Log.d(LogTag.ROBOT, "TURN Robot " + r.id + " has reached target angle " + targetAngle);

                // Pop state off the stack
                r.popState();
            }
        }
    }
}