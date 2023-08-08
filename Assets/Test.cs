using FishNet.Managing;
using FishNet.Transporting.Tugboat;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    public NetworkManager networkManager;

    private void Start()
    {
        networkManager.ServerManager.StartConnection();
        networkManager.ClientManager.StartConnection();
    }
}