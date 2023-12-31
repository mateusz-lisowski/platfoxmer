using UnityEngine;

namespace _193396
{
	[CreateAssetMenu(menuName = "193396/Data/Behavior/Hurt")]
	public class HurtBehaviorData : ScriptableObject
	{
		[Tooltip("Layer detecting whether the ground can instantly kill")]
		public RuntimeSettings.LayerMaskInput canInstaKillLayer;
		[Tooltip("Name identifying the entity")]
		public string killEventName;

		[Space(10)]

		[Tooltip("Time of invulnerability after getting hit")]
		public float invulnerabilityTime = 0.0f;
		[Tooltip("Time to ignore input after getting hit")]
		public float distressTime = 0.3f;

		[Space(5)]

		[Tooltip("Cumulative strength of received attacks resulting in death")]
		public int health = 3;
		[Tooltip("Maximum strength of an attack that is blocked")]
		public int maxBlock = 0;
		[Tooltip("Is changing direction to face towards incoming projectile")]
		public bool faceHit = true;
	}
}