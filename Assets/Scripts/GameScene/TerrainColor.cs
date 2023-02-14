using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TerrainColor : MonoBehaviour
{
    public Tilemap tilemap; //assigned in inspector

    private readonly float speed = 8;

    private readonly Color32[] colors = new Color32[6];
    private Color32 frost = new(140, 228, 232, 255);
    private Color32 wind = new(205, 205, 255, 255);
    private Color32 lightning = new(255, 236, 0, 255);
    private Color32 flame = new(255, 122, 0, 255);
    private Color32 water = new(35, 182, 255, 255);
    private Color32 venom = new(23, 195, 0, 255);

    private Color32 startColor;
    private Color32 endColor;
    private int colorRotation = 0;

    private float lerpIncrement;

    private void Awake()
    {
        colors[0] = frost;
        colors[1] = wind;
        colors[2] = lightning;
        colors[3] = flame;
        colors[4] = water;
        colors[5] = venom;

        colorRotation = Random.Range(0, 6);

        startColor = colors[colorRotation];
        endColor = colors[colorRotation == 5 ? 0 : colorRotation + 1];
    }

    private void Update()
    {
        if (tilemap.color != endColor)
        {
            lerpIncrement += Time.deltaTime / speed;
            tilemap.color = Color.Lerp(startColor, endColor, lerpIncrement);
        }
        else if (tilemap.color == endColor)
        {
            lerpIncrement = 0;

            colorRotation++;
            if (colorRotation == 6) colorRotation = 0;
            startColor = colors[colorRotation];
            endColor = colors[colorRotation == 5 ? 0 : colorRotation + 1];
        }
    }
}