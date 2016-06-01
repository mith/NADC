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
		if (health <= 0) {
			RpcRespawn ();
		}
	}

	[ClientRpc]
	void RpcRespawn ()
	{
		if (isLocalPlayer) {
			transform.position = Vector3.zero;
		}
	}
}
