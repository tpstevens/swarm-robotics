using UnityEngine;

using System;
using System.Collections.Generic;

using CommSystem;
using Utilities;

namespace Robots
{
    public class Robot
    {
        public readonly float OBJECT_DETECT_DISTANCE = 1f;
        public readonly float VELOCITY = 3.0f;
        
        // TODO: does exposing these break encapsulation?
        public GameObject body;
        public GameObject carriedResource;
        public Queue<CommMessage> unhandledMessages;
        public RobotSensors sensors;

        private readonly bool PRINT_ROBOT_DETECTION = false; // TODO move to config file, default false
        private readonly float COLLISION_NOTIFICATION_TIME = 1.0f;
        private readonly float SENSOR_CHECK_TIME = 0.5f;
        public readonly uint id;

        private bool collided = false;
        private float collisionNotificationTimer = 0.0f;
        private float sensorCheckTimer; // timers for things (TODO: create timed callback scheduler)
        private Stack<RobotState> stateStack;

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

            carriedResource = null;

            unhandledMessages = new Queue<CommMessage>();
            stateStack = new Stack<RobotState>();
            sensors = new RobotSensors(radarRange);
            sensorCheckTimer = 0.02f * (id % 50); // so not all robots check sensors at every frame

            stateStack.Push(new RobotStateWait());

            // Insert other states here to test without waiting for satellite
            {
            }

            stateStack.Push(new RobotStateSleep(1.0f)); // allow robots time to fall to ground

            body.name = "Robot " + id;
            body.transform.position = startPosition;
            body.transform.rotation = Quaternion.AngleAxis(startRotation, Vector3.up);

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

        /// <summary>
        /// Immediately updates all the robot's sensors. Does not reset sensor check timer.
        /// </summary>
        /// <returns>The robot's sensor array.</returns>
        public RobotSensors checkSensors()
        {
            detectRadar(0.0f);
            return sensors;
        }

        public GameObject getObjectInFront(float maxDistance = -1f)
        {
            GameObject result = null;

            if (maxDistance < 0)
                maxDistance = OBJECT_DETECT_DISTANCE;

            RaycastHit hitInfo;
            if (Physics.Raycast(body.transform.position,
                                body.transform.forward,
                                out hitInfo,
                                OBJECT_DETECT_DISTANCE + body.transform.localScale.x / 2.0f))
            {
                result = hitInfo.transform.gameObject;
            }

            return result;
        }

        public float getOrientation()
        {
            return Vector3.Angle(Vector3.forward, body.transform.forward);
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
        /// Used in RobotState.update() to pop states off the stack when they're finished.
        /// Breaks encapsulation?
        /// </summary>
        public void popState()
        {
            if (stateStack.Count == 0)
                Log.a(LogTag.ROBOT, "Attempting to pop state off an empty stack!");

            stateStack.Pop();
            
            if (stateStack.Count == 0)
                Log.a(LogTag.ROBOT, "Robot " + id + " is currently stateless");

            stateStack.Peek().prepareToResume();
        }

        /// <summary>
        /// Used in RobotState.update() to push new states onto the stack.
        /// Breaks encapsulation?
        /// </summary>
        public void pushState(RobotState state)
        {
            stateStack.Push(state);
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

            // Perform the state-specific update function that corresponds with the current state.
            if (stateStack.Count == 0)
                Log.a(LogTag.ROBOT, "Robot " + id + " is currently stateless");
            else
                stateStack.Peek().update(this);
        }

        ////////////////////////////////////////////////////////////////////////
        // Private Sensor Functions
        ////////////////////////////////////////////////////////////////////////
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
                    if (PRINT_ROBOT_DETECTION && hitInfo.transform.CompareTag("Robot"))
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