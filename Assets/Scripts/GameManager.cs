using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class GameManager : NetworkManager
{
	public event Health.DeathDelegate EventDeath;

	public override void OnServerAddPlayer (NetworkConnection conn, short playerControllerId)
	{
		var player = (GameObject)Instantiate (playerPrefab, GetStartPosition ().localPosition, Quaternion.identity);
		player.GetComponent<PlayerScore> ().RegisterEvents (this);
		player.GetComponent<Health> ().EventDeath += EventDeath;
		NetworkServer.AddPlayerForConnection (conn, player, playerControllerId);
	}
}
