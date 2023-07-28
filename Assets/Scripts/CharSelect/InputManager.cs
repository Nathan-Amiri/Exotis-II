using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class InputManager : MonoBehaviour
{
    public static Dictionary<string, KeyCode> InputIndex { get; private set; }

    private bool waitingForInput;
    private InputButton currentButton;
    private string currentKey;

    private readonly Dictionary<string, KeyCode> defaultIndex = new()
    {
        ["Left"] = KeyCode.A,
        ["Right"] = KeyCode.D,
        ["Jump"] = KeyCode.Space,
        ["Fire"] = KeyCode.Mouse0,
        ["Spell1"] = KeyCode.Mouse1,
        ["Spell2"] = KeyCode.S,
        ["Spell3"] = KeyCode.LeftShift,
    };

    //must occur before InputButton's Start reads InputIndex
    private void Awake()
    {
        InputIndex = new();

        PopulateIndex("Left");
        PopulateIndex("Right");
        PopulateIndex("Jump");
        PopulateIndex("Fire");
        PopulateIndex("Spell1");
        PopulateIndex("Spell2");
        PopulateIndex("Spell3");
    }

    private void PopulateIndex(string indexKey)
    {
        if (PlayerPrefs.HasKey(indexKey))
        {
            //convert saved string to keycode
            KeyCode playerPrefsCode = (KeyCode)Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString(indexKey));
            InputIndex.Add(indexKey, playerPrefsCode);
        }
        else
            InputIndex.Add(indexKey, defaultIndex[indexKey]);
    }

    public void ChangeInput(InputButton inputButton, string newInput)
    {
        if (waitingForInput) return;

        currentButton = inputButton;
        currentKey = newInput;

        currentButton.tmpText.fontStyle = TMPro.FontStyles.Italic; //turns off bold
        currentButton.tmpText.fontSize = 15;
        currentButton.tmpText.text = "Press new key...";

        waitingForInput = true;
    }

    private void Update()
    {
        if (!waitingForInput) return;

        foreach (KeyCode keyCode in Enum.GetValues(typeof(KeyCode)))
        {
            if (Input.GetKeyDown(keyCode))
            {
                NewInput(keyCode);
                waitingForInput = false;
            }
        }
    }

    private void NewInput(KeyCode newCode)
    {
        currentButton.tmpText.fontStyle = TMPro.FontStyles.Bold; //turns off italics
        currentButton.tmpText.fontSize = 30;
        currentButton.tmpText.text = GetButtonText(newCode.ToString());

        InputIndex.Remove(currentKey);
        InputIndex.Add(currentKey, newCode);

        PlayerPrefs.SetString(currentKey, newCode.ToString());
    }

    private string GetButtonText(string newInput)
    {
        return newInput switch
        {
            "Alpha1" => "1",
            "Alpha2" => "2",
            "Alpha3" => "3",
            "Alpha4" => "4",
            "Alpha5" => "5",
            "Alpha6" => "6",
            "Alpha7" => "7",
            "Alpha8" => "8",
            "Alpha9" => "9",
            "Alpha0" => "0",
            "UpArrow" => "Up",
            "DownArrow" => "Down",
            "LeftArrow" => "Left",
            "RightArrow" => "Right",
            _ => newInput,
        };
    }


}