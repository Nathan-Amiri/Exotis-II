using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using FishNet;
using FishNet.Object;
using FishNet.Connection;
using FishNet.Managing.Scened;

public class CharSelect : NetworkBehaviour
{
    [HideInInspector] public GameManager gameManager; //^

    public Sprite emptyAvatar; //assigned in inspector

    public Image[] avatars;
    public Image p1Avatar; //^
    public Image p2Avatar; //^
    public Image p3Avatar; //^
    public Image p4Avatar; //^

    public Image charImage; //^
    public Image charType1; //^
    public Image charType2; //^

    public GameObject highlight1; //^
    public GameObject highlight2; //^

    public GameObject nobCanvas; //^

    public Button readyButton; //^

    private string selectedElemental;

    private void Awake()
    {
        avatars = new Image[4];
        avatars[0] = p1Avatar;
        avatars[1] = p2Avatar;
        avatars[2] = p3Avatar;
        avatars[3] = p4Avatar;
    }

    public void OnSpawn()
    {
        nobCanvas.SetActive(true); //it isn't active already because of a bug, this is a workaround

        RpcGetCurrentAvatars(InstanceFinder.ClientManager.Connection, GameManager.playerNumber);
    }

    [ServerRpc(RequireOwnership = false)]
    private void RpcGetCurrentAvatars(NetworkConnection conn, int newPlayer)
    {
        ChangeAvatar(newPlayer, "Empty");

        string[] serverAvatars = new string[4];
        for (int i = 0; i < avatars.Length; i++)
            if (avatars[i].sprite != null)
                serverAvatars[i] = avatars[i].sprite.name;

        RpcClientGetCurrentAvatars(conn, serverAvatars);
    }
    [TargetRpc]
    private void RpcClientGetCurrentAvatars(NetworkConnection conn, string[] serverAvatars)
    {
        for (int i = 0; i < avatars.Length; i++)
            ChangeAvatar(i + 1, serverAvatars[i]);
    }

    public void SelectElemental(string elemental, string type1, string type2, string stat1, string stat2)
    {
        RpcServerChangeAvatar(GameManager.playerNumber, "Empty");

        selectedElemental = elemental;

        charImage.sprite = Resources.Load<Sprite>("Elementals/" + selectedElemental);
        charType1.sprite = Resources.Load<Sprite>("Elements/" + type1);
        charType2.sprite = Resources.Load<Sprite>("Elements/" + type2);

        highlight1.SetActive(true);
        highlight2.SetActive(true);
        highlight1.transform.localPosition = new Vector2(167, stat1 == "Power" ? -220 : -285);
        highlight2.transform.localPosition = new Vector2(167, stat2 == "Speed" ? -355 : -425);

        readyButton.interactable = true;
    }

    public void SelectReady()
    {
        readyButton.interactable = false;
        CharSelectInfo charSelectInfo = new();
        //fill here
        gameManager.charSelectInfo = charSelectInfo;

        RpcServerChangeAvatar(GameManager.playerNumber, selectedElemental);
        RpcCheckIfReady();
    }

    [ServerRpc (RequireOwnership = false)]
    private void RpcServerChangeAvatar(int newPlayer, string newAvatar)
    {
        ChangeAvatar(newPlayer, newAvatar);
        RpcClientChangeAvatar(newPlayer, newAvatar);
    }

    [ObserversRpc]
    private void RpcClientChangeAvatar(int newPlayer, string newAvatar)
    {
        if (IsClientOnly)
            ChangeAvatar(newPlayer, newAvatar);
    }

    private void ChangeAvatar(int newPlayer, string newAvatar)
    {
        Image avatar = avatars[newPlayer - 1];
        avatar.sprite = newAvatar == "Empty" ? emptyAvatar : Resources.Load<Sprite>("Elementals/" + newAvatar);
    }

    [ServerRpc (RequireOwnership = false)]
    private void RpcCheckIfReady()
    {
        for (int i = 0; i < avatars.Length; i++)
            if (avatars[i].sprite != null && avatars[i].sprite.name == "Empty")
                return;

        gameManager.SceneChange("GameScene");
    }

}