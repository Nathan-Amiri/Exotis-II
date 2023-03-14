using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Component.Animating;

public class LightningAbilities : AbilityBase
{
    public override void OnSpawn(Player newPlayer, string newName)
    {
        base.OnSpawn(newPlayer, newName);

        bool hasCore = false;

        if (name == "Electrify")
        {
            cooldown = 8;
            hasRange = false;
            hasCore = true;
        }
        else if (name == "Blink")
        {
            cooldown = 4;
            hasRange = true;
            spellRange = 2.5f;
        }
        else if (name == "Recharge")
        {
            cooldown = 12;
            hasRange = false;
            RechargeSetup();
        }

        spellColor = player.lightning;
        if (hasCore)
            coreRenderer.color = spellColor.Equals(player.shellColor) ? player.coreColor : player.shellColor;
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

        StartCoroutine(StartCooldown());

        if (!IsOwner)
            return;

        blinkAnimator.SetTrigger("Blink");

        Vector2 blinkPosition;
        Vector2 blinkDirection = (aimPoint - casterPosition).normalized;

        if ((casterPosition - aimPoint).magnitude < spellRange)
            blinkPosition = aimPoint;
        else
            blinkPosition = casterPosition + blinkDirection * spellRange;

        float blinkIncrement = (blinkPosition - casterPosition).magnitude / 10;

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

    public Animator rechargeAnimator;
    public GameObject rechargeAura;
    private void RechargeSetup()
    {
        transform.SetParent(player.transform);
        transform.position = player.transform.position + new Vector3(0, .45f);
    }
    private void Recharge()
    {
        rechargeAnimator.SetTrigger("RechargeFade");
        StartCoroutine(RechargeChannel());
    }
    private IEnumerator RechargeChannel()
    {
        player.playerMovement.ToggleStun(true);
        yield return new WaitForSeconds(2);
        player.playerMovement.ToggleStun(false);
        StartCoroutine(RechargeBuff());
    }
    private IEnumerator RechargeBuff()
    {
        rechargeAura.SetActive(true);
        if (IsServer)
            player.HealthChange(3);
        player.StatChange("speed", 1);

        yield return new WaitForSeconds(5);

        rechargeAura.SetActive(false);
        player.StatChange("speed", -1);

        StartCoroutine(StartCooldown());
    }
}