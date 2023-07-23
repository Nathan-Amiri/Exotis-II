using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElectrifyElement : NetworkBehaviour
{
    public SpriteRenderer sr;
    public Rigidbody2D anchorRB;
    public LineRenderer tetherRenderer;
    public List<LineRenderer> enemyTetherRenderers;

    private DistanceJoint2D tetherJoint;

    //values identical to Electrify
    private readonly float swingSpeed = 9;
    private readonly float endBoost = 10;

    private Player swingingPlayer; //this client's player, if they're swinging on this tether
    private readonly Dictionary<LineRenderer, Player> enemySwingingPlayers = new(); //enemy players swinging on this tether

    private void Start()
    {
        Electrify.localElectrifyActive = false;

        anchorRB.transform.SetParent(null, true);

        //set 0 positions. (temporarily set 1 positions also, to prevent tethers from appearing on screen before update is called)
        for (int i = 0; i < 2; i++)
        {
            tetherRenderer.SetPosition(i, anchorRB.position);
            foreach (LineRenderer lineRenderer in enemyTetherRenderers)
                lineRenderer.SetPosition(i, anchorRB.position);
        }
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (Electrify.localElectrifyActive) return;

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
        Electrify.localElectrifyActive = true;

        RpcServerToggleEnemySwingingPlayer(true, swingingPlayer);

        tetherJoint = swingingPlayer.gameObject.AddComponent<DistanceJoint2D>();
        tetherJoint.connectedBody = anchorRB;

        swingingPlayer.playerMovement.ToggleFreeze(true);
        swingingPlayer.playerMovement.GiveJump();
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
                    entry.Key.enabled = false;
                    enemySwingingPlayers.Remove(entry.Key);
                    break;
                }
        }
    }

    private void DestroyTether() //run on owners only
    {
        swingingPlayer.playerMovement.ToggleFreeze(false);

        Destroy(tetherJoint);

        RpcServerToggleEnemySwingingPlayer(false, swingingPlayer);
        swingingPlayer = null;

        Electrify.localElectrifyActive = false;
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
            Vector2 direction = -1 * Vector2.Perpendicular(anchorRB.transform.position - swingingPlayer.transform.position).normalized;
            swingingPlayer.playerMovement.rb.velocity = input * swingSpeed * direction;

            if (Input.GetButtonDown("Jump"))
            {
                swingingPlayer.playerMovement.AddNewForce(input * endBoost * direction);

                DestroyTether();
            }
        }
    }
}