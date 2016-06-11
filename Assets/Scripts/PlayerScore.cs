using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Collections;

public class PlayerScore : NetworkBehaviour
{
	public Text KillsCounter;
	public Text DeathsCounter;

	[SyncVar (hook = "OnChangeDeaths")]
	public int Deaths = 0;
	[SyncVar (hook = "OnChangeKills")]
	public int Kills = 0;

	void Start ()
	{
		if (isLocalPlayer) {
			var hud = GameObject.Find ("HUD");
			KillsCounter = hud.transform.Find ("KillsCounter").GetComponent<Text> ();
			DeathsCounter = hud.transform.Find ("DeathsCounter").GetComponent<Text> ();
		}
	}

	[Server]
	public void RegisterEvents (GameManager gameManager)
	{
		gameManager.EventDeath += (player, killedBy) => {
			if (player == this.netId) {
				Deaths++;
			}
			if (killedBy == this.netId) {
				Kills++;
			}
		};
	}

	void OnChangeKills (int kills)
	{
		if (isLocalPlayer) {
			KillsCounter.text = kills.ToString ();
		}
	}

	void OnChangeDeaths (int deaths)
	{
		if (isLocalPlayer) {
			DeathsCounter.text = deaths.ToString ();
		}
	}
}
