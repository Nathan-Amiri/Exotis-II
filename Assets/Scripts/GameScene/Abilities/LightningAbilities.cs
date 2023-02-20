using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class LightningAbilities : AbilityBase
{
    public override void OnSpawn(Player newPlayer, string newName)
    {
        base.OnSpawn(newPlayer, newName);

        if (name == "Electrify")
        {
            cooldown = 8;
            hasRange = false;
        }
        else if (name == "Blink")
        {
            cooldown = 4;
            hasRange = true;
            abilityRange = 2.5f;
        }
        if (name == "Recharge")
        {
            cooldown = 8;
            hasRange = false;
        }
    }
    public override void TriggerAbility(bool isOwner, Vector2 casterPosition, Vector2 aimPoint)
    {
        if (name == "Electrify") Electrify();
        if (name == "Blink") Blink(isOwner, casterPosition, aimPoint);
        if (name == "Recharge") Recharge();
    }

    private void Electrify()
    {

    }

    public SpriteRenderer blinkRenderer;
    public Animator blinkAnimator; //assigned in inspector
    private Color32 blinkError = Color.red;
    private Color32 blinkNormal = new(255, 236, 0, 255);
    private void Blink(bool isOwner, Vector2 casterPosition, Vector2 aimPoint)
    {
        Vector2 blinkPosition;
        if ((casterPosition - aimPoint).magnitude < abilityRange)
            blinkPosition = aimPoint;
        else
        {
            Vector2 blinkDirection = (aimPoint - casterPosition).normalized;
            blinkPosition = new Vector2(player.transform.position.x, player.transform.position.y) + blinkDirection * abilityRange;
        }

        int layerMask = 1 << 3; //raycast only checks layer 3 (Terrain)
        RaycastHit2D hit = Physics2D.Raycast(blinkPosition, Vector2.zero, 0, layerMask);
        if (hit.collider != null)
        {
            if (!isOwner)
                return;

            blinkRenderer.color = blinkError;
            transform.position = blinkPosition;
            blinkAnimator.SetTrigger("Blink");
        }
        else
        {
            StartCoroutine(StartCooldown());

            blinkRenderer.color = blinkNormal;
            transform.position = player.transform.position;
            blinkAnimator.SetTrigger("Blink");

            if(isOwner)
                player.transform.position = blinkPosition;
        }
    }

    private void Recharge()
    {

    }
}