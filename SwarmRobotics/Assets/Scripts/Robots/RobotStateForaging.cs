using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities;
using CommSystem;

namespace Robots
{
    public class RobotStateForaging : RobotState
    {
        private enum ForagingState
        {
            REQUEST_RESOURCE = 0,
            PICK_UP_RESOURCE = 1,
            GO_TO_RESOURCE_HOME = 2,
            GO_TO_DROP_OFF = 3,
            DROP_OFF_RESOURCE = 4,
            RETURN_TO_BASE = 5,
            FINISHED = 6
        }

        private ForagingState state;
        private bool waiting;
        private Robot robot;
        private Vector2 result;
        private bool gotLocation;
        private Vector2 resultRH;
        private bool gotRHLocation;
        private GameObject resource;
        private Vector3 resultBase;
        private bool gotBaseLocation;
        private bool noMoreResources;
        private Queue<string> handledMessage;
        private Queue<Vector2> waitQueue;

        private Vector2 testMove;

        public RobotStateForaging()
        {

        }

        public bool StringToVector2(string sVector, out Vector2 locationTest)
        {
            if (sVector != null)
            {
                // Remove the parentheses
                if (sVector.StartsWith("(") && sVector.EndsWith(")"))
                {
                    sVector = sVector.Substring(1, sVector.Length - 2);
                }

                // split the items
                string[] sPesition = sVector.Split(',');

                // store as a Vector2
                Vector2 result2 = new Vector2(
                    float.Parse(sPesition[0]),
                    float.Parse(sPesition[1]));
                locationTest = result2;
                return true;
            }
            else
            {
                locationTest = Vector2.zero;
                return false;
            }
        }

        public bool StringToVector3(string sVector, out Vector3 locationTest)
        {
            if (sVector != null)
            {
                // Remove the parentheses
                if (sVector.StartsWith("(") && sVector.EndsWith(")"))
                {
                    sVector = sVector.Substring(1, sVector.Length - 2);
                }

                // split the items
                string[] sPesition = sVector.Split(',');

                // store as a Vector2
                Vector3 result3 = new Vector3(
                    float.Parse(sPesition[0]),
                    float.Parse(sPesition[1]),
                    float.Parse(sPesition[2]));
                locationTest = result3;
                return true;
            }
            else
            {
                locationTest = Vector3.zero;
                return false;
            }
        }

