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

    /// <summary>
    /// Notify a robot that it has collided another.
    /// </summary>
    /// <param name="robotId">The id of the robot that collided.</param>
    /// <param name="collision">The Collision object.</param>
    public void notifyCollision(int robotId, Collision collision)
    {
        robots[robotId].notifyCollision();
    }

    /// <summary>
    /// Implementation of MonoBehaviour.Start(). Reads the argument files, create the environment, 
    /// place robots, and pause the simulation. 
    /// </summary>
    void Start()
    {
        Log.w(LogTag.MAIN, "Loading scene " + SceneManager.GetActiveScene().name);

        // initialize random number generator
        Random.InitState(System.DateTime.Now.Millisecond);

        // reads args from file, creates default if necessary
        Args args = new Args();

        // create environment (ground and obstacles) and place robots
        initialize(args.configFileName);

        // pause game
        Time.timeScale = 0.0f;
    }

    /// <summary>
    /// Implementation of MonoBehaviour.Update(). Process user input and then call each robot's 
    /// update() function.
    /// </summary>
    void Update()
    {
        // pause/stop the sim, reset the level, etc.
        processUserInput();

        // distribute messages and call each robot's update() function
        updateSim();
    }

    /// <summary>
    /// Create the environment, including initializing and scaling the ground if necessary.
    /// </summary>
    /// <param name="config">The current Configuration.</param>
    /// <returns>Whether the environment was initialized successfully.</returns>
    private bool generateEnvironment(Configuration config)
    {
        float scaledGroundLength = config.GroundLength / 10.0f; // plane's dimensions are scaled up
        bool result = true;

        if (scaledGroundLength <= 0f)
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
            Ground.transform.localScale = new Vector3(scaledGroundLength, 
                                                      1.0f, 
                                                      scaledGroundLength);
            Ground.transform.position = Vector3.zero;
            Ground.transform.SetParent(EnvironmentObjects.transform);

            // Add barriers around the ground, assuming it's rectangular
            GameObject positiveX = GameObject.CreatePrimitive(PrimitiveType.Cube);
            positiveX.name = "Positive X";
            positiveX.transform.localScale = new Vector3(0.1f, 0.25f, config.GroundLength + 0.2f);
            positiveX.transform.position = new Vector3(config.GroundLength / 2.0f + 0.05f, 
                                                       0.125f, 
                                                       0.0f);
            positiveX.transform.parent = EnvironmentObjects.transform;

            GameObject negativeX = GameObject.CreatePrimitive(PrimitiveType.Cube);
            negativeX.name = "Negative X";
            negativeX.transform.localScale = new Vector3(0.1f, 0.25f, config.GroundLength + 0.2f);
            negativeX.transform.position = new Vector3(config.GroundLength / -2.0f - 0.05f, 
                                                       0.125f, 
                                                       0.0f);
            negativeX.transform.parent = EnvironmentObjects.transform;

            GameObject positiveZ = GameObject.CreatePrimitive(PrimitiveType.Cube);
            positiveZ.name = "Positive Z";
            positiveZ.transform.localScale = new Vector3(config.GroundLength + 0.2f, 0.25f, 0.1f);
            positiveZ.transform.position = new Vector3(0.0f, 
                                                       0.125f, 
                                                       config.GroundLength / 2.0f + 0.05f);
            positiveZ.transform.parent = EnvironmentObjects.transform;

            GameObject negativeZ = GameObject.CreatePrimitive(PrimitiveType.Cube);
            negativeZ.name = "Negative Z";
            negativeZ.transform.localScale = new Vector3(config.GroundLength + 0.2f, 0.25f, 0.1f);
            negativeZ.transform.position = new Vector3(0.0f, 
                                                       0.125f, 
                                                       config.GroundLength / -2.0f - 0.05f);
            negativeZ.transform.parent = EnvironmentObjects.transform;
        }

        return result;
    }

    /// <summary>
    /// Generate the environment and initialize the robots.
    /// </summary>
    /// <param name="configFile">The name of the config file.</param>
    /// <returns>Whether initialization completed successfully.</returns>
    private bool initialize(string configFile)
    {
        bool result = true;
        currentConfig = new Configuration(configFile); // reads the config file and sets parameters

        if (!generateEnvironment(currentConfig) || !placeRobots(currentConfig))
        {
            result = false;
            Log.a(LogTag.MAIN, "Failed to initialize scene using " + configFile);
        }
        else
        {
            // set overhead camera to see whole scene, etc
            repositionCameras(currentConfig);
        }

        return result;
    }

    /// <summary>
    /// Initialize and place the robots according to the configuration.
    /// </summary>
    /// <param name="config">The current Configuration.</param>
    /// <returns>Whether the robots were initialized and positioned successfully.</returns>
    private bool placeRobots(Configuration config)
    {
        // TODO: read number, placement shape, and location from config

        bool result = true;

        // Create robot header object
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

        robots = new Robot[config.NumRobots];

        if (config.SpawnShape == Configuration.eSpawnShape.SQUARE)
        {
            int numRobotsRoot = (int)Mathf.Sqrt(config.NumRobots);
            float robotSpacing = config.SpawnRadius * 2.0f / (numRobotsRoot - 1);
            for (int i = 0; i < numRobotsRoot; ++i)
            {
                for (int j = 0; j < numRobotsRoot; ++j)
                {
                    float x = config.SpawnCenter.x - config.SpawnRadius + i * robotSpacing;
                    float z = config.SpawnCenter.y - config.SpawnRadius + j * robotSpacing;
                    int id = numRobotsRoot * i + j;
                    Vector3 position = new Vector3(x, RobotPrefab.transform.position.y, z);
                    robots[numRobotsRoot * i + j] = new Robot(id, 
                                                              Instantiate(RobotPrefab), 
                                                              position, 
                                                              0.0f);
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Get user input,  perform application functions (quit, pause, reload, etc.).
    /// </summary>
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

    /// <summary>
    /// Resize the overhead camera to fit the entire scene.
    /// </summary>
    /// <param name="config">The current Configuration.</param>
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
                                                            -1.0f * config.GroundLength);
            overheadCamera.orthographic = true;
            overheadCamera.orthographicSize = config.GroundLength * 0.42f;
        }
    }

    /// <summary>
    /// Iterate through all the robots and call their update() method.
    /// </summary>
    private void updateSim()
    {
        for (int i = 0; i < robots.Length; ++i)
            robots[i].update();
    }
}
