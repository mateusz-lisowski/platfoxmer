using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows;

namespace _193396
{
	[RequireComponent(typeof(PlayerMovement))]
	public class PlayerRecord : PlayerInput
	{
		private class InputData
		{
			public InputData(string name_)
			{
				name = name_;
			}

			public string name;
			public int lastFrame = -1;
			public float holdEnd = 0f;
		}

		private PlayerMovement movement;

		private InputData moveUp;
		private InputData moveDown;
		private InputData moveLeft;
		private InputData moveRight;
		private InputData jump;
		private InputData dash;
		private InputData attack;
		private InputData attackDown;

		private Vector2 startPosition;


		public override void onAwake()
		{
			movement = controller.getBehavior<PlayerMovement>();
		}
		public override void onStart()
		{
			base.onStart();

			startPosition = transform.position;
			restart();
		}
		public override void onDisable()
		{
			base.onDisable();
			restart();
		}
		private void restart()
		{
			transform.position = startPosition;

			moveUp = new InputData("moveUp");
			moveDown = new InputData("moveDown");
			moveLeft = new InputData("moveLeft");
			moveRight = new InputData("moveRight");
			jump = new InputData("jump");
			dash = new InputData("dash");
			attack = new InputData("attack");
			attackDown = new InputData("downAttack");
		}

		public override void onUpdate()
		{
			isInputMoveUp = isEnabled && isDown(moveUp);
			isInputMoveDown = isEnabled && isDown(moveDown);
			isInputMoveLeft = isEnabled && isDown(moveLeft);
			isInputMoveRight = isEnabled && isDown(moveRight);

			isInputJump = isEnabled && isTyped(jump);
			isInputJumpReleased = !isEnabled || !isDown(jump);

			isInputDash = isEnabled && isTyped(dash);
			isInputDashReleased = !isEnabled || !isDown(dash);

			isInputPassthrough = false;

			isInputAttack = isEnabled && isTyped(attack);
			isInputAttackDown = isEnabled && isDown(attackDown);
		}

		public override string[] capturableEvents => new string[] { "input", "restart" };
		public override void onEvent(string eventName, object eventData)
		{
			if (eventName == "restart")
			{
				restart();
				return;
			}

			string data = (string)eventData;

			Debug.Log(transform.parent.parent.name + " (PlayerRecord) at " + Time.time
				+ ": " + eventName + ", " + data);

			if (tryParseEvent(ref moveUp, data)) return;
			if (tryParseEvent(ref moveDown, data)) return;
			if (tryParseEvent(ref moveLeft, data)) return;
			if (tryParseEvent(ref moveRight, data)) return;
			if (tryParseEvent(ref jump, data)) return;
			if (tryParseEvent(ref dash, data)) return;
			if (tryParseEvent(ref attack, data)) return;
			if (tryParseEvent(ref attackDown, data)) return;

			if (data == "bounce")
			{
				movement.registeredDownHitJump = true;
				movement.registeredDownHitHighJump = true;
			}
		}

		private bool tryParseEvent(ref InputData data, string eventData)
		{
			if (!eventData.StartsWith(data.name))
				return false;

			data.lastFrame = controller.currentUpdate;

			string rest = eventData.Substring(data.name.Length);
			if (rest != "")
			{
				rest = rest.Substring(1);

				data.holdEnd = Time.time + float.Parse(rest);
			}

			return true;
		}
	
		private bool isTyped(InputData data)
		{
			return data.lastFrame + 1 == controller.currentUpdate;
		}
		private bool isDown(InputData data)
		{
			return data.holdEnd > Time.time;
		}
	}
}