using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IcybreathElement : NetworkBehaviour, INetworkedElement
{
    private readonly Vector2[] spawnPositions = new Vector2[4]
    {
        new(-12, -11.5f),
        new(-12, 12.5f),
        new(12, -11.5f),
        new(12, 12.5f)
    };

    private readonly float[] spawnZRotations = { 45, -45, 135, 225 };

    private bool icyGrow;


    public int MapNumber() { return 4; }
    public GameObject GetGameObject() { return gameObject; }

    public void OnSpawn() //called by MapManager
    {
        if (!IsServer) return;

        StartCoroutine(IceControl());
    }

    private IEnumerator IceControl()
    {
        yield return new WaitForSeconds(3); //initial countdown

        while (gameObject.activeSelf)
        {
            yield return new WaitForSeconds(10);

            //determine zone on the server
            int spawnInt = Random.Range(0, 4);
            RpcSpawnIce(spawnInt);

            yield return new WaitForSeconds(8);

            RpcDespawnIce();
        }
    }

    [ObserversRpc]
    private void RpcSpawnIce(int spawnInt)
    {
        transform.SetPositionAndRotation(spawnPositions[spawnInt], Quaternion.Euler(0, 0, spawnZRotations[spawnInt]));
        StartCoroutine(IcyGrow());
    }

    [ObserversRpc]
    private void RpcDespawnIce()
    {
        transform.SetPositionAndRotation(new(20, 0), Quaternion.identity); //default
    }

    private IEnumerator IcyGrow()
    {
        icyGrow = true;
        yield return new WaitForSeconds(2);
        icyGrow = false;
        transform.position = new(0, .5f);
    }

    private void Update()
    {
        if (icyGrow)
            transform.position += 8.485f * Time.deltaTime * transform.right;
    }
}