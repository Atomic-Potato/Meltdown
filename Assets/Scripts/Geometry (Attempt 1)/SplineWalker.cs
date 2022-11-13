using UnityEngine;

public class SplineWalker : MonoBehaviour {

	public BezierSpline spline;

	public float speed;

	private float progress;

	private void Update () {
		progress += speed * Time.deltaTime;
		if (progress > 1f) {
			progress = 1f;
		}
        Vector3 position = spline.GetPoint(progress);
		transform.localPosition = position;
        transform.LookAt(position + spline.GetDirection(progress));
	}
}