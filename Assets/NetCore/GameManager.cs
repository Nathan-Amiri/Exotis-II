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
    //networked game manager

    //general GameManager code:

    //server variables:
    [HideInInspector] public int[] playerNumbers { get; private set; }
    private readonly int[] playerIDs = new int[4];
    private int sceneChangingPlayers;
    private int sceneLoadedPlayers;

    //client variables:
    static public int playerNumber { get; private set; }
    public GameObject waitCanvas; //assigned in inspector
    private SimpleManager simpleManager;

    private void Awake()
    {
        playerNumbers = new int[4];
    }

    private void OnEnable()
    {
        Beacon.Signal += ReceiveSignal;
    }
    private void OnDisable()
    {
        Beacon.Signal -= ReceiveSignal;
    }

    //connecting:

    [Client]
    private void ReceiveSignal()
    {
        if (playerNumber == 0)
        {
            //if client is connecting and not loading a scene
            simpleManager = GameObject.FindWithTag("SimpleManager").GetComponent<SimpleManager>();

            RpcFirstConnect(InstanceFinder.ClientManager.Connection);
        }
        else
        {
            SendConnectEvent(); //send client connected
            CheckIfAllLoaded(); //prepare to send all clients loaded
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RpcFirstConnect(NetworkConnection playerConnection)
    {
        for (int i = 0; i < playerNumbers.Length; i++)
            if (playerNumbers[i] == 0)
            {
                playerNumbers[i] = i + 1;
                playerIDs[i] = playerConnection.ClientId;
                RpcAssignPlayerNumber(playerConnection, i + 1);
                return;
            }
        Debug.LogError("Too Many Players");
    }

    [TargetRpc]
    private void RpcAssignPlayerNumber(NetworkConnection conn, int newPlayerNumber)
    {
        playerNumber = newPlayerNumber;
        SendConnectEvent();
    }

    //scene changing:

    [Server]
    public void SceneChange(string newScene)
    {
        TurnOnWaitCanvas();

        sceneLoadedPlayers = 0;

        sceneChangingPlayers = 0;
        for (int i = 0; i < playerNumbers.Length; i++)
            if (playerNumbers[i] != 0)
                sceneChangingPlayers++;

        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        SceneLoadData sceneLoadData = new(newScene);
        NetworkManager.SceneManager.LoadGlobalScenes(sceneLoadData);
        SceneUnloadData sceneUnloadData = new(currentScene);
        NetworkManager.SceneManager.UnloadGlobalScenes(sceneUnloadData);
        //wait for beacon signal
    }

    [ObserversRpc]
    private void TurnOnWaitCanvas()
    {
        waitCanvas.SetActive(true);
    }

    [ServerRpc (RequireOwnership = false)]
    private void CheckIfAllLoaded()
    {
        sceneLoadedPlayers++;
        if (sceneLoadedPlayers == sceneChangingPlayers)
            SendAllLoadedEvent();
    }

    public delegate void OnAllClientsLoadedAction(GameManager gm);
    public static event OnAllClientsLoadedAction OnAllClientsLoaded;

    [ObserversRpc]
    private void SendAllLoadedEvent()
    {
        waitCanvas.SetActive(false);

        OnAllClientsLoaded?.Invoke(this);
    }

    //disconnecting:
    public void Disconnect()
    {
        if (IsServer)
            ServerManager.StopConnection(false);
        else
            ClientManager.StopConnection();
    }
    public override void OnSpawnServer(NetworkConnection conn)
    {
        base.OnSpawnServer(conn);

        ServerManager.OnRemoteConnectionState += ClientDisconnected; //if client disconnects. Can't be subscribed in OnEnable
    }
    private void ClientDisconnected(NetworkConnection arg1, RemoteConnectionStateArgs arg2) //run on server
    {
        if (arg2.ConnectionState == RemoteConnectionState.Stopped)
        {
            for (int i = 0; i < playerIDs.Length; i++)
                if (playerIDs[i] == arg2.ConnectionId)
                {
                    playerIDs[i] = 0;
                    playerNumbers[i] = 0;
                    SendRemoteClientDisconnectEvent(i + 1);
                    UpdateAlivePlayers();
                    return;
                }
        }
    }
    public override void OnStopClient()
    {
        base.OnStopClient();
        playerNumber = 0;
        UnityEngine.SceneManagement.SceneManager.LoadScene(disconnectScene);

        if (simpleManager != null)
            simpleManager.OnDisconnect();
    }

    public delegate void OnClientConnectAction(GameManager gm);
    public static event OnClientConnectAction OnClientConnect;
    private void SendConnectEvent()
    {
        //when either this client first connects or when new scene has fully loaded for that client
        OnClientConnect?.Invoke(this);
    }

    public delegate void OnRemoteClientDisconnectAction(int disconnectedPlayer);
    public static event OnRemoteClientDisconnectAction OnRemoteClientDisconnect;
    private void SendRemoteClientDisconnectEvent(int disconnectedPlayer)
    {
        OnRemoteClientDisconnect?.Invoke(disconnectedPlayer);
    }


    //game-specific code:

    private readonly string disconnectScene = "CharSelect";

    [HideInInspector] public string[] charSelectInfo = new string[8]; //filled by CharSelect, accessed by Setup

    private void UpdateAlivePlayers() //run on server
    {
        Player.alivePlayers -= 1;
    }
}