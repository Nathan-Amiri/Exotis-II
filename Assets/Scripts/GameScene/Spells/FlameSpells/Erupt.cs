using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Erupt : SpellBase
{
    //assigned in prefab
    public GameObject flamePref;

    private readonly List<Flame> flames = new();

    private readonly float spreadAngle = 15;
    private readonly float launchSpeed = 6;

    public override void OnSpawn(Player newPlayer, string newName)
    {
        base.OnSpawn(newPlayer, newName);

        cooldown = 4;
        hasRange = false;
        spellColor = player.flame;

        for (int i = 0; i < 3; i++)
        {
            //flames can't be erupt's children because flames are not networked
            GameObject flameObject = Instantiate(flamePref);
            Flame flame = flameObject.GetComponent<Flame>();

            flame.owner = newPlayer.gameObject;
            flame.damage = -3;
            flames.Add(flame);

            Color color = spellColor.Equals(player.shellColor) ? player.coreColor : player.shellColor;
            color = new Color(color.r, color.g, color.b, .6f); //make transparent
            flame.coreRenderer.color = color;
        }
    }

    public override void TriggerSpell(Vector2 casterPosition, Vector2 aimPoint)
    {
        base.TriggerSpell(casterPosition, aimPoint);

        StartCoroutine(StartCooldown());

        Vector2 aimDirection = (aimPoint - casterPosition).normalized;

        foreach (Flame flame in flames)
        {
            flame.Reset();
            flame.transform.position = casterPosition + (aimDirection * .1f);
            flame.gameObject.SetActive(true);
        }

        flames[0].rb.velocity = aimDirection * launchSpeed;
        flames[1].rb.velocity = Quaternion.AngleAxis(spreadAngle, Vector3.forward) * aimDirection * launchSpeed;
        flames[2].rb.velocity = Quaternion.AngleAxis(-spreadAngle, Vector3.forward) * aimDirection * launchSpeed;
    }

    public override void GameEnd()
    {
        base.GameEnd();

        StartCoroutine(Disappear(0));
    }
}