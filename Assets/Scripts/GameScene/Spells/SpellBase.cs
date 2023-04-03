using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using FishNet.Object;
using UnityEngine.UI;

public abstract class SpellBase : NetworkBehaviour
{
    protected float cooldown; //set by derived class

    [NonSerialized] public bool spellLock; //read by Player

    [NonSerialized] public float spellRange; //set by derived class
    [NonSerialized] public Color32 spellColor; //^
    [NonSerialized] public bool hasRange; //^, read by Player

    [NonSerialized] public Player player;

    public virtual void OnSpawn(Player newPlayer, string newName) //called by player
    {
        name = newName;
        player = newPlayer;
    }

    public virtual void SetCore(SpriteRenderer coreRenderer)
    {
        coreRenderer.color = spellColor.Equals(player.shellColor) ? player.coreColor : player.shellColor;
    }

    public virtual void TriggerSpell(Vector2 casterPosition, Vector2 mousePosition)
    {
        spellLock = true;
    }

    [NonSerialized] public Image spellGray; //set by Player, not used by derived class
    private float remainingCooldown;
    protected IEnumerator StartCooldown()
    {
        remainingCooldown = cooldown;
        yield return new WaitForSeconds(cooldown);
        spellLock = false;
    }
    protected virtual void Update()
    {
        if (remainingCooldown > 0)
        {
            remainingCooldown -= Time.deltaTime;
            spellGray.fillAmount = remainingCooldown / cooldown;
        }
    }

    protected IEnumerator Disappear(float delay)
    {
        yield return new WaitForSeconds(delay);
        transform.position = new Vector2(-15, 0);
    }

    public virtual void GameEnd()
    {
        StopAllCoroutines();
        remainingCooldown = 0;
        spellGray.fillAmount = 0;
        spellLock = false;
    }
}