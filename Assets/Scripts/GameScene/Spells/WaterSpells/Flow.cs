using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flow : SpellBase
{
    public SpriteRenderer sr;
    public CircleCollider2D col;

    private bool hasHealed;

    public override void OnSpawn(Player newPlayer, string newName)
    {
        base.OnSpawn(newPlayer, newName);

        cooldown = 4;
        spellColor = player.water;

        transform.parent = player.transform;
        transform.localPosition = Vector3.zero;
    }
    
    public override void TriggerSpell(Vector2 casterPosition, Vector2 aimPoint)
    {
        base.TriggerSpell(casterPosition, aimPoint);

        StartCoroutine(StartCooldown());

        hasHealed = false;

        StartCoroutine(EndFlow());
    }

    private IEnumerator EndFlow()
    {
        sr.enabled = true;
        col.enabled = true;

        yield return new WaitForSeconds(.1f);

        sr.enabled = false;
        col.enabled = false;
    }

    private void OnTriggerEnter2D(Collider2D newCol)
    {
        if (IsServer && newCol.CompareTag("Player") && newCol.gameObject != player)
        {
            newCol.GetComponent<Player>().HealthChange(-1.5f);
            if (!hasHealed)
            {
                player.HealthChange(3);
                hasHealed = true;
            }
        }
    }
}