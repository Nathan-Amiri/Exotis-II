using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellButton : MonoBehaviour
{
    public SpellSelect spellSelect;

    public int spellNumber; //set in inspector

    public void ButtonPress()
    {
        spellSelect.SelectSpell(spellNumber);
    }
}