using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using FishNet.Object;
using FishNet;

public class MapManager : NetworkBehaviour
{
    public TidalwaveElement tidalwaveElement;
    public List<ElectrifyElement> electrifyElements;
    public IcybreathElement icybreathElement;
    public List<InfectElement> infectElements;

    public List<GameObject> maps = new();
    public List<Tilemap> tilemaps = new();

    private readonly List<int> usedMaps = new();
    private int currentMap = -1; //-1 = null

    private readonly Color32[] mapColors = new Color32[6]
    {
        new Color32(35, 182, 255, 255), //water
        new Color32(255, 122, 0, 255), //flame
        new Color32(205, 205, 255, 255), //wind
        new Color32(255, 236, 0, 255), //lightning
        new Color32(140, 228, 232, 255), //frost
        new Color32(23, 195, 0, 255) //venom
    };

    public void LoadNewMap() //called by Player
    {
        if (currentMap != -1)
        {
            maps[currentMap].SetActive(false);
            usedMaps.Add(currentMap);
        }

        List<int> availableMaps = new();
        for (int i = 0; i < maps.Count; i++)
            availableMaps.Add(i);
        foreach (int i in usedMaps)
            availableMaps.Remove(i);

        if (availableMaps.Count == 0)
        {
            Debug.LogError("No available maps!");
            return;
        }

        currentMap = 3;//availableMaps[Random.Range(0, availableMaps.Count)];

        tilemaps[currentMap].color = mapColors[currentMap];

        maps[currentMap].SetActive(true);

        if (InstanceFinder.IsServer)
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
}