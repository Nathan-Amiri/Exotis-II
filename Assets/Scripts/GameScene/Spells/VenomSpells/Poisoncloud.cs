using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Poisoncloud : SpellBase
{
    public SpriteRenderer coreRenderer; //assigned in inspector

    public Animator poisonCloudAnim; //^

    public override void OnSpawn(Player newPlayer, string newName)
    {
        base.OnSpawn(newPlayer, newName);

        cooldown = 8;
        hasRange = false;
        spellColor = player.venom;
        SetCore(coreRenderer);
    }

    public override void TriggerSpell(Vector2 casterPosition, Vector2 aimPoint)
    {
        //check isGrounded before calling base.TriggerSpell, which will lock spell
        if (!player.playerMovement.isGrounded)
            return;

        base.TriggerSpell(casterPosition, aimPoint);

        StartCoroutine(StartCooldown());

        transform.position = casterPosition + (.17f * Vector2.down);
        poisonCloudAnim.SetTrigger("Grow");
        StartCoroutine(Disappear(4));
    }
    private void OnTriggerEnter2D(Collider2D col)
    {
        if (!IsServer)
            return;

        if (col.CompareTag("Player") && col.gameObject != player.gameObject)
            col.GetComponent<Player>().HealthChange(-3f);
    }
}