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
            GameObject flameObject = Instantiate(flamePref, transform.position = new Vector2(-15, 0), Quaternion.identity);
            Flame flame = flameObject.GetComponent<Flame>();

            flame.owner = newPlayer.gameObject;
            flame.damage = -3;
            flame.erupt = this;
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
        }

        flames[0].rb.velocity = aimDirection * launchSpeed;
        flames[1].rb.velocity = Quaternion.AngleAxis(spreadAngle, Vector3.forward) * aimDirection * launchSpeed;
        flames[2].rb.velocity = Quaternion.AngleAxis(-spreadAngle, Vector3.forward) * aimDirection * launchSpeed;
    }

    public void FlameDisappear(Flame flame) //called by Flame
    {
        flame.rb.velocity = Vector2.zero;
        flame.transform.position = new Vector2(-15, 0);
    }

    public override void GameEnd()
    {
        base.GameEnd();

        foreach (Flame flame in flames)
            FlameDisappear(flame);
    }
}