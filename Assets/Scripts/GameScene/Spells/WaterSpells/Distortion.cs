using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Distortion : SpellBase
{
    public SpriteRenderer coreRenderer; //assigned in inspector
    public SpriteRenderer distSR; //^
    public Rigidbody2D distRB; //^
    private int distDirection;
    private bool distorting;
    public override void OnSpawn(Player newPlayer, string newName)
    {
        base.OnSpawn(newPlayer, newName);

        cooldown = 8;
        hasRange = false;
        spellColor = player.water;

        byte transparency = (byte)(IsOwner ? 153 : 255);
        distSR.color = new Color32(player.shellColor.r, player.shellColor.g, player.shellColor.b, transparency);
        coreRenderer.color = new Color32(player.coreColor.r, player.coreColor.g, player.coreColor.b, transparency);
    }

    public override void TriggerSpell(Vector2 casterPosition, Vector2 aimPoint)
    {
        base.TriggerSpell(casterPosition, aimPoint);

        if (IsOwner)
            RpcSendDistortion(player.playerMovement.rb.velocity);
    }
    [ServerRpc]
    private void RpcSendDistortion(Vector2 startingVelocity)
    {
        RpcReceiveDistortion(startingVelocity);
    }
    [ObserversRpc]
    private void RpcReceiveDistortion(Vector2 startingVelocity)
    {
        StartCoroutine(StartCooldown());

        transform.position = player.transform.position;
        distRB.velocity = startingVelocity;
        distDirection = startingVelocity.x < 0 ? -1 : 1;
        distorting = true;

        if (!IsOwner)
        {
            player.spriteRenderer.enabled = false;
            player.coreRenderer.enabled = false;

            foreach (Transform aura in player.transform)
                if (aura.CompareTag("Aura"))
                {
                    aura.SetParent(transform);
                    aura.position = transform.position;
                }
        }

        StartCoroutine(DistortionDuration());
    }
    protected override void Update()
    {
        base.Update();

        if (distorting)
        {
            if (distRB.velocity.y < 0)
                distRB.velocity += (player.playerMovement.fallMultiplier - 1) * Physics2D.gravity.y * Time.deltaTime * Vector2.up; //identical code to playerMovement
            distRB.velocity = new(distDirection * player.playerMovement.moveSpeed * player.playerMovement.speedIncrease, distRB.velocity.y);
        }
    }
    private IEnumerator DistortionDuration()
    {
        StartCoroutine(DisappearDelay(1.5f));

        yield return new WaitForSeconds(1.5f);

        EndDistortion();
    }

    private void EndDistortion()
    {
        distorting = false;
        distDirection = 0;
        distRB.velocity = Vector2.zero;

        if (!IsOwner)
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

        EndDistortion();
    }
}