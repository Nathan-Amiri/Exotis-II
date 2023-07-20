using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TidalwaveElement : NetworkBehaviour
{
    public Animator anim;
    public BoxCollider2D boxCollider;

    private readonly Vector2 spawnPosition = new(4.375f, 1.625f);

    private readonly Dictionary<Player, Coroutine> damagingCoroutines = new(); //server only

    private readonly Vector2[] zones = new Vector2[4]
    {
        new Vector2(1, 1),
        new Vector2(-1, 1),
        new Vector2(1, -1),
        new Vector2(-1, -1)
    };

    public void OnSpawn() //called by MapManager
    {
        if (!IsServer) return;

        StartCoroutine(WaveControl());
    }

    private IEnumerator WaveControl()
    {
        yield return new WaitForSeconds(3); //initial countdown

        while(gameObject.activeSelf)
        {
            yield return new WaitForSeconds(6);

            //determine zone on the server
            Vector2 zone = zones[Random.Range(0, zones.Length)];
            RpcSpawnWave(zone);

            yield return new WaitForSeconds(6);

            RpcDespawnWave();
        }
    }

    [ObserversRpc]
    private void RpcSpawnWave(Vector2 zone)
    {
        //if zone is higher, use offset to get the new position (the y center of the map is not 0)
        Vector2 newSpawnPosition = zone.y == -1 ? spawnPosition : spawnPosition + new Vector2(0, 1);
        transform.position = newSpawnPosition * zone;
        anim.SetTrigger("FadeIn");
    }

    [ObserversRpc]
    private void RpcDespawnWave()
    {
        transform.position = new Vector2(20, 0); //default
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player") && IsServer)
        {
            Player target = col.GetComponent<Player>();
            Coroutine coroutine = StartCoroutine(DamagePlayer(target));
            damagingCoroutines.Add(target, coroutine);
        }
    }

    private void OnTriggerExit2D(Collider2D col)
    {
        if (col.CompareTag("Player") && IsServer)
        {
            Player target = col.GetComponent<Player>();
            StopCoroutine(damagingCoroutines[target]);
            damagingCoroutines.Remove(target);
        }
    }

    private IEnumerator DamagePlayer(Player target) //server only
    {
        while (true)
        {
            target.HealthChange(-1.5f);

            yield return new WaitForSeconds(1.5f);
        }
    }
}