using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Singe : SpellBase
{
    public SpriteRenderer coreRenderer;

    public override void OnSpawn(Player newPlayer, string newName)
    {
        base.OnSpawn(newPlayer, newName);

        cooldown = 8;
        hasRange = false;
        spellColor = player.flame;
        SetCore(coreRenderer);
    }
}