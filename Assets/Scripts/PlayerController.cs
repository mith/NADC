using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class PlayerController : NetworkBehaviour
{
	public GameObject arrowPrefab;
	public float arrowSpeed = 10;

	public float playerSpeed = 4;

	GameObject childCamera;

	public override void OnStartLocalPlayer ()
	{
		GetComponent<SpriteRenderer> ().color = new Color (0, 1, 0);
		childCamera = transform.FindChild ("ChildCamera").gameObject;
		childCamera.SetActive (true);
	}

	void Update ()
	{

		if (!isLocalPlayer)
			return;
		
		var x = Input.GetAxis ("Horizontal") * playerSpeed * Time.deltaTime;
		var y = Input.GetAxis ("Vertical") * playerSpeed * Time.deltaTime;

		transform.Translate (x, y, 0);

		if (Input.GetMouseButtonDown (0)) {
			var mousePos = childCamera.GetComponent<Camera> ().ScreenToWorldPoint (Input.mousePosition);

			var direction = Mathf.Atan2 (
				                transform.position.y - mousePos.y, 
				                transform.position.x - mousePos.x) * 180 / Mathf.PI;

			var dirQ = Quaternion.Euler (0, 0, direction + 90);
			CmdFire (dirQ);
		}
	}

	[Command]
	void CmdFire (Quaternion direction)
	{
		
		var bullet = (GameObject)Instantiate (
			             arrowPrefab,
			             transform.position + (direction * transform.up).normalized,
			             direction);

		bullet.GetComponent<Rigidbody2D> ().velocity = (direction * transform.up).normalized * arrowSpeed;

		NetworkServer.Spawn (bullet);

		Destroy (bullet, 3.0f);
	}
}
