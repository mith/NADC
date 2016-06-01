﻿using UnityEngine;
using System.Collections;

public class Arrow : MonoBehaviour
{
	void OnCollisionEnter2D (Collision2D collision)
	{
		var hit = collision.gameObject;
		var hitPlayer = hit.GetComponent<PlayerController> ();
		if (hitPlayer != null) {
			var health = hit.GetComponent<Health> ();
			health.TakeDamage (10);

			Destroy (gameObject);
		}
	}
}
