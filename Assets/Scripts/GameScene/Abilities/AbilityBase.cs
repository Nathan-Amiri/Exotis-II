using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using FishNet.Object;

public class AbilityBase : NetworkBehaviour
{
    public SpriteRenderer coreRenderer; //assigned in inspector

    protected float cooldown; //set by derived class

    [NonSerialized] public float abilityRange; //set by derived class
    [NonSerialized] public bool hasRange; //^, read by Player

    [NonSerialized] public Player player;

    [NonSerialized] public bool onCooldown = false;

    public virtual void OnSpawn(Player newPlayer, string newName) //called by player
    {
        name = newName;
        player = newPlayer;
    }

    public virtual void TriggerAbility(Vector2 casterPosition, Vector2 mousePosition) { }

    protected IEnumerator StartCooldown()
    {
        onCooldown = true;
        yield return new WaitForSeconds(cooldown);
        onCooldown = false;
    }
    protected IEnumerator Disappear(float delay)
    {
        yield return new WaitForSeconds(delay);
        transform.position = new Vector2(-15, 0);
    }
}