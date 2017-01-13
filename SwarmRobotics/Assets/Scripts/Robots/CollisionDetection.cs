using UnityEngine;

public class CollisionDetection : MonoBehaviour {

	private void OnCollisionEnter(Collision collision)
	{
		// SendMessage("handleCollision", collision);
		
		// TODO: may need to implement robot scripts as Monobehaviours
		// Messaging may get a little more difficult...instantiate messages as actual GameObjects,
		// or have MainScript update early?
	}
}
