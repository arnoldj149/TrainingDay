using UnityEngine;
using System.Collections;

public class FollowCameraScript : MonoBehaviour {
	
	public Transform target;
	private Vector2 previousMouse;
	private float pitch = 0;
	public float maxPitch = 60;
	
	// Use this for initialization
	void Start () {
		previousMouse = Input.mousePosition;
	}
	
	// Update is called once per frame
	void Update () {
		if (target != null)
		{
			transform.rotation = target.rotation;
			if (Screen.lockCursor)
			{
				pitch -= Input.GetAxis("Mouse Y") * 2f;
				if (pitch > maxPitch)
					pitch = maxPitch;
				else if (pitch < -maxPitch)
					pitch = -maxPitch;
					
				transform.Rotate(pitch, 0, 0);
			}
			transform.position = target.position + target.up * 1.8f - transform.forward * 2f;
		}

	}
}
