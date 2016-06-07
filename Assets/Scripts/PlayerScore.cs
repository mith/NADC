using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Collections;

public class PlayerScore : NetworkBehaviour
{
	public Text KillsCounter;
	public Text DeathsCounter;

	[SyncVar]
	public int Deaths = 0;
	[SyncVar]
	public int Kills = 0;

	// Use this for initialization
	void Start ()
	{
		if (isServer) {
			
			GetComponent<Health> ().EventDeath += (player, killedBy) => {
				Debug.Log ("Some idiot got killed!");
				Deaths++;
            };
                
		}

		if (isLocalPlayer) {
			
			DeathsCounter = GameObject.Find ("HUD").transform.FindChild ("DeathsCounter").GetComponent<Text> ();

			GetComponent<Health> ().EventDeath += (player, killedBy) => {
				Debug.Log ("Killed!");
				DeathsCounter.text = Deaths.ToString ();
			};
		}
	}

	public void KilledPlayer ()
	{
		Kills++;
        Debug.Log("Killed a bitch!");
		RpcUpdateKillCounter ();
	}

	[ClientRpc]
	void RpcUpdateKillCounter ()
	{
		if (isLocalPlayer) {
			KillsCounter = GameObject.Find ("HUD").transform.FindChild ("KillsCounter").GetComponent<Text> ();
			KillsCounter.text = Kills.ToString ();
		}
	}
}
