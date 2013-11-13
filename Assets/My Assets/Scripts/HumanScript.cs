using UnityEngine;
using System.Collections;

public class HumanScript : MonoBehaviour {
	
	private Vector3 syncStartPosition = Vector3.zero;
	private Vector3 syncEndPosition = Vector3.zero;
	private Quaternion syncStartRotation = Quaternion.identity;
	private Quaternion syncEndRotation = Quaternion.identity;
	private float syncTime = 0f;
	private float lastSyncTime = 0f;
	private float syncDelay = 0f;
	
	float v = 0;
	float h = 0;
	//float mv = 0;
	float mh = 0;
	
	Animator animator;
	
	// Use this for initialization
	void Start () {
		animator = GetComponent<Animator>();
		if (!networkView.isMine)
		{
			animator.applyRootMotion = false;
		}
		
	}
	
	// Update is called once per frame
	void Update () {
		
		if (networkView.isMine)
		{
			InputMovement();
		}
		else
		{
			SyncedMovement();
		}
		
		Face3dGUI();
		
		animator.SetFloat("SpeedV", v);
		animator.SetFloat("SpeedH", h);
		animator.SetFloat("Speed", Mathf.Sqrt(v * v + h * h));
		animator.SetFloat("Direction", mh);
	}
	
	private void InputMovement()
	{
		v = Input.GetAxis("Vertical");
		h = Input.GetAxis("Horizontal");
		//mv = Input.GetAxis("Mouse Y");
		mh = Input.GetAxis("Arrow Mouse X");
				
		rigidbody.AddTorque(0, mh, 0, ForceMode.Impulse);
	}
		
	private void SyncedMovement()
	{
		syncTime += Time.deltaTime;
		rigidbody.position = Vector3.Lerp(syncStartPosition, syncEndPosition , syncTime / syncDelay);
		rigidbody.rotation = Quaternion.Lerp(syncStartRotation, syncEndRotation, syncTime / syncDelay);
	}
	
	private void Face3dGUI()
	{
		GameObject camera = GameObject.FindGameObjectWithTag("MainCamera");
		GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
		for (int i = 0; i < players.Length; i++)
		{
			players[i].GetComponentInChildren<TextMesh>().transform.rotation = camera.transform.rotation;
		}
	}
	
	void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info)
	{	
		Vector3 networkPosition = Vector3.zero;
		Vector3 networkVelocity = Vector3.zero;
		Quaternion networkRotation = Quaternion.identity;
		Vector3 networkAngVelocity = Vector3.zero;
		float netV = 0;
		float netH = 0;
		float netD = 0;
		
		if (stream.isWriting)
		{
			networkPosition = rigidbody.position;
			networkVelocity = rigidbody.velocity;
			networkRotation = rigidbody.rotation;
			networkAngVelocity = rigidbody.angularVelocity;
			netV = v;
			netH = h;
			netD = mh;

			stream.Serialize(ref networkPosition);
			stream.Serialize(ref networkVelocity);
			stream.Serialize(ref networkRotation);
			stream.Serialize(ref networkAngVelocity);
			stream.Serialize(ref netV);
			stream.Serialize(ref netH);
			stream.Serialize(ref netD);
		}
		else
		{		
			stream.Serialize(ref networkPosition);
			stream.Serialize(ref networkVelocity);
			stream.Serialize(ref networkRotation);
			stream.Serialize(ref networkAngVelocity);
			stream.Serialize(ref netV);
			stream.Serialize(ref netH);
			stream.Serialize(ref netD);
			
			syncTime = 0f;
			syncDelay = Time.time - lastSyncTime;
			lastSyncTime = Time.time;
			
			syncStartPosition = rigidbody.position;
			syncEndPosition = networkPosition + networkVelocity * syncDelay;
			syncStartRotation = rigidbody.rotation;
			syncEndRotation = networkRotation;
			
			v = netV;
			h = netH;
			mh = netD;
		}
	}
	
	[RPC]
	void SetupPlayer(NetworkViewID id, string name)
	{
		GameObject player = NetworkView.Find(id).gameObject;
		
		player.name = name;
		player.GetComponentInChildren<TextMesh>().text = name;
	}
}
