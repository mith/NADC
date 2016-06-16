using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class MenuUI : MonoBehaviour {
    GameManager manager;

    void Awake() 
    {
        manager = GetComponent<GameManager>();
    }

    public void HostGame()
    {
        if (!string.IsNullOrEmpty(manager.PlayerName)) {
            manager.StartHost();
        }
    }

    public void JoinGame()
    {
        if (!string.IsNullOrEmpty(manager.PlayerName)) {
            manager.StartClient();
        }
    }
}
