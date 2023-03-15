using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindAbilities : AbilityBase
{
    public override void OnSpawn(Player newPlayer, string newName)
    {
        base.OnSpawn(newPlayer, newName);

        //bool hasCore = false;

        if (name == "Swoop")
        {
            cooldown = 8;
            hasRange = false;
            SwoopSetup();
        }
        else if (name == "Takeflight")
        {
            cooldown = 12;
            hasRange = false;
            FlightSetup();
        }
        else if (name == "Whirlwind")
        {
            cooldown = 8;
            hasRange = false;
        }

        spellColor = player.wind;
        //if (hasCore)
        //    coreRenderer.color = spellColor.Equals(player.shellColor) ? player.coreColor : player.shellColor;
    }
    public override void TriggerAbility(Vector2 casterPosition, Vector2 aimPoint)
    {
        base.TriggerAbility(casterPosition, aimPoint);

        if (name == "Swoop") Swoop();
        if (name == "Takeflight") Takeflight();
        if (name == "Whirlwind" ) Whirlwind();
    }
    protected override void Update()
    {
        base.Update();

        SwoopUpdate();
        FlightUpdate();
    }

    public SpriteRenderer swoopSR; //assigned in inspector
    private bool swooping;
    private void SwoopSetup()
    {
        transform.SetParent(player.transform);
        transform.position = player.transform.position;
    }
    private void Swoop()
    {
        if (IsServer)
            StartCoroutine(player.BecomeImmune(1));
        if (IsOwner)
        {
            player.playerMovement.ToggleStun(true);
            player.playerMovement.ToggleGravity(false);

            float angle = Vector2.Angle(player.mousePosition - (Vector2)player.transform.position, Vector2.right);
            int posOrNeg = (player.mousePosition - (Vector2)player.transform.position).y > 0 ? 1 : -1;
            transform.rotation = Quaternion.Euler(0, 0, angle * posOrNeg); //set the correct rotation before beginning to swoop

            swooping = true;
        }

        swoopSR.enabled = true;
        player.spriteRenderer.enabled = false;
        player.coreRenderer.enabled = false;

        StartCoroutine(EndSwoop());
    }
    private void SwoopUpdate() //run in update
    {
        if (swooping)
        {
            float angle = Vector2.Angle(player.mousePosition - (Vector2)transform.position, Vector2.right);
            int posOrNeg = (player.mousePosition - (Vector2)transform.position).y > 0 ? 1 : -1;
            Quaternion direction = Quaternion.Euler(0, 0, angle * posOrNeg);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, direction, Time.deltaTime * 100);

            player.playerMovement.rb.velocity = transform.right * 5;
        }
    }
    private IEnumerator EndSwoop()
    {
        yield return new WaitForSeconds(.9f);
        if (IsOwner)
        {
            player.playerMovement.ToggleStun(false);
            player.playerMovement.ToggleGravity(true);
            swooping = false;
        }
        swoopSR.enabled = false;
        player.spriteRenderer.enabled = true;
        player.coreRenderer.enabled = true;

        StartCoroutine(StartCooldown());
    }

    public GameObject flightAura; //assigned in inspector
    private bool flying;
    private void FlightSetup()
    {
        flightAura.transform.SetParent(player.transform);
        flightAura.transform.position = player.transform.position;
    }
    private void Takeflight()
    {
        if (IsOwner)
        {
            player.playerMovement.ToggleGravity(false);
            flying = true;
        }

        flightAura.SetActive(true);

        StartCoroutine(EndFlight());
    }
    private void FlightUpdate() //run in update
    {
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

    private void Whirlwind()
    {

    }
}
