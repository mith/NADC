using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class Health : NetworkBehaviour
{
	public const int maxHealth = 100;

	[SyncVar]
	public int health = maxHealth;

	public void TakeDamage (int amount)
	{
		if (!isServer)
			return;

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
