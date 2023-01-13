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

    public GameObject editorGrid; //^
    public GameObject hud;
    public Index index; //^

    private GameManager gameManager;
    private Player player;

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
        
        SpawnPlayer(InstanceFinder.ClientManager.Connection, gameManager.playerNumber, gameManager.charSelectInfo);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnPlayer(NetworkConnection conn, int newPlayerNumber, string[] newInfo)
    {
        GameObject newPlayerObject = Instantiate(playerPref);
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
        newPlayer.OnSpawn(index);
        StartCoroutine(Countdown());
    }

    private IEnumerator Countdown()
    {
        yield return new WaitForSeconds(.3f);
        countdownText.text = "3";
        countdownAnim.SetTrigger("TrCountdown");
        yield return new WaitForSeconds(.9f);
        countdownText.text = "2";
        countdownAnim.SetTrigger("TrCountdown");
        yield return new WaitForSeconds(.9f);
        countdownText.text = "1";
        countdownAnim.SetTrigger("TrCountdown");
        yield return new WaitForSeconds(.9f);
        countdownText.text = "Go!";
        countdownAnim.SetTrigger("TrCountdown");
        player.playerMovement.isStunned = false;
    }
}