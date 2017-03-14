using UnityEngine;

using Utilities;

namespace Robots
{
    public class RobotStatePlaceResource : RobotState
    {
        private static float placementDistance = 1.5f;

        private Vector2 position;

        public RobotStatePlaceResource(Vector2 position)
        {
            this.position = position;
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

                if (r.carriedResource != null)
                {
                    r.pushState(new RobotStateMove(position, placementDistance));
                }
                else
                {
                    Log.e(LogTag.ROBOT, "Initializing RobotPlaceResource, but robot isn't carrying anything.");
                    finished = true;
                }
            }

            ////////////////////////////////////////////////////////////////////////////////////////
            // Reset variables if robot is returning from another state
            ////////////////////////////////////////////////////////////////////////////////////////
            if (resume)
            {
                resume = false;

                Vector2 robotPosition = new Vector2(r.body.transform.position.x, 
                                                    r.body.transform.position.z);
                if (r.carriedResource != null 
                    && Vector2.Distance(robotPosition, position) <= placementDistance)
                {
                    Log.w(LogTag.ROBOT, "Robot " + r.id + " has placed " + r.carriedResource.transform.name);

                    r.carriedResource.transform.SetParent(null);
                    r.carriedResource.transform.position = new Vector3(position.x, 0.5f, position.y);
                    r.carriedResource.transform.rotation = new Quaternion();
                    r.carriedResource = null;
                }

                finished = true;
            }

            ////////////////////////////////////////////////////////////////////////////////////////
            // Update: intentionally empty
            ////////////////////////////////////////////////////////////////////////////////////////

            ////////////////////////////////////////////////////////////////////////////////////////
            // Process messages: disabled here
            ////////////////////////////////////////////////////////////////////////////////////////

            ////////////////////////////////////////////////////////////////////////////////////////
            // "Clean up" robot state
            ////////////////////////////////////////////////////////////////////////////////////////
            if (finished)
            {
                r.popState();
            }
        }
    }
}
