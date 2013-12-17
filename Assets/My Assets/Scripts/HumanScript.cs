using UnityEngine;
using System.Collections;

public class HumanScript : MonoBehaviour {
	
	private Vector3 syncStartPosition;
	private Vector3 syncEndPosition;
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
	Transform healthBar;
	LineRenderer line;
	Transform[] eyes = new Transform[2];
	Material clothes;
	
	// Use this for initialization
	void Start () {
		animator = GetComponent<Animator>();
		
		Transform[] trans = GetComponentsInChildren<Transform>();
		for (int i = 0; i < trans.Length; i++)
		{
			if (trans[i].tag == "Health")
				healthBar = trans[i];
		}
		
		if (!networkView.isMine)
		{
			animator.applyRootMotion = false;
		}		
		
		syncStartPosition = transform.position;
		syncEndPosition = transform.position;
		
		line = GetComponent<LineRenderer>();
		GameObject[] references = GameObject.FindGameObjectsWithTag("Eyes");
		for (int i = 0; i < references.Length; i++)
		{
			if (references[i].transform.parent.parent.parent.parent.parent.parent.parent.parent == transform &&
				references[i].name == "Ri_Eye_Mesh")
				eyes[0] = references[i].transform;
			else if (references[i].transform.parent.parent.parent.parent.parent.parent.parent.parent == transform &&
				references[i].name == "Le_Eye_Mesh")
				eyes[1] = references[i].transform;
		}
		
		clothes = GetComponentsInChildren<SkinnedMeshRenderer>()[2].materials[1];
		//clothes.SetColor("_Color", Color.cyan);
		
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
		
		//healthbar stuff
		CombatScript combat = GetComponent<CombatScript>();
		healthBar.localPosition = new Vector3(-.5f * (combat.maxHealth - combat.Health) / combat.maxHealth, 0, 0);
		healthBar.localScale = new Vector3(combat.Health / combat.maxHealth, 1, 2);
	
		//line render
		line.SetPosition(0, eyes[0].position);
		line.SetPosition(2, eyes[1].position);
	}
	
	private void InputMovement()
	{
		v = Input.GetAxis("Vertical");
		h = Input.GetAxis("Horizontal");
		
		if (Screen.lockCursor)
		{
			//mv = Input.GetAxis("Mouse Y");
			mh = Input.GetAxis("Mouse X");
					
			transform.Rotate(0, mh * 3f, 0);
			//rigidbody.AddTorque(0, mh * .5f, 0, ForceMode.Impulse);
		}
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
		//GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
		GameObject[] datas = GameObject.FindGameObjectsWithTag("PlayerData");
		for (int i = 0; i < datas.Length; i++)
		{
			//players[i].GetComponentInChildren<TextMesh>().transform.rotation = camera.transform.rotation;
			datas[i].transform.rotation = camera.transform.rotation;
		}
	}
	
	void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info)
	{	
		Vector3 networkPosition = Vector3.zero;
		Vector3 networkVelocity = Vector3.zero;
		Quaternion networkRotation = Quaternion.identity;
		Vector3 networkAngVelocity = Vector3.zero;
		
		if (stream.isWriting)
		{
			networkPosition = rigidbody.position;
			networkVelocity = rigidbody.velocity;
			networkRotation = rigidbody.rotation;
			networkAngVelocity = rigidbody.angularVelocity;
			//netV = v;
			//netH = h;
			//netD = mh;

			stream.Serialize(ref networkPosition);
			stream.Serialize(ref networkVelocity);
			stream.Serialize(ref networkRotation);
			stream.Serialize(ref networkAngVelocity);
			//stream.Serialize(ref netV);
			//stream.Serialize(ref netH);
			//stream.Serialize(ref netD);
		}
		else
		{		
			stream.Serialize(ref networkPosition);
			stream.Serialize(ref networkVelocity);
			stream.Serialize(ref networkRotation);
			stream.Serialize(ref networkAngVelocity);
			//stream.Serialize(ref netV);
			//stream.Serialize(ref netH);
			//stream.Serialize(ref netD);
			
			syncTime = 0f;
			syncDelay = Time.time - lastSyncTime;
			lastSyncTime = Time.time;
			
			syncStartPosition = rigidbody.position;
			syncEndPosition = networkPosition + networkVelocity * syncDelay;
			syncStartRotation = rigidbody.rotation;
			syncEndRotation = networkRotation;
			
			v = Vector3.Dot(syncStartPosition - syncEndPosition, -transform.forward) / syncDelay;
			h = Vector3.Dot(syncStartPosition - syncEndPosition, -transform.right) / syncDelay;
			
			if (Mathf.Abs(v) < .3f)
				v = 0;
			if (Mathf.Abs(h) < .3f)
				h = 0;
			//mh = netD;
		}
	}
	
	void OnCollisionEnter(Collision coll)
	{
			
	}
	
	[RPC]
	void SetupPlayer(NetworkViewID id, string name, int type)
	{
		GameObject player = NetworkView.Find(id).gameObject;
		GameObject gameGO = GameObject.Find("GameManagerGO");
		
		player.name = name;
		player.GetComponentInChildren<TextMesh>().text = name;
		
		player.GetComponent<AttackerScript>().enabled = false;
		player.GetComponent<AttractorScript>().enabled = false;
		player.GetComponent<HealerScript>().enabled = false;
		
		player.GetComponent<HumanScript>().clothes = player.GetComponentsInChildren<SkinnedMeshRenderer>()[2].materials[1];
		Color color = Color.black;

		switch (type)
		{
		case (int) NetworkManager.ClassType.Attacker:
			Debug.Log("0");
			player.GetComponent<AttackerScript>().enabled = true;
			color = Color.red;
			break;
		case (int) NetworkManager.ClassType.Attractor:
			Debug.Log("1");
			player.GetComponent<AttractorScript>().enabled = true;
			color = Color.blue;
			break;
		case (int) NetworkManager.ClassType.Healer:
			Debug.Log("2");
			player.GetComponent<HealerScript>().enabled = true;
			color = Color.green;
			break;
		default:
			Debug.Log("3");
			break;
		}
		player.GetComponent<HumanScript>().clothes.SetColor("_Color", color);
	}
}
