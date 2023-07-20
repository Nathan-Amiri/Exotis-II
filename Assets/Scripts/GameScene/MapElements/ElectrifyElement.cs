using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElectrifyElement : NetworkBehaviour
{
    public static bool localPlayerIsSwinging; //local player cannot swing on more than one tether at once

    public SpriteRenderer sr;
    public Rigidbody2D anchorRB;
    public LineRenderer tetherRenderer;
    public List<LineRenderer> enemyTetherRenderers;

    private DistanceJoint2D tetherJoint;

    private readonly float swingSpeed = .15f; //identical value to Electrify

    private int reverse; //swing direction is reversed when player is above the anchor
    private bool horizontalDown; //true when player presses a new horizontal key

    private Player swingingPlayer; //this client's player, if they're swinging on this tether
    private readonly Dictionary<LineRenderer, Player> enemySwingingPlayers = new(); //enemy players swinging on this tether

    private void Start()
    {
        localPlayerIsSwinging = false;

        anchorRB.transform.SetParent(null, true);

        tetherRenderer.SetPosition(0, anchorRB.position);
        foreach (LineRenderer lineRenderer in enemyTetherRenderers)
            lineRenderer.SetPosition(0, anchorRB.position);
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (localPlayerIsSwinging) return;

        if (!col.CompareTag("Player")) return;

        Player player = col.GetComponent<Player>();
        if (player.IsOwner)
        {
            swingingPlayer = player;
            StartSwing();
        }
    }

    private void StartSwing()
    {
        localPlayerIsSwinging = true;

        RpcServerToggleEnemySwingingPlayer(true, swingingPlayer);

        tetherJoint = swingingPlayer.gameObject.AddComponent<DistanceJoint2D>();
        tetherJoint.connectedBody = anchorRB;

        UpdateReverse();

        swingingPlayer.playerMovement.LockMovement(true);
        swingingPlayer.playerMovement.GiveJump();
        swingingPlayer.playerMovement.ToggleGravity(false);
    }

    private void UpdateReverse()
    {
        reverse = swingingPlayer.transform.position.y > anchorRB.position.y ? 1 : -1;
    }

    [ServerRpc (RequireOwnership = false)]
    private void RpcServerToggleEnemySwingingPlayer(bool on, Player enemy)
    {
        RpcClientToggleEnemySwingingPlayer(on, enemy);
    }
    [ObserversRpc]
    private void RpcClientToggleEnemySwingingPlayer(bool on, Player enemy)
    {
        if (enemy.IsOwner) return;

        if (on)
        {
            LineRenderer enemyTether = null;
            foreach (LineRenderer tether in enemyTetherRenderers)
                if (!tether.enabled)
                {
                    enemyTether = tether;
                    break;
                }

            enemyTether.enabled = true;
            enemySwingingPlayers.Add(enemyTether, enemy);
        }
        else
        {
            foreach (KeyValuePair<LineRenderer, Player> entry in enemySwingingPlayers)
                if (entry.Value == enemy)
                {
                    Debug.Log(entry.Key);
                    Debug.Log(entry.Value);
                    entry.Key.enabled = false;
                    enemySwingingPlayers.Remove(entry.Key);
                    break;
                }
        }
    }

    private void DestroyTether() //run on owners only
    {
        swingingPlayer.playerMovement.LockMovement(false);
        swingingPlayer.playerMovement.ToggleGravity(true);

        Destroy(tetherJoint);

        RpcServerToggleEnemySwingingPlayer(false, swingingPlayer);
        swingingPlayer = null;

        localPlayerIsSwinging = false;
    }

    private void Update()
    {
        sr.enabled = swingingPlayer == null;

        tetherRenderer.SetPosition(1, swingingPlayer != null ? swingingPlayer.transform.position : transform.position);
        foreach(LineRenderer tether in enemyTetherRenderers)
            if (tether.enabled)
            {
                Player enemy = enemySwingingPlayers[tether];
                tether.SetPosition(1, enemy.transform.position);
            }

        if (swingingPlayer != null)
        {
            //swing
            float input = Input.GetAxisRaw("Horizontal");
            Vector2 direction = Vector2.Perpendicular(anchorRB.transform.position - swingingPlayer.transform.position).normalized;
            swingingPlayer.playerMovement.rb.velocity += input * swingSpeed * reverse * direction;

            //update reverse whenever a new direction is pressed
            if (input == 0)
                horizontalDown = false;
            else if (horizontalDown == false)
            {
                UpdateReverse();
                horizontalDown = true;
            }

            if (Input.GetButtonDown("Jump"))
                DestroyTether();
        }
    }
}