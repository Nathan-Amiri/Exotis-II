using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Icybreath : SpellBase
{
    public SpriteRenderer coreRenderer; //assigned in inspector
    public Transform icyPivot; //^
    private bool icyGrow;

    public override void OnSpawn(Player newPlayer, string newName)
    {
        base.OnSpawn(newPlayer, newName);

        cooldown = 12;
        hasRange = false;
        spellColor = player.frost;
        SetCore(coreRenderer);

        icyPivot.SetParent(player.spellParent.transform, true);
        transform.SetParent(icyPivot, true);
    }

    public override void TriggerSpell(Vector2 casterPosition, Vector2 aimPoint)
    {
        base.TriggerSpell(casterPosition, aimPoint);

        StartCoroutine(StartCooldown());

        float angle = Vector2.Angle(aimPoint - casterPosition, Vector2.right);
        int posOrNeg = (aimPoint - casterPosition).y > 0 ? 1 : -1;
        Quaternion rotation = Quaternion.Euler(0, 0, angle * posOrNeg);
        Vector3 position = casterPosition + .5f * -(Vector2)transform.up;
        icyPivot.transform.SetPositionAndRotation(position, rotation);

        icyGrow = true;
        StartCoroutine(EndIcyGrow());
    }
    protected override void Update()
    {
        base.Update();

        if (icyGrow)
            icyPivot.localScale += new Vector3(3 * Time.deltaTime, 0);
    }
    private IEnumerator EndIcyGrow()
    {
        yield return new WaitForSeconds(1.5f);
        icyGrow = false;
        StartCoroutine(IcyReset());
    }
    private IEnumerator IcyReset()
    {
        yield return new WaitForSeconds(3.5f);
        icyPivot.rotation = Quaternion.identity;
        icyPivot.localScale = new Vector2(.4f, .4f);
        icyPivot.position = new Vector2(-15, 0);
    }

    public override void GameEnd()
    {
        base.GameEnd();

        icyGrow = false;
        icyPivot.rotation = Quaternion.identity;
        icyPivot.localScale = new Vector2(.4f, .4f);
        icyPivot.position = new Vector2(-15, 0);
    }
}