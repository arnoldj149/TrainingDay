using UnityEngine;
using System.Collections;

public class SpawnManager : MonoBehaviour {
	
	public GameObject playerPrefab;
	public GameObject enemyPrefab;
	
	private GameObject[] spawnPoints;
	private float timer = 0;
	public float enemyFrequency = 4;
	private bool spawnEnemies = false;
	private int maxEnemies = 20;
	public bool SpawnEnemies
	{
		get {return spawnEnemies;}
		set
		{
			networkView.RPC("SpawningEnemies",RPCMode.All, value);
		}
	}
	
	public bool PlayerAlive
	{
		get 
		{
			GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
			for (int i = 0; i < players.Length; i++)
			{
				if (players[i].networkView.isMine)
					return true;
			}
			return false;
		}
	}
	
	// Use this for initialization
	void Start () 
	{
		spawnPoints = GameObject.FindGameObjectsWithTag("Respawn");
	}
	
	// Update is called once per frame
	void Update () 
	{
		timer += Time.deltaTime;
		if (timer > enemyFrequency)
		{
			timer = 0;
			if (Network.isServer && spawnEnemies && GameObject.FindGameObjectsWithTag("Enemy").Length < maxEnemies)
			{
				SpawnEnemy();
			}
		}
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
	
	public void SpawnObject(GameObject prefab, bool networked, int spawnNum, int groupNum)
	{
		SpawnObject(prefab, networked, spawnPoints[spawnNum].transform.position, spawnPoints[spawnNum].transform.rotation, groupNum);
	}
	
	public void SpawnPlayer()
	{
		GameObject player = (GameObject) Network.Instantiate(playerPrefab, new Vector3(Random.Range(-4, 4), 2, Random.Range(-4, 4)), 
			Quaternion.identity, 1);
		Camera.main.GetComponent<FollowCameraScript>().target = player.transform;
		player.networkView.RPC("SetupPlayer", RPCMode.AllBuffered, player.networkView.viewID, 
			PlayerPrefs.GetString("playerName"), PlayerPrefs.GetInt("typeSelection"));
	} 
	
	public void SpawnEnemy()
	{
		SpawnObject(enemyPrefab, true, Random.Range(0, spawnPoints.Length), 2);
	}
	
	[RPC]
	public void SpawningEnemies(bool spawn)
	{
		spawnEnemies = spawn;
		if (spawn && !PlayerAlive)
		{
			SpawnPlayer();
		}
	}
}
