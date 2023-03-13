using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterAbilities : AbilityBase
{
    public override void OnSpawn(Player newPlayer, string newName)
    {
        base.OnSpawn(newPlayer, newName);

        bool hasCore = false;

        if (name == "Flow")
        {
            cooldown = 4;
            hasRange = false;
        }
        else if (name == "Distortion")
        {
            cooldown = 8;
            hasRange = false;
            if (IsOwner)
            {
                distRB.bodyType = RigidbodyType2D.Dynamic;

                distSR.color = new Color32(player.lighterColor.r, player.lighterColor.g, player.lighterColor.b, 153);
                coreRenderer.color = new Color32(player.darkerColor.r, player.darkerColor.g, player.darkerColor.b, 153);
            }
        }
        else if (name == "Tidalwave")
        {
            cooldown = 12;
            hasRange = false;
            hasCore = true;
        }

        spellColor = player.water;
        if (hasCore)
            coreRenderer.color = spellColor.Equals(player.lighterColor) ? player.darkerColor : player.lighterColor;
    }
    public override void TriggerAbility(Vector2 casterPosition, Vector2 aimPoint)
    {
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

    public SpriteRenderer distSR; //assigned in inspector
    public Rigidbody2D distRB; //^
    private int distDirection;
    private void Distortion()
    {
        StartCoroutine(StartCooldown());

        if (IsOwner)
        {
            transform.position = player.transform.position;
            distDirection = player.playerMovement.rb.velocity.x < 0 ? -1 : 1;
            distRB.velocity = player.playerMovement.rb.velocity;

            player.distorting = true;
        }
        else
        {
            player.spriteRenderer.enabled = false;
            player.coreSpriteRenderer.enabled = false;
        }

        StartCoroutine(EndDistortion());
    }
    protected override void Update()
    {
        base.Update();

        if (IsOwner && player.distorting)
        {
            if (distRB.velocity.y < 0)
                distRB.velocity += (player.playerMovement.fallMultiplier - 1) * Physics2D.gravity.y * Time.deltaTime * Vector2.up; //identical code to playerMovement
            distRB.velocity = new(distDirection * player.playerMovement.moveSpeed * player.playerMovement.speedIncrease, distRB.velocity.y);
        }
    }
    private IEnumerator EndDistortion()
    {
        if (IsOwner)
            StartCoroutine(Disappear(1.5f));

        yield return new WaitForSeconds(1.5f);

        if (IsOwner)
        {
            distDirection = 0;
            distRB.velocity = Vector2.zero;
        }

        player.distorting = false;
        player.spriteRenderer.enabled = true;
        player.coreSpriteRenderer.enabled = true;
    }

    public SpriteRenderer tidalSR; //assigned in inspector
    public Rigidbody2D tidalRb; //^
    public BoxCollider2D tidalCol; //^
    public Animator tidalAnim; //^
    private void TidalWave(Vector2 casterPosition, Vector2 aimPoint)
    {
        StartCoroutine(StartCooldown());

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
    private void OnEnterTidalWave(Collider2D col)
    {
        if (!IsServer)
            return;

        if (col.CompareTag("Player") && col.gameObject != player.gameObject)
            col.GetComponent<Player>().HealthChange(-3);
    }
}