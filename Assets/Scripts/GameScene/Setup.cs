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
    public Animator countdownAnim; //^
    public TMP_Text countdownText; //^
    public TMP_Text winnerText; //^

    public GameObject editorGrid; //^
    public GameObject hud; //^
    public Index index; //^

    private GameManager gameManager;
    private Player player;

    private Vector3 playerPosition;

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

    [ObserversRpc]
    private void RpcStartPlayer(GameObject newPlayerObject, int newPlayerNumber, string[] newInfo)
    {
        Player newPlayer = newPlayerObject.GetComponent<Player>();
        if (newPlayer.Owner == InstanceFinder.ClientManager.Connection)
            player = newPlayer;

        newPlayer.charSelectInfo = newInfo;
        newPlayer.playerHud = hud.transform.GetChild(newPlayerNumber - 1).gameObject;
        newPlayer.gameManager = gameManager;
        newPlayer.countdownAnim = countdownAnim;
        newPlayer.countdownText = countdownText;
        newPlayer.winnerText = winnerText;

        newPlayer.OnSpawn(index);
    }
}