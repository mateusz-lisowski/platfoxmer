using System.Collections;
using UnityEditor;
using UnityEngine;

namespace _193396
{
	[RequireComponent(typeof(Animator))]
    public class EndingController : MonoBehaviour
    {
		public CameraController cameraController;
		public Transform beginTarget;
		[Space(5)]
		public float speed = 12f;

		private GameObject moveUpTarget;
		private Vector3 lastTargetPosition;


		public void focus()
		{
			moveUpTarget = new GameObject("Camera Follow Up");
			moveUpTarget.transform.position = lastTargetPosition;
			moveUpTarget.transform.parent = GameManager.instance.runtimeGroup[GameManager.RuntimeGroup.Effects];

			cameraController.pushTarget(moveUpTarget.transform);
		}
		public void startMovingUp()
		{
			cameraController.boundary = null;
			StartCoroutine(moveUp(moveUpTarget.transform));
		}
		public void showScore()
		{
			transform.Find("game-summary").gameObject.SetActive(true);
		}
		public void quit()
		{
			MergeController.loadMainMenu();
		}

		private void Awake()
		{
			transform.GetComponent<Animator>().SetBool("isMerged", MergeController.isMerged());
		}

		private void Update()
		{
			if (beginTarget != null)
				lastTargetPosition = beginTarget.transform.position;
		}


		private IEnumerator moveUp(Transform transform)
		{
			while (true)
			{
				yield return null;
				Vector3 position = transform.position;
				position.y += speed * Time.deltaTime;
				transform.position = position;
			}
		}
	}
}