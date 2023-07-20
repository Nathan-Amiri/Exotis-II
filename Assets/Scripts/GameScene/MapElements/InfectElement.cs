using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfectElement : NetworkBehaviour
{
    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            Player target = col.GetComponent<Player>();
            if (!target.IsOwner) return;

            Rigidbody2D rb = target.GetComponent<Rigidbody2D>();
            rb.velocity = new Vector2(rb.velocity.x, 10);

            //damage detected on the client rather than on the server to ensure that damage and explosion always coincide
            RpcSendDamageToServer(target);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RpcSendDamageToServer(Player target)
    {
        target.HealthChange(-1.5f);
    }
}