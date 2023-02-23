using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using FishNet.Component.Animating;

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
    public override void TriggerAbility(Vector2 casterPosition, Vector2 aimPoint)
    {
        if (name == "Electrify") Electrify();
        if (name == "Blink") Blink(casterPosition, aimPoint);
        if (name == "Recharge") Recharge();
    }

    private void Electrify()
    {

    }

    public SpriteRenderer blinkRenderer;
    public NetworkAnimator blinkAnimator; //assigned in inspector
    private void Blink(Vector2 casterPosition, Vector2 aimPoint)
    {
        transform.position = player.transform.position;
        //^ using current position rather than casterPosition for lag compensation (looks better and doesn't affect gameplay)

        if (!IsOwner)
            return;

        blinkAnimator.SetTrigger("Blink");

        Vector2 blinkPosition;
        Vector2 blinkDirection = (aimPoint - casterPosition).normalized;

        if ((casterPosition - aimPoint).magnitude < abilityRange)
            blinkPosition = aimPoint;
        else
            blinkPosition = casterPosition + blinkDirection * abilityRange;

        float blinkIncrement = (blinkPosition - casterPosition).magnitude / 10;

        int layerMask = 1 << 3; //raycast only checks layer 3 (Terrain)
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

        StartCoroutine(StartCooldown());
        player.transform.position = blinkPosition;
    }

    private void Recharge()
    {

    }
}