using UnityEngine;
using System.Collections;

public class HealerScript : MonoBehaviour {
	
	LineRenderer line;
	private float timer = 0;
	public float healAmount = .2f;
	public float healRate = .1f;
	
	// Use this for initialization
	void Start () {
		line = GetComponent<LineRenderer>();
		
	}
	
	// Update is called once per frame
	void Update () {
		if (networkView.isMine)
		{
			if (Input.GetMouseButton(0) && timer > healRate)
				Heal();
		}
		timer += Time.deltaTime;
		
		if (timer > .2f)
		{
			line.enabled = false;
		}
		
		line.SetWidth(.07f, .07f);
		line.material.SetColor("_Color", new Color(line.material.color.r, line.material.color.g, line.material.color.b, timer * 5f));
		
	}
		
	void Heal ()
	{
		Camera cam = Camera.main;
		RaycastHit hit;
		
		if (Physics.Raycast(cam.transform.position, cam.transform.forward, out hit))
		{
			GameObject target = hit.collider.gameObject;
			if (target.tag == "Player")
			{
				networkView.RPC("Healed", RPCMode.All, networkView.viewID, target.networkView.viewID, hit.point);
			}
			else
			{
				networkView.RPC("Healed", RPCMode.All, networkView.viewID, networkView.viewID, hit.point);
			}
		}
	}
	
	[RPC]
	void Healed (NetworkViewID id, NetworkViewID target, Vector3 hit)
	{
		GameObject healer = NetworkView.Find(id).gameObject;
		GameObject other = NetworkView.Find(target).gameObject;
		LineRenderer line = healer.GetComponent<LineRenderer>();
		
		line.enabled = true;
		line.SetPosition(1, hit);
		line.material.SetColor("_Color", Color.green);
		timer = 0;
		
		if (id != target && other.networkView.isMine)
			other.GetComponent<CombatScript>().AlterHealth(healAmount);
	}
}
