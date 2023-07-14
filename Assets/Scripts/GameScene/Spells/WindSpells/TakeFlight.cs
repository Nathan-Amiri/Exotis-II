using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TakeFlight : SpellBase
{
    public GameObject flightAura; //assigned in inspector
    private bool flying;

    public override void OnSpawn(Player newPlayer, string newName)
    {
        base.OnSpawn(newPlayer, newName);

        cooldown = 12;
        spellColor = player.wind;

        transform.SetParent(player.transform);
        transform.position = player.transform.position;
    }

    public override void TriggerSpell(Vector2 casterPosition, Vector2 aimPoint)
    {
        base.TriggerSpell(casterPosition, aimPoint);

        if (IsOwner)
        {
            player.playerMovement.ToggleGravity(false);
            flying = true;
        }

        flightAura.SetActive(true);

        StartCoroutine(EndFlight());
    }
    protected override void Update()
    {
        base.Update();

        if (flying)
        {
            int up = Input.GetButton("Jump") ? 1 : -1;
            player.playerMovement.rb.velocity = new Vector2(player.playerMovement.rb.velocity.x, 3 * up * player.playerMovement.speedIncrease);
        }
    }
    private IEnumerator EndFlight()
    {
        yield return new WaitForSeconds(4.5f);
        if (IsOwner)
        {
            player.playerMovement.ToggleGravity(true);
            flying = false;
        }

        flightAura.SetActive(false);

        StartCoroutine(StartCooldown());
    }

    public override void GameEnd()
    {
        base.GameEnd();

        //if flightaura is active, EndFlight was running
        if (flightAura.activeSelf)
        {
            if (IsOwner)
            {
                player.playerMovement.ToggleGravity(true);
                flying = false;
            }
            flightAura.SetActive(false);
        }
    }
}