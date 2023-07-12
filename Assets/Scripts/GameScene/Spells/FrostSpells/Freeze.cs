using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Freeze : SpellBase
{
    public SpriteRenderer coreRenderer;

    public override void OnSpawn(Player newPlayer, string newName)
    {
        base.OnSpawn(newPlayer, newName);

        cooldown = 12;
        hasRange = false;
        spellColor = player.frost;
        SetCore(coreRenderer);
    }

    public override void TriggerSpell(Vector2 casterPosition, Vector2 aimPoint)
    {
        base.TriggerSpell(casterPosition, aimPoint);

        StartCoroutine(StartCooldown());

        transform.position = aimPoint;
        StartCoroutine(Disappear(5));
    }
    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            if (col.gameObject == player.gameObject)
                player.StatChange("speed", 1);
            else
                col.GetComponent<Player>().StatChange("speed", -2);
        }
    }
    private void OnTriggerExit2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            if (col.gameObject == player.gameObject)
                player.StatChange("speed", -1);
            else
                col.GetComponent<Player>().StatChange("speed", 2);
        }
    }

    public override void GameEnd()
    {
        base.GameEnd();

        StartCoroutine(Disappear(0));
    }
}