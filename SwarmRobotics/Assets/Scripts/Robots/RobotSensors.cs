namespace Robots
{
    public class RobotSensors
    {
        public readonly float radarRange;

        public float currentAngleOffset = 0.0f;

        /// <summary>
        /// Distances to closest object within radar range in as many directions as there are
        /// spaces in this array. If no object is found, the distance will be -1.
        /// </summary>
        public float[] radar = new float[32];

        public RobotSensors(float radarRange)
        {
            this.radarRange = radarRange;
        }
    }
}
