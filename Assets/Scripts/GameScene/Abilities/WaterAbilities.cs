using FishNet.Object;
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

            DistSetup();
        }
        else if (name == "Tidalwave")
        {
            cooldown = 12;
            hasRange = false;
            hasCore = true;
        }

        spellColor = player.water;
        if (hasCore)
            coreRenderer.color = spellColor.Equals(player.shellColor) ? player.coreColor : player.shellColor;
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
    private bool distorting;
    private void DistSetup()
    {
        byte transparency = (byte)(IsOwner ? 153 : 255);
        distSR.color = new Color32(player.shellColor.r, player.shellColor.g, player.shellColor.b, transparency);
        coreRenderer.color = new Color32(player.coreColor.r, player.coreColor.g, player.coreColor.b, transparency);
    }
    private void Distortion()
    {
        if (IsOwner)
            RpcSendDistortion(player.playerMovement.rb.velocity);
    }
    [ServerRpc]
    private void RpcSendDistortion(Vector2 startingVelocity)
    {
        RpcReceiveDistortion(startingVelocity);
    }
    [ObserversRpc]
    private void RpcReceiveDistortion(Vector2 startingVelocity)
    {
        StartCoroutine(StartCooldown());

        transform.position = player.transform.position;
        distRB.velocity = startingVelocity;
        distDirection = startingVelocity.x < 0 ? -1 : 1;
        distorting = true;

        if (!IsOwner)
        {
            player.spriteRenderer.enabled = false;
            player.coreRenderer.enabled = false;
        }

        StartCoroutine(EndDistortion());
    }
    protected override void Update()
    {
        base.Update();

        if (distorting)
        {
            if (distRB.velocity.y < 0)
                distRB.velocity += (player.playerMovement.fallMultiplier - 1) * Physics2D.gravity.y * Time.deltaTime * Vector2.up; //identical code to playerMovement
            distRB.velocity = new(distDirection * player.playerMovement.moveSpeed * player.playerMovement.speedIncrease, distRB.velocity.y);
        }
    }
    private IEnumerator EndDistortion()
    {
        StartCoroutine(Disappear(1.5f));

        yield return new WaitForSeconds(1.5f);

        distorting = false;
        distDirection = 0;
        distRB.velocity = Vector2.zero;

        if (!IsOwner)
        {
            player.spriteRenderer.enabled = true;
            player.coreRenderer.enabled = true;
        }
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