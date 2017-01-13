﻿using UnityEngine;

using Utilities;

public class TestMain : MonoBehaviour
{
	public GameObject robot;
	public Camera overallCamera;
	public Camera followCamera;
	public float WALK_SPEED = 5f;
	private bool followingRobot = false;

	private void Start()
	{
		updateCameraState();
	}

	private void Update()
	{
		processUserInput();
	}

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

	private void updateCameraState()
	{
		overallCamera.enabled = !followingRobot;
		followCamera.enabled = followingRobot;
	}
}
