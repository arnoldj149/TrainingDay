using UnityEngine;
using System.Collections;

public class AttractionScript : MonoBehaviour {

	float duration = 5f;
	float attractRad = 3;
	
	// Use this for initialization
	void Start () {
		DestroyObject(gameObject, duration);
		transform.localScale = new Vector3(transform.localScale.x * 4, transform.localScale.y,transform.localScale.z * 4);
	}
	
	// Update is called once per frame
	void Update () {
		
		Collider[] effected = Physics.OverlapSphere(transform.position, attractRad);
		
		if (Network.isServer)
		{
			for (int i = 0; i < effected.Length; i++)
			{
				if (effected[i].gameObject.tag == "Enemy" && !effected[i].GetComponent<EnemyScript>().Attracted)
				{
					effected[i].GetComponent<EnemyScript>().AttractTo(transform.position);
				}
			}
		}
		
		transform.localScale = new Vector3(transform.localScale.x * .99f, transform.localScale.y,transform.localScale.z * .99f);
		renderer.material.color = Color.Lerp(renderer.material.color, Color.white, .04f / duration);
	}
}
