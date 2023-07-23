using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WhirlwindElement : MonoBehaviour
{
    private readonly float windForce = .22f; //identical force to Whirlwind

    private Player blownPlayer; //this client's player, if it's currently in the wind

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

    private void Update()
    {
        if (blownPlayer != null)
            blownPlayer.playerMovement.AddNewForce(windForce * transform.right);
    }
}