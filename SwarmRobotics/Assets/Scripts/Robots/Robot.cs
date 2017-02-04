using UnityEngine;

using System.Collections.Generic;

using CommSystem;
using Utilities;

namespace Robots
{
    public class Robot {

        public GameObject body;

        private readonly bool PRINT_ROBOT_DETECTION = false; // TODO move to config file, default false

        private readonly float COLLISION_NOTIFICATION_TIME = 1.0f;
        private readonly float VELOCITY = 2.0f;

        private bool collided = false;
        private float collisionNotificationTimer = 0.0f;
        private Queue<CommMessage> unhandledMessages;
        private Rigidbody rigidbody;
        private RobotState.StateId previousState;
        private Sensors sensors;
        private uint id;

        // timers for things (TODO: create timed callback scheduler)
        private readonly float SENSOR_CHECK_TIME = 1.0f;
        private float sensorCheckTimer = 0.0f;

        private Stack<RobotState.StateId> stateStack;
        private Stack<RobotState.StateStorage_MoveTo> stateStorageStack_MoveTo;
        private Stack<RobotState.StateStorage_Sleep> stateStorageStack_Sleep;
        private Stack<RobotState.StateStorage_TurnTo> stateStorageStack_TurnTo;
        private Stack<RobotState.StateStorage_Wait> stateStorageStack_Wait;

        private class Sensors
        {
            public readonly float radarRange;

            public float currentAngleOffset = 0.0f;

            /// <summary>
            /// Distances to closest object within radar range in as many directions as there are
            /// spaces in this array. If no object is found, the distance will be -1.
            /// </summary>
            public float[] radar = new float[32];

            public Sensors(float radarRange)
            {
                this.radarRange = radarRange;
            }
        }

