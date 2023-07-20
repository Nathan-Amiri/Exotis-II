using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet;
using FishNet.Object;
using FishNet.Connection;
using TMPro;

public class Setup : NetworkBehaviour
{
    public GameObject playerPref; //assigned in inspector
    public GameObject hud; //^
    public GameObject spellParent; //^
    public Animator countdownAnim; //^
    public TMP_Text countdownText; //^
    public TMP_Text winnerText; //^
    public PlayAgain playAgain; //^
    public MapManager mapManager;

    private GameManager gameManager;

    private Vector3 playerPosition;

    private void OnEnable()
    {
        GameManager.OnAllClientsLoaded += OnSpawn;
    }
    private void OnDisable()
    {
        GameManager.OnAllClientsLoaded -= OnSpawn;
    }
    public void OnSpawn(GameManager gm)
    {
        gameManager = gm;

        if (GameManager.playerNumber == 1)
            playerPosition = new Vector3(-5.5f, -2.5f);
        else if (GameManager.playerNumber == 2)
            playerPosition = new Vector3(5.5f, -2.5f);
        else if (GameManager.playerNumber == 3)
            playerPosition = new Vector3(-7.5f, 3);
        else if (GameManager.playerNumber == 4)
            playerPosition = new Vector3(7.5f, 3f);

        SpawnPlayer(InstanceFinder.ClientManager.Connection, GameManager.playerNumber, gameManager.charSelectInfo, playerPosition);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnPlayer(NetworkConnection conn, int newPlayerNumber, string[] newInfo, Vector3 newPlayerPosition)
    {
        GameObject newPlayerObject = Instantiate(playerPref, newPlayerPosition, Quaternion.identity);
        InstanceFinder.ServerManager.Spawn(newPlayerObject, conn);
        RpcStartPlayer(newPlayerObject, newPlayerNumber, newInfo);
    }

    [ObserversRpc (BufferLast = true)] //bufferlast is needed because this rpc is run on clients that may not have received the beacon signal yet
    private void RpcStartPlayer(GameObject newPlayerObject, int newPlayerNumber, string[] newInfo)
    {
        Player newPlayer = newPlayerObject.GetComponent<Player>();

        newPlayer.charSelectInfo = newInfo;
        newPlayer.playerHUD = hud.transform.GetChild(newPlayerNumber).GetComponent<PlayerHUD>();
        newPlayer.spellParent = spellParent;
        newPlayer.gameManager = gameManager;
        newPlayer.countdownAnim = countdownAnim;
        newPlayer.countdownText = countdownText;
        newPlayer.winnerText = winnerText;
        newPlayer.playAgain = playAgain;
        newPlayer.mapManager = mapManager;

        newPlayer.OnSpawn();
    }
}