using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using FishNet.Object;
using FishNet.Connection;

public class AbilityBase : NetworkBehaviour
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

    public void TriggerAbility(Vector2 casterPosition, Vector2 mousePosition)
    {
        StartAbility(casterPosition, mousePosition);
        RpcServerTriggerAbility(ClientManager.Connection, casterPosition, mousePosition);
    }
    [ServerRpc]
    protected void RpcServerTriggerAbility(NetworkConnection caster, Vector2 casterPosition, Vector2 mousePosition)
    {
        RpcClientTriggerAbility(caster, casterPosition, mousePosition);
    }
    [ObserversRpc]
    protected void RpcClientTriggerAbility(NetworkConnection caster, Vector2 casterPosition, Vector2 mousePosition)
    {
        if (caster != ClientManager.Connection)
            StartAbility(casterPosition, mousePosition);
    }

    protected virtual void StartAbility(Vector2 casterPosition, Vector2 mousePosition) { }

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