using UnityEngine;

namespace _193396
{
	[CreateAssetMenu(menuName = "193396/Data/Terrain")]
	public class TerrainData : ScriptableObject
	{
		[System.Serializable]
		public struct GroundDropping
		{
			[Tooltip("Time before droppable platform breaks")]
			public float shakeTime;
			[Tooltip("Time needed for ground to respawn")]
			public float respawnTime;

			[Space(10)]

			[Tooltip("Object to instantiate for every region")]
			public GameObject prefab;

			[Space(5)]

			[Tooltip("Effect to play on default")]
			public GameObject idleEffectPrefab;
			[Tooltip("Effect to play on shake")]
			public GameObject shakeEffectPrefab;
			[Tooltip("Effect to play on break")]
			public GameObject breakEffectPrefab;
		}
		[System.Serializable]
		public struct GroundBreaking
		{
			[Tooltip("Time needed for ground to respawn")]
			public float respawnTime;

			[Space(10)]

			[Tooltip("Object to instantiate for every region")]
			public GameObject prefab;

			[Space(5)]

			[Tooltip("Effect to play on break")]
			public GameObject breakEffectPrefab;
		}
		[System.Serializable]
		public struct GroundMoving
		{
			[Tooltip("Layers that slide with the ground")]
			public RuntimeSettings.LayerMaskInput slidingLayers;
		}

		[Tooltip("Layers that prevent ground respawn")]
		public RuntimeSettings.LayerMaskInput collidingLayers;
		[Space(5)]
		public GroundDropping groundDropping;
		public GroundBreaking groundBreaking;
		public GroundMoving groundMoving;
	}
}