using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tidalwave : SpellBase
{
    public SpriteRenderer coreRenderer; //assigned in inspector

    public SpriteRenderer tidalSR; //assigned in inspector
    public Rigidbody2D tidalRb; //^
    public BoxCollider2D tidalCol; //^
    public Animator tidalAnim; //^

    public override void OnSpawn(Player newPlayer, string newName)
    {
        base.OnSpawn(newPlayer, newName);

        cooldown = 12;
        spellColor = player.water;
        SetCore(coreRenderer);
    }

    public override void TriggerSpell(Vector2 casterPosition, Vector2 aimPoint)
    {
        base.TriggerSpell(casterPosition, aimPoint);

        StartCoroutine(StartCooldown());

        float angle = Vector2.Angle(aimPoint - casterPosition, Vector2.right);
        int posOrNeg = (aimPoint - casterPosition).y > 0 ? 1 : -1;
        Quaternion rotation = Quaternion.Euler(0, 0, angle * posOrNeg);
        Vector3 position = casterPosition;
        transform.SetPositionAndRotation(position, rotation);

        Color32 tempColor = tidalSR.color;
        tempColor.a = 0; //make transparent
        tidalSR.color = tempColor;

        StartCoroutine(TidalWaveFade());
    }
    private IEnumerator TidalWaveFade()
    {
        tidalAnim.SetTrigger("FadeIn");
        yield return new WaitForSeconds(1);
        tidalAnim.StopPlayback();
        StartTidalWave();
    }
    private void StartTidalWave()
    {
        tidalCol.enabled = true;
        tidalRb.velocity = 5 * transform.right;
        StartCoroutine(ResetTidalWave());
    }
    private IEnumerator ResetTidalWave()
    {
        yield return new WaitForSeconds(5);
        transform.SetPositionAndRotation(new Vector2(-15, 0), Quaternion.identity);
        tidalRb.velocity = Vector2.zero;
        tidalCol.enabled = false;
    }
    private void OnTriggerEnter2D(Collider2D col)
    {
        if (!IsServer)
            return;

        if (col.CompareTag("Player") && col.gameObject != player.gameObject)
            col.GetComponent<Player>().HealthChange(-3);
    }
}