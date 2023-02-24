using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AbilitySelect : MonoBehaviour
{
    public CharSelect charSelect; //assigned in inspector


    private readonly Button[] buttons = new Button[6];
    public Button button0; //^
    public Button button1; //^
    public Button button2; //^
    public Button button3; //^
    public Button button4; //^
    public Button button5; //^
    public Button clearButton; //^

    private readonly GameObject[] boxes = new GameObject[6];
    public GameObject box0; //^
    public GameObject box1; //^
    public GameObject box2; //^
    public GameObject box3; //^
    public GameObject box4; //^
    public GameObject box5; //^

    private readonly Image[] images = new Image[6];
    public Image image0; //^
    public Image image1; //^
    public Image image2; //^
    public Image image3; //^
    public Image image4; //^
    public Image image5; //^

    private readonly TMP_Text[] texts = new TMP_Text[6];
    public TMP_Text text0; //^
    public TMP_Text text1; //^
    public TMP_Text text2; //^
    public TMP_Text text3; //^
    public TMP_Text text4; //^
    public TMP_Text text5; //^

    private readonly string[] waterAbilities = new string[] { "Flow", "Distortion", "Tidalwave" };
    private readonly string[] flameAbilities = new string[] { "Erupt", "Singe", "Heatup" };
    private readonly string[] windAbilities = new string[] { "Swoop", "Takeflight", "Whirlwind" };
    private readonly string[] lightningAbilities = new string[] { "Electrify", "Blink", "Recharge" };
    private readonly string[] frostAbilities = new string[] { "Icybreath", "Hail", "Freeze" };
    private readonly string[] venomAbilities = new string[] { "Fangedbite", "Infect", "Poisoncloud" };

    private List<string> selectedAbilities = new();

    private void Awake()
    {
        buttons[0] = button0;
        buttons[1] = button1;
        buttons[2] = button2;
        buttons[3] = button3;
        buttons[4] = button4;
        buttons[5] = button5;

        boxes[0] = box0;
        boxes[1] = box1;
        boxes[2] = box2;
        boxes[3] = box3;
        boxes[4] = box4;
        boxes[5] = box5;

        images[0] = image0;
        images[1] = image1;
        images[2] = image2;
        images[3] = image3;
        images[4] = image4;
        images[5] = image5;

        texts[0] = text0;
        texts[1] = text1;
        texts[2] = text2;
        texts[3] = text3;
        texts[4] = text4;
        texts[5] = text5;
    }

    public void ElementalSelected(string element1, string element2, Color32[] elementColors)
    {
        string[] element1Abilities = GetAbilities(element1);
        string[] element2Abilities = GetAbilities(element2);

        Clear();

        for (int i = 0; i < 3; i++)
        {
            texts[i].text = element1Abilities[i];
            texts[i + 3].text = element2Abilities[i];

            images[i].color = elementColors[0];
            images[i + 3].color = elementColors[1];
        }
    }

    private string[] GetAbilities(string element)
    {
        switch (element)
        {
            case "water":
                return waterAbilities;
            case "flame":
                return flameAbilities;
            case "wind":
                return windAbilities;
            case "lightning":
                return lightningAbilities;
            case "frost":
                return frostAbilities;
            case "venom":
                return venomAbilities;
        }

        Debug.LogError(element + " is not a valid Element");
        return null;
    }

    public void SelectAbility(int abilityNumber)
    {
        selectedAbilities.Add(texts[abilityNumber].text);

        boxes[abilityNumber].SetActive(true);

        clearButton.interactable = true;

        if (selectedAbilities.Count == 3)
        {
            charSelect.AbilitiesReady(selectedAbilities.ToArray());

            foreach (Button button in buttons)
                button.interactable = false;

            return;
        }

        buttons[abilityNumber].interactable = false;
    }

    public void Clear() //called by clear button (and ElementalSelected)
    {
        foreach (Button button in buttons)
            button.interactable = true;

        foreach (GameObject box in boxes)
            box.SetActive(false);

        selectedAbilities = new();

        clearButton.interactable = false;

        charSelect.Clear(); //turns off ready button
    }
}