using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrostAbilities : AbilityBase
{
    public override void OnSpawn(Player newPlayer, string newName)
    {
        base.OnSpawn(newPlayer, newName);

        bool hasCore = false;

        if (name == "Icybreath")
        {
            hasRange = false;
            hasCore = true;
        }
        else if (name == "Hail")
        {
            hasRange = true;
            hasCore = true;
        }
        else if (name == "Freeze")
        {
            cooldown = 8;
            hasRange = false;
            hasCore = true;
        }

        spellColor = player.frost;
        if (hasCore)
            coreRenderer.color = spellColor.Equals(player.lighterColor) ? player.darkerColor : player.lighterColor;
    }
    public override void TriggerAbility(Vector2 casterPosition, Vector2 aimPoint)
    {
        if (name == "Icybreath") IcyBreath();
        if (name == "Hail") Hail();
        if (name == "Freeze") Freeze(aimPoint);
    }
    private void OnTriggerEnter2D(Collider2D col)
    {
        if (name == "Freeze") OnEnterFreeze(col);
    }
    private void OnTriggerExit2D(Collider2D col)
    {
        if (name == "Freeze") OnExitFreeze(col);
    }

    private void IcyBreath()
    {

    }

    private void Hail()
    {

    }

    private void Freeze(Vector2 aimPoint)
    {
        StartCoroutine(StartCooldown());

        transform.position = aimPoint;
        StartCoroutine(Disappear(4));
    }
    private void OnEnterFreeze(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            if (col.gameObject == player.gameObject)
                player.StatChange("speed", 1);
            else
                col.GetComponent<Player>().StatChange("speed", -1);
        }
    }
    private void OnExitFreeze(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            if (col.gameObject == player.gameObject)
                player.StatChange("speed", -1);
            else
                col.GetComponent<Player>().StatChange("speed", 1);
        }
    }
}