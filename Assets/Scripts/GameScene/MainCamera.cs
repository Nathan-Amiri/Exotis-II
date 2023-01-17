using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCamera : MonoBehaviour
{
    public static float screenShakeIntensity = 0;
    private float screenShakeAmount = 0.02f;
    private float screenShakeDecreaseFactor = 5;

    private Vector3 defaultPosition = new(0, 0, -10);
    private Vector3 cachedPosition;

    private void Update()
    {
        if (screenShakeIntensity > 0)
        {
            if (cachedPosition != default)
                cachedPosition = transform.position;
            Vector2 newPosition = Random.insideUnitCircle;
            transform.position = new Vector3(newPosition.x, newPosition.y) * screenShakeAmount + defaultPosition;
            screenShakeIntensity -= Time.deltaTime * screenShakeDecreaseFactor;
        }
        else
        {
            screenShakeIntensity = 0;
            if (cachedPosition != default)
            {
                transform.position = cachedPosition;
                cachedPosition = default;
            }
        }
    }
}