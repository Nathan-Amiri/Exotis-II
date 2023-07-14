using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Hail : SpellBase
{
    public SpriteRenderer hitboxRenderer;
    public Animator anim;
    public OnEnterDamage onEnterDamage;

    public override void OnSpawn(Player newPlayer, string newName)
    {
        base.OnSpawn(newPlayer, newName);

        cooldown = 4;
        spellColor = player.frost;

        onEnterDamage.damage = -3;
        onEnterDamage.owner = player.gameObject;

        SetCore(hitboxRenderer);
    }

    public override void TriggerSpell(Vector2 casterPosition, Vector2 aimPoint)
    {
        base.TriggerSpell(casterPosition, aimPoint);

        StartCoroutine(HailDuration());

        transform.position = aimPoint;
    }

    private IEnumerator HailDuration()
    {
        anim.SetTrigger("Hail");

        yield return new WaitForSeconds(2);

        Disappear();
        StartCoroutine(StartCooldown());
    }

    public override void GameEnd()
    {
        base.GameEnd();

        Disappear();
    }
}