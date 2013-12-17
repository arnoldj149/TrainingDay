using UnityEngine;
using System.Collections;

public class EnemyScript : MonoBehaviour {
	
	public GameObject explosionPrefab;
	
	private MeshRenderer attractFlag;
	
	public float counter = 0;
	private float findInterval = .5f;
	private bool attracted = false;
	public bool Attracted 
	{
		get { return attracted; }
	}
	private bool stunned = false;
	
	public float speed = 4;
	public float Speed
	{
		get
		{
			if (stunned)
				return 0;
			else if (!Attracted)
				return speed;
			else
				return speed * .25f;
		}
		set { speed = value; }
	}
	public float accel;
	public float Accel
	{
		get
		{
			if (stunned)
				return 0;
			else if (!Attracted)
				return accel; 
			else
				return accel * 2f;
		}
		set { accel = value; }
	}
	
	public Vector3 target;
	
	private Vector3 syncStartPosition = Vector3.zero;
	private Vector3 syncEndPosition = Vector3.zero;
	private Quaternion syncStartRotation = Quaternion.identity;
	private Quaternion syncEndRotation = Quaternion.identity;
	private float syncTime = 0f;
	private float lastSyncTime = 0f;
	private float syncDelay = 0f;
	
	// Use this for initialization
	void Start () 
	{
		FindTarget();
		syncStartPosition = transform.position;
		syncEndPosition = transform.position;
		attractFlag = GetComponentsInChildren<MeshRenderer>()[1];
		attractFlag.enabled = false;
	}
	
	// Update is called once per frame
	void Update () 
	{
		counter += Time.deltaTime;
		
		if (networkView.isMine)
		{
			if (counter > 0)
			{
				FindTarget();
				counter = -findInterval;
				attractFlag.enabled = false;
				attracted = false;
				stunned = false;
			}
			
			Vector3 delta = new Vector3(target.x - transform.position.x, 0, target.z - transform.position.z);
			
			//rigidbody.MovePosition(Vector3.Lerp(transform.position, target, 7));

			delta.Normalize();
			delta *= Accel;
			rigidbody.AddForce(delta, ForceMode.Impulse);
			if (rigidbody.velocity.magnitude > Speed)
			{
				Vector3 slow = -rigidbody.velocity;
				slow.Normalize();
				slow *= rigidbody.velocity.magnitude - Speed;
				
				rigidbody.AddForce(slow, ForceMode.Impulse);
			}
			
		}
		else
		{
			SyncedMovement();
			if (counter > 0)
			{
				counter = -findInterval;
				attractFlag.enabled = false;
				attracted = false;
				stunned = false;
			}
		}
		
	}
	
	private void SyncedMovement()
	{
		syncTime += Time.deltaTime;
		rigidbody.position = Vector3.Lerp(syncStartPosition, syncEndPosition , syncTime / syncDelay);
		rigidbody.rotation = Quaternion.Lerp(syncStartRotation, syncEndRotation, syncTime / syncDelay);
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

			stream.Serialize(ref networkPosition);
			stream.Serialize(ref networkVelocity);
			stream.Serialize(ref networkRotation);
			stream.Serialize(ref networkAngVelocity);
		}
		else
		{		
			stream.Serialize(ref networkPosition);
			stream.Serialize(ref networkVelocity);
			stream.Serialize(ref networkRotation);
			stream.Serialize(ref networkAngVelocity);
			
			syncTime = 0f;
			syncDelay = Time.time - lastSyncTime;
			lastSyncTime = Time.time;
			
			syncStartPosition = rigidbody.position;
			syncEndPosition = networkPosition + networkVelocity * syncDelay;
			syncStartRotation = rigidbody.rotation;
			syncEndRotation = networkRotation;
		}
	}
	
	void OnCollisionEnter(Collision coll)
	{

	}
	
	private void FindTarget()
	{
		GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
		if (players.Length > 0)
			target = players[Random.Range(0, players.Length)].transform.position;
		else
		{
			Explode();
			GameObject.Find("GameManagerGO").GetComponent<SpawnManager>().SpawnEnemies = false;
		}
	}
	
	public void AttractTo(Vector3 point)
	{
		target = point;
		networkView.RPC("AttractedTo", RPCMode.All, networkView.viewID);
	}
	
	[RPC]
	public void AttractedTo(NetworkViewID id)
	{
		if (networkView.viewID == id)
		{
			counter = -findInterval;
			attractFlag.enabled = true;
			attracted = true;
		}
	}
	
	public void Explode()
	{
		networkView.RPC("Exploded", RPCMode.All, transform.position, transform.rotation); 
		GetComponent<CombatScript>().Die();
	}
	
	[RPC]
	public void Exploded(Vector3 pos, Quaternion rot)
	{
		GameObject.Instantiate(explosionPrefab, pos, rot);
	}
	
	public void Stun(float time)
	{
		networkView.RPC("Stunned", RPCMode.All, networkView.viewID, time);
	}
	
	[RPC]
	public void Stunned(NetworkViewID id, float time)
	{
		if (networkView.viewID == id)
		{
			counter = -time;
			attractFlag.enabled = true;
			stunned = true;
		}
	}
}
