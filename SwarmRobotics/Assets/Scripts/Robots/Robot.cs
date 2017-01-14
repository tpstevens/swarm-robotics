﻿using UnityEngine;

using Utilities;

namespace Robotics
{
	public class Robot {

		public GameObject body;

		private readonly float TIMER_PERIOD = 2.0f;
		private readonly float VELOCITY = 2.0f;

		private bool collided = false;
		private float timer = 0.5f;
		private int id;
		private Rigidbody rigidbody;

		public Robot(int id, GameObject body, Vector3 startPosition, float startRotation)
		{
			this.body = body;
			this.id = id;

			body.name = "Robot " + id;
			body.transform.position = startPosition;
			body.transform.parent = GameObject.Find("Robots").transform;
			rigidbody = body.GetComponent<Rigidbody>();

			if (rigidbody == null)
			{
				Log.e(LogTag.ROBOTICS, "Failed to find rigidbody on robot " + id);
			}
			else
			{
				rigidbody.position = startPosition;
				rigidbody.rotation = Quaternion.AngleAxis(startRotation, Vector3.up);
			}
		}

		public void handleCollision()
		{
			collided = true;
		}

		public void update()
		{
			timer -= Time.deltaTime;

			if (timer <= 0.0f)
			{
				timer = TIMER_PERIOD;
				randomizeDirection();

				// body.GetComponent<Renderer>().material.color = Color.red;
			}

			if (collided)
			{
				collided = false;
				body.GetComponent<Renderer>().material.color = Color.gray;

				// TODO: on collision, robot turns and moves in the opposite direction
				// TODO: add arrows so we can see robot direction

				// rigidbody.AddForce(-1 * rigidbody.velocity, ForceMode.VelocityChange);
				// rigidbody.transform.rotation.SetLookRotation(rigidbody.velocity, Vector3.up);
				// Vector3 velocity = Vector3.Normalize(rigidbody.transform.forward) * VELOCITY;
				// rigidbody.AddForce(velocity, ForceMode.VelocityChange);

				randomizeDirection();
				timer = TIMER_PERIOD;
			}
		}

		private void randomizeDirection()
		{
			float angle = Random.Range(0, 359);
			rigidbody.transform.rotation = Quaternion.AngleAxis(angle, Vector3.up);

			Vector3 velocity = Vector3.Normalize(rigidbody.transform.forward) * VELOCITY;
			rigidbody.AddForce(-1 * rigidbody.velocity, ForceMode.VelocityChange);
			rigidbody.AddForce(velocity, ForceMode.VelocityChange);
		}
	}
}

/* NOTES
 *
 * All robots add themselves to global Robot array on creation? To avoid lookups
 * on collisions.
 * 
 */