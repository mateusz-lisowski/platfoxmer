using UnityEngine;

namespace _193396
{
	[RequireComponent(typeof(FlipBehavior))]
	[RequireComponent(typeof(GroundedBehavior))]
	public class JumpAtBehavior : EntityBehavior
	{
		public JumpAtBehaviorData data;
		public Transform target;
		[field: Space(10)]
		[field: SerializeField, ReadOnly] public bool isJumping { get; private set; }
		[field: Space(5)]
		[field: SerializeField, ReadOnly] public float jumpCooldown { get; private set; }
		[field: Space(10)]
		[field: SerializeField, ReadOnly] public float jumpSpeed { get; private set; }

		private FlipBehavior direction;
		private GroundedBehavior ground;


		public override void onAwake()
		{
			direction = controller.getBehavior<FlipBehavior>();
			ground = controller.getBehavior<GroundedBehavior>();
		}

		public override void onUpdate()
		{
			jumpCooldown -= Time.deltaTime;

			updateJump();
		}

		public override bool onFixedUpdate()
		{
			if (!isJumping)
				ground.tryStopSlopeFixedUpdate();

			addSmoothForce(isJumping ? jumpSpeed : 0f, 1f, transform.right);

			return true;
		}


		private bool canJump()
		{
			return jumpCooldown <= 0 && ground.isGrounded && target != null;
		}
		private void jump()
		{
			isJumping = true;
			jumpCooldown = data.cooldown;

			direction.faceTowards(target.position);

			float xt = Mathf.Max(Mathf.Abs(target.position.x - transform.position.x), 0.01f);
			float yt = Mathf.Min(target.position.y - transform.position.y, xt * data.eccentricity);

			float height = Mathf.Max((4 * data.eccentricity * xt * xt) / (4 * data.eccentricity * xt - yt) * data.eccentricity, 
				data.minHeight);
			float force = Mathf.Sqrt(2 * -Physics2D.gravity.y * height);

			float distance = yt == 0f ? xt : 2 * xt * (height - Mathf.Sqrt(height * (height - yt))) / yt;

			float t = 2 * force / -Physics2D.gravity.y;
			jumpSpeed = distance / t;

			if (force > controller.rigidBody.velocity.y)
			{
				force -= controller.rigidBody.velocity.y;
				controller.rigidBody.AddForce(force * Vector2.up, ForceMode2D.Impulse);
			}

			drawParabola(controller.rigidBody.position, 
				controller.rigidBody.velocity + jumpSpeed * (Vector2)transform.right, 
				Physics2D.gravity, t);

			controller.onEvent("jumped", null);
		}
		private void updateJump()
		{
			if (ground.isGrounded)
			{
				if (isJumping)
				{
					if (Vector2.Distance(transform.position, target.position) > 1f)
						direction.faceTowards(target.position);
					else if (Vector2.Dot(transform.right, target.right) < -0.01f)
						direction.flip();

					if (data.autoResetTarget)
						target = null;
				}

				isJumping = false;
			}

			if (canJump())
			{
				jump();
			}

			if (isJumping && !ground.isFalling)
				ground.disableGroundedNextFrame();
		}

		private void drawParabola(Vector2 position, Vector2 velocity, Vector2 acceleration, float time)
		{
			Vector2 startPosition = position;

			for (float t = 0.1f; t < time + 0.1f / 2; t += 0.1f)
			{
				Vector2 nextPosition = startPosition + (velocity + 0.5f * acceleration * t) * t;

				Debug.DrawLine(position, nextPosition, Color.red, time);

				position = nextPosition;
			}
		}
	}
}