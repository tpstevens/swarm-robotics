using UnityEngine;

using Utilities;

namespace Robots
{
    public class RobotStateSleep : RobotState
    {
        private float timer;

        public RobotStateSleep(float timer)
        {
            this.timer = timer;
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
                Log.d(LogTag.ROBOT, "Robot " + r.id + " is sleeping for " + timer + " s");
            }

            ////////////////////////////////////////////////////////////////////////////////////////
            // Reset variables if robot is returning from another state
            ////////////////////////////////////////////////////////////////////////////////////////
            if (resume)
            {
                resume = false;
                Log.d(LogTag.ROBOT, "Robot " + r.id + " is returning to sleep (" + timer + " s remain)");
            }

            ////////////////////////////////////////////////////////////////////////////////////////
            // Update
            ////////////////////////////////////////////////////////////////////////////////////////
            timer -= Time.deltaTime;
            if (timer <= 0.0f)
                finished = true;

            ////////////////////////////////////////////////////////////////////////////////////////
            // Process messages: TODO
            ////////////////////////////////////////////////////////////////////////////////////////

            ////////////////////////////////////////////////////////////////////////////////////////
            // "Clean up" robot state
            ////////////////////////////////////////////////////////////////////////////////////////
            if (finished)
            {
                Log.d(LogTag.ROBOT, "Robot " + r.id + " has finished sleeping");

                // Pop state off the stack
                r.popState();
            }
        }
    }
}
