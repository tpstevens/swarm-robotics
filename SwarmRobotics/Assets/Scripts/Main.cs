using UnityEngine;
using UnityEngine.SceneManagement;

using Robotics;
using Utilities;

public class Main : MonoBehaviour
{
	public Camera overheadCamera;
	public GameObject EnvironmentObjects;
	public GameObject Ground;
	public GameObject RobotObjects;
	public GameObject RobotPrefab;

	private Configuration currentConfig;
	private Robot[] robots;

	public void notifyCollision(int robotId, Collision collision)
	{
		robots[robotId].handleCollision();
	}

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
		updateSim();
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
			// Create environment header object
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

			// Create ground
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

			// Position and scale ground according to configuration
			Ground.transform.localScale = new Vector3(groundLength, 1.0f, groundLength);
			Ground.transform.position = Vector3.zero;
			Ground.transform.SetParent(EnvironmentObjects.transform);

			// Add barriers around the ground, assuming it's rectangular
			GameObject positiveX = GameObject.CreatePrimitive(PrimitiveType.Cube);
			positiveX.name = "Positive X";
			positiveX.transform.localScale = new Vector3(0.1f, 0.25f, config.GroundLength + 0.2f);
			positiveX.transform.position = new Vector3(config.GroundLength / 2.0f + 0.05f, 0.125f, 0.0f);
			positiveX.transform.parent = EnvironmentObjects.transform;

			GameObject negativeX = GameObject.CreatePrimitive(PrimitiveType.Cube);
			negativeX.name = "Negative X";
			negativeX.transform.localScale = new Vector3(0.1f, 0.25f, config.GroundLength + 0.2f);
			negativeX.transform.position = new Vector3(config.GroundLength / -2.0f - 0.05f, 0.125f, 0.0f);
			negativeX.transform.parent = EnvironmentObjects.transform;

			GameObject positiveZ = GameObject.CreatePrimitive(PrimitiveType.Cube);
			positiveZ.name = "Positive Z";
			positiveZ.transform.localScale = new Vector3(config.GroundLength + 0.2f, 0.25f, 0.1f);
			positiveZ.transform.position = new Vector3(0.0f, 0.125f, config.GroundLength / 2.0f + 0.05f);
			positiveZ.transform.parent = EnvironmentObjects.transform;

			GameObject negativeZ = GameObject.CreatePrimitive(PrimitiveType.Cube);
			negativeZ.name = "Negative Z";
			negativeZ.transform.localScale = new Vector3(config.GroundLength + 0.2f, 0.25f, 0.1f);
			negativeZ.transform.position = new Vector3(0.0f, 0.125f, config.GroundLength / -2.0f - 0.05f);
			negativeZ.transform.parent = EnvironmentObjects.transform;
		}

		return result;
	}

	private bool initialize(string configFile)
	{
		bool result = true;
		currentConfig = new Configuration(configFile);

		if (!generateEnvironment(currentConfig) || !placeRobots(currentConfig))
		{
			result = false;
			Log.a(LogTag.MAIN, "Failed to initialize scene using " + configFile);
		}
		else
		{
			repositionCameras(currentConfig);
		}

		return result;
	}

	private bool placeRobots(Configuration config)
	{
		// TODO: read number, placement shape, and location from config

		bool result = true;

		Random.InitState(System.DateTime.Now.Millisecond);
		robots = new Robot[17];

		// Create environment header object
		if (RobotObjects == null)
		{
			RobotObjects = GameObject.Find("Robots");
			if (RobotObjects != null)
			{
				Log.d(LogTag.MAIN, "Located Robots header object in scene");
			}
			else
			{
				Log.d(LogTag.MAIN, "Created Robots header object");
				RobotObjects = new GameObject("Robots");
				RobotObjects.transform.position = Vector3.zero;
				RobotObjects.transform.rotation = new Quaternion(0f, 0f, 0f, 0f);
			}
		}

		for (int i = 0; i < 9; ++i)
		{
			Vector3 position = new Vector3(-8 + 2 * i, RobotPrefab.transform.position.y, -8 + 2 * i);
			robots[i] = new Robot(i, Instantiate(RobotPrefab), position, 40.0f * i);
		}

		for (int i = 9; i < 13; ++i)
		{
			Vector3 position = new Vector3(8 - 2 * (i - 9), RobotPrefab.transform.position.y, -8 + 2 * (i - 9));
			robots[i] = new Robot(i, Instantiate(RobotPrefab), position, 40.0f * i);
		}

		for (int i = 13; i < 17; ++i)
		{
			Vector3 position = new Vector3(8 - 2 * (i - 8), RobotPrefab.transform.position.y, -8 + 2 * (i - 8));
			robots[i] = new Robot(i, Instantiate(RobotPrefab), position, 40.0f * i);
		}

		return result;
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

	private void repositionCameras(Configuration config)
	{
		if (overheadCamera == null)
		{
			Log.e(LogTag.MAIN, "Reference to overhead camera is NULL");
		}
		else
		{
			overheadCamera.transform.position = new Vector3(config.GroundLength,
			                                                0.8f * config.GroundLength, 
															- 1.0f * config.GroundLength);
			overheadCamera.orthographic = true;
			overheadCamera.orthographicSize = config.GroundLength * 0.42f;
		}
	}

	private void updateSim()
	{
		for (int i = 0; i < robots.Length; ++i)
			robots[i].update();
	}
}
