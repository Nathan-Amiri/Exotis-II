using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FinalScores : MonoBehaviour
{
    public List<CharImage> playerAvatars = new();
    public List<TMP_Text> rankTexts = new();
    public List<TMP_Text> usernames = new();
    public List<TMP_Text> scores = new();

    public GameObject backToLobby;
    public GameObject waitForHost;

    public void SelectBackToLobby()
    {

    }
}