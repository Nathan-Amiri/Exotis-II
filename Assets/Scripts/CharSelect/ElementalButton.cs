using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElementalButton : MonoBehaviour
{
    public CharSelect charSelect;

    public string type1; //set in inspector
    public string type2; //^
    public string stat1; //^
    public string stat2; //^

    public void ButtonPress()
    {
        charSelect.SelectElemental(name,  type1, type2, stat1, stat2);
    }
}