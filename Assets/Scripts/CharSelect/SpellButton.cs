using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class SpellButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public SpellSelect spellSelect;
    public TMP_Text buttonText;
    public GameObject description;
    public RectTransform descriptionBackTR;
    public TMP_Text descriptionText;
    public int spellNumber;

    public void ButtonPress()
    {
        spellSelect.SelectSpell(spellNumber);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        StartCoroutine(DescriptionDelay());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        StopAllCoroutines();
        description.SetActive(false);
        descriptionText.text = "";
    }

    private IEnumerator DescriptionDelay()
    {
        yield return new WaitForSeconds(.5f);

        description.SetActive(true);
        descriptionText.text = spellSelect.descriptions[buttonText.text];
    }
}