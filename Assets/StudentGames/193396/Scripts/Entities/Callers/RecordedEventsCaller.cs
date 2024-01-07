using System;
using System.Collections.Generic;
using UnityEngine;

namespace _193396
{
	public class RecordedEventsCaller : MonoBehaviour
	{
		public EntityBehaviorController controller;

		[Serializable]
		public struct RecordedEvent
		{
			public string eventName;
			public string eventData;
			[Space(5)]
			public float timeOffset;
		}

		[field: Space(10)]
		public float restartTime = float.PositiveInfinity;
		public List<RecordedEvent> events;

		[Space(10)]
		[SerializeField, ReadOnly] private float currentTime = 0f;
		[SerializeField, ReadOnly] private int currentEvent = 0;
		[SerializeField, ReadOnly] private float currentEventTime = 0f;


		private void restart()
		{
			currentTime = 0f;
			currentEvent = 0;
			currentEventTime = 0f;
		}

		private void OnEnable()
		{
			restart();
		}
		private void Update()
		{
			currentTime += Time.deltaTime;

			while (currentEvent < events.Count)
			{
				if (currentEventTime > currentTime)
					break;

				var e = events[currentEvent];

				controller.onEvent(e.eventName, e.eventData);
				currentEvent++;
				if (currentEvent < events.Count)
					currentEventTime += events[currentEvent].timeOffset;
				else
					currentEventTime = float.PositiveInfinity;
			}

			if (currentTime >= restartTime)
				restart();
		}
	}
}