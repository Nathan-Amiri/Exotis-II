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

        }
        else if (name == "Hail")
        {

        }
        if (name == "Freeze")
        {
            cooldown = 10;
            hasRange = true;
            abilityRange = 10;
        }
    }

    public override void TriggerAbility(Vector2 casterPosition, Vector2 mousePosition)
    {
        base.TriggerAbility(casterPosition, mousePosition);

        if (name == "IcyBreath") IcyBreath();
        if (name == "Hail") Hail();
        if (name == "Freeze") Freeze(casterPosition, mousePosition);
    }

    private void IcyBreath()
    {

    }

    private void Hail()
    {

    }

    private void Freeze(Vector2 casterPosition, Vector2 mousePosition)
    {
        transform.position = mousePosition;
    }
}