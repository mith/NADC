using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class OnHitDamager : NetworkBehaviour
{
	public GameObject Owner;

	public bool DestroyOnHit;

	public int Damage = 10;

	[ServerCallback]
	void OnCollisionEnter2D (Collision2D collision)
	{
		var hit = collision.collider;
		if (hit.transform.parent == null)
			return;
		
		var hitPlayer = hit.transform.parent.GetComponent<PlayerController> ();
		if (hitPlayer == null)
			return;

		Debug.Log ("collision speed: " + collision.relativeVelocity.magnitude);

		if (collision.relativeVelocity.magnitude < 1)
			return;

		var health = hit.transform.parent.GetComponent<Health> ();
		health.TakeDamage (Owner, Damage);

		if (DestroyOnHit)
			Destroy (gameObject);
	}
}
