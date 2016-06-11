using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Collections;

public class HealthHUD : NetworkBehaviour
{

	public GameObject HealthDisplay;


    public void RegisterEvents(GameManager gameManager)
    {
        gameManager.EventHealthChange += (player, amount, oldHealth, newHealth) => {
            if (player == GetComponent<NetworkIdentity>().netId) {
                RpcUpdateHealthBar(newHealth);
            }
        };
    }

    [ClientRpc]
    void RpcUpdateHealthBar(int current)
    {
        if (isLocalPlayer) {
            HealthDisplay = GameObject.Find("HUD").transform.FindChild("HealthBackground/Health").gameObject;
            var maxHp = GetComponent<Health>().maxHealth;
            var hpImage = HealthDisplay.GetComponent<Image>();

            if (current > 0) {
                hpImage.fillAmount = (float)current / (float)maxHp;
            } else {
                hpImage.fillAmount = 0;
            }
        }
    }
}
