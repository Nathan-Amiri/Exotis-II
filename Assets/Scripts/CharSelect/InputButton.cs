using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InputButton : MonoBehaviour
{
    public InputManager inputManager;
    public Button button;
    public TMP_Text tmpText;

    public string input;

    //must occur after InputButton's Awake populates InputIndex
    private void Start()
    {
        tmpText.text = InputManager.InputIndex[input].ToString();
    }

    public void SelectButton()
    {
        inputManager.ChangeInput(this, input);
    }
}