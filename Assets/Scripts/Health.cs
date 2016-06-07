using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class Health : NetworkBehaviour
{
	public delegate void DeathDelegate (NetworkInstanceId player, NetworkInstanceId killedBy);

	public delegate void HealthChangeDelegate (int amount, int oldHealth, int newHealth);

	[SyncEvent]
	public event DeathDelegate EventDeath;

	[SyncEvent]
	public event HealthChangeDelegate EventHealthChange;

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
		int oldHealth = health;
		health -= amount;
		EventHealthChange (-amount, oldHealth, health);
		RpcBleed ();
		if (health <= 0) {
			var spawn = NetworkManager.singleton.GetStartPosition ();
			EventDeath (GetComponent<NetworkIdentity> ().netId, byPlayer.GetComponent<NetworkIdentity> ().netId);
			health = maxHealth;
			EventHealthChange (maxHealth, 0, maxHealth);
			RpcRespawn (spawn.position);
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
		if (isLocalPlayer) {
			transform.position = position;
		}
	}
}
