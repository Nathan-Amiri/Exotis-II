using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface INetworkedElement
{
    public int MapNumber();
    public GameObject GetGameObject();
    public void OnSpawn();

    public void OnDespawn();
}