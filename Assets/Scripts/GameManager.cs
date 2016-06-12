using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class GameManager : NetworkManager
{
    public string PlayerName {
        get;
        set;
    }
    
	public event Health.DeathDelegate EventDeath;
    public event Health.HealthChangeDelegate EventHealthChange;
    public event PlayerController.WeaponChangeDelegate EventWeaponChange;
    public event PlayerController.WeaponChargingDelegate EventWeaponCharging;

	public override void OnServerAddPlayer (NetworkConnection conn, short playerControllerId)
	{
		var player = (GameObject)Instantiate (playerPrefab, GetStartPosition ().localPosition, Quaternion.identity);
        NetworkServer.AddPlayerForConnection (conn, player, playerControllerId);
		player.GetComponent<PlayerScore> ().RegisterEvents (this);
        player.GetComponent<Health> ().EventDeath += (p, k) => {
            Debug.Log("Gamemanager: Player " + p + " killed by player " + k);
            EventDeath(p, k);
        }; 
        player.GetComponent<HealthHUD>().RegisterEvents(this);
        player.GetComponent<Health>().EventHealthChange += (p, a, o, n) => {
            Debug.Log("Gamemanager: Player " + p + " took " + a + " damage");
            EventHealthChange(p, a, o, n);
        };

        player.GetComponent<PlayerController>().EventWeaponChange += (p, w) => {
            Debug.Log("Gamemanager: Player " + p + " changed to weapon " + w);
            EventWeaponChange(p, w);
        };

        player.GetComponent<WeaponHUD>().RegisterEvents(this);
        player.GetComponent<TopScoreHUD>().RegisterEvents(this);

        Debug.Log("GameManager: player " + player.GetComponent<NetworkIdentity>().netId + " events registered");
	}
}
