using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;

public class OnEnterDamage : NetworkBehaviour
{
    //used for spells that have multiple damaging hitboxes
    [NonSerialized] public float damage;
    [NonSerialized] public GameObject owner;

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (IsServer && col.CompareTag("Player") && col.gameObject != owner)
                col.GetComponent<Player>().HealthChange(damage);
    }
}