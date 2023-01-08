using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet;
using FishNet.Object;
using FishNet.Connection;
using FishNet.Managing.Scened;

public class Setup : NetworkBehaviour
{
    public GameObject playerPref; //assigned in inspector

    public GameObject editorGrid; //^

    public CharSelectInfo charSelectInfo;

    public void OnSpawn() //called by GameManager
    {
        editorGrid.SetActive(false);
        SpawnPlayer(InstanceFinder.ClientManager.Connection);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnPlayer(NetworkConnection conn)
    {
        GameObject newPlayer = Instantiate(playerPref);
        InstanceFinder.ServerManager.Spawn(newPlayer, conn);
        RpcStartPlayer(newPlayer);
    }

    [ObserversRpc]
    private void RpcStartPlayer(GameObject newPlayer)
    {
        newPlayer.GetComponent<Player>().OnSpawn();
    }
}