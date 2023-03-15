using FishNet.Object;
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
            cooldown = 8;
            hasRange = false;
            InfectSetup();
        }
        else if (name == "Poisoncloud")
        {
            cooldown = 8;
            hasRange = false;
            hasCore = true;
        }

        spellColor = player.venom;
        if (hasCore)
            coreRenderer.color = spellColor.Equals(player.shellColor) ? player.coreColor : player.shellColor;
    }
    public override void TriggerAbility(Vector2 casterPosition, Vector2 aimPoint)
    {
        base.TriggerAbility(casterPosition, aimPoint);

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

    public GameObject infectAura; //assigned in inspector
    public OnEnterDamage[] infectTraps = new OnEnterDamage[3]; //^
    public SpriteRenderer[] trapCores = new SpriteRenderer[3]; //^
    public Animator[] infectAnims = new Animator[3]; //^
    private int infectCounter;
    private void InfectSetup()
    {
        infectAura.transform.SetParent(player.transform);
        infectAura.transform.position = player.transform.position;

        foreach (OnEnterDamage infectTrap in infectTraps)
            infectTrap.owner = player.gameObject;

        foreach (SpriteRenderer trapCore in trapCores)
            trapCore.color = spellColor.Equals(player.shellColor) ? player.coreColor : player.shellColor;
    }
    private void Infect()
    {
        infectAura.SetActive(true);

        player.infectSpell = this;

        foreach (OnEnterDamage infectTrap in infectTraps)
            infectTrap.transform.position = new Vector2(-15, 0);
    }
    [ObserversRpc]
    public void RpcTriggerInfect(Vector2 missilePosition) //called by Missile
    {
        infectTraps[infectCounter].transform.position = missilePosition;
        infectAnims[infectCounter].SetTrigger("Grow");

        infectCounter += 1;
        if (infectCounter == 3)
        {
            infectAura.SetActive(false);

            infectCounter = 0;
            player.infectSpell = null;

            StartCoroutine(StartCooldown());
        }
    }

    public Animator poisonCloudAnim; //assigned in inspector
    private void PoisonCloud(Vector2 casterPosition)
    {
        StartCoroutine(StartCooldown());

        transform.position = casterPosition + (.17f * Vector2.down);
        poisonCloudAnim.SetTrigger("Grow");
        StartCoroutine(Disappear(4));
    }
    private void OnEnterPoisonCloud(Collider2D col)
    {
        if (!IsServer)
            return;

        if (col.CompareTag("Player") && col.gameObject != player.gameObject)
            col.GetComponent<Player>().HealthChange(-3f);
    }
}