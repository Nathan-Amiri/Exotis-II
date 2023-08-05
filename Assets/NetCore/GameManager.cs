using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet;
using FishNet.Object;
using FishNet.Connection;
using FishNet.Managing.Scened;
using FishNet.Transporting;
using System;

public class GameManager : NetworkBehaviour
{
    //networked game manager

    //general GameManager code:

    //server variables:
    public int[] playerNumbers { get; private set; }
    private readonly int[] playerIDs = new int[4];
    private int sceneChangingPlayers;
    private int sceneLoadedPlayers;

    //client variables:
    static public int playerNumber { get; private set; }
    public Canvas waitCanvas; //assigned in inspector
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
        //set canvas camera every time client connects or loads
        waitCanvas.worldCamera = Camera.main;

        if (playerNumber == 0)
        {
            //if client is connecting and not loading a scene
            simpleManager = GameObject.FindWithTag("SimpleManager").GetComponent<SimpleManager>();

            RpcFirstConnect(InstanceFinder.ClientManager.Connection);
        }
        else //if client is loading a scene and not initially connecting
        {
            SendConnectOrLoadEvent(); //send client connectedorload
            CheckIfAllLoaded(); //prepare to send all clients loaded
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RpcFirstConnect(NetworkConnection playerConnection)
    {
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != connectionScene)
            RpcSceneConditionFailed(playerConnection);

        for (int i = 0; i < playerNumbers.Length; i++)
            if (playerNumbers[i] == 0)
            {
                playerNumbers[i] = i + 1;
                playerIDs[i] = playerConnection.ClientId;
                RpcAssignPlayerNumber(playerConnection, i + 1);
                return;
            }
        RpcPlayerNumberConditionFailed(playerConnection);
    }

    [TargetRpc]
    private void RpcSceneConditionFailed(NetworkConnection conn)
    {
        simpleManager.errorText.text = "Error: Host is already in a game!";
        ClientManager.StopConnection();
    }
    [TargetRpc]
    private void RpcPlayerNumberConditionFailed(NetworkConnection conn)
    {
        simpleManager.errorText.text = "Error: Too Many Players!";
        ClientManager.StopConnection();
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
        waitCanvas.gameObject.SetActive(true);
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
        waitCanvas.gameObject.SetActive(false);

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
        UnityEngine.SceneManagement.SceneManager.LoadScene(connectionScene);

        if (simpleManager != null)
            simpleManager.OnDisconnect();
    }

    public delegate void OnClientConnectOrLoadAction(GameManager gm);
    public static event OnClientConnectOrLoadAction OnClientConnectOrLoad;
    private void SendConnectOrLoadEvent()
    {
        //when either this client first connects or when new scene has fully loaded for that client
        OnClientConnectOrLoad?.Invoke(this);
    }

    public delegate void OnRemoteClientDisconnectAction(int disconnectedPlayer);
    public static event OnRemoteClientDisconnectAction OnRemoteClientDisconnect;
    private void SendRemoteClientDisconnectEvent(int disconnectedPlayer)
    {
        OnRemoteClientDisconnect?.Invoke(disconnectedPlayer);
    }


    //game-specific code:

    //the scene where clients first connect, and which is loaded upon disconnecting
    private readonly string connectionScene = "CharSelect";

    [NonSerialized] public string[] charSelectInfo = new string[8]; //filled by CharSelect, accessed by Setup

    [NonSerialized] public PlayerScoreInfo[] playerScoreInfos = new PlayerScoreInfo[4];

    private int roundNumber = 0;

    public delegate void OnGameEndAction(GameManager gm);
    public static event OnGameEndAction OnGameEnd;

    public delegate void OnNewRoundAction();
    public static event OnNewRoundAction OnNewRound;

    [ServerRpc(RequireOwnership = false)]
    public void RpcServerAddScoreInfo(PlayerScoreInfo newInfo, int player)
    {
        RpcClientAddScoreInfo(newInfo, player);
    }
    [ObserversRpc (BufferLast = true)]
    private void RpcClientAddScoreInfo(PlayerScoreInfo newInfo, int player)
    {
        playerScoreInfos[player - 1] = newInfo;
    }

    private void UpdateAlivePlayers() //run on server
    {
        Player.alivePlayers -= 1;
    }

    [ServerRpc (RequireOwnership = false)]
    public void RpcServerRoundWon(int winner)
    {
        roundNumber += 1;
        RpcClientRoundWon(winner, roundNumber == 6);
    }
    [ObserversRpc]
    private void RpcClientRoundWon(int winner, bool gameEnded)
    {
        playerScoreInfos[winner - 1].score += 1;

        //invoke after updating score
        if (gameEnded)
            OnGameEnd?.Invoke(this); //tell FinalScore the game is over
        else
            OnNewRound?.Invoke(); //tell player to start a new round
    }
}