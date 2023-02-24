using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VenomAbilities : AbilityBase
{
    public override void OnSpawn(Player newPlayer, string newName)
    {
        base.OnSpawn(newPlayer, newName);

        bool hasCore = false;

        if (name == "Fangedbite")
        {
            cooldown = 8;
            hasRange = false;
            hasCore = true;
        }
        else if (name == "Infect")
        {
            cooldown = 12;
            hasRange = false;
        }
        else if (name == "Poisoncloud")
        {
            cooldown = 8;
            hasRange = false;
            hasCore = true;
        }

        if (hasCore)
            coreRenderer.color = player.venom.Equals(player.lighterColor) ? player.darkerColor : player.lighterColor;

    }
    public override void TriggerAbility(Vector2 casterPosition, Vector2 aimPoint)
    {
        if (name == "Fangedbite") FangedBite();
        if (name == "Infect") Infect();
        if (name == "Poisoncloud" && player.playerMovement.isGrounded) PoisonCloud(casterPosition);
    }
    private void OnTriggerEnter2D(Collider2D col)
    {
        if (name == "Poisoncloud") OnEnterPoisonCloud(col);
    }

    private void FangedBite()
    {

    }

    private void Infect()
    {

    }

    public Animator poisonCloudAnimator; //assigned in inspector
    private void PoisonCloud(Vector2 casterPosition)
    {
        if (IsOwner)
            StartCoroutine(StartCooldown());

        transform.position = casterPosition + (.17f * Vector2.down);
        poisonCloudAnimator.SetTrigger("Grow");
        StartCoroutine(Disappear(3));
    }
    private void OnEnterPoisonCloud(Collider2D col)
    {
        if (!IsServer)
            return;

        if (col.CompareTag("Player") && col.gameObject != player.gameObject)
            col.GetComponent<Player>().HealthChange(-1.5f);
    }
}