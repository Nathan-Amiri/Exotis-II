using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet;
using FishNet.Object;
using FishNet.Connection;
using FishNet.Managing.Scened;

public class GameManager : NetworkBehaviour
{
    //server variables:
    public int numberOfPlayers = 0;

    //client variables:
    public static int playerNumber;
    private bool clientReady = false;
    private string currentScene;


    //on first connect:
    private void CheckIfObserversLoaded() //run in update
    {
        if (IsClient && !clientReady)
        {
            clientReady = true;
            RpcFirstConnect(InstanceFinder.ClientManager.Connection);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RpcFirstConnect(NetworkConnection playerConnection)
    {
        numberOfPlayers += 1;
        RpcAssignPlayerNumber(playerConnection, numberOfPlayers);
    }

    [TargetRpc]
    private void RpcAssignPlayerNumber(NetworkConnection conn, int newPlayerNumber)
    {
        playerNumber = newPlayerNumber;
        currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        OnClientConnectOrLoad();
    }

    //scene changing:
    [Server]
    public void SceneChange(string newScene)
    {
        SceneLoadData sceneLoadData = new(newScene);
        NetworkManager.SceneManager.LoadGlobalScenes(sceneLoadData);
        SceneUnloadData sceneUnloadData = new(currentScene);
        NetworkManager.SceneManager.UnloadGlobalScenes(sceneUnloadData);

        SceneHasChanged(newScene);
    }

    [ObserversRpc(BufferLast = true)]
    private void SceneHasChanged(string newScene)
    {
        currentScene = newScene; //can't use GetActiveScene here, it's delayed

        Invoke(nameof(OnClientConnectOrLoad), 1); //find a way to delay this until network objects are active!
    }

    //disconnect:






    private CharSelect charSelect;
    private Setup setup;

    [HideInInspector] public CharSelectInfo charSelectInfo; //assigned by CharSelect


    [Client]
    private void OnClientConnectOrLoad() //run when either this client first connects or when new scene has fully loaded
    {
        //can't use GameObject.Find when loading into a scene, must use InactiveReference as NobObjects aren't set active yet

        if (currentScene == "CharSelect")
        {
            charSelect = GameObject.FindGameObjectWithTag("SceneReference").GetComponent<SceneReference>().charSelect;
            charSelect.gameManager = this;
            charSelect.OnSpawn();
        }
        else if (currentScene == "GameScene")
        {
            setup = GameObject.FindGameObjectWithTag("SceneReference").GetComponent<SceneReference>().setup;
            setup.charSelectInfo = charSelectInfo;
            setup.OnSpawn();
        }
    }

    private void Update()
    {
        CheckIfObserversLoaded();
    }
}
public struct CharSelectInfo
{

}