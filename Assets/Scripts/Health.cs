using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class Health : NetworkBehaviour
{
	public delegate void DeathDelegate (GameObject player, GameObject killedBy);

	public delegate void HealthChangeDelegate (NetworkInstanceId player, int amount, int oldHealth, int newHealth);


	public event DeathDelegate EventDeath;

	public event HealthChangeDelegate EventHealthChange;

    bool Invulnerable = false;

	public int maxHealth = 100;

	[SyncVar]
	public int health;

	public override void OnStartServer ()
	{
		health = maxHealth;
		base.OnStartServer ();
	}

	[Server]
	public void TakeDamage (GameObject byPlayer, int amount)
	{
        if (Invulnerable)
            return;
        
        NetworkInstanceId currentPlayerId = GetComponent<NetworkIdentity>().netId;
        NetworkInstanceId byPlayerId = byPlayer.GetComponent<NetworkIdentity>().netId;
		int oldHealth = health;
		health -= amount;
        if (EventHealthChange != null) {
            EventHealthChange(currentPlayerId, -amount, oldHealth, health);
        }
		RpcBleedLight ();
        Debug.Log("Player " + currentPlayerId + " took " + amount + " damage from player " + byPlayerId);
		if (health <= 0) {
			var spawn = NetworkManager.singleton.GetStartPosition ();
            Respawn (spawn.position);

            if (EventDeath != null) {
                EventDeath(this.gameObject, byPlayer);
            }
	
            if (EventHealthChange != null) {
                EventHealthChange(currentPlayerId, maxHealth, 0, maxHealth);
            }

            Debug.Log("Player " + currentPlayerId + " was killed by " + byPlayerId);
		}
	}

	[ClientRpc]
	void RpcBleedLight ()
	{
		transform.Find ("Blood").GetComponent<ParticleSystem> ().Emit (3);
	}

    [ClientRpc]
    void RpcBleedHeavy ()
    {
        transform.Find ("Blood").GetComponent<ParticleSystem> ().Emit (6);
    }

	void Respawn (Vector3 position)
	{
        StartCoroutine(RespawnTimer(position));
	}

    IEnumerator RespawnTimer(Vector3 position)
    {
        GetComponent<PlayerController>().IsControlEnabled = false;
        Invulnerable = true;

        for(int x = 0; x < 20; x++) {
            RpcBleedHeavy();
            yield return new WaitForSeconds(0.2f);
        }

        GetComponent<PlayerController>().IsControlEnabled = true;
        Invulnerable = false;
        RpcSpawnAt(position);
    }

    [ClientRpc]
    void RpcSpawnAt(Vector3 position)
    {
        transform.position = position;
    }
}
