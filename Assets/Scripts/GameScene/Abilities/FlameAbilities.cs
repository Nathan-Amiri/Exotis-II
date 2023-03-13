using FishNet.Component.Animating;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class FlameAbilities : AbilityBase
{
    public override void OnSpawn(Player newPlayer, string newName)
    {
        base.OnSpawn(newPlayer, newName);

        bool hasCore = false;

        if (name == "Erupt")
        {
            cooldown = 4;
            hasRange = false;
            hasCore = true;
        }
        else if (name == "Singe")
        {
            cooldown = 8;
            hasRange = false;
            hasCore = true;
        }
        else if (name == "Heatup")
        {
            cooldown = 12;
            hasRange = false;
            HeatSetup();
        }

        spellColor = player.flame;
        if (hasCore)
            coreRenderer.color = spellColor.Equals(player.shellColor) ? player.coreColor : player.shellColor;
    }
    public override void TriggerAbility(Vector2 casterPosition, Vector2 aimPoint)
    {
        if (name == "Erupt") Erupt();
        if (name == "Singe") Singe();
        if (name == "Heatup") Heatup();
    }

    private void Erupt()
    {

    }

    private void Singe()
    {

    }

    public Animator heatAnimator;
    public GameObject heatAura;
    private void HeatSetup()
    {
        transform.SetParent(player.transform);
    }
    private void Heatup()
    {
        transform.position = player.transform.position + new Vector3(0, .45f); //uses player's current position, not castPosition
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
