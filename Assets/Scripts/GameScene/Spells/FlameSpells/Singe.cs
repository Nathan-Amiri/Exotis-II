using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Singe : SpellBase
{
    public SpriteRenderer coreRenderer;
    public Rigidbody2D rb;

    private readonly float launchSpeed = 8;
    private readonly float explodeForce = 20;

    public override void OnSpawn(Player newPlayer, string newName)
    {
        base.OnSpawn(newPlayer, newName);

        cooldown = 8;
        spellColor = player.flame;
        SetCore(coreRenderer);
    }

    public override void TriggerSpell(Vector2 casterPosition, Vector2 aimPoint)
    {
        base.TriggerSpell(casterPosition, aimPoint);

        StartCoroutine(StartCooldown());

        rb.constraints = RigidbodyConstraints2D.FreezeRotation; //default

        StartCoroutine(DisappearDelay(7));

        Vector2 aimDirection = (aimPoint - casterPosition).normalized;
        transform.position = casterPosition + (aimDirection * .25f);
        rb.velocity = aimDirection * launchSpeed;
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Terrain"))
        {
            rb.velocity = Vector2.zero;
            rb.constraints = RigidbodyConstraints2D.FreezeAll;
        }
        else if (col.CompareTag("Player"))
        {
            Player target = col.gameObject.GetComponent<Player>();
            if (!target.IsOwner) return;

            Vector2 explodeDirection = (col.transform.position - transform.position).normalized;
            target.playerMovement.rb.velocity *= new Vector2(1, 0);
            target.playerMovement.AddNewForce(explodeDirection * explodeForce);
            target.playerMovement.GiveJump();

            //damage detected on the client rather than on the server to ensure that damage and explosion always coincide
            if (!IsOwner)
                RpcSendDamageToServer(target);
        }
    }

    [ServerRpc (RequireOwnership = false)]
    private void RpcSendDamageToServer(Player target)
    {
        target.HealthChange(-3);
    }

    public override void GameEnd()
    {
        base.GameEnd();

        rb.velocity = Vector2.zero;
        rb.constraints = RigidbodyConstraints2D.FreezeAll;

        Disappear();
    }
}