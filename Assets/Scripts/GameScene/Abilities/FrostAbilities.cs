using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrostAbilities : AbilityBase
{
    public override void OnSpawn(Player newPlayer, string newName)
    {
        base.OnSpawn(newPlayer, newName);

        if (name == "Icy Breath")
        {
            hasRange = false;
        }
        else if (name == "Hail")
        {
            hasRange = true;
        }
        if (name == "Freeze")
        {
            cooldown = 1;
            hasRange = true;
            abilityRange = 3.5f;
        }
    }

    public override void TriggerAbility(Vector2 casterPosition, Vector2 aimPoint)
    {
        base.TriggerAbility(casterPosition, aimPoint);

        if (name == "Icy Breath") IcyBreath();
        if (name == "Hail") Hail();
        if (name == "Freeze") Freeze(casterPosition, aimPoint);
    }

    private void IcyBreath()
    {

    }

    private void Hail()
    {

    }

    private void Freeze(Vector2 casterPosition, Vector2 aimPoint)
    {
        transform.position = aimPoint;
    }
}