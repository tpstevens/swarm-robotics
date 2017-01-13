using UnityEngine;
using UnityEngine.SceneManagement;

using Utilities;

public class Main : MonoBehaviour
{
	public GameObject EnvironmentObjects;
	public GameObject Ground;

	private Configuration currentConfig;

	private void Start()
	{
		Log.w(LogTag.MAIN, "Loading scene " + SceneManager.GetActiveScene().name);

		Args args = new Args();	// reads args from file, creates default if necessary
		initialize(args.configFileName);
		Time.timeScale = 0.0f;
	}

	private void Update()
	{
		processUserInput();
	}

	private bool generateEnvironment(Configuration config)
	{
		float groundLength = config.GroundLength / 10.0f;
		bool result = true;

		if (groundLength <= 0f)
		{
			result = false;
		}
		else
		{
			if (EnvironmentObjects == null)
			{
				EnvironmentObjects = GameObject.Find("Environment");
				if (EnvironmentObjects != null)
				{
					Log.d(LogTag.MAIN, "Located Environment header object in scene");
				}
				else
				{
					Log.d(LogTag.MAIN, "Created Environment header object");
					EnvironmentObjects = new GameObject("Environment");
					EnvironmentObjects.transform.position = Vector3.zero;
					EnvironmentObjects.transform.rotation = new Quaternion(0f, 0f, 0f, 0f);
				}
			}

			if (Ground == null)
			{
				Ground = GameObject.Find("Ground");
				if (Ground != null)
				{
					Log.d(LogTag.MAIN, "Located Ground in scene");
				}
				else
				{
					Log.d(LogTag.MAIN, "Created Ground");
					Ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
					Ground.name = "Ground";
				}
			}

			Ground.transform.localScale = new Vector3(groundLength, 1.0f, groundLength);
			Ground.transform.position = Vector3.zero;
			Ground.transform.SetParent(EnvironmentObjects.transform);
		}

		return result;
	}

	private bool initialize(string configFile)
	{
		bool result = true;
		Configuration config = new Configuration(configFile);

		if (!generateEnvironment(config) || !placeRobots(config))
		{
			result = false;
			Log.a(LogTag.MAIN, "Failed to initialize scene using " + configFile);
		}

		return result;
	}

	private bool placeRobots(Configuration config)
	{
		return true;
	}

	private void processUserInput()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			ApplicationManager.quit();
		}
		else if (Input.GetKeyDown(KeyCode.R))
		{
			ApplicationManager.reloadScene();
		}
		else if (Input.GetKeyDown(KeyCode.P) || Input.GetKeyDown(KeyCode.Space))
		{
			Time.timeScale = (Time.timeScale != 0.0f) ? 0f : 1f;
			Log.d(LogTag.MAIN, "Timescale set to " + Time.timeScale);
		}
	}
}
