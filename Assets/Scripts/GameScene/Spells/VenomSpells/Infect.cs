using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;

public class Infect : SpellBase
{
    public GameObject infectAura; //assigned in inspector
    public OnEnterDamage[] infectTraps = new OnEnterDamage[3]; //^
    public SpriteRenderer[] trapCores = new SpriteRenderer[3]; //^
    public Animator[] infectAnims = new Animator[3]; //^
    private int infectCounter;

    public override void OnSpawn(Player newPlayer, string newName)
    {
        base.OnSpawn(newPlayer, newName);

        cooldown = 12;
        spellColor = player.venom;

        infectAura.transform.SetParent(player.transform);
        infectAura.transform.position = player.transform.position;

        foreach (OnEnterDamage infectTrap in infectTraps)
        {
            infectTrap.damage = -1.5f;
            infectTrap.owner = player.gameObject;
        }

        foreach (SpriteRenderer trapCore in trapCores)
            trapCore.color = spellColor.Equals(player.shellColor) ? player.coreColor : player.shellColor;
    }

    public override void TriggerSpell(Vector2 casterPosition, Vector2 aimPoint)
    {
        base.TriggerSpell(casterPosition, aimPoint);

        infectAura.SetActive(true);

        player.infectSpell = this;

        foreach (OnEnterDamage infectTrap in infectTraps)
            infectTrap.transform.position = new Vector2(-15, 0);
    }
    public void FireInfectedMissile()
    {
        infectCounter += 1;
        if (infectCounter == 3)
        {
            infectAura.SetActive(false);

            infectCounter = 0;

            StartCoroutine(StartCooldown());
        }
    }
    [ObserversRpc]
    public void RpcTriggerInfect(Vector2 missilePosition) //called by Missile
    {
        infectTraps[infectCounter].transform.position = missilePosition;
        infectAnims[infectCounter].SetTrigger("Grow");

        if (infectCounter == 0)
            player.infectSpell = null;
    }

    public override void GameEnd()
    {
        base.GameEnd();

        infectAura.SetActive(false);
        infectCounter = 0;
        player.infectSpell = null;

        foreach (OnEnterDamage infectTrap in infectTraps)
            infectTrap.transform.position = new Vector2(-15, 0);
    }
}