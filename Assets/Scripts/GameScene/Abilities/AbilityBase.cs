using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AbilityBase : MonoBehaviour
{
    [NonSerialized] public Player player;

    [NonSerialized] public bool onCooldown = false;

    public virtual void TriggerAbility(Vector2 casterPosition, Vector2 mousePosition) { }
}