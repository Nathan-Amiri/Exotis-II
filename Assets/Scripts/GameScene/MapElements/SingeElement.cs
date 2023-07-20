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
            col.GetComponent<Rigidbody2D>().velocity = explodeDirection * explodeForce;
            target.playerMovement.GiveJump();
        }
    }
}