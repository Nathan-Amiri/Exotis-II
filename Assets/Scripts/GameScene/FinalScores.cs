using FishNet;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class FinalScores : MonoBehaviour
{
    public List<CharImage> playerAvatars = new();
    public List<TMP_Text> rankTexts = new();
    public List<TMP_Text> usernames = new();
    public List<TMP_Text> scores = new();

    public GameObject scoreScreen;
    public GameObject backToLobby;
    public GameObject waitForHost;

    private GameManager gameManager;

    private readonly string[] ranks = new string[] { "1st", "2nd", "3rd", "4th" };

    private void OnEnable()
    {
        GameManager.OnGameEnd += GameEnd;
    }
    private void OnDisable()
    {
        GameManager.OnGameEnd -= GameEnd;
    }
    public void GameEnd(GameManager gm)
    {
        scoreScreen.SetActive(true);

        if (InstanceFinder.IsServer)
            backToLobby.SetActive(true);
        else
            waitForHost.SetActive(true);

        gameManager = gm;

        //order infos by score. If any infos are default, scores will be zero, so they'll be last
        gameManager.playerScoreInfos = gameManager.playerScoreInfos.OrderByDescending(x => x.score).ToArray();
        int currentRank = 0;
        for (int i = 0; i  < gameManager.playerScoreInfos.Length; i++)
        {
            if (gameManager.playerScoreInfos[i].username == default) //can return here without missing a player--see note above
                return;

            PlayerScoreInfo info = gameManager.playerScoreInfos[i];

            playerAvatars[i].charCore.color = info.shellColor;
            playerAvatars[i].charShell.color = info.coreColor;
            usernames[i].text = info.username;
            scores[i].text = info.score.ToString();

            if (i == 0)
                rankTexts[i].text = ranks[0];
            else if (info.score == gameManager.playerScoreInfos[i - 1].score)
                rankTexts[i].text = rankTexts[i - 1].text;
            else
            {
                currentRank++;
                rankTexts[i].text = ranks[currentRank];
            }
        }
    }

    public void SelectBackToLobby()
    {
        gameManager.RequestSceneChange("CharSelect");
    }
}
public struct PlayerScoreInfo
{
    public Color32 shellColor;
    public Color32 coreColor;
    public string username;
    public int score;
}