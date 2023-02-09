using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityButton : MonoBehaviour
{
    public AbilitySelect abilitySelect;

    public int abilityNumber; //set in inspector

    public void ButtonPress()
    {
        abilitySelect.SelectAbility(abilityNumber);
    }
}