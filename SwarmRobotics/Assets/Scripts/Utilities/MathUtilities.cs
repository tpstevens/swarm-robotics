using UnityEngine;

namespace Utilities
{
    public class MathUtilities
    {
        public static float getFromToAngle(Vector3 robotPosition, Vector2 target)
        {
            // TODO: this doesn't properly handle opposite vectors
            Vector3 view = new Vector3(target.x, robotPosition.y, target.y) - robotPosition;
            return Vector3.Angle(view, Vector3.forward);
        }
    }
}