        public override void update(Robot r)
        {
            bool finished = false;
            Vector2 currentPosition = new Vector2(r.body.transform.position.x, r.body.transform.position.z);

            ////////////////////////////////////////////////////////////////////////////////////////
            // Initialize variables if necessary when first enter state
            ////////////////////////////////////////////////////////////////////////////////////////
            if (!initialized)
            {
                initialized = true;
                resume = true;
                noMoreResources = false;

                state = ForagingState.REQUEST_RESOURCE;
            }

            ////////////////////////////////////////////////////////////////////////////////////////
            // Reset variables if robot is returning from another state
            ////////////////////////////////////////////////////////////////////////////////////////

            if (resume)
            {
                resume = false;
                Log.d(LogTag.ROBOT, "Robot " + r.id + " is returning to foraging");

                switch (state)
                {
                case ForagingState.REQUEST_RESOURCE:
                    {
                        Log.d(LogTag.ROBOT, "ROBOT " + r.id + " has requested a resource.");
                        Comm.directMessage(r.id, Comm.SATELLITE, "resource_request");
                        break;
                    }
                case ForagingState.PICK_UP_RESOURCE:
                    {
                        testMove = new Vector2(10f, 10f);
                        r.pushState(new RobotStateMove(testMove));
                        state = ForagingState.GO_TO_DROP_OFF;
                        break;
                    }
                case ForagingState.GO_TO_RESOURCE_HOME:
                    {
                        state = ForagingState.GO_TO_DROP_OFF;

                        break;
                    }
                case ForagingState.GO_TO_DROP_OFF:
                    {
                        //request a position where to drop off the resource in the resource home
                        Comm.directMessage(r.id, Comm.SATELLITE, "resource_home_request");
                        break;
                    }
                case ForagingState.DROP_OFF_RESOURCE:
                    {
                        //drop off the resource
                        Comm.directMessage(r.id, Comm.SATELLITE, "resource_delivered");
                        break;
                    }
                case ForagingState.RETURN_TO_BASE:
                    {
                        state = ForagingState.REQUEST_RESOURCE;
                        break;
                    }
                case ForagingState.FINISHED:
                    {
                        finished = true;
                        break;
                    }
                default:
                    {
                        break;
                    }
                }
            }
            ////////////////////////////////////////////////////////////////////////////////////////
            // Update: check if robot has reached its destination (within specified tolerance)
            ////////////////////////////////////////////////////////////////////////////////////////


            ////////////////////////////////////////////////////////////////////////////////////////
            // Process messages: no, should be handled in other states
            ////////////////////////////////////////////////////////////////////////////////////////

            int count = r.unhandledMessages.Count;
            for (int i = 0; i < count; ++i)
            {
                CommMessage msg = r.unhandledMessages.Dequeue();

                if (msg.senderId == Comm.SATELLITE)
                {
                    switch (state)
                    {
                    case ForagingState.REQUEST_RESOURCE:
                        {
                            if (msg.text.StartsWith("resource_location"))
                            {
                                string[] lines = msg.text.Split('\t');
                                //parse the messgae sent back from the satellite
                                //will be sent the location of the resource
                                gotLocation = StringToVector2(lines[1], out result);
                                r.pushState(new RobotStateRetrieveResource(result));
                                state = ForagingState.PICK_UP_RESOURCE;
                            }
                            else if (msg.text.StartsWith("go_to_base"))
                            {
                                string[] lines = msg.text.Split('\t');
                                gotBaseLocation = StringToVector3(lines[1], out resultBase);
                                r.pushState(new RobotStateMove(resultBase));
                                state = ForagingState.FINISHED;
                            }
                            break;
                        }
                    case ForagingState.GO_TO_DROP_OFF:
                        {
                            if (msg.text.StartsWith("resource_home"))
                            {
                                string[] lines = msg.text.Split('\t');
                                gotRHLocation = StringToVector2(lines[1], out resultRH);
                            }
                            testMove = new Vector2(10f, 15f);
                            r.pushState(new RobotStateMove(testMove));
                            r.pushState(new RobotStatePlaceResource(resultRH));
                            state = ForagingState.DROP_OFF_RESOURCE;
                            break;
                        }
                    case ForagingState.DROP_OFF_RESOURCE:
                        {
                            if (msg.text.StartsWith("resource_location"))
                            {
                                string[] lines = msg.text.Split('\t');
                                //parse the messgae sent back from the satellite
                                //will be sent the location of the resource
                                gotLocation = StringToVector2(lines[1], out result);
                                r.pushState(new RobotStateRetrieveResource(result));
                                state = ForagingState.PICK_UP_RESOURCE;
                            }
                            else if (msg.text.StartsWith("go_to_base"))
                            {
                                string[] lines = msg.text.Split('\t');
                                gotBaseLocation = StringToVector3(lines[1], out resultBase);
                                r.pushState(new RobotStateMove(resultBase));
                                state = ForagingState.FINISHED;
                            }
                            break;
                        }
                    }
                }
                else
                {
                    Log.w(LogTag.ROBOT, "Robot " + r.id + " processed unknown message " + msg.text + " from " + msg.senderId);
                }
            }

            ////////////////////////////////////////////////////////////////////////////////////////
            // "Clean up" robot state
            ////////////////////////////////////////////////////////////////////////////////////////
            if (finished)
            {
                Log.d(LogTag.ROBOT, "Robot " + r.id + " has finished foraging");

                // Pop state off the stack
                r.popState();
            }
        }
    }
}