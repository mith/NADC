using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class PlayerController : NetworkBehaviour
{
	public GameObject arrowPrefab;
	public float arrowSpeed = 10;

	public float playerSpeed = 4;

	GameObject childCamera;

	[SyncVar]
	Color playerColor;

	void Awake ()
	{
		playerColor = Random.ColorHSV ();
	}

	void Start ()
	{
		GetComponent<SpriteRenderer> ().color = playerColor;
	}

	public override void OnStartLocalPlayer ()
	{
		childCamera = transform.FindChild ("ChildCamera").gameObject;
		childCamera.SetActive (true);
	}

	bool shooting;

	[ClientCallback]
	void Update ()
	{
		if (!isLocalPlayer)
			return;
		
		var x = Input.GetAxis ("Horizontal") * playerSpeed * Time.deltaTime;
		var y = Input.GetAxis ("Vertical") * playerSpeed * Time.deltaTime;

		GetComponent<Rigidbody2D> ().velocity = new Vector2 (x, y);

		transform.Translate (x, y, 0);

		if (Input.GetMouseButtonDown (0)) {
			if (!shooting) {
				StartCoroutine (Shoot ());
			}
		}
	}

	Quaternion CursorDirection ()
	{
		var mousePos = childCamera.GetComponent<Camera> ().ScreenToWorldPoint (Input.mousePosition);

		var direction = Mathf.Atan2 (
			                transform.position.y - mousePos.y, 
			                transform.position.x - mousePos.x) * 180 / Mathf.PI;

		return Quaternion.Euler (0, 0, direction + 90);	
	}

	IEnumerator Shoot ()
	{
		for (;;) {
			CmdFire (CursorDirection ());
			shooting = true;
			yield return new WaitForSeconds (0.5f);

			if (!Input.GetMouseButton (0)) {
				shooting = false;
				break;
			}
		}
	}

	[Command]
	void CmdFire (Quaternion direction)
	{
		
		var bullet = (GameObject)Instantiate (
			             arrowPrefab,
			             transform.position + (direction * transform.up).normalized,
			             direction * Quaternion.Euler (0, 0, 90));

		bullet.GetComponent<Rigidbody2D> ().velocity = (direction * transform.up).normalized * arrowSpeed;

		NetworkServer.Spawn (bullet);

		Destroy (bullet, 3.0f);
	}
}
