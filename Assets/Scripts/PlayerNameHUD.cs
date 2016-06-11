using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.UI;

public class PlayerNameHUD : NetworkBehaviour 
{

    [SyncVar(hook = "UpdateName")]
    public string PlayerName = "";

    public override void OnStartLocalPlayer()
    {
        CmdSetName(GameObject.Find("NetworkManager").GetComponent<GameManager>().PlayerName);
   
        base.OnStartLocalPlayer();
    }

    [Command]
    void CmdSetName(string name)
    {
        PlayerName = name;
    }
	
    void UpdateName(string name)
    {
        if (isClient) {
            var canvas = transform.FindChild("PlayerCanvas").gameObject;

            canvas.SetActive(true);
            canvas.transform.Find("PlayerNameText").GetComponent<Text>().text = name;
        }
    }

    public override void OnStartClient()
    {
        UpdateName(PlayerName);
        base.OnStartClient();
    }
}
