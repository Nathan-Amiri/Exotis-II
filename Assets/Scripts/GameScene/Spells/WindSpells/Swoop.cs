using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Swoop : SpellBase
{
    public SpriteRenderer swoopSR; //assigned in inspector
    private bool swooping;

    public override void OnSpawn(Player newPlayer, string newName)
    {
        base.OnSpawn(newPlayer, newName);

        cooldown = 8;
        spellColor = player.wind;

        transform.SetParent(player.transform);
        transform.position = player.transform.position;
    }

    public override void TriggerSpell(Vector2 casterPosition, Vector2 aimPoint)
    {
        base.TriggerSpell(casterPosition, aimPoint);

        if (IsServer)
            StartCoroutine(player.BecomeImmune(1));
        if (IsOwner)
        {
            player.playerMovement.ToggleStun(true);
            player.playerMovement.ToggleGravity(false);

            float angle = Vector2.Angle(player.mousePosition - (Vector2)player.transform.position, Vector2.right);
            int posOrNeg = (player.mousePosition - (Vector2)player.transform.position).y > 0 ? 1 : -1;
            transform.rotation = Quaternion.Euler(0, 0, angle * posOrNeg); //set the correct rotation before beginning to swoop

            swooping = true;
        }

        swoopSR.enabled = true;
        player.spriteRenderer.enabled = false;
        player.coreRenderer.enabled = false;

        StartCoroutine(EndSwoop());
    }
    protected override void Update()
    {
        base.Update();

        if (swooping)
        {
            float angle = Vector2.Angle(player.mousePosition - (Vector2)transform.position, Vector2.right);
            int posOrNeg = (player.mousePosition - (Vector2)transform.position).y > 0 ? 1 : -1;
            Quaternion direction = Quaternion.Euler(0, 0, angle * posOrNeg);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, direction, Time.deltaTime * 100);

            player.playerMovement.rb.velocity = transform.right * 5;
        }
    }
    private IEnumerator EndSwoop()
    {
        yield return new WaitForSeconds(.9f);
        if (IsOwner)
        {
            player.playerMovement.ToggleStun(false);
            player.playerMovement.ToggleGravity(true);
            swooping = false;
        }
        swoopSR.enabled = false;
        player.spriteRenderer.enabled = true;
        player.coreRenderer.enabled = true;

        StartCoroutine(StartCooldown());
    }

    public override void GameEnd()
    {
        base.GameEnd();

        //if swoopSR is enabled, EndSwoop was running
        if (swoopSR.enabled)
        {
            if (IsOwner)
            {
                player.playerMovement.ToggleGravity(true);
                swooping = false;
            }
            swoopSR.enabled = false;
            player.spriteRenderer.enabled = true;
            player.coreRenderer.enabled = true;
        }
    }
}