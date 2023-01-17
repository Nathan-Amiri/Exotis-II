using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundCheck : MonoBehaviour
{
    public PlayerMovement playerMovement; //assigned in inspector

    private void OnTriggerStay2D(Collider2D col)
    {
        if (col.gameObject.CompareTag("Terrain") || col.gameObject.CompareTag("Player"))
            playerMovement.isGrounded = true;
    }

    private void OnTriggerExit2D(Collider2D col)
    {
        if (col.gameObject.CompareTag("Terrain") || col.gameObject.CompareTag("Player"))
            StartCoroutine(playerMovement.CoyoteTime());
    }
}