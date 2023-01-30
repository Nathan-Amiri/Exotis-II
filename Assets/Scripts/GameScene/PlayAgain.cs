using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;
using TMPro;

public class PlayAgain : NetworkBehaviour
{
    public GameObject playAgainBackground; //assigned in inspector
    public GameObject choice; //^
    public GameObject wait; //^
    public TMP_Text playAgainText;
    private CharImage[] avatars;
    public CharImage player1Avatar; //^
    public CharImage player2Avatar; //^
    public CharImage player3Avatar; //^
    public CharImage player4Avatar; //^

    private readonly bool[] readyPlayers = new bool[4]; //server only

    private GameManager gameManager;

    private Color32[] lightAndDark = new Color32[2];

    public delegate void OnPlayAgainAction();
    public static event OnPlayAgainAction OnPlayAgain;

    private void OnEnable()
    {
        GameManager.OnClientConnectOrLoad += OnSpawn;
    }
    private void OnDisable()
    {
        GameManager.OnClientConnectOrLoad -= OnSpawn;
    }

    private void OnSpawn(GameManager gm)
    {
        avatars = new CharImage[4]; 
        avatars[0] = player1Avatar;
        avatars[1] = player2Avatar;
        avatars[2] = player3Avatar;
        avatars[3] = player4Avatar;

        playAgainBackground.SetActive(false);
        choice.SetActive(false);
        wait.SetActive(false);

        gameManager = gm;
    }

    public void NewPlayAgain(Player newPlayer, Color32[] newLightAndDark) //called by Player
    {
        lightAndDark = newLightAndDark;

        playAgainText.text = "Play Again?";

        playAgainBackground.SetActive(true);
        choice.SetActive(true);
    }

    public void SelectNewGame()
    {
        bool onlyTwoPlayers = true;
        for (int i = 0; i < gameManager.playerNumbers.Length; i++)
            if (gameManager.playerNumbers[i] == 3)
                onlyTwoPlayers = false;

        playAgainText.text = onlyTwoPlayers ? "Waiting for enemy" : "Waiting for enemies";

        choice.SetActive(false);
        wait.SetActive(true);

        RpcDeclareReady(GameManager.playerNumber, lightAndDark);
    }

    [ServerRpc (RequireOwnership = false)]
    private void RpcDeclareReady(int newPlayer, Color32[] newLightAndDark)
    {
        bool allPlayersReady = true;

        readyPlayers[newPlayer - 1] = true;
        for (int i = 0; i < readyPlayers.Length; i++)
            if (gameManager.playerNumbers[i] != 0 && !readyPlayers[i])
            {
                allPlayersReady = false;
                break;
            }

        if (allPlayersReady)
        {
            SendPlayAgainEvent();
            return;
        }

        CharImage avatar = avatars[newPlayer - 1];
        avatar.charShell.color = newLightAndDark[0];
        avatar.charCore.color = newLightAndDark[1];

        Color32[] serverColors = new Color32[8];
        int x = 0;
        for (int i = 0; i < 4; i++)
        {
            serverColors[x] = avatars[i].charShell.color;
            serverColors[x + 1] = avatars[i].charCore.color;
            x += 2; //i increases by 1, x increases by 2
        }

        RpcUpdateAvatars(serverColors);
    }

    [ObserversRpc]
    private void SendPlayAgainEvent() //sent to all clients, invokes an event on all Players on that client
    {
        playAgainBackground.SetActive(false);
        playAgainText.text = "";
        choice.SetActive(false);
        wait.SetActive(false);

        OnPlayAgain?.Invoke();
    }

    [ObserversRpc]
    private void RpcUpdateAvatars(Color32[] serverColors)
    {
        int x = 0;
        for (int i = 0; i < 4; i++)
        {
            avatars[i].charShell.color = serverColors[x];
            avatars[i].charCore.color = serverColors[x + 1];
            x += 2; //i increases by 1, x increases by 2
        }
    }

    public void SelectBackToLobby()
    {
        RequestSceneChange();
    }

    [ServerRpc (RequireOwnership = false)]
    private void RequestSceneChange()
    {
        gameManager.SceneChange("CharSelect");
    }
}
