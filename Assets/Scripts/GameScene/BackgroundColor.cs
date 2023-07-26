using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BackgroundColor : MonoBehaviour
{
    //assigned in inspector
    public Tilemap tilemap;
    //public int colorRotation; //inspector value is the starting color

    //private readonly float speed = 8;

    private readonly Color32[] colors = new Color32[6]
    {
        new(35, 182, 255, 20), //water
        new(255, 122, 0, 20), //flame
        new(205, 205, 255, 20), //wind
        new(255, 236, 0, 20), //lightning
        new(140, 228, 232, 20), //frost
        new(23, 195, 0, 20) //venom
    };

    //private Color32 startColor;
    //private Color32 endColor;

    //private float lerpIncrement;

    public void ChangeColor(int color)
    {
        tilemap.color = colors[color];
    }

    //private void Awake()
    //{
    //    startColor = colors[colorRotation];
    //    endColor = colors[colorRotation == 5 ? 0 : colorRotation + 1];
    //}

    //private void Update()
    //{
    //    if (tilemap.color != endColor)
    //    {
    //        lerpIncrement += Time.deltaTime / speed;
    //        tilemap.color = Color.Lerp(startColor, endColor, lerpIncrement);
    //    }
    //    else if (tilemap.color == endColor)
    //    {
    //        lerpIncrement = 0;

    //        colorRotation++;
    //        if (colorRotation == 6) colorRotation = 0;
    //        startColor = colors[colorRotation];
    //        endColor = colors[colorRotation == 5 ? 0 : colorRotation + 1];
    //    }
    //}
}