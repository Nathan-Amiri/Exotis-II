using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using FishNet.Object;
using System;

public class MapManager : NetworkBehaviour
{
    public TidalwaveElement tidalwaveElement;
    public List<ElectrifyElement> electrifyElements;
    public IcybreathElement icybreathElement;
    public List<InfectElement> infectElements;

    public List<GameObject> maps = new();
    public List<Tilemap> tilemaps = new();

    public Vector2[][] mapSpawnPositions = new Vector2[][]
    {
        //player 1 position, player 2 position, player 3 position, player 4 position
        new Vector2[] { new(-5.5f, -2.5f), new(5.5f, -2.5f), new(-7, 3), new(7, 3) }, //water map
        new Vector2[] { new(-6, -2.5f), new(6, -2.5f), new(-7.5f, 4), new(7.5f, 4) }, //flame map
        new Vector2[] { new(-2.5f, -2), new(2.5f, -2), new(-6.5f, 4.5f), new(6.5f, 4.5f) }, //wind map
        new Vector2[] { new(-8.5f, -.5f), new(8.5f, -.5f), new(-2.5f, 4), new(2.5f, 4) }, //lightning map
        new Vector2[] { new(-4.5f, 2.5f), new(4.5f, 2.5f), new(-4.5f, -3.5f), new(4.5f, -3.5f) }, //frost map
        new Vector2[] { new(-3, -.5f), new(3, -.5f), new(-8, -3.5f), new(8, -3.5f) } //venom map
    };

    private int roundNumber = -1;
    private int currentMap = -1; //-1 = null

    private bool hasRandomized;
    private int[] rotationOrder = new int[6] { 0, 1, 2, 3, 4, 5 };

    private readonly Color32[] mapColors = new Color32[6]
    {
        new Color32(35, 182, 255, 255), //water
        new Color32(255, 122, 0, 255), //flame
        new Color32(205, 205, 255, 255), //wind
        new Color32(255, 236, 0, 255), //lightning
        new Color32(140, 228, 232, 255), //frost
        new Color32(23, 195, 0, 255) //venom
    };

    [NonSerialized] public Player player; //set by Player. This client's owned player

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
        hasRandomized = true;
        LoadNewMap();
    }

    [ObserversRpc (BufferLast = true)]
    private void RpcSendOrder(int[] newOrder)
    {
        if (IsServer) return;

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

        roundNumber += 1;
        if (roundNumber == maps.Count)
        {
            EndGame();
            return;
        }
        currentMap = rotationOrder[roundNumber];


        tilemaps[currentMap].color = mapColors[currentMap];

        maps[currentMap].SetActive(true);

        player.SpawnPlayerOnMap(mapSpawnPositions[currentMap]);


        if (IsServer)
        {
            if (currentMap == 0)
            {
                tidalwaveElement.gameObject.SetActive(true);
                ServerManager.Spawn(tidalwaveElement.gameObject);
                tidalwaveElement.OnSpawn();
            }
            else if (currentMap == 3)
                foreach (ElectrifyElement element in electrifyElements)
                {
                    element.gameObject.SetActive(true);
                    ServerManager.Spawn(element.gameObject);
                }
            else if (currentMap == 4)
            {
                icybreathElement.gameObject.SetActive(true);
                ServerManager.Spawn(icybreathElement.gameObject);
                icybreathElement.OnSpawn();
            }
            else if (currentMap == 5)
                foreach (InfectElement element in infectElements)
                {
                    element.gameObject.SetActive(true);
                    ServerManager.Spawn(element.gameObject);
                }
        }
    }

    private void EndGame()
    {

    }
}