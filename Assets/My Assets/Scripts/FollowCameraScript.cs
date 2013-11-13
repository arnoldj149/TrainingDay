using UnityEngine;
using System.Collections;

public class FollowCameraScript : MonoBehaviour {
	
	public Transform target;
	
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		transform.position = target.position - target.forward * 2 + target.up * 1.5f;
		transform.LookAt(target.position + target.up * 1.5f);
	}
}
