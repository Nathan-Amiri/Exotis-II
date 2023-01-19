using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using FishNet;
using FishNet.Transporting.Tugboat;

public class SimpleManager : MonoBehaviour
{
    //non-networked game manager

    public GameObject escapeMenu; //assigned in inspector
    public TMP_Text exitDisconnectText; //^
    public Button joinLobby;
    public TMP_InputField ipAddress; //^
    public TextMeshProUGUI placeHolder; //^
    public Tugboat tugboat; //^

    public static SimpleManager instance = null;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        if (Input.GetButtonDown("EscapeMenu"))
        {
            exitDisconnectText.text = InstanceFinder.IsClient ? "Exit Game" : "Disconnect";
            escapeMenu.SetActive(!escapeMenu.activeSelf);
        }

        joinLobby.interactable = !placeHolder.enabled;
    }

    public void SelectStartLobby()
    {
        escapeMenu.SetActive(false);

        InstanceFinder.ServerManager.StartConnection();
    }

    public void SelectJoinLobby()
    {
        escapeMenu.SetActive(false);

        tugboat.SetClientAddress(ipAddress.text);
        InstanceFinder.ClientManager.StartConnection();
    }

    public void SelectExitDisconnect()
    {
        escapeMenu.SetActive(false);

        if (!InstanceFinder.IsClient)
            Application.Quit();
        else
            InstanceFinder.ClientManager.StopConnection();
    }
}