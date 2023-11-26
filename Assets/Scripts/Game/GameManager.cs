using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
	public static GameManager instance { get; private set; }
    public Effects effectsInstance;

	public Transform runtimeEnemiesGroup;
	public Transform runtimeCollectiblesGroup;
	public Transform runtimeProjectilesGroup;


	private void Awake()
    {
        instance = this;

		// Initialize singletons here, because ScriptableObject that is not attached may not be
        // created and instance would not be set.
		Effects.instance = effectsInstance;


		runtimeEnemiesGroup = new GameObject("Enemies").transform;
		runtimeCollectiblesGroup = new GameObject("Collectibles").transform;
		runtimeProjectilesGroup = new GameObject("Projectiles").transform;

		Transform runtimeGroup = new GameObject("Runtime").transform;
		runtimeEnemiesGroup.parent = runtimeGroup;
		runtimeCollectiblesGroup.parent = runtimeGroup;
		runtimeProjectilesGroup.parent = runtimeGroup;

	}

	void Update()
	{
		if (Input.GetKey(KeyCode.T))
			Time.timeScale = 0.2f;
		else
			Time.timeScale = 1f;

		if (Input.GetKeyDown(KeyCode.U))
			Debug.Break();
	}
}