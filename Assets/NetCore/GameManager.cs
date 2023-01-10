using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet;
using FishNet.Object;
using FishNet.Connection;
using FishNet.Managing.Scened;
using FishNet.Transporting;

public class GameManager : NetworkBehaviour
{
    //general GameManager code:

    //server variables:
    public int[] playerNumbers = new int[10];

    //client variables:
    public int playerNumber;

    private void OnEnable()
    {
        Beacon.Signal += ReceiveSignal;
    }
    private void OnDisable()
    {
        Beacon.Signal -= ReceiveSignal;
    }

    [Client]
    private void ReceiveSignal()
    {
        if (playerNumber == 0)
            RpcFirstConnect(InstanceFinder.ClientManager.Connection);
        else
            SendConnectOrLoadEvent();
    }

    [ServerRpc(RequireOwnership = false)]
    private void RpcFirstConnect(NetworkConnection playerConnection)
    {
        for (int i = 0; i < playerNumbers.Length; i++)
            if (playerNumbers[i] == 0)
            {
                playerNumbers[i] = i + 1;
                RpcAssignPlayerNumber(playerConnection, i + 1);
                return;
            }
        Debug.LogError("Too Many Players");
    }

    [TargetRpc]
    private void RpcAssignPlayerNumber(NetworkConnection conn, int newPlayerNumber)
    {
        playerNumber = newPlayerNumber;
        SendConnectOrLoadEvent();
    }

    //scene changing:
    [Server]
    public void SceneChange(string newScene)
    {
        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        SceneLoadData sceneLoadData = new(newScene);
        NetworkManager.SceneManager.LoadGlobalScenes(sceneLoadData);
        SceneUnloadData sceneUnloadData = new(currentScene);
        NetworkManager.SceneManager.UnloadGlobalScenes(sceneUnloadData);
        //wait for beacon signal
    }

    //disconnect:
    public override void OnSpawnServer(NetworkConnection conn)
    {
        base.OnSpawnServer(conn);

        ServerManager.OnRemoteConnectionState += ClientDisconnected; //if client disconnects. Can't be subscribed in OnEnable
    }
    private void ClientDisconnected(NetworkConnection arg1, RemoteConnectionStateArgs arg2) //run on server
    {
        if (arg2.ConnectionState == RemoteConnectionState.Stopped)
        {
            int i = playerNumbers.Length;
            playerNumbers = new int[i]; //reset playerNumbers
            RpcCheckIfConnected(); //check which clients are still connected
        }
    }

    [ObserversRpc]
    private void RpcCheckIfConnected()
    {
        RpcUpdatePlayerNumbers(playerNumber);
    }

    [ServerRpc (RequireOwnership = false)]
    private void RpcUpdatePlayerNumbers(int newPlayer)
    {
        playerNumbers[newPlayer - 1] = newPlayer;
    }

    public delegate void OnClientConnectOrLoadAction(GameManager gm);
    public static event OnClientConnectOrLoadAction OnClientConnectOrLoad;
    private void SendConnectOrLoadEvent() //run when either this client first connects or when new scene has fully loaded
    {
        OnClientConnectOrLoad?.Invoke(this);
    }



        //game-specific code:

    [HideInInspector] public CharSelectInfo charSelectInfo; //filled by CharSelect, accessed by Setup
}
public struct CharSelectInfo
{
    public string elementalName;
    //spells
}