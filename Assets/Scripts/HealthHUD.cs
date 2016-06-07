using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Collections;

public class HealthHUD : NetworkBehaviour
{

	public GameObject HealthDisplay;

	// Use this for initialization
	void Start ()
	{
		if (isLocalPlayer) {
			HealthDisplay = GameObject.Find ("HUD").transform.FindChild ("HealthBackground/Health").gameObject;
			var maxHp = GetComponent<Health> ().maxHealth;
			var hpImage = HealthDisplay.GetComponent<Image> ();
			GetComponent<Health> ().EventHealthChange += (int amount, int oldHealth, int newHealth) => {
				Debug.Log ("Update health bar");
				if (newHealth > 0) {
					hpImage.fillAmount = (float)newHealth / (float)maxHp;
				} else {
					hpImage.fillAmount = 0;
				}
			};
		}
	
	}
}
