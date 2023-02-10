using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrostAbilities : AbilityBase
{
    public override void TriggerAbility(Vector2 casterPosition, Vector2 mousePosition)
    {
        base.TriggerAbility(casterPosition, mousePosition);

        transform.position = mousePosition;
    }
}