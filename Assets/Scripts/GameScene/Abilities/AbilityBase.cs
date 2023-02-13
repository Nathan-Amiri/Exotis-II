using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AbilityBase : MonoBehaviour
{
    protected float cooldown; //set by derived class
    [NonSerialized] public bool hasRange; //read by Player, set by derived class
    [NonSerialized] public float abilityRange; //set by derived class

    [NonSerialized] public Player player;

    [NonSerialized] public bool onCooldown = false;

    public virtual void OnSpawn(Player newPlayer, string newName) //called by player
    {
        name = newName;
        player = newPlayer;
    }

    public virtual void TriggerAbility(Vector2 casterPosition, Vector2 mousePosition)
    {
        StartCoroutine(StartCooldown());
    }

    protected IEnumerator StartCooldown()
    {
        onCooldown = true;
        yield return new WaitForSeconds(cooldown);
        onCooldown = false;
    }
}