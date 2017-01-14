using UnityEngine;

using System;

public class CollisionDetection : MonoBehaviour {

	private Main mainScript;

	private void Start()
	{
		mainScript = (Main)GameObject.Find("Scripts").GetComponent(typeof(Main));
	}

	private void OnCollisionEnter(Collision collision)
	{
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
