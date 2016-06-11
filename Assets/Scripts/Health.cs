using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class Health : NetworkBehaviour
{
	public delegate void DeathDelegate (NetworkInstanceId player, NetworkInstanceId killedBy);

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
		RpcBleed ();
        Debug.Log("Player " + currentPlayerId + " took " + amount + " damage from player " + byPlayerId);
		if (health <= 0) {
			var spawn = NetworkManager.singleton.GetStartPosition ();
            RpcRespawn (spawn.position);
            health = maxHealth;

            if (EventDeath != null) {
                EventDeath(currentPlayerId, byPlayerId);
            }
	
            if (EventHealthChange != null) {
                EventHealthChange(currentPlayerId, maxHealth, 0, maxHealth);
            }

            Debug.Log("Player " + currentPlayerId + " was killed by " + byPlayerId);
		}
	}

	[ClientRpc]
	void RpcBleed ()
	{
		transform.Find ("Blood").GetComponent<ParticleSystem> ().Emit (3);
	}

	[ClientRpc]
	void RpcRespawn (Vector3 position)
	{
        StartCoroutine(RespawnTimer(position));
	}

    [Command]
    void CmdSetInvulnerable(bool invulnerable)
    {
        Invulnerable = invulnerable;
    }

    IEnumerator RespawnTimer(Vector3 position)
    {
        if (isLocalPlayer) {
            GetComponent<PlayerController>().IsControlEnabled = false;
            CmdSetInvulnerable(true);
        }

        for(int x = 0; x < 20; x++) {
            transform.Find ("Blood").GetComponent<ParticleSystem> ().Emit (6);
            yield return new WaitForSeconds(0.2f);
        }

        if (isLocalPlayer) {
            GetComponent<PlayerController>().IsControlEnabled = true;
            CmdSetInvulnerable(false);
            transform.position = position;
        }
    }
}
