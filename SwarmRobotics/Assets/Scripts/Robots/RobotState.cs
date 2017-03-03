using Utilities;

namespace Robots
{
    public abstract class RobotState
    {
        protected bool initialized = false;
        protected bool resume = false;

        /// <summary>
        /// Inform the state that on the next update() call, it will be on top of the robot's state
        /// stack. Useful for resetting parameters like movement.
        /// </summary>
        public void prepareToResume()
        {
            if (resume)
                Log.a(LogTag.ROBOT, "Attempting to resume a state that is not waiting to resume!");
            else
                resume = initialized; // don't resume if not even initialized
        }

        public abstract void update(Robot r);
    }
}
