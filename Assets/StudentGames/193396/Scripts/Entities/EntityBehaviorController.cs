using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace _193396
{
	public struct EntityMessage
	{
		public EntityMessage(string name_, object data_)
		{
			name = name_;
			data = data_;
		}

		public string name;
		public object data;
	}

	public abstract class EntityEventReceiver : MonoBehaviour
	{
		[Serializable]
		public struct InputtableEvent
		{
			public static bool matches(string name, string data, string eventName, object eventData)
			{
				return name == eventName && (data == "" || (eventData != null && data == eventData.ToString()));
			}
			public bool matches(string eventName, object eventData)
			{
				return matches(name, data, eventName, eventData);
			}

			public string name;
			public string data;
		}

		public virtual string[] capturableEvents { get => Array.Empty<string>(); }
		public virtual void onEvent(string eventName, object eventData) { }
	}

	[RequireComponent(typeof(EntityBehaviorController))]
	public abstract class EntityBehavior : EntityEventReceiver
	{
		public void addSmoothForce(float targetSpeed, float accelerationCoefficient, Vector2 direction)
		{
			float speedDif = targetSpeed - Vector2.Dot(controller.rigidBody.velocity, direction);
			float movement = speedDif * accelerationCoefficient / Time.fixedDeltaTime;

			controller.rigidBody.AddForce(movement * direction, ForceMode2D.Force);
		}

		public virtual void onAwake() { }
		public virtual void onStart() { }
		public virtual void onUpdate() { }
		public virtual bool onFixedUpdate() { return false; }

		public void setController(EntityBehaviorController parent)
		{
			controller = parent;
		}
		public EntityBehaviorController controller { get; private set; }

		public void disableCurrentFixedUpdate()
		{
			lastFixedUpdateDisabled = controller.currentFixedUpdate;
		}
		public bool currentFixedUpdateDisabled()
		{
			return lastFixedUpdateDisabled == controller.currentFixedUpdate;
		}
		private int lastFixedUpdateDisabled = -1;
	}

	public class EntityBehaviorController : MonoBehaviour
	{
		private List<EntityBehavior> behaviors;

		public Rigidbody2D rigidBody { get; private set; }
		public Animator animator { get; private set; }
		public SpriteRenderer spriteRenderer { get; private set; }
		public Transform hitbox { get; private set; }

		public int currentUpdate { get; private set; }
		public int currentFixedUpdate { get; private set; }

		private Dictionary<string, List<EntityEventReceiver>> eventDispatcher;


		public B getBehavior<B>() where B : EntityBehavior
		{
			return (B)behaviors.Find(b => b.GetType() == typeof(B));
		}
		public List<B> getBehaviors<B>() where B : EntityBehavior
		{
			return behaviors.FindAll(b => b.GetType() == typeof(B)).Cast<B>().ToList();
		}

		private void Awake()
		{
			rigidBody = transform.GetComponent<Rigidbody2D>();
			animator = transform.Find("Sprite").GetComponent<Animator>();
			spriteRenderer = transform.Find("Sprite").GetComponent<SpriteRenderer>();
			hitbox = transform.Find("Hitbox")?.GetComponent<Transform>();

			currentUpdate = 0;
			currentFixedUpdate = 0;

			behaviors = new List<EntityBehavior>();

			foreach (var behavior in transform.GetComponents<EntityBehavior>())
			{
				behavior.setController(this);

				behaviors.Add(behavior);
			}

			foreach (var behavior in behaviors)
				behavior.onAwake();

			initializeEventDispatcher();
		}
		private void Start()
		{
			foreach (var behavior in behaviors)
				behavior.onStart();
		}
		private void Update()
		{
			currentUpdate++;

			foreach (var behavior in behaviors)
				behavior.onUpdate();
		}
		private void FixedUpdate()
		{
			currentFixedUpdate++;

			foreach (var behavior in behaviors)
				if (!behavior.currentFixedUpdateDisabled())
					if (behavior.onFixedUpdate())
						break;
		}

		public void onEvent(string eventName, object eventData)
		{
			if (eventDispatcher.ContainsKey(eventName))
				foreach (var receiver in eventDispatcher[eventName])
					receiver.onEvent(eventName, eventData);

			if (eventName == "destroy")
				Destroy(gameObject);
		}
		public void onMessage(EntityMessage msg)
		{
			onEvent(msg.name, msg.data);
		}


		private void initializeEventDispatcher()
		{
			eventDispatcher = new Dictionary<string, List<EntityEventReceiver>>();

			foreach (var receiver in transform.GetComponentsInChildren<EntityEventReceiver>())
				foreach (var capturableEvent in receiver.capturableEvents)
					if (!eventDispatcher.ContainsKey(capturableEvent))
						eventDispatcher.Add(capturableEvent, new List<EntityEventReceiver> { receiver });
					else
					{
						var dispatcher = eventDispatcher[capturableEvent];
						if (!dispatcher.Contains(receiver))
							dispatcher.Add(receiver);
					}
		}
	}
}