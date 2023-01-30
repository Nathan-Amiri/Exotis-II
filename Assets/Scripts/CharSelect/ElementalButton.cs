using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ElementalButton : MonoBehaviour
{
    private Button button;

    private CharSelect charSelect;

    public string type1; //assigned in inspector
    public string type2; //^
    public string stat1; //^
    public string stat2; //^

    private void OnEnable()
    {
        GameManager.OnClientConnectOrLoad += OnSpawn;
    }
    private void OnDisable()
    {
        GameManager.OnClientConnectOrLoad -= OnSpawn;
    }

    private void OnSpawn(GameManager gm)
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(ButtonPress);
        charSelect = GameObject.Find("NobScripts").GetComponent<CharSelect>(); //can't be run in awake, since nobscripts haven't loaded yet (on scene change)
    }

    public void ButtonPress()
    {
        charSelect.SelectElemental(name,  type1, type2, stat1, stat2);
    }
}