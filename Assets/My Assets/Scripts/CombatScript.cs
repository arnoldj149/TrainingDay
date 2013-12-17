using UnityEngine;
using System.Collections;

public class CombatScript : MonoBehaviour {
	
	public float maxHealth = 10;
	private float health;
	public float Health
	{
		get { return health; }
		set 
		{ 
			health = value; 
			if (health > maxHealth)
				health = maxHealth;
			else if (health < 0)
				health = 0;
		}
	}
	public bool Alive
	{
		get { return health > 0;}
	}
	
	// Use this for initialization
	void Start () {
		health = maxHealth;
	}
	
	// Update is called once per frame
	void Update () {
		if (!Alive && networkView.isMine)
		{
			Die();
			//Network.DestroyPlayerObjects(networkView.viewID.owner);
		}
	}
	
	void OnCollisionEnter(Collision coll)
	{
		if (tag == "Player" && coll.gameObject.tag == "Enemy" && networkView.isMine)
		{
			AlterHealth(-1);
			coll.gameObject.GetComponent<EnemyScript>().Explode();
		}
	}
	
	public void Die()
	{
		if (tag == "Player")
		{
			Network.RemoveRPCs(networkView.viewID.owner, 1);
			Network.Destroy(gameObject);
		}	
		else if (tag == "Enemy")
		{
			Network.RemoveRPCs(networkView.viewID);
			Network.Destroy(gameObject);
		}
	}
	
	public void AlterHealth(float delta)
	{
		Health += delta;
		networkView.RPC("HealthAltered",RPCMode.Others, networkView.viewID, Health);
	}
	
	[RPC]
	void HealthAltered(NetworkViewID id, float health)
	{
		GameObject combatant = NetworkView.Find(id).gameObject;
		combatant.GetComponent<CombatScript>().Health = health;
		
	}
}
