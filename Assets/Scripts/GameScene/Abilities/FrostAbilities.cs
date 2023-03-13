using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrostAbilities : AbilityBase
{
    public override void OnSpawn(Player newPlayer, string newName)
    {
        base.OnSpawn(newPlayer, newName);

        bool hasCore = false;

        if (name == "Icybreath")
        {
            cooldown = 12;
            hasRange = false;
            hasCore = true;

            icyPivot.SetParent(player.abilityParent.transform, true);
            transform.SetParent(icyPivot, true);
        }
        else if (name == "Hail")
        {
            hasRange = true;
            hasCore = true;
        }
        else if (name == "Freeze")
        {
            cooldown = 8;
            hasRange = false;
            hasCore = true;
        }

        spellColor = player.frost;
        if (hasCore)
            coreRenderer.color = spellColor.Equals(player.lighterColor) ? player.darkerColor : player.lighterColor;
    }
    public override void TriggerAbility(Vector2 casterPosition, Vector2 aimPoint)
    {
        if (name == "Icybreath") IcyBreath(casterPosition, aimPoint);
        if (name == "Hail") Hail();
        if (name == "Freeze") Freeze(aimPoint);
    }
    private void OnTriggerEnter2D(Collider2D col)
    {
        if (name == "Freeze") OnEnterFreeze(col);
    }
    private void OnTriggerExit2D(Collider2D col)
    {
        if (name == "Freeze") OnExitFreeze(col);
    }

    public Transform icyPivot; //assigned in inspector
    private bool icyGrow;
    private void IcyBreath(Vector2 casterPosition, Vector2 aimPoint)
    {
        float angle = Vector2.Angle(aimPoint - casterPosition, Vector2.right);
        int posOrNeg = (aimPoint - casterPosition).y > 0 ? 1 : -1;
        icyPivot.rotation = Quaternion.Euler(0, 0, angle * posOrNeg);

        StartCoroutine(StartCooldown());

        icyPivot.position = casterPosition + .7f * new Vector2(transform.right.x, transform.right.y);

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

    private void Hail()
    {

    }

    private void Freeze(Vector2 aimPoint)
    {
        StartCoroutine(StartCooldown());

        transform.position = aimPoint;
        StartCoroutine(Disappear(4));
    }
    private void OnEnterFreeze(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            if (col.gameObject == player.gameObject)
                player.StatChange("speed", 1);
            else
                col.GetComponent<Player>().StatChange("speed", -1);
        }
    }
    private void OnExitFreeze(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            if (col.gameObject == player.gameObject)
                player.StatChange("speed", -1);
            else
                col.GetComponent<Player>().StatChange("speed", 1);
        }
    }
}