using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using FishNet.Managing;
using Unity.Services.Authentication;
using Unity.Services.Core;
using System.Threading.Tasks;
using Unity.Services.Lobbies.Models;
using Unity.Services.Lobbies;
using FishNet.Transporting.FishyUnityTransport;
using Unity.Networking.Transport.Relay;
using Unity.Services.Relay.Models;
using Unity.Services.Relay;
using FishNet.Managing.Client;
using FishNet.Managing.Server;
using FishNet;
using System;

public class SimpleManager : MonoBehaviour
{
    //non-networked game manager

    public static SimpleManager instance = null;

    //assigned in prefab
    public GameObject escapeMenu;
    ////^ escapemenu turned off on disconnect by GameManager
    //public TMP_Text errorText;
    public TMP_Text exitDisconnectText;
    //public Button startLobby;
    //public Button joinLobby;
    //public TMP_InputField ipAddress;
    //public TextMeshProUGUI placeHolder;
    //public TMP_Dropdown resolutionDropdown;
    public Button createRoom;
    public Button joinRoom;
    public TMP_Text errorText;


    private GameManager gameManager;

    public TMP_InputField usernameField;
    public TextMeshProUGUI usernamePlaceHolder;
    public TMP_InputField roomNameField;
    public TextMeshProUGUI roomNamePlaceholder;
    public TMP_Dropdown resolutionDropdown;

    private NetworkManager networkManager;

    public Lobby currentLobby;

    private string roomName;

