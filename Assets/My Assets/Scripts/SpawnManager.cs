using UnityEngine;
using System.Collections;

public class SpawnManager : MonoBehaviour {
	
	public GameObject playerPrefab;
	
	
	// Use this for initialization
	void Start () 
	{
	
	}
	
	// Update is called once per frame
	void Update () 
	{
	
	}
	
	public void SpawnObject(GameObject prefab, bool networked, Vector3 position, Quaternion rotation, int groupNum)
	{
		if (networked)
		{
			Network.Instantiate( prefab, position, rotation, 0);
		}
		else
		{
			
		}
	}
	
	public void SpawnPlayer()
	{
		GameObject player = (GameObject) Network.Instantiate(playerPrefab, new Vector3(0, 2, 0), Quaternion.identity, 0);
		Camera.main.GetComponent<FollowCameraScript>().target = player.transform;
		player.networkView.RPC("SetupPlayer", RPCMode.AllBuffered, player.networkView.viewID, PlayerPrefs.GetString("playerName"));
	} 
}
