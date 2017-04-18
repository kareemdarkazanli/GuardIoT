using UnityEngine;

public class PinchZoom : MonoBehaviour
{
	public float perspectiveZoomSpeed = 0.5f;        // The rate of change of the field of view in perspective mode.
	public float orthoZoomSpeed = 0.5f;        // The rate of change of the orthographic size in orthographic mode.
	private GameObject parentObject;
	public Canvas _Canvas;
	Vector2?[] oldTouchPositions = {
		null,
		null
	};
	Vector2 oldTouchVector;
	float oldTouchDistance;

	private Vector3 originalPos;
	 
	void Start(){
		originalPos = transform.position;
		parentObject = new GameObject("ParentObject");
	}

	void Update()
	{
		if(!_Canvas.isActiveAndEnabled){
			// If there are two touches on the device...
			if (Input.touchCount == 0) {
				oldTouchPositions[0] = null;
				oldTouchPositions[1] = null;
			}
			else if (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Moved) {
				Vector2 touchDeltaPosition = Input.GetTouch(0).deltaPosition;
				transform.Translate(-touchDeltaPosition.x * 5, -touchDeltaPosition.y * 5, -touchDeltaPosition.y * 5);
			}

			else if (Input.touchCount == 2)
			{
				// Store both touches.
				Touch touchZero = Input.GetTouch(0);
				Touch touchOne = Input.GetTouch(1);

				// Find the position in the previous frame of each touch.
				Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
				Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

				// Find the magnitude of the vector (the distance) between the touches in each frame.
				float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
				float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

				// Find the difference in the distances between each frame.
				float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;

				// If the camera is orthographic...
				if (Camera.main.orthographic)
				{
					// ... change the orthographic size based on the change in distance between the touches.
					Camera.main.orthographicSize += deltaMagnitudeDiff * orthoZoomSpeed;

					// Make sure the orthographic size never drops below zero.
					Camera.main.orthographicSize = Mathf.Max(Camera.main.orthographicSize, 0.1f);
				}
				else
				{
					// Otherwise change the field of view based on the change in distance between the touches.
					Camera.main.fieldOfView += deltaMagnitudeDiff * perspectiveZoomSpeed;

					// Clamp the field of view to make sure it's between 0 and 180.
					Camera.main.fieldOfView = Mathf.Clamp(Camera.main.fieldOfView, 0.1f, 179.9f);
				}
			}


			if(Input.touchCount==1 && Input.GetTouch(0).phase == TouchPhase.Began && Input.GetTouch(0).tapCount==2)
			{
				parentObject.transform.localScale = Vector3.one;
				parentObject.transform.position = new Vector3(originalPos.x*-1, originalPos.y*-1, originalPos.z);
				transform.position = originalPos;
			}
		}


	}

}