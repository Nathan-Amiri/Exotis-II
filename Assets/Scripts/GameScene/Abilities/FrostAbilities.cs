using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrostAbilities : AbilityBase
{
    public override void OnSpawn(Player newPlayer, string newName)
    {
        base.OnSpawn(newPlayer, newName);

        if (name == "Icybreath")
        {
            hasRange = false;
        }
        else if (name == "Hail")
        {
            hasRange = true;
        }
        if (name == "Freeze")
        {
            cooldown = 8;
            hasRange = true;
            abilityRange = 3.5f;
        }
    }
    public override void TriggerAbility(Vector2 casterPosition, Vector2 aimPoint)
    {
        base.TriggerAbility(casterPosition, aimPoint);

        if (name == "Icybreath") IcyBreath();
        if (name == "Hail") Hail();
        if (name == "Freeze") Freeze(casterPosition, aimPoint);
    }
    private void OnTriggerEnter2D(Collider2D col)
    {
        if (name == "Freeze") OnEnterFreeze(col);
    }
    private void OnTriggerExit2D(Collider2D col)
    {
        if (name == "Freeze") OnExitFreeze(col);
    }
    private IEnumerator Disappear(float delay)
    {
        yield return new WaitForSeconds(delay);
        transform.position = new Vector2(-15, 0);
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
        StartCoroutine(Disappear(4));
    }
    private void OnEnterFreeze(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            if (col.gameObject == player.gameObject)
                player.StatChange("speed", 2);
            else
                col.GetComponent<Player>().StatChange("speed", -1);
        }
    }
    private void OnExitFreeze(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            if (col.gameObject == player.gameObject)
                player.StatChange("speed", -2);
            else
                col.GetComponent<Player>().StatChange("speed", 1);
        }
    }
}