        /// <summary>
        /// Constructs a Robot object and assigns the Robot header object as its parent, if found.
        /// </summary>
        /// <param name="id">The robot's ID. There is no check for uniqueness, so be careful!</param>
        /// <param name="body">The GameObject corresponding to the robot in the scene.</param>
        /// <param name="startPosition">The starting position of the robot.</param>
        /// <param name="startRotation">The starting rotation of the robot.</param>
        public Robot(uint id, GameObject body, Vector3 startPosition, float startRotation, float radarRange)
        {
            this.body = body;
            this.id = id;

            unhandledMessages = new Queue<CommMessage>();
            stateStack = new Stack<RobotState.StateId>();
            sensors = new Sensors(radarRange);

            stateStorageStack_MoveTo = new Stack<RobotState.StateStorage_MoveTo>();
            stateStorageStack_Sleep = new Stack<RobotState.StateStorage_Sleep>();
            stateStorageStack_TurnTo = new Stack<RobotState.StateStorage_TurnTo>();
            stateStorageStack_Wait = new Stack<RobotState.StateStorage_Wait>();

            stateStack.Push(RobotState.StateId.WAIT);
            stateStorageStack_Wait.Push(new RobotState.StateStorage_Wait());

            // Enable this code to test single robot moving ahead
            if (true)
            {
                stateStack.Push(RobotState.StateId.MOVE_TO);
                stateStorageStack_MoveTo.Push(new RobotState.StateStorage_MoveTo(new Vector2(0.0f, 5.0f),
                                                                                 VELOCITY,
                                                                                 0.05f));

                stateStack.Push(RobotState.StateId.SLEEP);
                stateStorageStack_Sleep.Push(new RobotState.StateStorage_Sleep(2.0f));
            }

            body.name = "Robot " + id;
            body.transform.position = startPosition;
            rigidbody = body.GetComponent<Rigidbody>();

            if (rigidbody == null)
            {
                Log.e(LogTag.ROBOT, "Failed to find rigidbody on robot " + id);
            }
            else
            {
                rigidbody.position = startPosition;
                rigidbody.rotation = Quaternion.AngleAxis(startRotation, Vector3.up);

                GameObject robotHeader = GameObject.Find("Robots");
                if (robotHeader == null) 
                {
                    Log.w(LogTag.ROBOT, "Created Robots header object");

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
            handleMessages();

            sensorCheckTimer -= Time.deltaTime;
            if (sensorCheckTimer <= 0.0f)
            {
                sensorCheckTimer = SENSOR_CHECK_TIME;
                checkSensors();
            }

            // TODO: move collision handling code into a state? what if Robot isn't in a movement related
            // state?

            // Handle any collisions and reset the flag if necessary
            if (collided)
            {
                collided = false;
                body.GetComponent<Renderer>().material.color = Color.gray;
                collisionNotificationTimer = COLLISION_NOTIFICATION_TIME;
            }
            else if (collisionNotificationTimer != float.MinValue)
            {
                collisionNotificationTimer -= Time.deltaTime;
                if (collisionNotificationTimer <= 0.0f)
                {
                    body.GetComponent<Renderer>().material.color = Color.red;
                    collisionNotificationTimer = float.MinValue;
                }
            }

            stateUpdate();
        }

        /// <summary>
        /// Process the queue of unhandled messages.
        /// </summary>
        private void handleMessages()
        {
            CommMessage msg;

            while (unhandledMessages.Count > 0)
            {
                // TODO: process messages
                msg = unhandledMessages.Dequeue();
            }
        }

        /// <summary>
        /// Perform the state-specific update function that corresponds with the current state.
        /// </summary>
        private void stateUpdate()
        {
            if (stateStack.Count == 0)
            {
                Log.a(LogTag.ROBOT, "Robot " + id + " is currently stateless");
            }

            RobotState.StateId currentState = stateStack.Peek();

            switch (currentState) {
            case RobotState.StateId.MOVE_TO:
                stateUpdate_MoveTo();
                break;
            case RobotState.StateId.SLEEP:
                stateUpdate_Sleep();
                break;
            case RobotState.StateId.TURN_TO:
                stateUpdate_TurnTo();
                break;
            case RobotState.StateId.WAIT:
                stateUpdate_Wait();
                break;
            default:
                break;
            }

            previousState = currentState;
        }

        ////////////////////////////////////////////////////////////////////////
        // Private State Functions (TODO: add checks for nothing on storage stack)
        ////////////////////////////////////////////////////////////////////////
        private void stateUpdate_MoveTo()
        {
            if (stateStorageStack_MoveTo.Count <= 0)
            {
                Log.a(LogTag.ROBOT,
                      "Entered stateUpdate_MoveTo() without an associated storage container.");
            }

            bool finished = false;
            RobotState.StateStorage_MoveTo storage = stateStorageStack_MoveTo.Peek();

            if (!storage.initialized) // initialize when first enter state
            {
                storage.initialized = true;

                Log.d(LogTag.ROBOT, "Robot " + id + " is moving towards target position " + storage.target);

                // will this cause problems with moving to extremely close positions?
                Vector3 velocity = Vector3.Normalize(rigidbody.transform.forward) * VELOCITY;
                rigidbody.AddForce(velocity, ForceMode.VelocityChange);
            }

            // TODO: check if we're getting close and slow down

            Vector2 position2d = new Vector2(body.transform.position.x,
                                             body.transform.position.z);
            if (Vector2.Distance(position2d, storage.target) <= storage.tolerance)
            {
                finished = true;
            }

            if (finished) // "clean up"
            {
                Log.d(LogTag.ROBOT, "Robot " + id + " has reached target position " + storage.target);
                body.transform.position = new Vector3(storage.target.x, body.transform.position.y, storage.target.y);

                // end state and pop off the stack
                rigidbody.AddForce(-1 * rigidbody.velocity, ForceMode.VelocityChange);

                stateStack.Pop();
                stateStorageStack_MoveTo.Pop();
            }
        }

        private void stateUpdate_Sleep()
        {
            if (stateStorageStack_Sleep.Count <= 0)
            {
                Log.a(LogTag.ROBOT, 
                      "Entered stateUpdate_Sleep() without an associated storage container.");
            }

            bool finished = false;
            RobotState.StateStorage_Sleep storage = stateStorageStack_Sleep.Peek();

            if (!storage.initialized)
            {
                storage.initialized = true;
                Log.d(LogTag.ROBOT, "Robot " + id + " started sleeping for " + storage.timer + " seconds");
            }

            storage.timer -= Time.deltaTime;
            if (storage.timer <= 0.0f)
                finished = true;

            if (finished)
            {
                Log.d(LogTag.ROBOT, "Robot " + id + " has finished sleeping");

                stateStack.Pop();
                stateStorageStack_Sleep.Pop();
            }
        }

        private void stateUpdate_TurnTo()
        {
            if (stateStorageStack_TurnTo.Count <= 0)
            {
                Log.a(LogTag.ROBOT, "Entered stateUpdate_TurnTo() without an associated storage container.");
            }

            // TODO
            bool finished = false;

            if (finished)
            {
                
            }
        }

        private void stateUpdate_Wait()
        {
            if (stateStorageStack_Wait.Count <= 0)
            {
                Log.a(LogTag.ROBOT, "Entered stateUpdate_Wait() without an associated storage container.");
            }

            if (previousState != RobotState.StateId.WAIT)
            {
                Log.d(LogTag.ROBOT, "Robot " + id + " is waiting");
            }

            // TODO process messages? what goes here? Could add some sort of base update that happens 
            // (checking messages, checking for collisions, etc)
        }

        ////////////////////////////////////////////////////////////////////////
        // Private Sensor Functions
        ////////////////////////////////////////////////////////////////////////
        private void checkSensors()
        {
            detectRadar(0.0f);
        }

        private void detectRadar(float radialOffset)
        {
            if (sensors == null)
                Log.a(LogTag.ROBOT, "Robot " + id + " attempted to check radar without a sensor array.");

            int numDivisions = sensors.radar.Length;
            float radialDivision = 360.0f / numDivisions;
            RaycastHit hitInfo;
            Vector3 raycastDirection;

            for (int i = 0; i < numDivisions; ++i)
            {
                float angle = i * radialDivision + radialOffset;
                raycastDirection = Quaternion.Euler(0, angle, 0) * body.transform.forward;
                if (Physics.Raycast(body.transform.position, 
                                    raycastDirection, 
                                    out hitInfo, 
                                    sensors.radarRange))
                {
                    sensors.radar[i] = hitInfo.distance;
                    if (PRINT_ROBOT_DETECTION &&  hitInfo.transform.CompareTag("Robot"))
                    {
                        Log.d(LogTag.ROBOT, "Robot " + id + " detected " + hitInfo.transform.name +
                                            " at heading " + angle + " and distance " + hitInfo.distance);
                    }
                }
                else
                {
                    sensors.radar[i] = -1;
                }
            }
        }
    }
}

/**
 * Add timed functions to robot (every 2 seconds, update sensors) with timed callback scheduler for
 * sensors checks (or should those only happen in certain states?). add conditions for timed checks, and
 * timer resets if condition not met at that time?
 * 
 * Add methods to print state stacks (and storages, by making all StateStorage_ classes extend a single
 * StateStorage with mandatory ToString()).
 */