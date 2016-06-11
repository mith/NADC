﻿using UnityEngine;
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
        manager.StartHost();
    }

    public void JoinGame()
    {
        manager.StartClient();
    }
}
