using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet;

public class Flame : MonoBehaviour
{
    //assigned in prefab
    public Rigidbody2D rb;
    public SpriteRenderer coreRenderer; //accessed by Erupt

    //set by Erupt
    [NonSerialized] public float damage;
    [NonSerialized] public GameObject owner;

    private readonly List<GameObject> damagedPlayers = new();

    private void Update()
    {
        transform.rotation = Quaternion.LookRotation(Vector3.forward, Quaternion.identity * new Vector3(rb.velocity.x, rb.velocity.y));
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Terrain"))
            gameObject.SetActive(false);
        else if (InstanceFinder.IsServer && col.CompareTag("Player") && col.gameObject != owner)
        {
            //prevent players from being damaged by more than one flame per erupt cast
            if (damagedPlayers.Contains(col.gameObject)) return;

            col.GetComponent<Player>().HealthChange(damage);
            damagedPlayers.Add(col.gameObject);
        }
    }

    public void Reset() //called by Erupt
    {
        damagedPlayers.Clear();
    }
}