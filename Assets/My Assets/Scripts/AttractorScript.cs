using UnityEngine;
using System.Collections;

public class AttractorScript : MonoBehaviour {
	
	public GameObject attractPrefab;
	
	float attractTimer = 0;
	float stunTimer = 0;
	public float attractRate = 2f;
	public float stunRate = 1f;
	public float stunTime = 8f;
		
	LineRenderer line;
	Transform[] eyes = new Transform[2];
	
	// Use this for initialization
	void Start () {
		line = GetComponent<LineRenderer>();
	}
	
	// Update is called once per frame
	void Update () {
		if (networkView.isMine)
		{
			if (Input.GetMouseButton(1) && attractTimer > attractRate)
				Attract();
			if (Input.GetMouseButton(0) && stunTimer > stunRate)
				Stun();
		}
		
		attractTimer += Time.deltaTime;
		stunTimer += Time.deltaTime;
		if (stunTimer > .2f)
		{
			line.enabled = false;
		}
		
		line.SetWidth((attractTimer % .05f), (attractTimer % .05f));
	}
		
	void Attract ()
	{
		networkView.RPC("Attracted", RPCMode.All, networkView.viewID, transform.position);
		attractTimer = 0;
	}
	
	[RPC]
	void Attracted (NetworkViewID id, Vector3 focus)
	{
		GameObject.Instantiate(attractPrefab, focus, Quaternion.identity);
	}
	
	void Stun ()
	{
		Camera cam = Camera.main;
		RaycastHit hit;
		
		if (Physics.Raycast(cam.transform.position, cam.transform.forward, out hit))
		{
			GameObject target = hit.collider.gameObject;
			if (target.tag == "Enemy" && hit.collider.isTrigger)
			{
				networkView.RPC("Stunned", RPCMode.All, networkView.viewID, target.networkView.viewID, hit.point);
			}
			else
			{
				networkView.RPC("Stunned", RPCMode.All, networkView.viewID, networkView.viewID, hit.point);
			}
		}
			
	}
	
	[RPC]
	void Stunned (NetworkViewID id, NetworkViewID target, Vector3 hit)
	{
		GameObject attacker = NetworkView.Find(id).gameObject;
		LineRenderer line = attacker.GetComponent<LineRenderer>();
		
		line.enabled = true;
		line.SetPosition(1, hit);
		line.material.SetColor("_Color", Color.blue);
		stunTimer = 0;
		
		if (target != id)
		{
			GameObject other = NetworkView.Find(target).gameObject;
			other.GetComponent<EnemyScript>().Stun(stunTime);
		}
	}
}
