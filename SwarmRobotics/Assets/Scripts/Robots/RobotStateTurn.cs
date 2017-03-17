using UnityEngine;

using Utilities;

namespace Robots
{
    public class RobotStateTurn : RobotState
    {
        private readonly Vector2 targetPosition;
        private readonly float angularSpeed = 180f;

        private float timeTaken = 0;
        private float timeToTurn = 0;
        private Quaternion initialRotation;
        private Quaternion targetRotation;

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

                initialRotation = r.body.transform.rotation;

                Vector3 target = new Vector3(targetPosition.x, r.body.transform.position.y, targetPosition.y);

                if (target - r.body.transform.position != Vector3.zero)
                {
                    targetRotation = Quaternion.LookRotation(target - r.body.transform.position, Vector3.up);

                    float angle = Quaternion.Angle(initialRotation, targetRotation);

                    while (angle < 0)
                        angle += 360;

                    if (angle > 180)
                        angle = 360 - 180;

                    timeToTurn = angle / angularSpeed;
                }
                else
                {
                    timeTaken = timeToTurn;
                }
            }

            ////////////////////////////////////////////////////////////////////////////////////////
            // Update
            ////////////////////////////////////////////////////////////////////////////////////////
            if (timeTaken < timeToTurn)
            {
                timeTaken += Time.deltaTime;
                float progress = timeTaken / timeToTurn;
                float sphericalProgress = progress - Mathf.Sin(2 * Mathf.PI * progress) / (2 * Mathf.PI);// Mathf.Atan(10 * (progress - 0.5f)) / 2.8f + 0.5f;// (Mathf.Cos(2 * Mathf.PI * progress + Mathf.PI) + 1) / 2.0f;
                r.body.transform.rotation = Quaternion.Lerp(initialRotation, targetRotation, sphericalProgress);
            }
            else
            {
                finished = true;
            }

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