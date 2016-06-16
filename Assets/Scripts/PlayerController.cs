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
	public float arrowSpeed = 20;

	public float normalSpeed = 300;
    // The amount the walking speed gets divided when attacking
    public float attackingSpeedPenalty = 0.5f;

    [SyncVar]
    public bool IsControlEnabled;
        
    public delegate void WeaponChangeDelegate (NetworkInstanceId player, Weapon weapon);
    public delegate void WeaponChargingDelegate (NetworkInstanceId player, bool charging, float percentage);

    public event WeaponChangeDelegate EventWeaponChange;
    public event WeaponChargingDelegate EventWeaponCharging;

    GameObject body;
    GameObject shield;

    Rigidbody2D rigidBody;

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
        body = transform.Find("Body").gameObject;
        shield = transform.Find("Body/Shield").gameObject;

        GetComponent<NetworkTransformChild>().target = body.transform;
	}

    public override void OnStartServer()
    {
        playerColor = Random.ColorHSV(0, 1, 0.2f, 1, 0.5f, 1);
        base.OnStartServer();
    }

	void Start ()
	{
        body.GetComponent<SpriteRenderer>().color = playerColor;
	}

	public override void OnStartLocalPlayer ()
	{
		childCamera = transform.FindChild ("ChildCamera").gameObject;
		childCamera.SetActive (true);

		Instantiate (hudPrefab).name = "HUD";

        CmdChangeWeapon(currentWeapon);

        rigidBody = GetComponent<Rigidbody2D>();
	}

	bool attacking;

	[ClientCallback]
	void Update ()
	{
		if (!isLocalPlayer)
			return;

        if (!IsControlEnabled)
            return;

        body.transform.localRotation = CursorDirection();

        if (Input.GetButtonDown("Weapon1")) {
            CurrentWeapon = Weapon.SwordAndShield;
        } else if (Input.GetButtonDown("Weapon2")) {
            CurrentWeapon = Weapon.Bow;
        }

        var walkingSpeed = normalSpeed * (attacking ? attackingSpeedPenalty : 1);
		
        var x = Input.GetAxis("Horizontal");
        var y = Input.GetAxis("Vertical");

        rigidBody.MovePosition( rigidBody.position + (new Vector2(x, y).normalized * walkingSpeed * Time.deltaTime));

        if (Input.GetMouseButton(1)) {
            CmdBlock(true);
        } else {
            CmdBlock(false);
            if (Input.GetMouseButtonDown(0)) {
                if (!attacking) {
                    StartCoroutine(Attack());
                }
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

            if (!Input.GetMouseButton (0) || Input.GetMouseButton(1)) {
				attacking = false;
				break;
			}
		}
	}

    [Command]
    void CmdSwing (Quaternion direction)
    {
        var hit = Physics2D.Linecast(transform.position + direction * (Vector2.up * 0.1f), transform.position + direction * (Vector2.up * 2));
        if (hit.collider == null)
            return;
        
        var health = hit.collider.GetComponent<Health>();
        if (health != null) {
            health.TakeDamage(this.gameObject, 60);
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
    void CmdBlock (bool block)
    {
        shield.SetActive(block);
        RpcBlock(block);
    }

    [ClientRpc]
    void RpcBlock (bool block)
    {
        shield.SetActive(block);
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
