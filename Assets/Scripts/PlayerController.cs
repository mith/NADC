using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public enum Weapon {
    Bow,
    SwordAndShield,
}

public class PlayerController : NetworkBehaviour
{
	public GameObject hudPrefab;
	public GameObject arrowPrefab;
	public float arrowSpeed = 10;

	public float normalSpeed = 4;
    // The amount the walking speed gets divided when attacking
    public float attackingSpeedPenalty = 0.5f;

    [SyncVar]
    public bool IsControlEnabled;
        
    public delegate void WeaponChangeDelegate (NetworkInstanceId player, Weapon weapon);
    public delegate void WeaponChargingDelegate (NetworkInstanceId player, bool charging, float percentage);

    public event WeaponChangeDelegate EventWeaponChange;
    public event WeaponChargingDelegate EventWeaponCharging;

    [SyncVar]
    Weapon currentWeapon;

    public Weapon CurrentWeapon {
        get {
            return currentWeapon;
        }
        set {
            CmdChangeWeapon(value);
        }
    }

	GameObject childCamera;

	[SyncVar]
	Color playerColor;

	void Awake ()
	{
        IsControlEnabled = true;
		playerColor = Random.ColorHSV (0, 1, 0.2f, 1, 0.5f, 1);

	}

	void Start ()
	{
		GetComponent<SpriteRenderer> ().color = playerColor;
	}

	public override void OnStartLocalPlayer ()
	{
		childCamera = transform.FindChild ("ChildCamera").gameObject;
		childCamera.SetActive (true);

		Instantiate (hudPrefab).name = "HUD";

        CmdChangeWeapon(currentWeapon);
	}

	bool attacking;

	[ClientCallback]
	void Update ()
	{
		if (!isLocalPlayer)
			return;

        if (!IsControlEnabled)
            return;

        if (Input.GetButtonDown("Weapon1")) {
            CurrentWeapon = Weapon.SwordAndShield;
        } else if (Input.GetButtonDown("Weapon2")) {
            CurrentWeapon = Weapon.Bow;
        }

        var walkingSpeed = normalSpeed * (attacking ? attackingSpeedPenalty : 1);
		
		var x = Input.GetAxis ("Horizontal") * walkingSpeed * Time.deltaTime;
		var y = Input.GetAxis ("Vertical") * walkingSpeed * Time.deltaTime;

		GetComponent<Rigidbody2D> ().velocity = new Vector2 (x, y);

		transform.Translate (x, y, 0);

		if (Input.GetMouseButtonDown (0)) {
			if (!attacking) {
				StartCoroutine (Attack ());
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

	IEnumerator Attack ()
	{
		for (;;) {
            if (CurrentWeapon == Weapon.SwordAndShield) {
                CmdSwing(CursorDirection());  
            } else if (CurrentWeapon == Weapon.Bow) {
                CmdFire(CursorDirection());
            }
            attacking = true;
			yield return new WaitForSeconds (0.5f);

			if (!Input.GetMouseButton (0)) {
				attacking = false;
				break;
			}
		}
	}

    [Command]
    void CmdSwing (Quaternion direction)
    {
        var colliders = Physics2D.OverlapCircleAll(transform.position + direction * (Vector2.up * 2), 0.5f);
        foreach (var collider in colliders) {
            var health = collider.GetComponent<Health>();
            if (health != null) {
                health.TakeDamage(this.gameObject, 60);
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

		bullet.GetComponent<Arrow> ().ShotBy = this.gameObject;

		Destroy (bullet, 3.0f);
	}

    [Command]
    void CmdChangeWeapon(Weapon weapon)
    {
        currentWeapon = weapon;
        if (EventWeaponChange != null) {
            EventWeaponChange(this.netId, weapon);
        }
    }

    void OnWeaponChange (Weapon weapon)
    {
        
    }
}
