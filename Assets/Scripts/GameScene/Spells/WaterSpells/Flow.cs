using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flow : SpellBase
{
    public override void OnSpawn(Player newPlayer, string newName)
    {
        base.OnSpawn(newPlayer, newName);

        cooldown = 4;
        hasRange = false;
        spellColor = player.water;
    }
    
    public override void TriggerSpell(Vector2 casterPosition, Vector2 aimPoint)
    {
        base.TriggerSpell(casterPosition, aimPoint);


    }
}