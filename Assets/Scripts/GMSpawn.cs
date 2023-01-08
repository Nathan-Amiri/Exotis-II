using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;
using FishNet;

public class GMSpawn : NetworkBehaviour
{
    public GameObject gameManager; //assigned in inspector

    private bool hasInstantiated;
    private void Update()
    {
        if (IsServer && !hasInstantiated)
        {
            hasInstantiated = true;
            GameObject gm = Instantiate(gameManager);
            InstanceFinder.ServerManager.Spawn(gm);
        }
    }
}