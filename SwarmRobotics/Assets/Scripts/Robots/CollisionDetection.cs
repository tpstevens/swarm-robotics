using UnityEngine;

public class CollisionDetection : MonoBehaviour {

    private MainInterface mainScript;

    /// <summary>
    /// Implementation of MonoBehavior.Start()
    /// </summary>
    void Start()
    {
        mainScript = (MainInterface)GameObject.Find("Scripts").GetComponent(typeof(MainInterface));
    }

    /// <summary>
    /// Notify the main script that a robot collision has occurred. Called when the attached Collider
    /// first receieves a collision event.
    /// </summary>
    /// <param name="collision"></param>
    private void OnCollisionEnter(Collision collision)
    {
        // TODO handle collisions with walls while excluding floors

        if (collision.transform.name.StartsWith("Robot"))
        {
            string name = gameObject.name;
            int id = int.Parse(name.Substring(name.LastIndexOf(' ') + 1));
            mainScript.notifyCollision(id, collision); // bad style?
        }
        
        // TODO: may need to implement robot scripts as Monobehaviours
        // Messaging may get a little more difficult...instantiate messages as actual GameObjects,
        // or have MainScript update early?
    }
}
