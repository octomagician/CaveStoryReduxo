using UnityEngine;

public class CameraBehaviour : MonoBehaviour {

	public Transform target;
	public float dampingTime = 1f;

	private Vector3 velocity;
	private Vector3 proxyPosition;

	private void LateUpdate() {
		if (target == null) {
			return;
		}

		proxyPosition = Vector3.SmoothDamp(proxyPosition, target.position, ref velocity, dampingTime);
		transform.position = new Vector3(proxyPosition.x, proxyPosition.y, -10f);
	}

}
