using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hail : SpellBase
{
    public SpriteRenderer coreRenderer; //assigned in inspector

    public override void OnSpawn(Player newPlayer, string newName)
    {
        base.OnSpawn(newPlayer, newName);

        cooldown = 4;
        hasRange = true;
        spellColor = player.frost;
        SetCore(coreRenderer);
    }

    public override void TriggerSpell(Vector2 casterPosition, Vector2 aimPoint)
    {
        base.TriggerSpell(casterPosition, aimPoint);


    }

    public override void GameEnd()
    {
        base.GameEnd();


    }
}