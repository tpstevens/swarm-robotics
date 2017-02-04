using UnityEngine;

using CommSystem;
using Utilities;

public interface MainInterface
{
    /// <summary>
    /// Return the currently active configuration.
    /// </summary>
    /// <returns>The currently active configuration.</returns>
    Config getCurrentConfig();
    
    /// <summary>
    /// Returns the number of robots.
    /// </summary>
    /// <returns>The number of robots.</returns>
    int getNumRobots();

    GameObject getMessageIndicatorPrefab();

    /// <summary>
    /// Get the position of the robot with the given ID.
    /// </summary>
    /// <param name="robotId">The robot ID.</param>
    /// <param name="position">The Vector3 that will be assigned to the robot position.</param>
    /// <returns>Whether the position was successfully assigned.</returns>
    bool getRobotPosition(uint robotId, out Vector3 position);

    /// <summary>
    /// Notify a robot that it has collided with another.
    /// </summary>
    /// <param name="robotId">The id of the robot that collided.</param>
    /// <param name="collision">The Collision object.</param>
    void notifyCollision(uint robotId, Collision collision);

    /// <summary>
    /// Notify a robot that it has received a message.
    /// </summary>
    /// <param name="robotId">The recipient's ID.</param>
    /// <param name="msg">The message.</param>
    void notifyMessage(uint robotId, CommMessage msg);
}
