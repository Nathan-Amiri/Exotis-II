using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public interface INetworkedElement
{
    public int MapNumber();
    public GameObject GetGameObject();
    public void OnSpawn();
}