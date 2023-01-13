using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Index : MonoBehaviour
{
    private Player player;

    public void LoadAttributes(Player newPlayer, string[] newCharSelectInfo)
    {
        player = newPlayer;

        SendMessage(newCharSelectInfo[0]); //using sendmessage to call methods by string. Invoke is delayed
    }

    //elemental index:
    private void Nymph()
    {
        player.maxHealth += 3;
        player.StatChange("speed", 1);
    }
    private void Dragon()
    {
        player.maxHealth += 3;
        player.StatChange("range", 1);
    }
    private void Griffin()
    {
        player.StatChange("power", 1);
        player.StatChange("speed", 1);
    }
    private void Chimera()
    {
        player.StatChange("power", 1);
        player.StatChange("range", 1);
    }

    //spell index:

}
