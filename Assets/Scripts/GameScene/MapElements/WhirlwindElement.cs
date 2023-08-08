using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WhirlwindElement : MonoBehaviour
{
    //identical force to Whirlwind
    private readonly float windForce = 100;

    //this client's player, if it's currently in the wind
    private Player blownPlayer;

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            //blow player if this client owns it
            Player newPlayer = col.GetComponent<Player>();
            if (newPlayer.IsOwner)
                blownPlayer = newPlayer;
        }
    }

    private void OnTriggerExit2D(Collider2D col)
    {
        if (blownPlayer != null && col.CompareTag("Player") && col.GetComponent<Player>() == blownPlayer)
            blownPlayer = null;
    }

    private void FixedUpdate()
    {
        if (blownPlayer != null)
            blownPlayer.playerMovement.AddNewForce(windForce * Time.fixedDeltaTime * transform.right);
    }
}