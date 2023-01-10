using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet;
using FishNet.Object;
using FishNet.Connection;

public class Setup : NetworkBehaviour
{
    public GameObject playerPref; //assigned in inspector

    public GameObject editorGrid; //^

    public CharSelectInfo charSelectInfo;

    private void OnEnable()
    {
        GameManager.OnClientConnectOrLoad += OnSpawn;
    }
    private void OnDisable()
    {
        GameManager.OnClientConnectOrLoad -= OnSpawn;
    }
    public void OnSpawn(GameManager gm)
    {
        editorGrid.SetActive(false);

        charSelectInfo = gm.charSelectInfo;
        
        SpawnPlayer(InstanceFinder.ClientManager.Connection);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnPlayer(NetworkConnection conn)
    {
        GameObject newPlayer = Instantiate(playerPref);
        InstanceFinder.ServerManager.Spawn(newPlayer, conn);
        RpcStartPlayer(conn, newPlayer);
    }

    [TargetRpc]
    private void RpcStartPlayer(NetworkConnection conn, GameObject newPlayer)
    {
        Debug.Log(charSelectInfo.elementalName);
        newPlayer.GetComponent<Player>().OnSpawn();
    }
}