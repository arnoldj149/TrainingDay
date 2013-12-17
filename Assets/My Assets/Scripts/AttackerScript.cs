using UnityEngine;
using System.Collections;

public class AttackerScript : MonoBehaviour {
	
	public GameObject explosionPrefab;
	
	LineRenderer line;
	float timer = 0;
	public float fireRate = .35f;
	
	float explosionRad = 2;
	float explosionForce = 150;
	
	// Use this for initialization
	void Start () {
		line = GetComponent<LineRenderer>();
	}
	
	// Update is called once per frame
	void Update () {
		
		if (networkView.isMine)
		{
			if (Input.GetMouseButton(0) && timer > fireRate)
				Attack();
			
		}
			timer += Time.deltaTime;
			if (timer > .2f)
			{
				line.enabled = false;
			}
		
			line.SetWidth(.101f - timer * .5f, .101f - timer * .5f);
	}
	
	void Attack ()
	{
		Camera cam = Camera.main;
		RaycastHit hit;
		
		if (Physics.Raycast(cam.transform.position, cam.transform.forward, out hit))
		{
			GameObject target = hit.collider.gameObject;
			if (target.tag == "Enemy" && hit.collider.isTrigger)
			{
				networkView.RPC("Attacked", RPCMode.All, networkView.viewID, target.networkView.viewID, hit.point);
			}
			else
			{
				networkView.RPC("Attacked", RPCMode.All, networkView.viewID, networkView.viewID, hit.point);
			}
		}
			
	}
	
	[RPC]
	void Attacked (NetworkViewID id, NetworkViewID target, Vector3 hit)
	{
		GameObject attacker = NetworkView.Find(id).gameObject;
		GameObject other = NetworkView.Find(target).gameObject;
		LineRenderer line = attacker.GetComponent<LineRenderer>();
		
		line.enabled = true;
		line.SetPosition(1, hit);
		line.material.SetColor("_Color", Color.red);
		timer = 0;
		
		Collider[] effected = Physics.OverlapSphere(hit, explosionRad);
		
		if (Network.isServer)
		{
			for (int i = 0; i < effected.Length; i++)
			{
				//effected[i].gameObject.rigidbody.AddExplosionForce(explosionForce, hit, explosionRad);
			}
		}
		
		GameObject.Instantiate(explosionPrefab, hit, Quaternion.identity);
		
		if (id != target && other.networkView.isMine)
			other.GetComponent<CombatScript>().AlterHealth(-1);
	}
}
