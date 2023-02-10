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
        player.lighterColor = player.wind;
        player.darkerColor = player.water;
    }
    private void Leviathan()
    {
        player.maxHealth += 3;
        player.StatChange("range", 1);
        player.lighterColor = player.frost;
        player.darkerColor = player.water;
    }
    private void Dragon()
    {
        player.maxHealth += 3;
        player.StatChange("range", 1);
        player.lighterColor = player.lightning;
        player.darkerColor = player.flame;
    }
    private void Griffin()
    {
        player.StatChange("power", 1);
        player.StatChange("speed", 1);
        player.lighterColor = player.wind;
        player.darkerColor = player.lightning;
    }
    private void Chimera()
    {
        player.StatChange("power", 1);
        player.StatChange("range", 1);
        player.lighterColor = player.flame;
        player.darkerColor = player.venom;
    }

    //spell index:

}
