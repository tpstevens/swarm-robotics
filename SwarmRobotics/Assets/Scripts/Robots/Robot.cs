using UnityEngine;

using System.Collections.Generic;

using CommSystem;
using Utilities;

namespace Robotics
{
    public class Robot {

        public GameObject body;

        private readonly float TIMER_PERIOD = 2.0f;
        private readonly float VELOCITY = 2.0f;

        private bool collided = false;
        private float timer = 0.5f;
        private Queue<CommMessage> unhandledMessages;
        private Rigidbody rigidbody;
        private uint id;

        /// <summary>
        /// Constructs a Robot object and assigns the Robot header object as its parent, if found.
        /// </summary>
        /// <param name="id">The robot's ID. There is no check for uniqueness, so be careful!</param>
        /// <param name="body">The GameObject corresponding to the robot in the scene.</param>
        /// <param name="startPosition">The starting position of the robot.</param>
        /// <param name="startRotation">The starting rotation of the robot.</param>
        public Robot(uint id, GameObject body, Vector3 startPosition, float startRotation)
        {
            this.body = body;
            this.id = id;

            unhandledMessages = new Queue<CommMessage>();

            body.name = "Robot " + id;
            body.transform.position = startPosition;
            rigidbody = body.GetComponent<Rigidbody>();

            if (rigidbody == null)
            {
                Log.e(LogTag.ROBOTICS, "Failed to find rigidbody on robot " + id);
            }
            else
            {
                rigidbody.position = startPosition;
                rigidbody.rotation = Quaternion.AngleAxis(startRotation, Vector3.up);

                GameObject robotHeader = GameObject.Find("Robots");
                if (robotHeader == null) 
                {
                    Log.w(LogTag.ROBOTICS, "Created Robots header object");

                    robotHeader = new GameObject("Robots");
                    robotHeader.transform.position = Vector3.zero;
                    robotHeader.transform.rotation = new Quaternion(0f, 0f, 0f, 0f);
                }

                body.transform.parent = robotHeader.transform;
            }
        }

        /// <summary>
        /// Notify the robot that it has collided with another. The collision should be handled
        /// inside the update() function, not here.
        /// </summary>
        public void notifyCollision()
        {
            collided = true;
        }

        /// <summary>
        /// Notify the robot that it has receieved a message. The message will be added to
        /// the end of the queue and handled inside the update() function, not here.
        /// </summary>
        /// <param name="msg"></param>
        public void queueMessage(CommMessage msg)
        {
            unhandledMessages.Enqueue(msg);
        }

        /// <summary>
        /// The equivalent of MonoBehaviour.Update(), but called by the main script.
        /// </summary>
        public void update()
        {
            timer -= Time.deltaTime;

            handleMessages();

            if (timer <= 0.0f)
            {
                timer = TIMER_PERIOD;
                randomizeDirection();
            }

            // Handle any collisions and reset the flag if necessary
            if (collided)
            {
                collided = false;
                body.GetComponent<Renderer>().material.color = Color.gray;

                // TODO: on collision, robot turns and moves in the opposite direction
                // TODO: add arrows so we can see robot direction

                // rigidbody.AddForce(-1 * rigidbody.velocity, ForceMode.VelocityChange);
                // rigidbody.transform.rotation.SetLookRotation(rigidbody.velocity, Vector3.up);
                // Vector3 velocity = Vector3.Normalize(rigidbody.transform.forward) * VELOCITY;
                // rigidbody.AddForce(velocity, ForceMode.VelocityChange);

                randomizeDirection();
                timer = TIMER_PERIOD;
            }
        }

        /// <summary>
        /// Process the queue of unhandled messages.
        /// </summary>
        private void handleMessages()
        {
            CommMessage msg;

            while (unhandledMessages.Count > 0)
            {
                msg = unhandledMessages.Dequeue();
            }
        }

        /// <summary>
        /// Rotate the robot in a random direction and reset its velocity. At the moment, robots 
        /// move in the same direction as their current orientation.
        /// </summary>
        private void randomizeDirection()
        {
            float angle = Random.Range(0, 359);
            rigidbody.transform.rotation = Quaternion.AngleAxis(angle, Vector3.up);

            Vector3 velocity = Vector3.Normalize(rigidbody.transform.forward) * VELOCITY;
            rigidbody.AddForce(-1 * rigidbody.velocity, ForceMode.VelocityChange);
            rigidbody.AddForce(velocity, ForceMode.VelocityChange);
        }
    }
}
