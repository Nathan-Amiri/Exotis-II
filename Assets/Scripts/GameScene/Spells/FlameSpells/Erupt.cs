using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Erupt : SpellBase
{
    public SpriteRenderer coreRenderer; //assigned in inspector

    public override void OnSpawn(Player newPlayer, string newName)
    {
        base.OnSpawn(newPlayer, newName);

        cooldown = 4;
        hasRange = false;
        spellColor = player.flame;
        SetCore(coreRenderer);
    }
}