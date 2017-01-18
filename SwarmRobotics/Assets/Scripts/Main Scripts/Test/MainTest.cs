using UnityEngine;
using UnityEngine.SceneManagement;

using Utilities;

public class MainTest : MonoBehaviour
{
    public GameObject robot;
    public Camera overviewCamera;
    public Camera followCamera;
    public float WALK_SPEED = 5f;
    private bool followingRobot = false;

    /// <summary>
    /// Implementation of MonoBehavior.Start()
    /// </summary>
    void Start()
    {
        Log.w(LogTag.MAIN, "Loading scene " + SceneManager.GetActiveScene().name);

        updateCameraState();
    }

    /// <summary>
    /// Implementation of the MonoBehaviour's Update() function
    /// </summary>
    void Update()
    {
        processUserInput();
    }

    /// <summary>
    /// Get user input,  perform application functions (quit, pause, change cameras), and move the
    /// robot
    /// </summary>
    private void processUserInput()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
        else if (Input.GetKeyDown(KeyCode.R))
        {
            ApplicationManager.reloadScene();
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            followingRobot = !followingRobot;
            updateCameraState();
        }

        if (robot != null)
        {
            Rigidbody r = robot.GetComponent<Rigidbody>();
            float angle, forwardVelocity = 0.0f;
            Vector3 unusedAxis;
            r.rotation.ToAngleAxis(out angle, out unusedAxis);

            if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
            {
                forwardVelocity += WALK_SPEED;
            }

            if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
            {
                forwardVelocity -= WALK_SPEED;
            }

            if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
            {
                angle += 2.0f;
            }

            if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
            {
                angle -= 2.0f;
            }

            while (angle < 0.0)
                angle += 360f;

            while (angle > 360f)
                angle -= 360f;

            r.rotation = Quaternion.AngleAxis(angle, Vector3.up);

            Vector3 velocity = Vector3.Normalize(r.transform.forward) * forwardVelocity;
            velocity.y = r.velocity.y;
            r.velocity = velocity;
        }
    }

    /// <summary>
    /// Activate either the overview or follow cameras according to the value of followingRobot
    /// </summary>
    private void updateCameraState()
    {
        overviewCamera.enabled = !followingRobot;
        followCamera.enabled = followingRobot;
    }
}
