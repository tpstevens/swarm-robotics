using UnityEngine;

using Utilities;

namespace Robots
{
    public class RobotStateRetrieveResource : RobotState
    {
        private static float retrievalDistance = 1.5f;

        private Vector2 position;

        public RobotStateRetrieveResource(Vector2 position)
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

                if (r.carriedResource == null)
                {
                    resume = true;
                }
                else
                {
                    Log.e(LogTag.ROBOT, "Initializing RobotPlaceResource, but robot is already carrying object.");
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
                if (Vector2.Distance(robotPosition, position) > retrievalDistance)
                {
                    r.pushState(new RobotStateTurn(position));
                    r.pushState(new RobotStateMove(position, retrievalDistance));
                }
                else
                {
                    GameObject resource = r.getObjectInFront();
                    if (resource != null && resource.CompareTag("Resource"))
                    {
                        Log.w(LogTag.ROBOT, "Robot " + r.id + " has picked up " + resource.transform.name);
                        resource.transform.SetParent(r.body.transform);
                        resource.transform.position = new Vector3(r.body.transform.position.x, 1.5f, r.body.transform.position.z);
                        r.carriedResource = resource;
                        finished = true;
                    }
                    else
                    {
                        Log.e(LogTag.ROBOT, "Robot " + r.id + " isn't facing any resources");
                        Time.timeScale = 0.0f;
                    }
                }
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
