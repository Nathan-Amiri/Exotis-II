using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fangedbite : SpellBase
{
    public SpriteRenderer neckSR;
    public Animator anim;

    private readonly List<GameObject> bittenPlayers = new(); //only used by server

    public override void OnSpawn(Player newPlayer, string newName)
    {
        base.OnSpawn(newPlayer, newName);

        cooldown = 4;
        spellColor = player.venom;

        SetCore(neckSR);
    }

    public override void TriggerSpell(Vector2 casterPosition, Vector2 aimPoint)
    {
        base.TriggerSpell(casterPosition, aimPoint);

        bittenPlayers.Clear();

        StartCoroutine(StartCooldown());

        float angle = Vector2.Angle(aimPoint - casterPosition, Vector2.right);
        int posOrNeg = (aimPoint - casterPosition).y > 0 ? 1 : -1;
        Quaternion rotation = Quaternion.Euler(0, 0, angle * posOrNeg);
        Vector3 position = casterPosition;
        transform.SetPositionAndRotation(position, rotation);

        anim.SetTrigger("Bite");
        //bite animation duration is 1.3 seconds
        StartCoroutine(DisappearDelay(1.3f));
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (IsServer && col.CompareTag("Player") && col.gameObject != player.gameObject)
        {
            if (bittenPlayers.Contains(col.gameObject)) return; //prevent players from being damaged by multiple hitboxes

            col.GetComponent<Player>().HealthChange(-3);
            bittenPlayers.Add(col.gameObject);
        }
    }

    public override void GameEnd()
    {
        base.GameEnd();

        Disappear();
    }
}