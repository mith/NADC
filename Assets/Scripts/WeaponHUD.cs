using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.UI;

public class WeaponHUD : NetworkBehaviour 
{
    Text currentWeaponLabel;

    public override void OnStartLocalPlayer()
    {
        currentWeaponLabel = GameObject.Find("HUD")
            .transform.Find("WeaponsPanel/CurrentWeaponLabel")
            .GetComponent<Text>();
        base.OnStartLocalPlayer();
    }

    public void RegisterEvents(GameManager gameManager)
    {
        gameManager.EventWeaponChange += (player, weapon) => {
            if (player == this.netId) {
                RpcUpdateWeaponLabel(weapon);
            }
        };
    }

    [ClientRpc]
    void RpcUpdateWeaponLabel(Weapon weapon)
    {
        if (isLocalPlayer) {
            switch(weapon) {
                case Weapon.Bow:
                    currentWeaponLabel.text = "Bow";
                    break;
                case Weapon.SwordAndShield:
                    currentWeaponLabel.text = "Sword";
                    break;
            }
        }
    }
}
