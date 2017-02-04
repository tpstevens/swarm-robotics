using UnityEngine;

namespace Robots
{
    public class RobotState
    {
        ////////////////////////////////////////////////////////////////////////
        // State ID Definitions
        ////////////////////////////////////////////////////////////////////////
        public enum StateId { WAIT, MOVE_TO, SLEEP, TURN_TO };

        ////////////////////////////////////////////////////////////////////////
        // State Storage Definitions
        ////////////////////////////////////////////////////////////////////////

        public class StateStorage_MoveTo
        {
            public readonly float speed;
            public readonly float tolerance;
            public readonly Vector2 target;

            public bool initialized = false;

            public StateStorage_MoveTo(Vector2 target, float speed, float tolerance)
            {
                this.speed = speed;
                this.target = target;
                this.tolerance = tolerance;
            }
        }

        public class StateStorage_Sleep
        {
            public bool initialized = false;
            public float timer;

            public StateStorage_Sleep(float timer)
            {
                this.timer = timer;
            } 

        }

        public class StateStorage_TurnTo
        {
            public readonly float speed;
            public readonly float target;
            public readonly float tolerance;

            public bool initialized = false;

            public StateStorage_TurnTo(float target, float speed, float tolerance)
            {
                this.speed = speed;
                this.target = target;
                this.tolerance = tolerance;
            }
        }
        public class StateStorage_Wait
        {
            public readonly string args;

            public bool initialized = false;

            public StateStorage_Wait()
            {
                // Does None really need a state variable? We could track statistics about how long
                // things remain stationery...
            }
        }
    }
}

/**
 * Create StateStorage_<state ID> as a subclass of StateStorage?
 */
