using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class Arrow : NetworkBehaviour
{
	public GameObject ShotBy;

    [ServerCallback]
	void OnCollisionEnter2D (Collision2D collision)
	{
		var hit = collision.gameObject;
		var hitPlayer = hit.GetComponent<PlayerController> ();
		if (hitPlayer != null) {
			var health = hit.GetComponent<Health> ();
			health.TakeDamage (ShotBy, 10);

			Destroy (gameObject);
		}
	}
}
