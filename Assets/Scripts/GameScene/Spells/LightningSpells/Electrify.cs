using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Electrify : SpellBase
{
    public PolygonCollider2D electrifyCol; //assigned in inspector

    public override void OnSpawn(Player newPlayer, string newName)
    {
        base.OnSpawn(newPlayer, newName);

        cooldown = 8;
        hasRange = false;
        spellColor = player.lightning;
    }

    public override void TriggerSpell(Vector2 casterPosition, Vector2 aimPoint)
    {
        base.TriggerSpell(casterPosition, aimPoint);

        StartCoroutine(StartCooldown());

        electrifyCol.enabled = true;
        player.playerMovement.ToggleStun(true);
        player.playerMovement.ToggleGravity(false);
        player.playerMovement.rb.velocity = Vector2.zero; //toggleStun doesn't alter y velocity

        float angle = Vector2.Angle(aimPoint - casterPosition, Vector2.right);
        int posOrNeg = (aimPoint - casterPosition).y > 0 ? 1 : -1;
        transform.rotation = Quaternion.Euler(0, 0, (angle * posOrNeg) - 90);
        transform.position = player.transform.position + (.7f * transform.up); //uses new rotation, so can't use SetPositionAndRotation

        StartCoroutine(ElectrifyToggle());
        StartCoroutine(ElectrifyDashStart(transform.up));
    }
    private IEnumerator ElectrifyToggle()
    {
        yield return new WaitForSeconds(.1f);
        electrifyCol.enabled = false;
    }
    private IEnumerator ElectrifyDashStart(Vector2 direction)
    {
        yield return new WaitForSeconds(.4f);
        player.playerMovement.ToggleStun(false);
        player.playerMovement.rb.velocity += direction * 20;
        StartCoroutine(DashEnd());

        transform.position -= (Vector3)(1.4f * direction);

        StartCoroutine(Disappear(.2f));
    }
    private IEnumerator DashEnd()
    {
        yield return new WaitForSeconds(.1f);
        player.playerMovement.ToggleGravity(true);
    }

    public override void GameEnd()
    {
        base.GameEnd();


    }
}