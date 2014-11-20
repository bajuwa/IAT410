﻿using UnityEngine;
using System.Collections;

public class NetworkManager : MonoBehaviour {
	//Name MUST be unique on master server
	public GameObject anthill;
	public GameObject gatherer;
	public GameObject gathererRedSpawn;
	public GameObject gathererBlueSpawn;
	public GameObject redAnthillSpawn;
	public GameObject blueAnthillSpawn;
	private const string typeName = "ColoniesAntBattle";
	private string gameName = CreateGameServer.gameName;
	private HostData hostGame = JoinGame.hostGame;
	private int playerId = 1;
	
	private void Start () {
		if (hostGame == null) {
			StartServer();
		} else {
			Network.Connect(hostGame);
		}
	}
	// Use this for initialization
	void StartServer () {
		Network.InitializeServer(2, 25000, !Network.HavePublicAddress());
		MasterServer.RegisterHost(typeName, gameName);
	}
	
	//Messages

	
	void OnMasterServerEvent(MasterServerEvent mse) {
		if(mse == MasterServerEvent.RegistrationSucceeded) {
			Debug.Log("Registered");
		}
	}
	//instantiate Player 1 (the server) stuff
	void OnPlayerConnected() 
	{
		Network.Instantiate(gatherer, gathererRedSpawn.transform.position, Quaternion.identity, 0); //initial gatherer
		Network.Instantiate(anthill, redAnthillSpawn.transform.position, Quaternion.identity, 0); //initial anthill
	}
	//instantiate Player 2 (the client) stuff
	void OnConnectedToServer()
	{
		playerId = 2;
		GameObject antHillObject = (GameObject) Network.Instantiate(gatherer, gathererBlueSpawn.transform.position, Quaternion.identity,0);
		GameObject gathererObject = (GameObject) Network.Instantiate(anthill, blueAnthillSpawn.transform.position, Quaternion.identity, 0); //initial anthill
		NetworkView anthillNetwork = antHillObject.networkView;
		NetworkView gathererNetwork = gathererObject.networkView;
		networkView.RPC("changePlayerId", RPCMode.AllBuffered, anthillNetwork.viewID, gathererNetwork.viewID, playerId);
	}
	//tells both server and client that this anthill is player 2s
	[RPC] void changePlayerId(NetworkViewID anthillID, NetworkViewID gathererID, int player)
	{
		NetworkView anthillNetwork = NetworkView.Find(anthillID);
		NetworkView gathererNetwork = NetworkView.Find(gathererID);
		GameObject anthillObject = anthillNetwork.gameObject;
		GameObject gathererObject = gathererNetwork.gameObject;
		anthillObject.GetComponent<Ownable>().setAsMine(player);
		gathererObject.GetComponent<Ownable>().setAsMine(player);
	}
	// Update is called once per frame
	void Update () {

	}
	

	void OnGUI() {
		
	}
}
