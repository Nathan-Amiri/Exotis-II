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
        }
        else if (name == "TakeFlight")
        {
            cooldown = 12;
            hasRange = false;
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
        if (name == "Swoop") Swoop();
        if (name == "Takeflight") Takeflight();
        if (name == "Whirlwind" ) Whirlwind();
    }

    private bool swooping;
    private void Swoop()
    {
        StartCoroutine(StartCooldown());

        if (IsServer)
            StartCoroutine(player.BecomeImmune(1));
        if (IsOwner)
        {
            player.playerMovement.ToggleStun(true);
            player.playerMovement.ToggleGravity(false);

            float angle = Vector2.Angle(player.mousePosition - (Vector2)player.transform.position, Vector2.right);
            int posOrNeg = (player.mousePosition - (Vector2)player.transform.position).y > 0 ? 1 : -1;
            transform.rotation = Quaternion.Euler(0, 0, angle * posOrNeg);

            swooping = true;
        }

        player.spriteRenderer.enabled = false;
        player.coreRenderer.enabled = false;

        StartCoroutine(EndSwoop());
    }
    protected override void Update()
    {
        base.Update();

        if (swooping)
        {
            transform.position = player.transform.position;

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
        player.spriteRenderer.enabled = true;
        player.coreRenderer.enabled = true;

        transform.SetPositionAndRotation(new Vector2(-15, 0), Quaternion.identity);
    }

    private void Takeflight()
    {

    }

    private void Whirlwind()
    {

    }
}
