using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SpellSelect : MonoBehaviour
{
    //assigned in inspector
    public CharSelect charSelect;
    public Button clearButton;
    public List<GameObject> numbers = new();
    public List<Button> buttons = new();
    public List<GameObject> boxes = new();
    public List<TMP_Text> texts = new();

    private List<string> selectedAbilities = new();
    private List<int> spellNumbers = new(); //used in charselect for importing loadout

    private readonly string[] waterAbilities = new string[] { "Flow", "Distortion", "Tidalwave" };
    private readonly string[] flameAbilities = new string[] { "Erupt", "Singe", "Heatup" };
    private readonly string[] windAbilities = new string[] { "Swoop", "Takeflight", "Whirlwind" };
    private readonly string[] lightningAbilities = new string[] { "Electrify", "Blink", "Recharge" };
    private readonly string[] frostAbilities = new string[] { "Icybreath", "Hail", "Freeze" };
    private readonly string[] venomAbilities = new string[] { "Fangedbite", "Infect", "Poisoncloud" };

    [NonSerialized] public Dictionary<string, string> descriptions = new() //read by SpellButton
    {
        ["Flow"] = "Deal low damage to nearby enemies. If you hit one, heal yourself\nCooldown: 4",
        ["Distortion"] = "Briefly turn invisible, summoning a decoy in your place\nCooldown: 8",
        ["Tidalwave"] = "Summon a giant wave that surges forward, dealing high damage\nCooldown: 12",
        ["Erupt"] = "Launch three fireballs that\ndeal high damage\nCooldown: 4",
        ["Singe"] = "Throw out a fireball which deals high damage and, when touched, blasts you away\nCooldown: 8",
        ["Heatup"] = "Channel to temporarily increase power and range\nCooldown: 12",
        ["Swoop"] = "Quickly soar through the air, using your cursor to steer\nCooldown: 8",
        ["Takeflight"] = "Temporarily gain the\nability to fly. Recast to go up, release to go down\nCooldown: 12",
        ["Whirlwind"] = "Summon a powerful wind that blows you and/or enemies away\nCooldown: 4",
        ["Electrify"] = "Uses static electricity to tether yourself to nearby terrain. Jump to release the tether\nCooldown: 4",
        ["Blink"] = "Teleport a short distance away, leaving behind a zone which deals low damage\nCooldown: 8",
        ["Recharge"] = "Channel to heal yourself and temporarily increase your speed\nCooldown: 12",
        ["Icybreath"] = "Summon a long beam\nof ice as terrain\nCooldown: 12",
        ["Hail"] = "Create a storm cloud, which rains down hail after a delay, dealing high damage\nCooldown: 4",
        ["Freeze"] = "Create an icy zone. While inside, your speed is\nincreased and enemies are drastically slowed\nCooldown: 8",
        ["Fangedbite"] = "Summon a set of jaws which snap forward, dealing high damage\nCooldown: 4",
        ["Infect"] = "Your next 3 shots that hit terrain explode, spawning a poison cloud that deals low damage\nCooldown: 12",
        ["Poisoncloud"] = "Summon a toxic cloud that deals high damage and hides you from enemies. Must be cast while on the ground\nCooldown: 8"
    };

    public void ElementalSelected(string element1, string element2, Color32[] elementColors)
    {
        string[] element1Abilities = GetAbilities(element1);
        string[] element2Abilities = GetAbilities(element2);

        Clear();

        for (int i = 0; i < 3; i++)
        {
            texts[i].text = element1Abilities[i];
            texts[i + 3].text = element2Abilities[i];

            buttons[i].image.color = elementColors[0];
            buttons[i + 3].image.color = elementColors[1];
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

    public void SelectSpell(int spellNumber) //called by SpellButton and CharSelect
    {
        selectedAbilities.Add(texts[spellNumber].text);
        spellNumbers.Add(spellNumber);

        boxes[spellNumber].SetActive(true);

        clearButton.interactable = true;

        buttons[spellNumber].interactable = false;

        GameObject number = numbers[selectedAbilities.Count - 1];
        float buttonY = buttons[spellNumber].transform.position.y;
        number.transform.position = new Vector2(number.transform.position.x, buttonY);
        number.SetActive(true);

        if (selectedAbilities.Count == 3)
        {
            charSelect.AbilitiesReady(selectedAbilities.ToArray(), spellNumbers.ToArray());

            foreach (Button button in buttons)
                button.interactable = false;
        }
    }

    public void Clear() //called by clear button (and ElementalSelected)
    {
        foreach (Button button in buttons)
            button.interactable = true;

        foreach (GameObject box in boxes)
            box.SetActive(false);

        foreach (GameObject number in numbers)
            number.SetActive(false);

        selectedAbilities = new();
        spellNumbers = new();

        clearButton.interactable = false;

        charSelect.Clear(); //turns off ready button
    }
}