using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fangedbite : SpellBase
{
    public SpriteRenderer coreRenderer; //assigned in inspector

    public override void OnSpawn(Player newPlayer, string newName)
    {
        base.OnSpawn(newPlayer, newName);

        cooldown = 8;
        hasRange = false;
        spellColor = player.venom;
        SetCore(coreRenderer);
    }

    public override void TriggerSpell(Vector2 casterPosition, Vector2 aimPoint)
    {
        base.TriggerSpell(casterPosition, aimPoint);


    }
}