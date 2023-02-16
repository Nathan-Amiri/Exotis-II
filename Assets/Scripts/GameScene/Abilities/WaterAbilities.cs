using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class WaterAbilities : AbilityBase
{
    public override void OnSpawn(Player newPlayer, string newName)
    {
        base.OnSpawn(newPlayer, newName);

        if (name == "Flow")
        {
            cooldown = 4;
            hasRange = false;
        }
        else if (name == "Distortion")
        {
            cooldown = 8;
            hasRange = false;
        }
        if (name == "Tidalwave")
        {
            cooldown = 1;//12;
            hasRange = false;
        }
    }
    public override void TriggerAbility(Vector2 casterPosition, Vector2 aimPoint)
    {
        base.TriggerAbility(casterPosition, aimPoint);

        if (name == "Flow") Flow();
        if (name == "Distortion") Distortion();
        if (name == "Tidalwave") TidalWave(casterPosition, aimPoint);
    }
    private void OnTriggerEnter2D(Collider2D col)
    {
        if (name == "Tidalwave") OnEnterTidalWave(col);
    }

    private void Flow()
    {

    }

    private void Distortion()
    {

    }

    public SpriteRenderer tidalSR;
    public Rigidbody2D tidalRb;
    public BoxCollider2D tidalCol;
    public Animator tidalAnimator;
    private void TidalWave(Vector2 casterPosition, Vector2 aimPoint)
    {
        float angle = Vector2.Angle(aimPoint - casterPosition, Vector2.right);
        int posOrNeg = (aimPoint - casterPosition).y > 0 ? 1 : -1;
        transform.rotation = Quaternion.Euler(0, 0, angle * posOrNeg);

        Color32 tempColor = tidalSR.color;
        tempColor.a = 0;
        tidalSR.color = tempColor;

        transform.position = casterPosition;
        StartCoroutine(TidalWaveFade());
    }
    private IEnumerator TidalWaveFade()
    {
        tidalAnimator.SetTrigger("FadeIn");
        yield return new WaitForSeconds(1);
        tidalAnimator.StopPlayback();
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
    private void OnEnterTidalWave(Collider2D col)
    {
        if (col.CompareTag("Player") && col.gameObject != player.gameObject)
            col.GetComponent<Player>().HealthChange(-3);
    }
}