using UnityEngine;
using System.Collections;

public class ExplosionScript : MonoBehaviour {
	
	public float duration = .2f;
	
	// Use this for initialization
	void Start () {
		DestroyObject(gameObject, duration);
	}
	
	// Update is called once per frame
	void Update () {
		transform.localScale *= 1.1f;
		
		renderer.material.color = Color.Lerp(renderer.material.color, Color.clear, .05f / duration);
		//Color color = renderer.material.color;
		//color.a = 1 / duration;
		//renderer.material.SetColor("_TintColor", color);
	}
}
