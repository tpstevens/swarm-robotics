using UnityEngine;

public interface MainInterface
{
    /// <summary>
    /// Notify a robot that it has collided another.
    /// </summary>
    /// <param name="robotId">The id of the robot that collided.</param>
    /// <param name="collision">The Collision object.</param>
    void notifyCollision(int robotId, Collision collision);
}
