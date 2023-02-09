using FishNet;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Missile : MonoBehaviour
{
    [NonSerialized] public float missilePower; //set by player when instantiated
    [NonSerialized] public Player player; //set by player when instantiated

    public Rigidbody2D rb; //assigned in inspector
    public SpriteRenderer spriteRenderer; //assigned in inspector, read by Player
    public SpriteRenderer coreSprireRenderer; //^

    void Update()
    {
        transform.rotation = Quaternion.LookRotation(Vector3.forward, Quaternion.Euler(0, 0, 90) * new Vector3(rb.velocity.x, rb.velocity.y, 0));
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Terrain"))
        {
            spriteRenderer.enabled = false;
            gameObject.SetActive(false);
        }
        else if (col.CompareTag("Player") && col.gameObject != player.gameObject)
        {
            if (InstanceFinder.IsServer)
                col.gameObject.GetComponent<Player>().HealthChange(-missilePower);
            spriteRenderer.enabled = false;
            gameObject.SetActive(false);
        }
    }
}