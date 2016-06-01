using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class Health : NetworkBehaviour
{
	public const int maxHealth = 100;

	[SyncVar]
	public int health = maxHealth;

	[Server]
	public void TakeDamage (int amount)
	{
		health -= amount;
		RpcBleed ();
		if (health <= 0) {
			var spawn = NetworkManager.singleton.GetStartPosition ();
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