    private void OnEnable()
    {
        GameManager.OnClientConnectOrLoad += OnClientConnectOrLoad;
    }
    private void OnDisable()
    {
        GameManager.OnClientConnectOrLoad -= OnClientConnectOrLoad;
    }

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);

        //find networkmanager using gameobject.find because the networkmanager in the scene gets destroyed when returning to menuscene
        networkManager = GameObject.FindWithTag("NetworkManager").GetComponent<NetworkManager>();
    }

    private void Start()
    {
        DontDestroyOnLoad(gameObject);

        ToggleButtonsInteractable(false);

        if (PlayerPrefs.HasKey("Username"))
            usernameField.text = PlayerPrefs.GetString("Username");

        if (PlayerPrefs.HasKey("RoomName"))
            roomNameField.text = PlayerPrefs.GetString("RoomName");

        if (PlayerPrefs.HasKey("Resolution"))
            resolutionDropdown.value = PlayerPrefs.GetInt("Resolution");

        _ = ConnectToRelay();
    }

    private void Update()
    {
        if (roomNameField.text != roomName)
        {
            roomName = roomNameField.text;
            PlayerPrefs.SetString("RoomName", roomName);
        }

        if (Input.GetButtonDown("EscapeMenu") && gameManager != null)
            escapeMenu.SetActive(!escapeMenu.activeSelf);

        exitDisconnectText.text = gameManager == null ? "Exit Game" : "Disconnect";
    }

    private async Task ConnectToRelay() //run in Start
    {
        errorText.text = "Connecting...";

        await UnityServices.InitializeAsync();

        if (AuthenticationService.Instance.IsSignedIn) //true if returning to MenuScene while still connected to relay services
        {
            ToggleButtonsInteractable(true);
            errorText.text = "";
            return;
        }

        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Signed in " + AuthenticationService.Instance.PlayerId);
        };

        try
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();

            ToggleButtonsInteractable(true);
            errorText.text = "";
        }
        catch
        {
            errorText.text = "Failed to connect. Check your internet connection, then restart the game";
        }
    }

    private void ToggleButtonsInteractable(bool interactable)
    {
        createRoom.interactable = interactable;
        joinRoom.interactable = interactable;
    }

    private void OnClientConnectOrLoad(GameManager gm)
    {
        gameManager = gm;

        errorText.text = "";
        escapeMenu.SetActive(false);
    }

    public void ChangeUsername()
    {
        PlayerPrefs.SetString("Username", usernameField.text);
    }

    private void UsernameError()
    {
        errorText.text = "Must choose a username!";
    }

    private Unity.Services.Lobbies.Models.Player GetPlayer()
    {
        string playerName = usernameField.text;

        return new Unity.Services.Lobbies.Models.Player
        {
            Data = new Dictionary<string, PlayerDataObject>
            {
                { "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerName) }
            }
        };
    }

    private IEnumerator HandleLobbyHeartbeat() //keep lobby active (lobbies are automatically hidden after 30 seconds of inactivity)
    {
        while (currentLobby != null)
        {
            SendHeartbeat();
            yield return new WaitForSeconds(15);
        }
    }
    private async void SendHeartbeat()
    {
        await LobbyService.Instance.SendHeartbeatPingAsync(currentLobby.Id);
    }

    private async void CreateLobby(string newRoomName)
    {
        try
        {
            string lobbyName = "MyLobby";
            int maxPlayers = 4;
            CreateLobbyOptions createLobbyOptions = new()
            {
                IsPrivate = false,
                Player = GetPlayer(),
                Data = new Dictionary<string, DataObject> //RoomName = S1, JoinCode = S2
                {
                    //Unlike the technical (and meaningless) lobby name, RoomName is a public data value that is searchable. Exotis
                    //uses RoomName in place of a Lobby Code
                    { "RoomName", new DataObject(DataObject.VisibilityOptions.Public, newRoomName, DataObject.IndexOptions.S1) },
                }
            };

            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, createLobbyOptions);

            StartCoroutine(HandleLobbyHeartbeat());

            if (newRoomName == "")
                Debug.Log("Created Public Lobby");
            else
                Debug.Log("Created Private Lobby named " + lobby.Data["RoomName"].Value);

            TurnOnClient(true, lobby);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public async void SelectCreateRoom()
    {
        if (usernamePlaceHolder.enabled)
        {
            UsernameError();
            return;
        }
        if (roomNamePlaceholder.enabled)
        {
            errorText.text = "Must choose a room name!";
            return;
        }

        QueryLobbiesOptions queryLobbiesOptions = new()
        {
            Count = 50,
            Filters = new List<QueryFilter>() //RoomName = S1, JoinCode = S2
            {
                //find lobbies with RoomName equal to roomName
                new QueryFilter(QueryFilter.FieldOptions.S1, roomName.ToUpper(), QueryFilter.OpOptions.EQ)
            }
        };
        QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync(queryLobbiesOptions);

        if (queryResponse.Results.Count != 0)
        {
            errorText.text = "A room with this name is already in use. Please try another name";
            return;
        }


        CreateLobby(roomName.ToUpper());
    }

    public async void SelectJoinRoom()
    {
        if (usernamePlaceHolder.enabled)
        {
            UsernameError();
            return;
        }
        if (roomNamePlaceholder.enabled)
        {
            errorText.text = "Must provide the name of the room you'd like to join!";
            return;
        }

        try
        {
            QueryLobbiesOptions queryLobbiesOptions = new()
            {
                Count = 50,
                Filters = new List<QueryFilter>() //RoomName = S1, JoinCode = S2
                {
                    //find lobbies with RoomName equal to roomName
                    new QueryFilter(QueryFilter.FieldOptions.S1, roomName.ToUpper(), QueryFilter.OpOptions.EQ)
                }
            };
            QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync(queryLobbiesOptions);

            if (queryResponse.Results.Count == 0)
            {
                errorText.text = "Room not found. Check your room name and try again";
                return;
            }
            if (queryResponse.Results[0].AvailableSlots == 0)
            {
                errorText.text = "Room is already full!";
                return;
            }
            if (!queryResponse.Results[0].Data.ContainsKey("JoinCode"))
            {
                //JoinCode is created when player is connected to relay. It's possible to join the lobby before the relay connection
                //is established and before JoinCode is created
                errorText.text = "Room is still being created. Please wait a few seconds and try again";
                return;
            }

            Lobby lobby = await Lobbies.Instance.JoinLobbyByIdAsync(queryResponse.Results[0].Id);
            Debug.Log("Joined Lobby named " + lobby.Data["RoomName"].Value);

            TurnOnClient(false, lobby);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    private async void TurnOnClient(bool host, Lobby lobby)
    {
        errorText.text = "Loading Room...";
        ToggleButtonsInteractable(false);

        var utp = (FishyUnityTransport)networkManager.TransportManager.Transport;
        string joinCode;

        if (host)
        {
            // Set up HostAllocation
            Allocation hostAllocation = await RelayService.Instance.CreateAllocationAsync(4);
            utp.SetRelayServerData(new RelayServerData(hostAllocation, "dtls"));

            // Start Server Connection
            networkManager.ServerManager.StartConnection();

            // Set up JoinAllocation
            joinCode = await RelayService.Instance.GetJoinCodeAsync(hostAllocation.AllocationId);

            //SaveJoinCodeInLobbyData
            try
            {
                //update currentLobby
                currentLobby = await Lobbies.Instance.UpdateLobbyAsync(lobby.Id, new UpdateLobbyOptions
                {
                    Data = new Dictionary<string, DataObject> //RoomName = S1 JoinCode = S2
                    {
                        //only updates this piece of data--other lobby data remains unchanged
                        { "JoinCode", new DataObject(DataObject.VisibilityOptions.Public, joinCode, DataObject.IndexOptions.S2) }
                    }
                });
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }
        }
        else //if clientonly
        {
            currentLobby = lobby;

            //GetJoinCode
            joinCode = currentLobby.Data["JoinCode"].Value;
        }

        //Set up JoinAllocation
        JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
        utp.SetRelayServerData(new RelayServerData(joinAllocation, "dtls"));

        // Start Client Connection
        networkManager.ClientManager.StartConnection();
    }

    public async void SelectExitDisconnect()
    {
        if (gameManager == null)
            Application.Quit();
        else
        {
            try
            {
                if (InstanceFinder.IsServer)
                    await Lobbies.Instance.DeleteLobbyAsync(currentLobby.Id);
                else
                    await Lobbies.Instance.RemovePlayerAsync(currentLobby.Id, AuthenticationService.Instance.PlayerId);

                if (InstanceFinder.IsServer)
                    InstanceFinder.ServerManager.StopConnection(true);
                else
                    InstanceFinder.ClientManager.StopConnection();
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }
        }
    }

    public void OnDisconnect() //called by GameManager
    {
        escapeMenu.SetActive(true);

        _ = ConnectToRelay();
    }

    public void SelectNewResolution()
    {
        switch (resolutionDropdown.value)
        {
            case 0:
                Screen.SetResolution(1920, 1080, true);
                break;
            case 1:
                Screen.SetResolution(1280, 720, true);
                break;
            case 2:
                Screen.SetResolution(1366, 768, true);
                break;
            case 3:
                Screen.SetResolution(1600, 900, true);
                break;
        }
        PlayerPrefs.SetInt("Resolution", resolutionDropdown.value);
    }
}