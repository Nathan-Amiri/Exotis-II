using FishNet.Component.Animating;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blink : SpellBase
{
    public SpriteRenderer coreRenderer; //assigned in inspector
    public override void OnSpawn(Player newPlayer, string newName)
    {
        base.OnSpawn(newPlayer, newName);

        cooldown = 8;
        hasRange = true;
        spellRange = 2.5f;
        spellColor = player.lightning;
        SetCore(coreRenderer);
    }

    public override void TriggerSpell(Vector2 casterPosition, Vector2 aimPoint)
    {
        base.TriggerSpell(casterPosition, aimPoint);

        StartCoroutine(StartCooldown());

        transform.position = casterPosition;

        StartCoroutine(Disappear(5));

        if (!IsOwner)
            return;

        //blink teleport:
        Vector2 blinkPosition;
        Vector2 blinkDirection = (aimPoint - casterPosition).normalized;

        if ((casterPosition - aimPoint).magnitude < spellRange)
            blinkPosition = aimPoint;
        else
            blinkPosition = casterPosition + blinkDirection * spellRange;

        float blinkIncrement = (blinkPosition - casterPosition).magnitude / 10;

        //prevent blinking into walls
        int layerMask = 1 << 7; //raycast only checks layer 7 (Terrain)
        for (int i = 0; i < 11; i++) //loop happens 1 more times than there are blink increments
        {
            RaycastHit2D hit = Physics2D.Raycast(blinkPosition, Vector2.zero, 0, layerMask);
            if (hit.collider != null)
                blinkPosition -= blinkDirection * blinkIncrement;
            else if (i == 11) //just a precaution
                blinkPosition = casterPosition;
            else
                break;
        }

        player.transform.position = blinkPosition;
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (IsServer && col.CompareTag("Player") && col.gameObject != player.gameObject)
            col.GetComponent<Player>().HealthChange(-1.5f);
    }

    public override void GameEnd()
    {
        base.GameEnd();

        StartCoroutine(Disappear(0));
    }
}