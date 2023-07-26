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

        StartCoroutine(DisappearDelay(4));
    }
    private void OnTriggerEnter2D(Collider2D col)
    {
        if (!col.CompareTag("Player"))
            return;
        Debug.Log(col.name);
        if (!IsOwner && col.gameObject == player.gameObject)
        {
            player.spriteRenderer.enabled = false;
            player.coreRenderer.enabled = false;

            foreach (Transform aura in player.transform)
                if (aura.CompareTag("Aura"))
                {
                    aura.SetParent(transform);
                    aura.position = new Vector3(0, -15, 0);
                }
        }
        else if (IsServer && col.gameObject != player.gameObject)
            col.GetComponent<Player>().HealthChange(-3);
    }

    private void OnTriggerExit2D(Collider2D col)
    {
        if (!IsOwner && col.CompareTag("Player") && col.gameObject == player.gameObject)
        {
            player.spriteRenderer.enabled = true;
            player.coreRenderer.enabled = true;

            foreach (Transform aura in transform)
                if (aura.CompareTag("Aura"))
                {
                    aura.SetParent(player.transform);
                    aura.position = player.transform.position;
                }
        }
    }

    public override void GameEnd()
    {
        base.GameEnd();

        Disappear();
    }
}