using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Heatup : SpellBase
{
    public Animator heatAnimator; //assigned in inspector
    public GameObject heatAura; //^
    public override void OnSpawn(Player newPlayer, string newName)
    {
        base.OnSpawn(newPlayer, newName);

        cooldown = 12;
        hasRange = false;
        spellColor = player.flame;

        transform.SetParent(player.transform);
    }

    public override void TriggerSpell(Vector2 casterPosition, Vector2 aimPoint)
    {
        base.TriggerSpell(casterPosition, aimPoint);

        transform.position = player.transform.position + new Vector3(0, .45f);
        heatAnimator.SetTrigger("HeatFade");
        StartCoroutine(HeatChannel());
    }
    private IEnumerator HeatChannel()
    {
        player.playerMovement.ToggleStun(true);
        yield return new WaitForSeconds(2);
        player.playerMovement.ToggleStun(false);
        StartCoroutine(HeatBuff());
    }
    private IEnumerator HeatBuff()
    {
        heatAura.SetActive(true);
        player.StatChange("power", 1);
        player.StatChange("range", 1);

        yield return new WaitForSeconds(5);

        heatAura.SetActive(false);
        player.StatChange("power", -1);
        player.StatChange("range", -1);

        StartCoroutine(StartCooldown());
    }
}