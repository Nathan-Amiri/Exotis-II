using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using FishNet.Object;
using System;

public class MapManager : NetworkBehaviour
{
    public FinalScores finalScores;

    public List<BackgroundColor> backgroundColors = new();

    public List<GameObject> networkedElementGameObjects = new();

    public List<GameObject> maps = new();
    public List<Tilemap> tilemaps = new();


    [NonSerialized] public Player player; //set by Player. This client's owned player


    private readonly List<INetworkedElement> networkedElements = new();

    private readonly Vector2[][] mapSpawnPositions = new Vector2[][]
    {
        //player 1 position, player 2 position, player 3 position, player 4 position
        new Vector2[] { new(-5.5f, -3f), new(5.5f, -3), new(-7, 2.5f), new(7, 2.5f) }, //water map
        new Vector2[] { new(-6, -3f), new(6, -3), new(-7.5f, 3.5f), new(7.5f, 3.5f) }, //flame map
        new Vector2[] { new(-2.5f, -2.5f), new(2.5f, -2.5f), new(-6.5f, 4), new(6.5f, 4) }, //wind map
        new Vector2[] { new(-8.5f, -1), new(8.5f, -1), new(-2.5f, 3.5f), new(2.5f, 3.5f) }, //lightning map
        new Vector2[] { new(-4.5f, 2), new(4.5f, 2), new(-4.5f, -4f), new(4.5f, -4) }, //frost map
        new Vector2[] { new(-3, -1), new(3, -1), new(-8, -4), new(8, -4) } //venom map
    };

    private readonly Color32[] mapColors = new Color32[6]
    {
        new Color32(35, 182, 255, 255), //water
        new Color32(255, 122, 0, 255), //flame
        new Color32(205, 205, 255, 255), //wind
        new Color32(255, 236, 0, 255), //lightning
        new Color32(140, 228, 232, 255), //frost
        new Color32(23, 195, 0, 255) //venom
    };

    private int roundNumber = -1;
    private int currentMap = -1; //-1 = null

    private bool hasRandomized;
    private int[] rotationOrder = new int[6] { 0, 1, 2, 3, 4, 5 };

    private void Awake()
    {
        //can't serialize interfaces, so they must be found and added manually
        foreach (GameObject tmp in networkedElementGameObjects)
            networkedElements.Add(tmp.GetComponent<INetworkedElement>());
    }

    [ServerRpc (RequireOwnership = false)]
    private void RpcRandomizeOrder()
    {
        for (int i = 0; i < rotationOrder.Length; i++)
        {
            int tmp = rotationOrder[i];
            int random = UnityEngine.Random.Range(0, rotationOrder.Length);
            rotationOrder[i] = rotationOrder[random];
            rotationOrder[random] = tmp;
        }

        RpcSendOrder(rotationOrder);
    }

    [ObserversRpc (BufferLast = true)]
    private void RpcSendOrder(int[] newOrder)
    {
        rotationOrder = newOrder;
        hasRandomized = true;
        LoadNewMap();
    }

    public void LoadNewMap() //called by Player
    {
        if (!hasRandomized)
        {
            if (IsServer)
                RpcRandomizeOrder(); //get maporder from server

            return;
        }


        if (currentMap != -1)
            maps[currentMap].SetActive(false);

        if (IsServer)
            foreach (INetworkedElement element in networkedElements)
                if (element.MapNumber() == currentMap)
                {
                    element.OnDespawn();
                    ServerManager.Despawn(element.GetGameObject());
                }


        roundNumber += 1;
        currentMap = rotationOrder[roundNumber];


        tilemaps[currentMap].color = mapColors[currentMap];

        maps[currentMap].SetActive(true);

        player.SpawnPlayerOnMap(mapSpawnPositions[currentMap]);


        if (IsServer)
            foreach (INetworkedElement element in networkedElements)
            {
                if (element.MapNumber() != currentMap)
                    continue;

                element.GetGameObject().SetActive(true);
                ServerManager.Spawn(element.GetGameObject());
                element.OnSpawn();
            }


        foreach (BackgroundColor backgroundColor in backgroundColors)
            backgroundColor.ChangeColor(currentMap);
    }
}