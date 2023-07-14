using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Electrify : SpellBase
{
    public LineRenderer tetherRenderer;
    public Rigidbody2D anchorRB;

    private Vector2 failAimDirection;
    private Vector2 tetherHitPoint;
    private DistanceJoint2D tetherJoint;

    private readonly float maxTetherLength = 2.25f;

    public override void OnSpawn(Player newPlayer, string newName)
    {
        base.OnSpawn(newPlayer, newName);

        cooldown = 4;
        spellColor = player.lightning;

        anchorRB.transform.SetParent(null);
    }

    public override void TriggerSpell(Vector2 casterPosition, Vector2 aimPoint)
    {
        base.TriggerSpell(casterPosition, aimPoint);


        if (!IsOwner) return;

        Vector2 aimDirection = (aimPoint - casterPosition).normalized;
        int layerMask = 1 << 7;
        RaycastHit2D hit = Physics2D.Raycast(player.transform.position, aimDirection, maxTetherLength, layerMask);
        if (hit.collider == null)
        {
            cooldown = .5f; //if it fails

            StartCoroutine(ElectrifyFail(aimDirection));
            RpcSendServerElectrifyFail(aimDirection);
        }
        else
        {
            cooldown = 4; //default

            ToggleTether(true, hit.point);
            RpcServerToggleTether(true, hit.point);

            anchorRB.transform.position = hit.point;
            tetherJoint = player.gameObject.AddComponent<DistanceJoint2D>();
            tetherJoint.connectedBody = anchorRB;

            player.playerMovement.GiveJump();

            player.StatChange("speed", 2);
        }

        StartCoroutine(StartCooldown());
    }

    [ServerRpc]
    private void RpcSendServerElectrifyFail(Vector2 aimDirection)
    {
        RpcSendClientElectrifyFail(aimDirection);
    }
    [ObserversRpc]
    private void RpcSendClientElectrifyFail(Vector2 aimDirection)
    {
        if (IsOwner) return;

        StartCoroutine(ElectrifyFail(aimDirection));
    }
    private IEnumerator ElectrifyFail(Vector2 aimDirection)
    {
        tetherRenderer.enabled = true;
        failAimDirection = aimDirection;
        yield return new WaitForSeconds(.1f);
        failAimDirection = default;
        tetherRenderer.enabled = false;
    }

    [ServerRpc]
    private void RpcServerToggleTether(bool on, Vector2 newHitPoint)
    {
        RpcClientToggleTether(on, newHitPoint);
    }
    [ObserversRpc]
    private void RpcClientToggleTether(bool on, Vector2 newHitPoint)
    {
        if (IsOwner) return;

        ToggleTether(on, newHitPoint);
    }
    private void ToggleTether(bool on, Vector2 newHitPoint) //run on owners and non-owners
    {
        if (on)
        {
            tetherRenderer.enabled = true;
            tetherHitPoint = newHitPoint;
        }
        else
        {
            tetherRenderer.enabled = false;
            tetherHitPoint = default;
        }
    }

    private void DestroyTether() //run on owners only
    {
        player.StatChange("speed", -2);

        Destroy(tetherJoint);
        ToggleTether(false, default);
        RpcServerToggleTether(false, default);
    }

    protected override void Update()
    {
        base.Update();

        if (tetherRenderer.enabled == true)
        {
            tetherRenderer.SetPosition(0, player.transform.position);
            if (failAimDirection != default)
                tetherRenderer.SetPosition(1, player.transform.position + ((Vector3)failAimDirection * maxTetherLength));
            else
                tetherRenderer.SetPosition(1, tetherHitPoint);
        }

        if (IsOwner && tetherHitPoint != default && Input.GetButtonDown("Jump"))
            DestroyTether();
    }

    public override void GameEnd()
    {
        base.GameEnd();

        if (IsOwner)
            DestroyTether();
    }
}