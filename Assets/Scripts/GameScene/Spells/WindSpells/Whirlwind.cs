using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Whirlwind : SpellBase
{
    public SpriteRenderer spriteRenderer;
    public SpriteRenderer coreRenderer;
    public BoxCollider2D boxCollider;

    private readonly float windForce = 35;

    //direction wind is blowing
    private Vector2 windDirection;
    //this client's player, if it's currently in the wind
    private Player blownPlayer;

    public override void OnSpawn(Player newPlayer, string newName)
    {
        base.OnSpawn(newPlayer, newName);

        cooldown = 4;
        spellColor = player.wind;

        SetCore(coreRenderer);
    }

    public override void TriggerSpell(Vector2 casterPosition, Vector2 aimPoint)
    {
        base.TriggerSpell(casterPosition, aimPoint);

        windDirection = (aimPoint - casterPosition).normalized;

        Vector2 aimPosition = casterPosition + (windDirection * .3f);

        float angle = Vector2.Angle(windDirection, Vector2.right);
        int posOrNeg = windDirection.y > 0 ? 1 : -1;
        Quaternion newRotation = Quaternion.Euler(0, 0, angle * posOrNeg);
        
        transform.SetPositionAndRotation(aimPosition, newRotation);

        float duration = 2;

        StartCoroutine(DisappearDelay(duration));
        StartCoroutine(CooldownDelay(duration));
    }

    private IEnumerator CooldownDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        StartCoroutine(StartCooldown());
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            //blow player if this client owns it
            Player newPlayer = col.GetComponent<Player>();
            if (newPlayer.IsOwner)
                blownPlayer = newPlayer;
        }
    }
    private void OnTriggerExit2D(Collider2D col)
    {
        if (blownPlayer != null && col.CompareTag("Player") && col.GetComponent<Player>() == blownPlayer)
            blownPlayer = null;
    }

    protected override void Update()
    {
        base.Update();

        //wind is more powerful horizontally (to compensate for x movement)
        if (blownPlayer != null)
            blownPlayer.playerMovement.rb.AddForce(windForce * windDirection);
    }

    public override void GameEnd()
    {
        base.GameEnd();

        Disappear();
    }
}