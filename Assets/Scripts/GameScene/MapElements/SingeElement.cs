using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingeElement : MonoBehaviour
{
    private readonly float explodeForce = 20; //force identical to singe

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            Player target = col.gameObject.GetComponent<Player>();
            if (!target.IsOwner) return;

            Vector2 explodeDirection = (col.transform.position - transform.position).normalized;
            target.playerMovement.rb.velocity *= new Vector2(1, 0);
            target.playerMovement.AddNewForce(explodeDirection * explodeForce);
            target.playerMovement.GiveJump();
        }
    }
}