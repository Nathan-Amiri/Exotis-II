using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapManager : MonoBehaviour
{
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

    private void Start()
    {
        LoadNewMap();
    }

    public void LoadNewMap()
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

        currentMap = availableMaps[Random.Range(0, availableMaps.Count)];

        tilemaps[currentMap].color = mapColors[currentMap];

        maps[currentMap].SetActive(true);
    }
}